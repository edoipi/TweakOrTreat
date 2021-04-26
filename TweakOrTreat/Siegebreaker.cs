using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Designers;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.ContextData;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace TweakOrTreat
{
    class Siegebreaker
    {
        static LibraryScriptableObject library => Main.library;


        class BreakerRushPropertyValueGetter : PropertyValueGetter
        {
            public BlueprintFeature feature;
            public override int GetInt(UnitEntityData unit)
            {
                var ret = 0;
                if(unit.Descriptor.Progression.Features.HasFact(feature))
                {
                    ret += 2;
                    ModifiableValue ac = unit.Descriptor.Stats.GetStat<ModifiableValue>(StatType.AC);
                    if (ac != null)
                    {
                        var armorEnhancement = 0;
                        var shieldEnhancement = 0;
                        //var armorEnhancement = ac.ApplyModifiersFiltered(0, (ModifiableValue.Modifier m) => m.ModDescriptor == ModifierDescriptor.ArmorEnhancement);
                        //var shieldEnhancement = ac.ApplyModifiersFiltered(0, (ModifiableValue.Modifier m) => m.ModDescriptor == ModifierDescriptor.ShieldEnhancement);
                        //Main.logger.Log($"armor: {armorEnhancement}");
                        //Main.logger.Log($"shield: {shieldEnhancement}");
                        //ret += Math.Max(armorEnhancement, shieldEnhancement);
                        var modifiers = ac.GetDisplayModifiers();
                        foreach(var mod in modifiers)
                        {
                            if(mod.ItemSource != null)
                            {
                                if(mod.ModDescriptor == ModifierDescriptor.Armor)
                                {
                                    armorEnhancement = Math.Max(armorEnhancement, GameHelper.GetItemEnhancementBonus(mod.ItemSource));
                                }
                                if (mod.ModDescriptor == ModifierDescriptor.Shield)
                                {
                                    shieldEnhancement = Math.Max(shieldEnhancement, GameHelper.GetItemEnhancementBonus(mod.ItemSource));
                                }
                            }
                        }

                        //Main.logger.Log($"armor: {armorEnhancement}");
                        //Main.logger.Log($"shield: {shieldEnhancement}");
                        ret += Math.Max(armorEnhancement, shieldEnhancement);
                    }
                }

                ModifiableValueAttributeStat strength = unit.Descriptor.Stats.GetStat<ModifiableValueAttributeStat>(StatType.Strength);
                if (strength != null)
                {
                    ret += strength.Bonus;
                }
                return ret;
            }
        }

        public class RunActionOnAttack : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
        {
            public ActionList Action;

            public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {

            }

            public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                MechanicsContext maybeContext = base.Fact.MaybeContext;
                if (evt.AttackRoll.IsHit)
                {
                    using (maybeContext.GetDataScope(evt.Target))
                    {
                        this.Action.Run();
                    }
                    //using (new ContextAttackData(evt.AttackRoll, (Projectile)null))
                    //    (this.Fact as IFactContextOwner)?.RunActionInContext(this.action, (TargetWrapper)evt.Target);
                }
            }
        }

        static internal void load()
        {
            var fighter = Helpers.GetClass("48ac8db94d5de7645906c7d0ad3bcfbd");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SiegebreakerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Siegebreaker");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The siegebreaker is trained to break through lines of enemy soldiers.");
            });
            Helpers.SetField(archetype, "m_ParentClass", fighter);
            library.AddAsset(archetype, "");

            var fighterFeatSelection = library.Get<BlueprintFeature>("41c8486641f7d6d4283ca9dae4147a9f");
            var bravery = library.Get<BlueprintFeature>("f6388946f9f472f4585591b80e9f2452");
            var weaponMastery = library.Get<BlueprintFeature>("55f516d7d1fc5294aba352a5a1c92786");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, fighterFeatSelection),
                Helpers.LevelEntry(2, fighterFeatSelection, bravery),
                Helpers.LevelEntry(4, fighterFeatSelection),
                Helpers.LevelEntry(6, bravery),
                Helpers.LevelEntry(8, fighterFeatSelection),
                Helpers.LevelEntry(10, bravery),
                Helpers.LevelEntry(14, bravery),
                Helpers.LevelEntry(18, bravery),
                Helpers.LevelEntry(20, weaponMastery)
            };

            var bullRush = library.Get<BlueprintFeature>("b3614622866fe7046b787a548bbd7f59");
            var bullGreaterRush = library.Get<BlueprintFeature>("72ba6ad46d94ecd41bad8e64739ea392");

            //bullRush.MaybeReplaceComponent

            var bullRushAddFacts = bullRush.GetComponents<AddFacts>().Select(
                addFact => Helpers.Create<FactSinglify>(
                    f =>
                    {
                        f.NewFacts = addFact.Facts;
                    }
                )
            ).ToArray();
            
            bullRush.AddComponents(bullRushAddFacts);
            bullRush.RemoveComponents<AddFacts>();

            var breakerRush = Helpers.CreateFeature(
                "BreakerRushFeature",
                "Breaker Rush",
                "At 1st level, a siegebreaker can attempt bull rush or overrun combat maneuvers without provoking attacks of opportunity. When he performs either combat maneuver, he deals an amount of bludgeoning damage equal to his Strength bonus (minimum 1). If he has Improved Bull Rush or Improved Overrun, the damage dealt by the appropriate maneuver increases by 2 and he adds any enhancement bonus from his armor or shield (though such enhancement bonuses do not stack, if both armor and shield are magic).",
                "",
                Overrun.overrunAbility.Icon,
                FeatureGroup.Feat,
                //Overrun.overrunAbility.CreateAddFact()
                Helpers.Create<FactSinglify>(
                    f =>
                    {
                        f.NewFacts = new BlueprintUnitFact[] { Overrun.overrunAbility };
                    }
                )
            );
            breakerRush.AddComponents(bullRushAddFacts);

            var breakerRushOverrunProperty = Helpers.Create<BlueprintUnitProperty>();
            breakerRushOverrunProperty.name = "BreakerRushOverrunProperty";
            Main.library.AddAsset(breakerRushOverrunProperty, "");
            breakerRushOverrunProperty.AddComponent(Helpers.Create<BreakerRushPropertyValueGetter>(
                b =>
                {
                    b.feature = Overrun.overrunFeature;
                }
            ));
            var breakerRushOverrunContextValue = new ContextValue() { ValueType = ContextValueType.CasterCustomProperty, CustomProperty = breakerRushOverrunProperty };

            var breakerRushBullRushProperty = Helpers.Create<BlueprintUnitProperty>();
            breakerRushBullRushProperty.name = "BreakerRushBullRushProperty";
            Main.library.AddAsset(breakerRushBullRushProperty, "");
            breakerRushBullRushProperty.AddComponent(Helpers.Create<BreakerRushPropertyValueGetter>(
                b =>
                {
                    b.feature = bullRush;
                }
            ));
            var breakerRushBullRushContextValue = new ContextValue() { ValueType = ContextValueType.CasterCustomProperty, CustomProperty = breakerRushBullRushProperty };

            breakerRush.AddComponents(
                Helpers.Create<ManeuverTrigger>(
                    mt =>
                    {
                        mt.OnlySuccess = false;
                        mt.ManeuverType = CombatManeuver.Overrun;
                        mt.Action = Helpers.CreateActionList(
                            Helpers.CreateActionDealDamage(
                                PhysicalDamageForm.Bludgeoning,
                                Helpers.CreateContextDiceValue(DiceType.Zero, bonus: breakerRushOverrunContextValue),
                                IgnoreCritical: false)
                        );
                    }
                ),
                Helpers.Create<ManeuverTrigger>(
                    mt =>
                    {
                        mt.OnlySuccess = false;
                        mt.ManeuverType = CombatManeuver.BullRush;
                        mt.Action = Helpers.CreateActionList(
                            Helpers.CreateActionDealDamage(
                                PhysicalDamageForm.Bludgeoning,
                                Helpers.CreateContextDiceValue(DiceType.Zero, bonus: breakerRushBullRushContextValue),
                                IgnoreCritical: false)
                        );
                    }
                )
            );
            breakerRush.ReapplyOnLevelUp = true;

            var breakerMomentum = Helpers.CreateFeature(
                "BreakerMomentumFeature",
                "Breaker Momentum",
                "At 2nd level, when a siegebreaker successfully bull rushes a foe, he can attempt an overrun combat maneuver check against that foe as a free action.",
                "",
                null,
                FeatureGroup.Feat,
                Helpers.Create<ManeuverTrigger>(
                    mt =>
                    {
                        mt.OnlySuccess = true;
                        mt.ManeuverType = CombatManeuver.BullRush;
                        mt.Action = Helpers.CreateActionList(
                            Helpers.Create<ContextActionCombatManeuver>(c =>
                                {
                                    c.Type = CombatManeuver.Overrun;
                                    c.OnSuccess = Helpers.CreateActionList();
                                }
                            )
                        );
                    }
                )
            );

            var blowIcon = Overrun.makeIcon(@"blow.png");
            var disorientingBlow = Helpers.CreateFeature(
                "DisorientingBlowFeature",
                "Disorienting Blow",
                "At 8th level, as an immediate action, a siegebreaker can distract a foe he just hit with an attack or combat maneuver, imposing a –2 penalty on the foe’s attack rolls, caster level checks, or skill checks for 1 round. The foe can negate this penalty with a successful Fortitude save (DC = 10 + 1/2 the siegebreaker’s fighter level + his Strength modifier). At 14th level, the penalty increases to –4.",
                "",
                blowIcon,
                FeatureGroup.Feat
                
            );

            var nameToComponent = new List<(string, BlueprintComponent)>() {
                ( "Attack Rolls", Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable) ),
                ( "Skill Checks", Helpers.Create<BuffAllSkillsBonusAbilityValue>(b => b.Value = Helpers.CreateContextValue(AbilityRankType.Default)) ),
                ( "Caster Level Checks", Helpers.Create<CallOfTheWild.NewMechanics.CasterLevelCheckBonus>(b => b.Value = Helpers.CreateContextValue(AbilityRankType.Default)) ),
                ("Nauseate", Common.createAddCondition(UnitCondition.Nauseated))
            };

            var abilities = new List<AddFacts>();

            foreach (var entry in nameToComponent)
            {
                var name = Regex.Replace(entry.Item1, @"\s+", "");
                var disorientingBlowDebuff = Helpers.CreateBuff(
                    $"DisorientingBlow{name}Debuff",
                    disorientingBlow.Name + $" ({entry.Item1})",
                    disorientingBlow.Description,
                    "",
                    disorientingBlow.Icon,
                    null,
                    entry.Item2,
                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.Custom, classes: new BlueprintCharacterClass[] { fighter },
                                                    customProgression: new (int, int)[] { (13, -2), (21, -4) })
                );

                var applyDebuff = Common.createContextActionApplyBuff(disorientingBlowDebuff, Helpers.CreateContextDuration(1), is_permanent: false, dispellable: false, duration_seconds: 9);
                var effect = Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(null, applyDebuff));

                var conditionalAction = Helpers.CreateConditional(
                    new Condition[] {
                        Helpers.Create<CallOfTheWild.TurnActionMechanics.ContextConditionHasAction>(c => {c.check_caster = true; c.has_swift = true; })
                    },
                    new GameAction[] {
                        Helpers.Create<CallOfTheWild.TurnActionMechanics.ConsumeAction>(c => { c.from_caster = true; c.consume_swift = true; }),
                    effect
                    }
                );

                var disorientingBlowBuff = Helpers.CreateBuff(
                    $"DisorientingBlow{name}Buff",
                    disorientingBlow.Name + $" ({entry.Item1})",
                    disorientingBlow.Description,
                    "",
                    disorientingBlow.Icon,
                    null,
                    Helpers.Create<ManeuverTrigger>(
                        mt =>
                        {
                            mt.OnlySuccess = true;
                            mt.ManeuverType = CombatManeuver.None;
                            mt.Action = Helpers.CreateActionList(
                                conditionalAction
                            );
                        }
                    ),
                    Helpers.Create<RunActionOnAttack>(
                        a =>
                        {
                            a.Action = Helpers.CreateActionList(
                                conditionalAction
                            );
                        }
                    ),
                    Common.createContextCalculateAbilityParamsBasedOnClass(archetype.GetParentClass(), StatType.Strength)
                );

                var disorientingBlowAbilty = Helpers.CreateActivatableAbility(
                    $"disorientingBlow{name}ToggleAbility",
                    disorientingBlow.Name + $" ({entry.Item1})",
                    disorientingBlow.Description,
                    "",
                    disorientingBlow.Icon,
                    disorientingBlowBuff,
                    AbilityActivationType.Immediately,
                    Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                    null
                );
                disorientingBlowAbilty.DeactivateImmediately = true;
                disorientingBlowAbilty.Group = ActivatableAbilityGroupExtension.ExtraGroup2.ToActivatableAbilityGroup();

                //disorientingBlow.AddComponents(
                //    disorientingBlowAbilty.CreateAddFact()
                //);
                abilities.Add(disorientingBlowAbilty.CreateAddFact());
            }

            var masterfulDistraction = Helpers.CreateFeature(
                "MasterfulDistractionFeature",
                "Masterful Distraction",
                "At 20th level, a siegebreaker can nauseate a foe he targets with his disorienting blow instead of inflicting a penalty.",
                "",
                blowIcon,
                FeatureGroup.Feat,
                abilities.Last()
            );

            abilities.RemoveAt(abilities.Count - 1);
            disorientingBlow.AddComponents(abilities);

            var armoredVigorIcon = Overrun.makeIcon(@"armored_vigor.png");

            var resource = Helpers.CreateAbilityResource("ArmoredVigorResource", "", "", "", null);
            resource.SetIncreasedByStat(3, StatType.Strength);
            //resource.m_MaxAmount.

            var armoredVigor = Helpers.CreateFeature(
                "ArmoredVigorFeature",
                "Armored Vigor",
                "At 2nd level as a swift action, a siegebreaker can gain 2 temporary hit points that last for 1 minute. He can use this ability a number of times per day equal to 3 + his Constitution modifier. At 6th level and every 4 levels thereafter, the number of temporary hit points the siegebreaker gains increases by 2, to a maximum of 10 at 18th level.",
                "",
                armoredVigorIcon,
                FeatureGroup.Feat,
                resource.CreateAddAbilityResource()
            );
            armoredVigor.Ranks = 5;

            var amoredVigorBuff = Helpers.CreateBuff(
                $"ArmoredVigorBuff",
                armoredVigor.Name,
                armoredVigor.Description,
                "",
                armoredVigor.Icon,
                null,
                Helpers.Create<TemporaryHitPointsFromAbilityValue>(t => { t.Value = Helpers.CreateContextValue(AbilityRankType.Default); t.RemoveWhenHitPointsEnd = true; }),
                Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.FeatureRank, feature: armoredVigor,
                                                progression: ContextRankProgression.MultiplyByModifier, min: 0, stepLevel: 2)
            );

            var ability = Helpers.CreateAbility(
                "ArmoredVigorAbility",
                armoredVigor.Name,
                armoredVigor.Description,
                "",
                armoredVigor.Icon,
                AbilityType.Supernatural,
                CommandType.Swift,
                AbilityRange.Personal,
                Helpers.oneRoundDuration,
                "",
                Helpers.CreateRunActions(Common.createContextActionApplyBuff(amoredVigorBuff, Helpers.CreateContextDuration(1, DurationRate.Minutes))),
                Helpers.CreateResourceLogic(resource)
            );

            armoredVigor.AddComponent(ability.CreateAddFact());

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, breakerRush),
                Helpers.LevelEntry(2, breakerMomentum, armoredVigor),
                Helpers.LevelEntry(4, CallOfTheWild.NewFeats.disruptive),
                Helpers.LevelEntry(6, armoredVigor),
                Helpers.LevelEntry(8, disorientingBlow),
                Helpers.LevelEntry(10, armoredVigor),
                Helpers.LevelEntry(14, armoredVigor),
                Helpers.LevelEntry(18, armoredVigor),
                Helpers.LevelEntry(20, masterfulDistraction)
            };
            fighter.Archetypes = fighter.Archetypes.AddToArray(archetype);
            fighter.Progression.UIGroups = fighter.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(breakerRush, breakerMomentum, CallOfTheWild.NewFeats.disruptive, disorientingBlow, masterfulDistraction)
            );
        }
    }
}
