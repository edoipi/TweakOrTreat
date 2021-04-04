using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    [ComponentName("Increase spell descriptor DC")]
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class IncreaseSpellDescriptorDCUnpatched : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        // Token: 0x060087B9 RID: 34745 RVA: 0x0022F6A0 File Offset: 0x0022D8A0
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            SpellDescriptorComponent component = evt.Spell.GetComponent<SpellDescriptorComponent>();
            if (component != null && component.Descriptor.HasAnyFlag(this.Descriptor))
            {
                evt.AddBonusDC(this.BonusDC);
            }
        }

        // Token: 0x060087BA RID: 34746 RVA: 0x00002FA8 File Offset: 0x000011A8
        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt)
        {
        }

        // Token: 0x04005809 RID: 22537
        public SpellDescriptorWrapper Descriptor;

        // Token: 0x0400580A RID: 22538
        public int BonusDC;
    }
    class Elf
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var elf = library.Get<BlueprintRace>("25a5878d125338244896ebd3238226c8");
            var elvenImmunities = library.Get<BlueprintFeature>("2483a523984f44944a7cf157b21bf79c");
            var keenSenses = library.Get<BlueprintFeature>("9c747d24f6321f744aa1bb4bd343880d");
            var elvenWeaponFamiliarity = library.Get<BlueprintFeature>("03fd1e043fc678a4baf73fe67c3780ce");
            var elvenMagic = library.Get<BlueprintFeature>("55edf82380a1c8540af6c6037d34f322");

            var elvenImmunitiesComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = elvenImmunities;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = elvenImmunities;
                })
            };

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

            var elvenWeaponFamiliarityComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = elvenWeaponFamiliarity;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = elvenWeaponFamiliarity;
                })
            };

            var elvenMagicComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = elvenMagic;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = elvenMagic;
                })
            };

            var moonkissed = Utils.CreateFeature("MoonkissedFeature", "Moonkissed",
                "Some Spiresworn, especially those born within the Spire itself, are mystically warded from birth against dangers both mental and physical. Elves with this alternate racial trait gain a +1 racial bonus on saving throws.",
                "", null, FeatureGroup.Racial,
                keenSensesComponents,
                elvenImmunitiesComponents,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddStatBonus(StatType.SaveWill, 1, ModifierDescriptor.Racial),
                    Helpers.CreateAddStatBonus(StatType.SaveReflex, 1, ModifierDescriptor.Racial),
                    Helpers.CreateAddStatBonus(StatType.SaveFortitude, 1, ModifierDescriptor.Racial),
                }
            );

            var skillFocus = library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a");
            var humanRaised = Utils.CreateFeatureSelection("HumanRaisedFeature", "Human-Raised",
                "Forlorn—elves raised outside of elven communities—are accustomed to other races’ brevity of life. Although they lose the opportunity to train in traditional elven arts, these elves pick up a bit of their adoptive parents’ skills. They gain Skill Focus as a bonus feat.",
                "", null, FeatureGroup.Racial,
                elvenMagicComponents,
                elvenWeaponFamiliarityComponents
            );
            humanRaised.AllFeatures = skillFocus.AllFeatures;
        
            var spellFocus = library.Get<BlueprintParametrizedFeature>("16fa59cc9a72a6043b566b49184f53fe");
            var illustriousUrbanite = Utils.CreateFeatureSelection("IllustriousUrbaniteFeature", "Illustrious Urbanite",
                "City elves have a remarkable ability to combine magic harmoniously with their surroundings. They gain Spell Focus with conjuration, illusion, or transmutation spells as a bonus feat.",
                "", null, FeatureGroup.Racial,
                keenSensesComponents
            );
            var urbaniteFeatures = new Dictionary<SpellSchool, BlueprintFeature>();
            {
                var urbaniteShools = new List<SpellSchool>() { SpellSchool.Conjuration, SpellSchool.Illusion, SpellSchool.Transmutation };
                foreach (var school in urbaniteShools)
                {
                    var feature = Helpers.CreateFeature(
                        school.ToString() + "spellfocusurbanite",
                        spellFocus.Name + $": {LocalizedTexts.Instance.SpellSchoolNames.GetText(school)}",
                        spellFocus.Description,
                        "",
                        null,
                        FeatureGroup.None,
                        Common.createAddParametrizedFeatures(spellFocus, school)
                    );
                    urbaniteFeatures[school] = feature;
                }
                illustriousUrbanite.AllFeatures = urbaniteFeatures.Values.ToArray();
            }

            //EnumUtils.GetValues<SpellSchool>()

            var overwhelmingMagic = Utils.CreateFeatureSelection("OverwhelmingMagicFeature", "Overwhelming Magic",
                "Some elves obsess over the fundamentals of magic, training for decades to add layers of potent spellwork before they ever begin practicing true spells. This builds a foundation that makes their magic increasingly difficult to resist. These elves gain Spell Focus as a bonus feat.",
                "", null, FeatureGroup.Racial,
                elvenMagicComponents,
                elvenWeaponFamiliarityComponents
            );
            var overwhelmingMagicFeatures = new Dictionary<SpellSchool, BlueprintFeature>();
            {
                foreach (var school in EnumUtils.GetValues<SpellSchool>().Where(s => s != SpellSchool.None))
                {
                    var feature = Helpers.CreateFeature(
                        school.ToString() + "spellfocusoverwhelmingmagic",
                        spellFocus.Name + $": {LocalizedTexts.Instance.SpellSchoolNames.GetText(school)}",
                        spellFocus.Description,
                        "",
                        null,
                        FeatureGroup.None,
                        Common.createAddParametrizedFeatures(spellFocus, school)
                    );
                    overwhelmingMagicFeatures[school] = feature;
                }
                overwhelmingMagic.AllFeatures = overwhelmingMagicFeatures.Values.ToArray();
            }

            foreach(var school in urbaniteFeatures.Keys)
            {
                var uFeature = urbaniteFeatures[school];
                var oFeature = overwhelmingMagicFeatures[school];
                uFeature.AddComponent(Helpers.PrerequisiteNoFeature(oFeature));
                oFeature.AddComponent(Helpers.PrerequisiteNoFeature(uFeature));
            }


            var longLimbed = Utils.CreateFeature("LongLimbedFeature", "Long-Limbed",
                "Elves with this racial trait have a base move speed of 35 feet.",
                "", null, FeatureGroup.Racial,
                elvenWeaponFamiliarityComponents,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddStatBonus(StatType.Speed, 5, ModifierDescriptor.UntypedStackable)
                }
            );

            var feyThought = UniversalRacialTraits.makeFeyThoughts("Elf", elvenWeaponFamiliarityComponents);

            var sleepRes = Helpers.CreateAbilityResource("ElfSleepResource", "", "", "", null, Array.Empty<BlueprintComponent>());
            sleepRes.SetFixedResource(1);
            var sleepSpellLike = RacesUnleashed.Util.CreateRacialAbility("bb7ecad2d3d2c8247a38f44855c99061", "ElfSleepSpellLike",
                "9ca725bbb05d4ebaa32ee3c9c8987296", sleepRes);

            sleepSpellLike.AddComponent(Helpers.Create<RacesUnleashed.AbilityCasterHasStat>( 
                c =>
                 {
                     c.MinLevel = 11;
                     c.StatType = StatType.Charisma;
                 }
             ));

            var dreamspeaker = Utils.CreateFeature("DreamspeakerFeature", "Dreamspeaker",
                "A few elves have the ability to tap into the power of sleep, dreams, and prescient reverie. Elves with this racial trait add +1 to the saving throw DCs of spells of the divination school and sleep effects they cast. In addition, elves with Charisma scores of 11 or higher may use sleep once per day as a spell-like ability (caster level is equal to the elf’s character level).",
                "", null, FeatureGroup.Racial,
                elvenImmunitiesComponents,
                new BlueprintComponent[]
                {
                    Helpers.Create<IncreaseSpellDescriptorDCUnpatched>(
                        c =>
                        {
                            c.Descriptor = SpellDescriptor.Sleep;
                            c.BonusDC = 1;
                        }
                    ),
                    Helpers.Create<IncreaseSpellSchoolDC>(
                        c =>
                        {
                            c.School = SpellSchool.Divination;
                            c.BonusDC = 1;
                        }
                    ),
                    sleepSpellLike.CreateAddFact(),
                    sleepRes.CreateAddAbilityResource()
                }
            );

            RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(elf, 3, new List<BlueprintFeature>() {
                moonkissed,
                humanRaised,
                illustriousUrbanite,
                overwhelmingMagic,
                longLimbed,
                feyThought,
                dreamspeaker
            });
        }
    }
}
