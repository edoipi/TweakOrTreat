using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class OathAgainstChaos
    {
        static BlueprintCharacterClass paladin;
        static BlueprintFeature smiteChaos;
        static BlueprintAbility markOfJusticeSmiteChaos;
        public static BlueprintFeature abilityIntoFeature(BlueprintAbility ability, int level)
        {
            return Helpers.CreateFeature(
                "OathAgainstChaos" + ability.name,
                ability.Name,
                "A paladin's oath influences what magic she can perform. An oathbound paladin adds one spell to the paladin spell list at each paladin spell level she can cast (including spell levels for which she would only gain spells per day if her Charisma were high enough to grant bonus spells of that level). Her oath determines what spell is added to the spell list. If the paladin has multiple oaths, the spells from each oath are added to her spell list.\n"
                + ability.Name + ": " + ability.Description,
                "",
                ability.Icon,
                FeatureGroup.None,
                ability.CreateAddKnownSpell(paladin, level)
            );
        }

        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            paladin = Helpers.GetClass("bfa11238e7ae3544bbeb4d0b92e897ec");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "OathAgainstChaosArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Oath against Chaos");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Despite the seemingly inherent contradiction, the militant Hellknights of Cheliax count a number of paladins among their ranks, especially the Order of the Godclaw. Typically these paladins reconcile their allegiance to the Hellknights by dedication to the rule of law above all other things.");
            });
            Helpers.SetField(archetype, "m_ParentClass", paladin);
            library.AddAsset(archetype, "");
            
            var smiteEvilFeature = library.Get<BlueprintFeature>("3a6db57fce75b0244a6a5819528ddf26");
            var channelEnergyFeature = library.Get<BlueprintFeature>("cb6d55dda5ab906459d18a435994a760");
            var layOnHandsResource = library.Get<BlueprintAbilityResource>("9dedf41d995ff4446a181f143c3db98c");
            var markOfJustice = library.Get<BlueprintFeature>("9f13fdd044ccb8a439f27417481cb00e");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, smiteEvilFeature),
                Helpers.LevelEntry(4, channelEnergyFeature),
                Helpers.LevelEntry(11, markOfJustice),
            };

            var command = CallOfTheWild.NewSpells.command;
            var silence = CallOfTheWild.NewSpells.silence;
            var deepSlumber = library.Get<BlueprintAbility>("7658b74f626c56a49939d9c20580885e");
            var wrathfulWeapon = CallOfTheWild.NewSpells.wrathful_weapon;

            var commandFeature = abilityIntoFeature(command, 1);
            var silenceFeature = abilityIntoFeature(silence, 2);
            var deepSlumberFeature = abilityIntoFeature(deepSlumber, 3);
            var wrathfulWeaponFeature = abilityIntoFeature(wrathfulWeapon, 4);

            //remove no-buff requirement from smite evil, make buff prolong itself
            {
                var smiteEvilAbility = library.Get<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec");
                var smiteAction = smiteEvilAbility.GetComponent<AbilityEffectRunAction>();

                var conditional = (Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)smiteAction.Actions.Actions[0];
                // according to dll it should be just ignored
                conditional.ConditionsChecker.Conditions[1] = null;

                var smiteEvilBuff = library.Get<BlueprintBuff>("b6570b8cbb32eaf4ca8255d0ec3310b0");
                smiteEvilBuff.Stacking = StackingType.Prolong;
            }

            // make mark of justice apply smite evil buff as well
            //{
            //    var auraOfJusticeAbility = library.Get<BlueprintAbility>("7a4f0c48829952e47bb1fd1e4e9da83a");
            //    var smiteEvilBuff = library.Get<BlueprintBuff>("b6570b8cbb32eaf4ca8255d0ec3310b0");

            //    var auraAction = auraOfJusticeAbility.GetComponent<AbilityEffectRunAction>();
            //    var conditional = (Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)auraAction.Actions.Actions[0];

            //    conditional.IfTrue.Actions = conditional.IfTrue.Actions.AddToArray(
            //        Common.createContextActionApplyBuff(smiteEvilBuff, Helpers.CreateContextDuration(bonus: 1, rate: DurationRate.Minutes), dispellable: false)
            //    );
            //}
            

            smiteChaos = Common.createSmite(
                "OathAgainstChaosSmiteChaos",
                "Smite Chaos",
                "This ability works like the standard paladin ability to smite evil, except the paladin gains bonuses against targets with chaotic alignments instead of evil.",
                "",
                "",
                smiteEvilFeature.Icon,
                new BlueprintCharacterClass[] { paladin },
                AlignmentComponent.Chaotic
            );

            var newSmiteEvil = Common.createSmite(
                "OathAgainstChaosSmiteEvil",
                smiteEvilFeature.Name,
                smiteEvilFeature.Description,
                "",
                "",
                smiteEvilFeature.Icon,
                new BlueprintCharacterClass[] { paladin },
                AlignmentComponent.Evil
            );

            {
                var smiteAbility = newSmiteEvil.GetComponent<AddFacts>().Facts.First() as BlueprintAbility;
                var spendOtherResource = Helpers.Create<ContextActionOnContextCaster>(
                    c => c.Actions = Helpers.CreateActionList(Common.createContextActionSpendResource(layOnHandsResource, 1))
                );
                smiteAbility.ReplaceComponent<AbilityEffectRunAction>(
                    c => c.Actions = Helpers.CreateActionList(c.Actions.Actions.AddToArray(spendOtherResource))
                );
                smiteAbility.AddComponent(
                    Helpers.Create<CallOfTheWild.NewMechanics.AbilityCasterHasResource>(
                        c =>
                        {
                            c.resource = layOnHandsResource;
                            c.amount = 1;
                        }
                    )
                );
            }

            var orderOfGood = Utils.CreateFeature("OathAgainstChaosOrderOfGood", "Order of Good",
                "When an oathbound paladin reaches 4th level, she gains the ability to spend one use of her lay on hands ability when using her smite chaos ability to instead smite evil, as the paladin ability of the same name.",
                "", smiteEvilFeature.Icon, FeatureGroup.None,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddFact(newSmiteEvil),
                }
            );

            var markOfJusticeAbility = library.Get<BlueprintAbility>("7a4f0c48829952e47bb1fd1e4e9da83a");
            var markOfJusticeBuff = library.Get<BlueprintBuff>("ac3c66782859eb84692a8782320ffd2c");
            markOfJusticeAbility.SetName("Mark of Justice - Smite Evil");

            //make it inherit name and icon
            markOfJusticeBuff.SetIcon(null);
            markOfJusticeBuff.SetName("");

            var markOfJusticeExtraCost = library.CopyAndAdd(markOfJusticeAbility, "MarkOfJusticeOathAgainstChaos", "");
            markOfJusticeExtraCost.SetDescription(markOfJusticeExtraCost.Description + "\nTo use this ability to share effect of Smite Evil oathbound paladin has to additionally spend two uses of her lay on hands ability.");
            {
                var spendOtherResource = Helpers.Create<ContextActionOnContextCaster>(
                    c => c.Actions = Helpers.CreateActionList(Common.createContextActionSpendResource(layOnHandsResource, 2))
                );
                markOfJusticeExtraCost.ReplaceComponent<AbilityEffectRunAction>(
                    c => c.Actions = Helpers.CreateActionList(c.Actions.Actions.AddToArray(spendOtherResource))
                );
                markOfJusticeExtraCost.AddComponent(
                    Helpers.Create<CallOfTheWild.NewMechanics.AbilityCasterHasResource>(
                        c =>
                        {
                            c.resource = layOnHandsResource;
                            c.amount = 2;
                        }
                    )
                );
            }

            var chaoticDescription = "At 11th level, a paladin can expend two uses of her smite chaos ability to grant the ability to smite chaos to all allies for 1 minute, using her bonuses. As a swift action, the paladin chooses one target within sight to smite. If this target is chaotic, the paladin's allies add her Charisma bonus (if any) to their attack rolls and add her paladin level to all damage rolls made against the target of her smite. Smite chaos attacks automatically bypass any DR the creature might possess.\nIn addition, while smite chaos is in effect, the paladin's allies gain a deflection bonus equal to her Charisma bonus (if any) to their AC against attacks made by the target of this smite. If the paladin targets a creature that is not chaotic, this smite is wasted with no effect. The mark of justice lasts until the target dies or the paladin selects a new target.";
            markOfJusticeSmiteChaos = library.CopyAndAdd(markOfJusticeAbility, "MarkOfJusticeSmiteChaos", "");
            markOfJusticeSmiteChaos.SetName("Mark of Justice - Smite Chaos");
            markOfJusticeSmiteChaos.SetDescription(chaoticDescription);
            {
                var markAction = markOfJusticeSmiteChaos.GetComponent<AbilityEffectRunAction>();
                var conditional = (Kingmaker.Designers.EventConditionActionSystem.Actions.Conditional)markAction.Actions.Actions[0];
                var conditions = new Kingmaker.ElementsSystem.Condition[] { Helpers.CreateContextConditionAlignment(AlignmentComponent.Chaotic, false, false), conditional.ConditionsChecker.Conditions[1] };
                var newAction = Helpers.CreateConditional(conditions, conditional.IfTrue.Actions, conditional.IfFalse.Actions);
                markOfJusticeSmiteChaos.ReplaceComponent(markAction, Helpers.CreateRunActions(newAction));
            }

            var markOfJusticeBoth = Utils.CreateFeature("MarkOfJusticeOathAgainstChaosBoth", "Mark of Justice",
                "When an oathbound paladin reaches 11th level she gains Mark of Justice as normal, except she shares effects of Smite Chaos instead of Smite Evil. She can also spend two uses of her lay on hands ability to share effects of the Smite Evil instead.",
                "", Helpers.GetIcon("9f13fdd044ccb8a439f27417481cb00e"), FeatureGroup.None,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddFact(markOfJusticeExtraCost),
                    Helpers.CreateAddFact(markOfJusticeSmiteChaos),
                }
            );

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, smiteChaos),
                Helpers.LevelEntry(4, commandFeature, orderOfGood),
                Helpers.LevelEntry(7, silenceFeature),
                Helpers.LevelEntry(10, deepSlumberFeature),
                Helpers.LevelEntry(11, markOfJusticeBoth),
                Helpers.LevelEntry(13, wrathfulWeaponFeature),
            };
            paladin.Archetypes = paladin.Archetypes.AddToArray(archetype);

            paladin.Progression.UIGroups = paladin.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(commandFeature, silenceFeature, deepSlumberFeature, wrathfulWeaponFeature)
            );

            foreach (var group in paladin.Progression.UIGroups)
            {
                if (group.Features.Contains(smiteEvilFeature))
                {
                    group.Features.Add(smiteChaos);
                }
                if (group.Features.Contains(channelEnergyFeature))
                {
                    group.Features.Add(orderOfGood);
                    group.Features.Add(markOfJusticeBoth);
                }
            }
        }
    }
}
