using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TweakOrTreat
{
    class Halfling
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            BlueprintRace halfling = library.Get<BlueprintRace>("b0c3ef2729c498f47970bb50fa1acd30");
            var fearless = library.Get<BlueprintFeature>("39144c817b70467499cc32e3cff59d81");
            var slowSpeed = library.Get<BlueprintFeature>("09bc9ccb8ee0ffe4b8827066b1ed7e11");
            var sureFooted = library.Get<BlueprintFeature>("0fe5db70b50cd894c849fc764c80bbb9");
            var halflingLuck = library.Get<BlueprintFeature>("84ffa66048d26b14c800a425199f9886");

            var weaponFamiliarityComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = WeaponFamiliarity.halflingWeaponFamiliarity;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = WeaponFamiliarity.halflingWeaponFamiliarity;
                })
            };

            var halflingLuckComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = halflingLuck;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = halflingLuck;
                })
            };

            var fearlessComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = fearless;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = fearless;
                })
            };

            var slowSpeedComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = slowSpeed;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = slowSpeed;
                })
            };

            var sureFootedComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = sureFooted;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = sureFooted;
                })
            };

            BlueprintFeature fleetOfFoot = Utils.CreateFeature("FleetOfFoot", "Fleet of Foot",
                "Some halflings are quicker than their kin but less cautious. Halflings with this racial trait move at normal speed and have a base speed of 30 feet. This racial trait replaces slow speed and sure-footed.",
                "69729a2a79e640ffa238bb489d6b2eb0", null, FeatureGroup.Racial, 
                slowSpeedComponents,
                sureFootedComponents
            );

            var feyThought = UniversalRacialTraits.makeFeyThoughts("Halfling", fearlessComponents);

            var caretaker = Utils.CreateFeatureSelection("CaretakerHaflingFeatureSelection", "Caretaker",
                "Humans often entrust halfling families with the care of children and animals, a task that has helped them develop keen insight. Such halflings gain a +2 racial bonus on Perception checks. In addition, when they acquire an animal companion, bonded mount, cohort, or familiar, that creature gains a +2 bonus to one ability score of the character’s choice. ",
                "", null, FeatureGroup.Racial,
                sureFootedComponents,
                halflingLuckComponents,
                weaponFamiliarityComponents,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.SkillPerception, 2, Kingmaker.Enums.ModifierDescriptor.Racial)
                }
            );
            caretaker.ReapplyOnLevelUp = true;
            caretaker.AllFeatures = Human.makeAnimalBonusFeatures("CaretakerHafling", caretaker.Name, caretaker.Description, StatTypeHelper.Attributes);

            RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(halfling, 2, new List<BlueprintFeature>() {
                fleetOfFoot,
                feyThought,
                caretaker
            });
        }
    }
}
