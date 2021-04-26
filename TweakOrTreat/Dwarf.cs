using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class Dwarf
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var dwarf = library.Get<BlueprintRace>("c4faf439f0e70bd40b5e36ee80d06be7");
            var dwarvenWeaponFamiliarity = library.Get<BlueprintFeature>("a1619e8d27fe97c40ba443f6f8ab1763");
            var stability = library.Get<BlueprintFeature>("2f254c6068d58b643b8de2fc7ec32dbb");
            var hardy = library.Get<BlueprintFeature>("f75d3b6110f04d1409564b9d7647db60");
            var defenceTraining = library.Get<BlueprintFeature>("f268a00e42618144e86c9db76af7f3e9");
            var hatred = library.Get<BlueprintFeature>("6cde66a7da5a2024c906d887db735223");

            var stabilityComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = stability;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = stability;
                })
            };

            var hardyComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = hardy;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = hardy;
                })
            };

            var defenceTrainingComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = defenceTraining;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = defenceTraining;
                })
            };

            var hatredComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = hatred;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = hatred;
                })
            };

            var ironCitizen = library.CopyAndAdd<BlueprintFeature>("7e0b8edeff464ec38caef19a12180a48", "IronCitizenDwarfFeature", "");
            var rmType = HarmonyLib.AccessTools.TypeByName("RacesUnleashed.RemoveFeature, RacesUnleashed");
            MethodInfo method = typeof(ExtensionMethods).GetMethod(nameof(ExtensionMethods.RemoveComponents), new Type[] { typeof(BlueprintScriptableObject) });
            MethodInfo generic = method.MakeGenericMethod(rmType);
            generic.Invoke(null, new object[] { ironCitizen });
            ironCitizen.RemoveComponents<PrerequisiteFeature>();
            ironCitizen.AddComponents(stabilityComponents);

            var magicResistant = library.CopyAndAdd<BlueprintFeature>("d7f52a396b9146b9a36c2735188a9788", "MagicResistantDwarfFeature", "");
            generic.Invoke(null, new object[] { magicResistant });
            magicResistant.RemoveComponents<PrerequisiteFeature>();
            magicResistant.AddComponents(hardyComponents);

            var unstoppable = library.CopyAndAdd<BlueprintFeature>("a46ef69edbe3466ab97fdce690ad64ee", "UnstoppableDwarfFeature", "");
            generic.Invoke(null, new object[] { unstoppable });
            unstoppable.RemoveComponents<PrerequisiteFeature>();
            unstoppable.AddComponents(hardyComponents);

            var wanderer = library.CopyAndAdd<BlueprintFeature>("18d1de9faafb401a871d80a0154ae5d5", "WandererDwarfFeature", "");
            generic.Invoke(null, new object[] { wanderer });
            wanderer.RemoveComponents<PrerequisiteFeature>();
            wanderer.AddComponents(hardyComponents);

            var kiResource = library.Get<BlueprintAbilityResource>("9d9c90a9a1f52d04799294bf91c80a82");
            var kiSpeedBuff = library.CopyAndAdd<BlueprintBuff>("9ea4ec3dc30cd7940a372a4d699032e7", "IronCitizenKiSpeedBoostBuff", "");
            kiSpeedBuff.ReplaceComponent<BuffMovementSpeed>(b => b.Value = 20);
            var kiSpeedBurst = library.CopyAndAdd<BlueprintAbility>("8c98b8f3ac90fa245afe14116e48c7da", "IronCitizenKiSpeedBoostAbility", "");
            {
                var action = Common.changeAction<ContextActionApplyBuff>(
                    kiSpeedBurst.GetComponent<AbilityEffectRunAction>().Actions.Actions,
                        c =>
                        {
                            c.Buff = kiSpeedBuff;
                            c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Rounds);
                        }
                );
                kiSpeedBurst.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(action));
                kiSpeedBurst.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = kiResource);
                kiSpeedBurst.SetNameDescription(
                    "Ki Speed Burst",
                    "A dwarf with this ki power can spend 1 point from his ki pool as a swift action to grant himself a sudden burst of speed. This increases the dwarf's base land speed by 10 feet for 1 round."
                );
            }

            var kiDodgeBuff = Helpers.CreateBuff(
                "KiDodgeBuff",
                "",
                "",
                "",
                null,
                null,
                Helpers.CreateAddStatBonus(StatType.AC, 2, ModifierDescriptor.Dodge)
            );
            var kiDodge = Helpers.CreateAbility(
                "KiDodgeAbilty",
                "Ki Dodge",
                "A dwarf with this ki power can spend 1 point from his ki pool as a swift action to grant himself a +2 dodge bonus to AC for 1 round.",
                "",
                Helpers.GetIcon("2b636b9e8dd7df94cbd372c52237eebf"),
                AbilityType.Supernatural,
                Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Swift,
                AbilityRange.Personal,
                "1 round",
                ""
            );
            {
                var action = Helpers.CreateApplyBuff(kiDodgeBuff, Helpers.CreateContextDuration(1, DurationRate.Rounds), false, false, true, true);
                kiDodge.AddComponent(Helpers.CreateResourceLogic(kiResource));
                kiDodge.AddComponent(Helpers.CreateRunActions(action));
            }
            //kiPoolRouge.AddComponent(Helpers.CreateAddFacts(kiSpeedBurst));
            var ironWithin = Utils.CreateFeature("IronWithinDwarfFeature", "Iron Within",
                "Dwarves with this racial trait gain 1 ki point. If the dwarf gains ki points from a different source, this ki point is added to that pool. In addition to any other ways in which the dwarf can use ki, the dwarf can expend the ki point as a swift action to either gain a +2 dodge bonus to AC for 1 round or increase her base speed by 20 feet for 1 round. The bonus ki point does not allow the dwarf to make a ki strike unless she has another ability that allows her to do so, such as the ki pool from the monk class. Like other ki points, this ki point is replenished each morning after 8 hours of rest or meditation.",
                "", null, FeatureGroup.Racial,
                defenceTrainingComponents,
                hatredComponents,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddFacts(kiSpeedBurst),
                    Helpers.CreateAddFact(kiDodge),
                    Helpers.CreateAddAbilityResource(kiResource),
                    Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = kiResource; i.Value = 1; })
                }
            );

            var feyThought = UniversalRacialTraits.makeFeyThoughts("Dwarf", hatredComponents);

            var relentless = Utils.CreateFeature("RelentlessDwarfFeature", "Relentless",
                "Dwarves are skilled at pushing their way through a battlefield, tossing aside lesser foes with ease. Dwarves with this racial trait receive a +2 bonus on combat maneuver checks made to bull rush or overrun an opponent.",
                "", null, FeatureGroup.Racial,
                stabilityComponents,
                new BlueprintComponent[]
                {
                    Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.Overrun, 2),
                    Common.createManeuverBonus(Kingmaker.RuleSystem.Rules.CombatManeuver.BullRush, 2)
                }
            );

            RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(dwarf, 3, new List<BlueprintFeature>() {
                ironCitizen,
                magicResistant,
                unstoppable,
                wanderer,
                ironWithin,
                feyThought,
                relentless
            });
        }
    }
}
