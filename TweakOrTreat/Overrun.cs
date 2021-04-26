using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace TweakOrTreat
{
    class Overrun
    {
        static LibraryScriptableObject library => Main.library;

        public static BlueprintFeature overrunFeature;
        public static BlueprintFeature greaterOverrunFeature;
        public static BlueprintAbility overrunAbility;

        public class OverrunUnitPart : AdditiveUnitPart
        {
            public bool canOverrunMultipleTargets()
            {
                return buffs.Count > 0;
            }
        }

        public class OverrunMultipleTargets : OwnedGameLogicComponent<UnitDescriptor>
        {
            public override void OnFactActivate()
            {
                this.Owner.Ensure<OverrunUnitPart>().addBuff(this.Fact);
            }


            public override void OnFactDeactivate()
            {
                this.Owner.Ensure<OverrunUnitPart>().removeBuff(this.Fact);
            }
        }
        public static Sprite makeIcon(string fileName)
        {
            var bytes = File.ReadAllBytes(UnityModManager.modsPath + @"/TweakOrTreat/Icons/" + fileName);
            var texture = new Texture2D(64, 64, TextureFormat.DXT5, false);
            texture.LoadImage(bytes);
            //texture.mipMapBias = -1.5f;
            //texture.mipmapCount = 1;
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0, 0));
        }

        class BuletteRampagePropertyValueGetter : PropertyValueGetter
        {
            public override int GetInt(UnitEntityData unit)
            {
                //unit.Descriptor.Progression.Features.HasFact()
                //Main.logger.Log("Begin");
                var ret = 0;
                ModifiableValue ac = unit.Descriptor.Stats.GetStat<ModifiableValue>(StatType.AC);
                //Main.logger.Log("Get");
                if (ac != null)
                {
                   // Main.logger.Log("Not Null");
                    var armorBonus = ac.ApplyModifiersFiltered(0, (ModifiableValue.Modifier m) => m.ModDescriptor == ModifierDescriptor.Armor);
                    //Main.logger.Log($"Bonus: {armorBonus}");
                    ret += armorBonus / 2;
                }

                ModifiableValueAttributeStat strength = unit.Descriptor.Stats.GetStat<ModifiableValueAttributeStat>(StatType.Strength);
                if (strength != null)
                {
                    ret += (int)(strength.Bonus * 1.5);
                }
                return ret;
            }
        }

        public static void addRagePowers(params BlueprintFeature[] p)
        {
            var selections = new BlueprintFeatureSelection[] {
                library.Get<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e"),
                library.Get<BlueprintFeatureSelection>("0c7f01fbbe687bb4baff8195cb02fe6a"),
                CallOfTheWild.Bloodrager.primalist_rage_power_selection,
                CallOfTheWild.Skald.skald_rage_powers,
                library.Get<BlueprintFeatureSelection>("ebc9d76630684678a7b7bb98dc61903e"),
                library.Get<BlueprintFeatureSelection>("131b18f000824d858fd7c6a68b24b1c9")
            };

            foreach(var s in selections)
            {
                s.AllFeatures = s.AllFeatures.AddToArray(p);
            }
        }


        static internal void load()
        {
            var powerAttack = library.Get<BlueprintFeature>("9972f33f977fc724c838e59641b2fca5");
            //var overrunAbilityOld = library.CopyAndAdd<BlueprintAbility>("1a3b471ecea51f7439a946b23577fd70", "OverrunNotTrampleAbilityOld", "");
            overrunAbility = library.CopyAndAdd<BlueprintAbility>("1a3b471ecea51f7439a946b23577fd70", "OverrunNotTrampleAbility", "");
            var overrunIcon = makeIcon(@"overrun.png");
            var buletteStyleIcon = makeIcon(@"bulette_style.png");
            var buletteLeapIcon = makeIcon(@"bulette_leap.png");
            var buletteRampageIcon = makeIcon(@"bulette_rampage.png");
            //CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = UnityModManager.modsPath + @"/TweakOrTreat/Icons/";
            var improvedOverrunIcon = makeIcon(@"improved_overrun.png");
            var greaterOverrunIcon = makeIcon(@"greater_overrun.png");
            //CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = UnityModManager.modsPath + @"/CallOfTheWild/Icons/";

            overrunAbility.RemoveComponents<AbilityCustomOverrun>();
            overrunAbility.AddComponent(
                Helpers.Create<OverrunAbilityLogic>(
                    o =>
                    {
                        o.Actions = Helpers.CreateActionList();

                    }
                )
            );
            overrunAbility.SetNameDescription(
                "Overrun",
                "You can attempt to run over your target, trampling them underfoot.\nIf your maneuver is successful, you will move through the target's space. If your attack exceeds your opponent's CMD by 5 or more, you move through the target's space and the target is knocked prone.\nIf your attempt fails, you halt in the space directly in front of the opponent, or the nearest unoccupied space in front of the target.\nYour allies will automatically allow you to pass through their space, but every ally you pass that way counts against the number of overrun attempts you can make in a round."
            );
            overrunAbility.LocalizedSavingThrow = Helpers.savingThrowNone;
            //Main.logger.Log(JsonConvert.SerializeObject(overrunAbility.Icon, Formatting.Indented));
            overrunAbility.SetIcon(overrunIcon);

            overrunFeature = Helpers.CreateFeature(
                "ImprovedOverrunFeature",
                "Improved Overrun",
                "You do not provoke an attack of opportunity when performing an overrun combat maneuver. In addition, you receive a +2 bonus on checks made to overrun a foe. You also receive a +2 bonus to your Combat Maneuver Defense whenever an opponent tries to overrun you. Targets of your overrun attempt may not choose to avoid you.",
                "",
                improvedOverrunIcon,
                FeatureGroup.Feat,
                // overrunAbilityOld.CreateAddFact(),
                Helpers.Create<FactSinglify>(
                    f =>
                    {
                        f.NewFacts = new BlueprintUnitFact[] { overrunAbility };
                    }
                ),
                Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Overrun, 2),
                Common.createManeuverDefenseBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Overrun, 2),
                Helpers.PrerequisiteStatValue(Kingmaker.EntitySystem.Stats.StatType.Strength, 13),
                Helpers.PrerequisiteStatValue(Kingmaker.EntitySystem.Stats.StatType.BaseAttackBonus, 1),
                powerAttack.PrerequisiteFeature()
            );

            greaterOverrunFeature = Helpers.CreateFeature(
                "GreaterOverrunFeature",
                "Greater Overrun",
                "You receive a +2 bonus on checks made to overrun a foe. This bonus stacks with the bonus granted by Improved Overrun. Whenever you overrun opponents, they provoke attacks of opportunity if they are knocked prone by your overrun.",
                "",
                greaterOverrunIcon,
                FeatureGroup.Feat,
                Helpers.Create<ManeuverProvokeAttack>(m => m.ManeuverType = Kingmaker.RuleSystem.Rules.CombatManeuver.Overrun),
                Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Overrun, 2),
                Helpers.PrerequisiteStatValue(Kingmaker.EntitySystem.Stats.StatType.Strength, 13),
                Helpers.PrerequisiteStatValue(Kingmaker.EntitySystem.Stats.StatType.BaseAttackBonus, 6),
                powerAttack.PrerequisiteFeature(),
                overrunFeature.PrerequisiteFeature()
            );
            
            

            var buletteChargeStyle = Helpers.CreateFeature(
                "BuletteChargeStyleFeature",
                "Bulette Charge Style",
                "While using this style, you gain a bonus on combat maneuver checks to overrun an opponent based on the armor you are wearing: +4 for heavy armor, +3 for medium armor or +2 for light armor.",
                "",
                buletteStyleIcon,
                FeatureGroup.Feat,
                powerAttack.PrerequisiteFeature(),
                overrunFeature.PrerequisiteFeature(),
                Helpers.PrerequisiteStatValue(StatType.Strength, 13),
                Helpers.Create<PrerequisiteProficiency>(p =>
                {
                    p.WeaponProficiencies = new WeaponCategory[0];
                    p.ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Heavy };
                })
            );

            var buletteLeap = Helpers.CreateFeature(
                "BuletteLeapFeature",
                "Bulette Leap",
                "While using Bulette Charge Style, you gain a bonus on Mobility checks equal to your Strength bonus. Additionally, when you perform an overrun combat maneuver, you can attempt to overrun multiple foes, but you take a cumulative –2 penalty on each successive overrun attempt in the same round.",
                "",
                buletteLeapIcon,
                FeatureGroup.Feat,
                powerAttack.PrerequisiteFeature(),
                overrunFeature.PrerequisiteFeature(),
                Helpers.PrerequisiteStatValue(StatType.Strength, 15),
                buletteChargeStyle.PrerequisiteFeature(),
                Helpers.Create<PrerequisiteProficiency>(p =>
                {
                    p.WeaponProficiencies = new WeaponCategory[0];
                    p.ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Heavy };
                })
            );

            var buletteRampage = Helpers.CreateFeature(
                "BuletteRampageFeature",
                "Bulette Rampage",
                "While you’re using Bulette Charge Style, whenever you succeed at an overrun combat maneuver check against a foe, that foe takes an amount of damage equal to 1d8 + 1/2 your armor bonus to AC + 1-1/2 times your Strength bonus.",
                "",
                buletteRampageIcon,
                FeatureGroup.Feat,
                powerAttack.PrerequisiteFeature(),
                overrunFeature.PrerequisiteFeature(),
                Helpers.PrerequisiteStatValue(StatType.Strength, 15),
                buletteChargeStyle.PrerequisiteFeature(),
                buletteLeap.PrerequisiteFeature(),
                Helpers.Create<PrerequisiteProficiency>(p =>
                {
                    p.WeaponProficiencies = new WeaponCategory[0];
                    p.ArmorProficiencies = new ArmorProficiencyGroup[] { ArmorProficiencyGroup.Heavy };
                })
            );

            var buletteStyleBuff = Helpers.CreateBuff(
                "BuletteChargeStyleBuff",
                buletteChargeStyle.Name,
                buletteChargeStyle.Description,
                "",
                buletteChargeStyle.Icon,
                null
            );

            var armorToBonus = new Dictionary<ArmorProficiencyGroup, int>() {
                {ArmorProficiencyGroup.Heavy, 4},
                {ArmorProficiencyGroup.Medium, 3},
                {ArmorProficiencyGroup.Light, 2}
            };

            foreach(var entry in armorToBonus)
            {
                var feature = library.CopyAndAdd(buletteChargeStyle, $"BuletteChargeArmor{entry.Value}Feature", "");
                feature.AddComponent(Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Overrun, entry.Value));
                buletteStyleBuff.AddComponent(Helpers.Create<CallOfTheWild.WeaponTrainingMechanics.AddFeatureOnArmor>(a => { a.feature = feature; a.required_armor = new ArmorProficiencyGroup[] { entry.Key }; }));
            }
            

            var buletteLeapBuff = Helpers.CreateBuff(
                "BuletteLeapBuff",
                buletteChargeStyle.Name,
                buletteChargeStyle.Description,
                "",
                buletteChargeStyle.Icon,
                null,
                Helpers.Create<OverrunMultipleTargets>(),
                Helpers.CreateAddContextStatBonus(StatType.SkillMobility, ModifierDescriptor.UntypedStackable),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Strength),
                Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Strength)
            );
            buletteLeapBuff.SetBuffFlags(BuffFlags.HiddenInUi);

            buletteStyleBuff.AddComponent(
                Helpers.CreateAddFactContextActions(
                    Helpers.CreateConditional(
                        Common.createContextConditionHasFact(buletteLeap),
                        Common.createContextActionApplyBuff(buletteLeapBuff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true)
                    )
                 )
            );

            var rampageProperty = Helpers.Create<BlueprintUnitProperty>();
            rampageProperty.name = "BuletteRampageProperty";
            Main.library.AddAsset(rampageProperty, "");
            rampageProperty.AddComponent(Helpers.Create<BuletteRampagePropertyValueGetter>());
            //var rampageRankConfig = Helpers.CreateContextRankConfig(ContextRankBaseValueType.CustomProperty, 
            //    customProperty: rampageProperty);
            var rampageContextValue = new ContextValue() { ValueType = ContextValueType.CasterCustomProperty, CustomProperty = rampageProperty };
            var buletteRampageBuff = Helpers.CreateBuff(
                "BuletteRampageBuff",
                buletteChargeStyle.Name,
                buletteChargeStyle.Description,
                "",
                buletteChargeStyle.Icon,
                null,
                Helpers.Create<ManeuverTrigger>(
                    mt =>
                    {
                        mt.OnlySuccess = true;
                        mt.ManeuverType = CombatManeuver.Overrun;
                        mt.Action = Helpers.CreateActionList(
                            Helpers.CreateActionDealDamage(
                                PhysicalDamageForm.Bludgeoning, 
                                Helpers.CreateContextDiceValue(DiceType.D8, 1, bonus: rampageContextValue), 
                                IgnoreCritical: false)
                        );
                    }
                )
                //rampageRankConfig
                
            );
            buletteRampageBuff.SetBuffFlags(BuffFlags.HiddenInUi);

            buletteStyleBuff.AddComponent(
                Helpers.CreateAddFactContextActions(
                    Helpers.CreateConditional(
                        Common.createContextConditionHasFact(buletteRampage),
                        Common.createContextActionApplyBuff(buletteRampageBuff, Helpers.CreateContextDuration(), is_child: true, dispellable: false, is_permanent: true)
                    )
                 )
            );

            var buletteStyleAbilty = Helpers.CreateActivatableAbility(
                "BuletteChargeStyleToggleAbility",
                buletteChargeStyle.Name,
                buletteChargeStyle.Description,
                "",
                buletteChargeStyle.Icon,
                buletteStyleBuff,
                AbilityActivationType.Immediately,
                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free,
                null
            );
            buletteStyleAbilty.DeactivateImmediately = true;
            buletteChargeStyle.AddComponent(buletteStyleAbilty.CreateAddFact());


            library.AddCombatFeats(overrunFeature, greaterOverrunFeature, buletteLeap, buletteChargeStyle, buletteRampage);


            var rageBuff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            
            var overbearingAdvance = Helpers.CreateFeature(
                "OverbearingAdvanceFeature",
                "Overbearing Advance",
                "While raging, the barbarian inflicts damage equal to her Strength bonus whenever she succeeds at an overrun combat maneuver.",
                "",
                null,
                FeatureGroup.RagePower
            );
            var overbearingAdvanceBuff = Helpers.CreateBuff(
                "overbearingAdvanceBuff",
                overbearingAdvance.Name,
                overbearingAdvance.Description,
                "",
                buletteChargeStyle.Icon,
                null,
                Helpers.Create<ManeuverTrigger>(
                    mt =>
                    {
                        mt.OnlySuccess = true;
                        mt.ManeuverType = CombatManeuver.Overrun;
                        mt.Action = Helpers.CreateActionList(
                            Helpers.CreateActionDealDamage(
                                PhysicalDamageForm.Bludgeoning,
                                Helpers.CreateContextDiceValue(DiceType.D8, 
                                bonus: new ContextValue() { ValueType = ContextValueType.CasterProperty, Property = UnitProperty.StatBonusStrength }),
                                IgnoreCritical: false)
                        );
                    }
                )
            );
            overbearingAdvanceBuff.SetBuffFlags(BuffFlags.HiddenInUi);
            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rageBuff, overbearingAdvanceBuff, overbearingAdvance);

            var barbarian = library.Get<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");
            var overbearingOnslaught = Helpers.CreateFeature(
                "OverbearingOnslaughtFeature",
                "Overbearing Onslaught",
                "While raging, the barbarian may overrun more than one target per round, with a –2 penalty on her CMB for each overrun check after the first.",
                "",
                null,
                FeatureGroup.RagePower,
                overbearingAdvance.PrerequisiteFeature(),
                Helpers.PrerequisiteClassLevel(barbarian, 6)
            );
            var overbearingOnslaughtBuff = Helpers.CreateBuff(
                "overbearingOnslaughtBuff",
                overbearingAdvance.Name,
                overbearingAdvance.Description,
                "",
                buletteChargeStyle.Icon,
                null,
                Helpers.Create<OverrunMultipleTargets>()
            );
            overbearingAdvanceBuff.SetBuffFlags(BuffFlags.HiddenInUi);

            Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(rageBuff, overbearingOnslaughtBuff, overbearingOnslaught);
            addRagePowers(overbearingAdvance, overbearingOnslaught);

            var brawlerFeatures = new List<BlueprintFeature>();
            foreach(var selection in CallOfTheWild.Brawler.maneuver_training)
            {
                var feature = selection.AllFeatures[0];
                var overrunTraining = library.CopyAndAdd(feature, feature.name.Replace("BullRush", "Overrrun"), "");
                overrunTraining.SetName(overrunTraining.Name.Replace("Bull Rush", "Overrrun"));
                overrunTraining.SetIcon(overrunAbility.Icon);
                foreach(var c in overrunTraining.GetComponents<CallOfTheWild.CombatManeuverMechanics.SpecificCombatManeuverBonus>())
                {
                    var replacement = c.CreateCopy();
                    replacement.maneuver_type = CombatManeuver.Overrun;
                    overrunTraining.ReplaceComponent(c, replacement);
                }
                
                overrunTraining.RemoveComponents<Prerequisite>();
                foreach(var f in brawlerFeatures)
                {
                    overrunTraining.AddComponent(f.PrerequisiteNoFeature());
                }

                selection.AllFeatures = selection.AllFeatures.AddToArray(overrunTraining);
                brawlerFeatures.Add(overrunTraining);
            }

            {
                var maneuverMastery = library.Get<BlueprintFeatureSelection>("b7d8794e176a4a91aa0449f587c99e79");
                var bull = library.Get<BlueprintFeature>("50251d351f304944af6ed32d73bd5417");
                var overrunMastery = library.CopyAndAdd(bull, bull.name.Replace("BullRush", "Overrrun"), "");
                overrunMastery.SetName(overrunMastery.Name.Replace("Bull Rush", "Overrrun"));
                overrunMastery.SetIcon(overrunAbility.Icon);
                overrunMastery.ReplaceComponent<CallOfTheWild.CombatManeuverMechanics.SpecificCombatManeuverBonusUnlessHasFacts>(
                    s =>
                    {
                        s.maneuver_type = CombatManeuver.Overrun;
                    }
                );

                maneuverMastery.AllFeatures = maneuverMastery.AllFeatures.AddToArray(overrunMastery);
            }

            {
                var maneuverMastery = library.Get<BlueprintFeatureSelection>("93095ef65b004c9dbd734ce936ad7bfa");
                var bull = library.Get<BlueprintFeature>("0dbb9c55feec4247add6a305a9b54896");
                var overrunMastery = library.CopyAndAdd(bull, bull.name.Replace("BullRush", "Overrrun"), "");
                overrunMastery.SetName(overrunMastery.Name.Replace("Bull Rush", "Overrrun"));
                overrunMastery.SetIcon(overrunAbility.Icon);
                overrunMastery.ReplaceComponent<CallOfTheWild.CombatManeuverMechanics.SpecificCombatManeuverBonusUnlessHasFacts>(
                    s =>
                    {
                        s.maneuver_type = CombatManeuver.Overrun;
                    }
                );

                foreach (var c in overrunMastery.GetComponents<AddFeatureOnClassLevel>())
                {
                    var replacement = c.CreateCopy();
                    if(replacement.Level == 7)
                    {
                        replacement.Feature = overrunFeature;
                    } else
                    {
                        replacement.Feature = greaterOverrunFeature;
                    }
                    overrunMastery.ReplaceComponent(c, replacement);
                }

                maneuverMastery.AllFeatures = maneuverMastery.AllFeatures.AddToArray(overrunMastery);
            }

            var agileManeuvers = library.Get<BlueprintFeature>("197306972c98bb843af738dc7529a7ac");
            agileManeuvers.AddComponent(overrunFeature.PrerequisiteFeature(true));
        }
    }
}
