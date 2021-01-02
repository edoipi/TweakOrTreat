using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
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
        static LibraryScriptableObject library => Main.library2;
        static internal void load()
        {
            var halfElf = library.Get<BlueprintRace>("b3646842ffbd01643ab4dac7479b20b0");
            var multitalented = library.TryGet<BlueprintFeatureSelection>("e6a72a23e75545bc9a57a6b94ffc8b69");

            var alternateFeatures = new List<BlueprintFeature>() { };
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
            }

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

            var dualMinded = library.CopyAndAdd<BlueprintFeature>("175d1577bb6c9a04baf88eec99c66334", "DualMinded", "");
            dualMinded.SetName("Dual Minded");
            dualMinded.SetDescription("The mixed ancestry of some half-elves makes them resistant to mental attacks. Half-elves with this racial trait gain a +2 bonus on all Will saving throws.");
            dualMinded.AddComponents(adaptabiltyComponents);

            alternateFeatures.AddRange(
                new List<BlueprintFeature>()
                {
                    dualMinded
                }
            );

            RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(halfElf, 2, alternateFeatures);
        }
    }
}
