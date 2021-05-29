//using Kingmaker;
//using Kingmaker.Blueprints;
//using Kingmaker.Blueprints.Facts;
//using Kingmaker.Controllers;
//using Kingmaker.Designers.Mechanics.Buffs;
//using Kingmaker.UnitLogic;
//using Kingmaker.UnitLogic.Buffs;
//using Kingmaker.UnitLogic.Buffs.Blueprints;
//using Kingmaker.UnitLogic.Mechanics;
//using Pathfinding.Util;
//using SharpCut;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading.Tasks;
//using TurnBased.Controllers;

//namespace TweakOrTreat
//{
//    [HarmonyLib.HarmonyPatch(typeof(BuffCollection), nameof(BuffCollection.AddBuffInternal))]
//    static class BuffCollection_AddBuffInternal_Patch
//    {
//        private static Buff AddBuffInternal(BuffCollection __instance, BlueprintBuff blueprint, MechanicsContext context, TimeSpan? duration)
//        {
//            if (__instance.m_Disabled)
//                return (Buff)null;
//            if (!blueprint.StayOnDeath && __instance.Owner.State != null && __instance.Owner.State.IsDead)
//                return (Buff)null;
//            TimeSpan timeSpan1 = Game.Instance.TimeController.GameTime;
//            if (CombatController.IsInTurnBasedCombat())
//            {
//                timeSpan1 = Game.Instance.TurnBasedCombatController.TurnStartTime;
//                //timeSpan1 = Game.Instance.TurnBasedCombatController.RoundStartTime;
//                //Main.logger.Log($"Turn start apply: {Game.Instance.TurnBasedCombatController.TurnStartTime}");
//                //Main.logger.Log($"Round start apply: {Game.Instance.TurnBasedCombatController.RoundStartTime}");
//            }
//            TimeSpan? nullable1 = duration.HasValue ? new TimeSpan?(timeSpan1 + duration.Value) : new TimeSpan?();
//            if (blueprint.Stacking != StackingType.Stack)
//            {
//                Buff buff = __instance.GetBuff(blueprint);
//                if (buff != null)
//                {
//                    switch (blueprint.Stacking)
//                    {
//                        case StackingType.Replace:
//                            __instance.RemoveFact((Fact)buff);
//                            break;
//                        case StackingType.Prolong:
//                            if (!nullable1.HasValue)
//                            {
//                                buff.MakePermanent();
//                            }
//                            else
//                            {
//                                TimeSpan endTime = buff.EndTime;
//                                TimeSpan? nullable2 = nullable1;
//                                if ((nullable2.HasValue ? (endTime < nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
//                                    buff.EndTime = nullable1.Value;
//                            }
//                            return buff;
//                        case StackingType.Ignore:
//                            return buff;
//                        case StackingType.Poison:
//                            if (duration.HasValue)
//                            {
//                                TimeSpan? nullable2 = duration;
//                                TimeSpan maxValue = TimeSpan.MaxValue;
//                                if ((nullable2.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() == maxValue ? 1 : 0) : 1) : 0) == 0 && !(buff.EndTime == TimeSpan.MaxValue))
//                                {
//                                    TimeSpan timeSpan2 = new TimeSpan(duration.Value.Ticks / 2L);
//                                    buff.EndTime += timeSpan2;
//                                    using (IEnumerator<BuffPoisonStatDamage> enumerator = buff.SelectComponents<BuffPoisonStatDamage>().GetEnumerator())
//                                    {
//                                        while (enumerator.MoveNext())
//                                            enumerator.Current.GetStacked();
//                                        goto label_32;
//                                    }
//                                }
//                            }
//                            buff.EndTime = TimeSpan.MaxValue;
//                            foreach (BuffPoisonStatDamage selectComponent in buff.SelectComponents<BuffPoisonStatDamage>())
//                                selectComponent.GetStacked();
//                            label_32:
//                            return buff;
//                        case StackingType.Summ:
//                            if (nullable1.HasValue)
//                                buff.EndTime = new TimeSpan?(buff.EndTime + duration.Value).Value;
//                            else
//                                buff.MakePermanent();
//                            return buff;
//                        default:
//                            throw new ArgumentOutOfRangeException();
//                    }
//                }
//            }
//            if ((bool)(UnityEngine.Object)__instance.m_AddBuffNow)
//                UberDebug.LogError((object)string.Format("Add buff ({0}) while handle another ({1})", (object)blueprint, (object)__instance.m_AddBuffNow), (object[])Array.Empty<object>());
//            __instance.m_EndTimeOverride = nullable1;
//            __instance.m_AddBuffNow = blueprint;

//            //MethodInfo fooA = typeof(OwnedFactCollection<UnitDescriptor>).GetMethod("AddFact", BindingFlags.Public | BindingFlags.Instance);

