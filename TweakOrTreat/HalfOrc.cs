using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class HalfOrc
    {
        static LibraryScriptableObject library => Main.library2;
        static internal void load()
        {
            var halforc = library.Get<BlueprintRace>("1dc20e195581a804890ddc74218bfd8e");
            var orcWeaponFamiliarity = library.Get<BlueprintFeature>("6ab6c271d1558344cbc746350243d17d");
            var ferocity = library.Get<BlueprintFeature>("c99f3405d1ef79049bd90678a666e1d7");
            var intimidating = library.Get<BlueprintFeature>("885f478dff2e39442a0f64ceea6339c9");

            var ferocityComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = ferocity;
                }),
                Helpers.Create<RemoveFeatureOnApply>(c =>
                {
                    c.Feature = ferocity;
                })
            };

            var orcWeaponFamiliarityComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = orcWeaponFamiliarity;
                }),
                Helpers.Create<RemoveFeatureOnApply>(c =>
                {
                    c.Feature = orcWeaponFamiliarity;
                })
            };

            var intimidatingComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = intimidating;
                }),
                Helpers.Create<RemoveFeatureOnApply>(c =>
                {
                    c.Feature = intimidating;
                })
            };

            var alternateFeatures = new List<BlueprintFeature>() { };

            if (HalfElf.multitalented != null)
            {
                var cotwFeatureSelection = library.Get<BlueprintFeature>("b622c599033943cabb019dd22c7cc7e1");

                halforc.Features = halforc.Features.RemoveFromArray(cotwFeatureSelection);

                var toothy = library.Get<BlueprintFeature>("5de9a0e25bf1480f8b543548bd7571ab");
                var sacredTatoo = library.Get<BlueprintFeature>("97a8d8abfbaa443890b16f6818c39b6c");
                toothy.RemoveComponents<RemoveFeatureOnApply>();
                sacredTatoo.RemoveComponents<RemoveFeatureOnApply>();

                toothy.AddComponents(ferocityComponents);
                sacredTatoo.AddComponents(ferocityComponents);

                alternateFeatures.Add(toothy);
                alternateFeatures.Add(sacredTatoo);
            }

            var endurance = library.Get<BlueprintFeature>("54ee847996c25cd4ba8773d7b8555174");
            var shamansApprentice = Utils.CreateFeature("ShamansApprenticeFeature", "Shaman’s Apprentice",
                "Only the most stalwart survive the years of harsh treatment that an apprenticeship to an orc shaman entails. Half-orcs with this trait gain Endurance as a bonus feat.",
                "", endurance.Icon, FeatureGroup.Racial,
                intimidatingComponents,
                new BlueprintComponent[]
                {
                    endurance.CreateAddFact()
                }
            );

            var burningAssurance = Utils.CreateFeature("BurningAssuranceFeature", "Burning Assurance",
                "Sandkin lack the chip on their shoulder that many half-orcs acquire as a result of prejudice, and their self-confidence puts others at ease. Desert half-orcs with this racial trait gain a +2 racial bonus on Diplomacy checks.",
                "", null, FeatureGroup.Racial,
                intimidatingComponents,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.CheckDiplomacy, 2, Kingmaker.Enums.ModifierDescriptor.Racial)
                }
            );


            var feyThought = UniversalRacialTraits.makeFeyThoughts("HalfOrc", orcWeaponFamiliarityComponents);

            var shamanEnhancement = Utils.CreateFeatureSelection("ShamanEnhancementFeatureSelection", "Shaman Enhancement",
                "Certain half-orcs know rituals to enhance the strength and brutality of their allies. These halforcs gain a +2 racial bonus on Knowledge (Arcana) checks. In addition, when such a half-orc acquires an animal companion, bonded mount, cohort, familiar, or spirit animal, that creature gains a +2 bonus to Strength, Dexterity, or Constitution, as selected by the half-orc.",
                "", null, FeatureGroup.Racial,
                intimidatingComponents,
                orcWeaponFamiliarityComponents,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.SkillKnowledgeArcana, 2, Kingmaker.Enums.ModifierDescriptor.Racial)
                }
            );
            shamanEnhancement.ReapplyOnLevelUp = true;
            shamanEnhancement.AllFeatures = Human.makeAnimalBonusFeatures("ShamanEnhancement", shamanEnhancement.Name, shamanEnhancement.Description, 
                new List<StatType>() { StatType.Strength, StatType.Dexterity, StatType.Constitution });

            var projection = Utils.CreateFeature("ProjectionHalfOrcFeature", "Projection",
                "Some half-orcs channel negative emotions through magic. The DCs of any saving throws against spells with the fear or pain descriptor they cast increase by 1.",
                "", null, FeatureGroup.Racial,
                orcWeaponFamiliarityComponents,
                ferocityComponents,
                new BlueprintComponent[]
                {
                    Helpers.Create<IncreaseSpellDescriptorDC>(
                        c =>
                        {
                            c.Descriptor = SpellDescriptor.Fear | (SpellDescriptor)CallOfTheWild.AdditionalSpellDescriptors.ExtraSpellDescriptor.Pain;
                            c.BonusDC = 1;
                        }
                    )
                }
            );

            alternateFeatures.AddRange(
                new List<BlueprintFeature>()
                {
                    feyThought,
                    shamansApprentice,
                    burningAssurance,
                    shamanEnhancement,
                    projection
                }
            );

            RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(halforc, 3, alternateFeatures);
        }
    }
}
