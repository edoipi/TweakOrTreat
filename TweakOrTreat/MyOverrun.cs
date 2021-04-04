using System;
using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Animation.Kingmaker;
using Pathfinding;
using UnityEngine;

namespace TweakOrTreat
{
    public class MyOverrun : AbilityCustomOverrun
    {
        void logStatus(UnitCombatState user, UnitEntityData target)
        {
            Main.logger.Log($"logging aoo status");
            Main.logger.Log($"PreventAttacksOfOpporunityNextFrame: {user.PreventAttacksOfOpporunityNextFrame}");
            Main.logger.Log($"target.PreventAttacksOfOpporunityNextFrame: {target.CombatState.PreventAttacksOfOpporunityNextFrame}");
            Main.logger.Log($"target.Descriptor.State.IsDead: {target.Descriptor.State.IsDead}");
            Main.logger.Log($"CanActInCombat: {user.CanActInCombat}");
            Main.logger.Log($"HasCondition: {user.Unit.Descriptor.State.HasCondition(UnitCondition.AttackOfOpportunityBeforeInitiative)}");
            Main.logger.Log($"CanAttackOfOpportunity: {user.CanAttackOfOpportunity}");
            Main.logger.Log($"CanAct: {user.Unit.Descriptor.State.CanAct}");
            
            UnitPartForceMove unitPartForceMove = target.Get<UnitPartForceMove>();
            Main.logger.Log($"target unitPartForceMove: {unitPartForceMove != null}");
            if(unitPartForceMove != null)
                Main.logger.Log($"target unitPartForceMove ProvokeAttackOfOpportunity: {unitPartForceMove.ProvokeAttackOfOpportunity}");

            Main.logger.Log($"CommandTargetUntargetable: {UnitCommand.CommandTargetUntargetable(user.Unit, target, null)}");

            Main.logger.Log($"HasMotionThisTick: {user.Unit.HasMotionThisTick}");

            Main.logger.Log($"GetThreatHand: {user.Unit.GetThreatHand() == null}");
            Main.logger.Log($"AttackOfOpportunityCount: {user.AttackOfOpportunityCount}");

            Main.logger.Log($"target.Memory.Contains: {target.Memory.Contains(user.Unit)}");

            Main.logger.Log($"target.ImmuneToAttackOfOpportunity: {target.Descriptor.State.HasCondition(UnitCondition.ImmuneToAttackOfOpportunity)}");
        }
        // Token: 0x170008A3 RID: 2211
        // (get) Token: 0x06002BE0 RID: 11232 RVA: 0x00008DE0 File Offset: 0x00006FE0
        public override bool IsEngageUnit
        {
            get
            {
                return true;
            }
        }

