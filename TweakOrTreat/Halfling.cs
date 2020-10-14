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
            BlueprintFeature fleetOfFoot = Helpers.CreateFeature("FleetOfFoot", "Fleet of Foot",
                "Some halflings are quicker than their kin but less cautious. Halflings with this racial trait move at normal speed and have a base speed of 30 feet. This racial trait replaces slow speed and sure-footed.",
                "69729a2a79e640ffa238bb489d6b2eb0", null, FeatureGroup.Racial, new BlueprintComponent[]
            {
                Helpers.Create<PrerequisiteFeature>(c =>  c.Feature = library.Get<BlueprintFeature>("09bc9ccb8ee0ffe4b8827066b1ed7e11") /*slow speed*/ ),
                Helpers.Create<PrerequisiteFeature>(c =>  c.Feature = library.Get<BlueprintFeature>("0fe5db70b50cd894c849fc764c80bbb9") /*sure footed*/ ),
                Helpers.Create<RemoveFeatureOnApply>(c => c.Feature = library.Get<BlueprintFeature>("09bc9ccb8ee0ffe4b8827066b1ed7e11") /*slow speed*/ ),
                Helpers.Create<RemoveFeatureOnApply>(c =>  c.Feature = library.Get<BlueprintFeature>("0fe5db70b50cd894c849fc764c80bbb9") /*sure footed*/ ),
            });

            BlueprintRace halfling = library.Get<BlueprintRace>("b0c3ef2729c498f47970bb50fa1acd30");
            RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(halfling, 1, new List<BlueprintFeature>() { fleetOfFoot });
        }
    }
}