//            //DynamicMethod baseBaseFoo = new DynamicMethod(
//            //            "AddFact_Base",
//            //            typeof(Fact),
//            //            new[] { typeof(OwnedFactCollection<UnitDescriptor>), typeof(BlueprintFact), typeof(MechanicsContext) },
//            //            typeof(OwnedFactCollection<UnitDescriptor>));
//            //ILGenerator il = baseBaseFoo.GetILGenerator();
//            //il.Emit(OpCodes.Ldarg, 0);
//            //il.EmitCall(OpCodes.Call, fooA, null);
//            //il.Emit(OpCodes.Ret);

//            var baseAddFact = typeof(OwnedFactCollection<UnitDescriptor>).GetNonVirtualInvoker<AddFactDel>("AddFact");
//            return (Buff)baseAddFact(__instance, (BlueprintFact)blueprint, context);
//            //var ret = (Buff)baseBaseFoo.Invoke(null, new object[] { __instance, (BlueprintFact)blueprint, context });
//            //return ret;
//            //return (Buff)__instance.base.AddFact((BlueprintFact)blueprint, context);
//        }
//        private delegate Fact AddFactDel(OwnedFactCollection<UnitDescriptor> instance, BlueprintFact f, MechanicsContext m);

//        internal static bool Prefix(ref Buff __result, BuffCollection __instance, BlueprintBuff blueprint, MechanicsContext context, TimeSpan? duration)
//        {
//            if (__instance != null)
//            {
//                __result = AddBuffInternal(__instance, blueprint, context, duration);
//            }
//            return false;
//        }
//    }

//    [HarmonyLib.HarmonyPatch(typeof(BuffCollection), nameof(BuffCollection.Tick))]
//    static class BuffCollection_Tick_Patch
//    {
//        public static void Tick(BuffCollection __instance)
//        {
//            if (__instance.m_IsOwnerAlive && __instance.Owner.State.IsDead)
//            {
//                List<Fact> list = ListPool<Fact>.Claim();
//                foreach (Fact fact in __instance.RawFacts)
//                {
//                    Buff buff = (Buff)fact;
//                    if (!buff.Blueprint.StayOnDeath)
//                    {
//                        list.Add(buff);
//                    }
//                }
//                foreach (Fact fact2 in list)
//                {
//                    __instance.RemoveFact(fact2);
//                }
//                ListPool<Fact>.Release(list);
//            }
//            __instance.m_IsOwnerAlive = !__instance.Owner.State.IsDead;
//            if (__instance.m_NextEvent == null)
//            {
//                return;
//            }
//            TimeSpan timeSpan = Game.Instance.TimeController.GameTime;
//            if (CombatController.IsInTurnBasedCombat())
//            {
//                timeSpan = Game.Instance.TurnBasedCombatController.TurnStartTime;
//                //timeSpan = Game.Instance.TurnBasedCombatController.RoundStartTime;
//                //Main.logger.Log($"Turn start tick: {Game.Instance.TurnBasedCombatController.TurnStartTime}");
//                //Main.logger.Log($"Round start tick: {Game.Instance.TurnBasedCombatController.RoundStartTime}");
//            }
//            while (__instance.m_NextEvent.NextEventTime <= timeSpan)
//            {
//                //if (CombatController.IsInTurnBasedCombat() && CombatController.CurrentUnit != __instance.Owner.Unit && CombatController.CurrentUnit != __instance.m_NextEvent.Context.MaybeCaster)
//                //{
//                //    return;
//                //}
//                if (__instance.m_NextEvent.NextTickTime <= timeSpan)
//                {
//                    __instance.m_NextEvent.TickMechanics();
//                }
//                if (__instance.m_NextEvent != null)
//                {
//                    if (__instance.m_NextEvent.EndTime <= timeSpan)
//                    {
//                        __instance.RemoveFact(__instance.m_NextEvent);
//                    }
//                    else
//                    {
//                        double totalSeconds = __instance.m_NextEvent.TickTime.TotalSeconds;
//                        double totalSeconds2 = (timeSpan - __instance.m_NextEvent.NextEventTime).TotalSeconds;
//                        if (totalSeconds2 >= 0.0 && totalSeconds2 / totalSeconds > 30.0)
//                        {
//                            __instance.m_NextEvent.NextTickTime = timeSpan - (totalSeconds * 30.0).Seconds();
//                            UberDebug.LogError(string.Format("{0} wants tick {1} times, reduced to {2} times", __instance.m_NextEvent, (int)(totalSeconds2 / totalSeconds), 30), Array.Empty<object>());
//                        }
//                    }
//                }
//                __instance.UpdateNextEvent();
//                if (__instance.m_NextEvent == null)
//                {
//                    return;
//                }
//            }
//        }

//        internal static bool Prefix(BuffCollection __instance)
//        {
//            Tick(__instance);
//            return false;
//        }
//    }

//    class BuffTickFix
//    {
//    }
//}