        // Token: 0x06002BE1 RID: 11233 RVA: 0x000B1EF9 File Offset: 0x000B00F9
        public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
        {
            UnitEntityData caster = context.Caster;
            Vector3 startPoint = caster.Position;
            Vector3 endPoint = target.Point;
            float maxDistance = this.StopOnCorpulence ? ((endPoint - startPoint).magnitude - caster.View.Corpulence) : (endPoint - startPoint).magnitude;
            caster.View.StopMoving();
            caster.View.AgentASP.AvoidanceDisabled = true;
            caster.View.AgentASP.MaxSpeedOverride = new float?(caster.CombatSpeedMps * 2f);
            caster.View.AgentASP.ForcePath(new ForcedPath(new List<Vector3>
            {
                startPoint,
                endPoint
            }), 1000000f);
            caster.Descriptor.AddBuff(BlueprintRoot.Instance.SystemMechanics.ChargeBuff, context, new TimeSpan?(1.Rounds().Seconds));
            if (this.AddBuffWhileRunning)
            {
                caster.Descriptor.AddBuff(this.AddBuffWhileRunning, context, null);
            }
            float t = Time.time;
            while (Time.time < t + this.DelayBeforeStart)
            {
                yield return null;
            }
            HashSet<UnitEntityData> overrunUnits = new HashSet<UnitEntityData>();
            float passedDistance = 0f;
            Vector2 dir = (endPoint - startPoint).normalized.To2D();
            Main.logger.Log($"dir: {dir}");
            while (caster.View.MovementAgent.IsReallyMoving)
            {
                if (Game.Instance.TurnBasedCombatController.WaitingForUI)
                {
                    yield return null;
                }
                bool flag = false;
                //if (!this.FirstTargetOnly || overrunUnits.Count <= 0)
                //{
                Main.logger.Log($"count: {overrunUnits.Count}");
                foreach (UnitEntityData unitEntityData in Game.Instance.State.Units)
                {
                    float magnitude = (unitEntityData.Position - caster.Position).magnitude;
                    float num = unitEntityData.View.Corpulence + caster.View.Corpulence + 1.Feet().Meters;
                    if (unitEntityData != caster && !unitEntityData.Descriptor.State.IsDead && magnitude <= num && !overrunUnits.Contains(unitEntityData))
                    {
                        Vector2 to = (unitEntityData.Position - caster.PreviousPosition).To2D();
                        if (Mathf.Abs(Vector2.SignedAngle(dir, to)) <= 45f)
                        {

                            bool flag2 = false;
                            if (overrunUnits.Count <= 0)
                            {
                                if (!this.AutoSuccess)
                                {
                                    caster.View.StopMoving();
                                    yield return null;
                                    logStatus(caster.CombatState, unitEntityData);
                                    //Game.Instance.CombatEngagementController.ForceAttackOfOpportunity(caster, unitEntityData);

                                    //caster.Commands.Run(new UnitAttackOfOpportunity(unitEntityData));
                                    //EventBus.RaiseEvent<IAttackOfOpportunityHandler>(delegate (IAttackOfOpportunityHandler h)
                                    //{
                                    //    h.HandleAttackOfOpportunity(caster, unitEntityData);
                                    //});
                                    //yield return null;
                                    RuleCombatManeuver ruleCombatManeuver = context.TriggerRule<RuleCombatManeuver>(new RuleCombatManeuver(caster, unitEntityData, CombatManeuver.Overrun));
                                    //yield return null;
                                    flag = !ruleCombatManeuver.Success;
                                    flag2 = ruleCombatManeuver.IsPartialSuccess;

                                    yield return null;

                                    caster.View.AgentASP.ForcePath(new ForcedPath(new List<Vector3>
                                    {
                                        caster.Position,
                                        endPoint
                                    }), 1000000f);
                                }
                            }
                            else
                            {
                                flag = true;
                            }
                            Main.logger.Log($"after flag calulation");
                            Main.logger.Log($"flag: {flag} flag2: {flag2}");
                            if (flag2)
                            {
                                UnitAnimationManager animationManager = unitEntityData.View.AnimationManager;
                                if (animationManager != null)
                                {
                                    animationManager.ExecuteIfIdle(UnitAnimationType.Dodge);
                                }
                            }
                            if (flag)
                            {
                                Main.logger.Log($"running cause flag: {flag}");
                                UnitAnimationManager animationManager2 = unitEntityData.View.AnimationManager;
                                Main.logger.Log($"animationManager2 is null: {animationManager2 == null}");
                                if (animationManager2 == null)
                                {
                                    break;
                                }
                                animationManager2.Execute(UnitAnimationType.Hit);
                                break;
                            }
                            else
                            {
                                Main.logger.Log($"running cause flag: {flag}");
                                overrunUnits.Add(unitEntityData);
                                Main.logger.Log($"count of overrun: {overrunUnits.Count}");
                                using (context.GetDataScope(unitEntityData))
                                {
                                    this.Actions.Run();
                                }
                                Main.logger.Log($"after action.run()");
                            }
                        }
                    }
                }
                //}
                passedDistance += (caster.Position - caster.PreviousPosition).magnitude;
                Main.logger.Log($"passed distance: {passedDistance}");
                Main.logger.Log($"max distance: {maxDistance}");
                Main.logger.Log($"flag: {flag}");
                if (passedDistance >= maxDistance || flag)
                {
                    caster.View.StopMoving();
                    break;
                }
                yield return null;
            }
            Main.logger.Log($"out of while");
            if (this.AddBuffWhileRunning)
            {
                context.Caster.Descriptor.RemoveFact(this.AddBuffWhileRunning);
            }
            t = Time.time;
            while (Time.time < t + this.DelayAfterFinish)
            {
                yield return null;
            }
            yield break;
        }

        // Token: 0x06002BE2 RID: 11234 RVA: 0x000B1F18 File Offset: 0x000B0118
        public override void Cleanup(AbilityExecutionContext context)
        {
            context.Caster.View.MovementAgent.AvoidanceDisabled = false;
            context.Caster.View.MovementAgent.MaxSpeedOverride = null;
            context.Caster.Descriptor.State.IsCharging = false;
            if (this.AddBuffWhileRunning)
            {
                context.Caster.Descriptor.RemoveFact(this.AddBuffWhileRunning);
            }
        }

        // Token: 0x06002BE3 RID: 11235 RVA: 0x000B1F94 File Offset: 0x000B0194
        public float GetMinRangeMeters(UnitEntityData caster)
        {
            return 10.Feet().Meters + caster.View.Corpulence;
        }

        // Token: 0x06002BE4 RID: 11236 RVA: 0x000B1915 File Offset: 0x000AFB15
        private static float GetMaxRangeMeters(UnitEntityData caster)
        {
            return caster.CombatSpeedMps * 6f;
        }

        // Token: 0x06002BE5 RID: 11237 RVA: 0x000B1FBC File Offset: 0x000B01BC
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            //Main.logger.Log($"calculating magnitude");
            float magnitude = (target.Point - caster.Position).magnitude;
            //Main.logger.Log($"magnitude: {magnitude}");
            var ret = ((magnitude <= MyOverrun.GetMaxRangeMeters(caster) && magnitude >= this.GetMinRangeMeters(caster)) || this.AutoSuccess) && ObstacleAnalyzer.TraceAlongNavmesh(caster.Position, target.Point) == target.Point;
            //Main.logger.Log($"ret: {ret}");
            return ret;
        }

        // Token: 0x04001CAE RID: 7342
        public BlueprintBuff AddBuffWhileRunning;

        // Token: 0x04001CAF RID: 7343
        [Tooltip("Use delays to accomodate starting and finishing animations like takeoff/landing")]
        public float DelayBeforeStart;

        // Token: 0x04001CB0 RID: 7344
        [Tooltip("Use delays to accomodate starting and finishing animations like takeoff/landing")]
        public float DelayAfterFinish;

        // Token: 0x04001CB1 RID: 7345
        public bool FirstTargetOnly;

        // Token: 0x04001CB2 RID: 7346
        public bool AutoSuccess;

        // Token: 0x04001CB3 RID: 7347
        public bool StopOnCorpulence;

        // Token: 0x04001CB4 RID: 7348
        public ActionList Actions;
    }
}
