using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class Multidisciplined : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            int spellbooks = 0;

            foreach(var clazz in this.Owner.Progression.Classes)
            {
                if(clazz.Spellbook != null)
                {
                    spellbooks++;
                }
            }

            if (spellbooks >= 2)
            {
                evt.AddBonusCasterLevel(1);
            }
        }



        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }
    }

    class HalfElf
    {
        public static BlueprintFeatureSelection multitalented;
        static LibraryScriptableObject library => Main.library2;
        static internal void load()
        {
            var halfElf = library.Get<BlueprintRace>("b3646842ffbd01643ab4dac7479b20b0");
            multitalented = library.TryGet<BlueprintFeatureSelection>("e6a72a23e75545bc9a57a6b94ffc8b69");

            var alternateFeatures = new List<BlueprintFeature>() { };

            var adaptabilty = library.Get<BlueprintFeatureSelection>("26a668c5a8c22354bac67bcd42e09a3f");
            var adaptabiltyComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = adaptabilty;
                }),
                Helpers.Create<RemoveFeatureOnApply>(c =>
                {
                    c.Feature = adaptabilty;
                }),
                Helpers.Create<RemoveSelection>(c =>
                {
                    c.selection = adaptabilty;
                })
            };

            if (multitalented != null)
            {
                Main.logger.Log("Favored class mod found");


                var multitalentedComponents = new BlueprintComponent[] {
                    Helpers.Create<PrerequisiteFeature>(c =>
                    {
                        c.Feature = multitalented;
                    }),
                    Helpers.Create<RemoveFeatureOnApply>(c =>
                    {
                        c.Feature = multitalented;
                    }),
                    Helpers.Create<RemoveSelection>(c =>
                    {
                        c.selection = multitalented;
                    })
                };

                var multidisciplined = Utils.CreateFeature("Multidisciplined", "Multidisciplined",
                    "Born to two races, half-elves have a knack for combining different magical traditions. If a halfelf with this racial trait has spellcasting abilities from at least two different classes, the effects of spells she casts from all her classes are calculated as though her caster level were 1 level higher, to a maximum of her character level.",
                    "", null, FeatureGroup.Racial,
                    multitalentedComponents,
                    new BlueprintComponent[]
                    {
                    Helpers.Create<Multidisciplined>()
                    }
                );

                alternateFeatures.Add(multidisciplined);

                var ancestralArms = library.TryGet<BlueprintFeatureSelection>("096059c5fac74798bd65c964a878964a");
                adaptabilty.AllFeatures = adaptabilty.AllFeatures.RemoveFromArray(ancestralArms);
                ancestralArms.AddComponents(adaptabiltyComponents);

                alternateFeatures.Add(ancestralArms);

                var feyThought = UniversalRacialTraits.makeFeyThoughts("HalfElf", multitalentedComponents);
                alternateFeatures.Add(feyThought);
            }

            

            var dualMinded = library.CopyAndAdd<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334", "DualMinded", "");
            dualMinded.SetName("Dual Minded");
            dualMinded.SetDescription("The mixed ancestry of some half-elves makes them resistant to mental attacks. Half-elves with this racial trait gain a +2 bonus on all Will saving throws.");
            dualMinded.AddComponents(adaptabiltyComponents);


            var drowWeapons = library.Get<BlueprintFeature>("10d0be4122534a9eb41173e88b3e8cc7");
            var elvenWeapons = library.Get<BlueprintFeature>("03fd1e043fc678a4baf73fe67c3780ce");

            var weaponFamiliarity = Utils.CreateFeature("HalfElfWeaponFamiliarityFeature", "Weapon Familiarity",
                "Half-elves raised among elves often feel pitied and mistrusted by their longer-lived kin, and yet they receive training in elf weapons. They gain the elf ’s weapon familiarity trait.",
                "", null, FeatureGroup.Racial,
                adaptabiltyComponents,
                new BlueprintComponent[]
                {
                    elvenWeapons.CreateAddFact()
                }
            );

            var drowTrained = Utils.CreateFeature("HalfElfDrowTrainedFeature", "Drow-Trained",
                "Darkborn (half-elves descended from drow) are proficient with the shortbow, rapier, and shortsword.",
                "", null, FeatureGroup.Racial,
                adaptabiltyComponents,
                new BlueprintComponent[]
                {
                    drowWeapons.CreateAddFact()
                }
            );

            var keenSenses = library.Get<BlueprintFeature>("9c747d24f6321f744aa1bb4bd343880d");
            var keenSensesComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = keenSenses;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = keenSenses;
                })
            };

            var spellResistance = Utils.CreateFeature("HalfElfSpellResistanceFeature", "Spell Resistance",
                "Some drow-blooded half-elves share the drow resistance to magic. A half-elf with this racial trait gains spell resistance equal to 6 + her character level.",
                "", null, FeatureGroup.Racial,
                adaptabiltyComponents,
                keenSensesComponents,
                new BlueprintComponent[]
                {
                    Helpers.Create<AddSpellResistance>(s => s.Value = Helpers.CreateContextValue(AbilityRankType.StatBonus)),
                    Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.CharacterLevel, 
                    progression: ContextRankProgression.BonusValue, type: AbilityRankType.StatBonus, stepLevel: 6)
                }
            );

            alternateFeatures.AddRange(
                new List<BlueprintFeature>()
                {
                    dualMinded,
                    weaponFamiliarity,
                    drowTrained,
                    spellResistance
                }
            );

            RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(halfElf, 2, alternateFeatures);
        }
    }
}
