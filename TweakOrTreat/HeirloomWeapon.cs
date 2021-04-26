using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    public class HeirloomWeapon
    {
        static LibraryScriptableObject library => Main.library;

        public static void load()
        {
            //EquipmentTrait	af37d78d7bc5451d943b63356f438949	Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection
            var equipmentTraits = library.Get<BlueprintFeatureSelection>("af37d78d7bc5451d943b63356f438949");
            foreach (var s in equipmentTraits.AllFeatures)
            {
                var prereq = s.GetComponent<ZFavoredClass.NewMechanics.PrerequisiteRace>();
                if(prereq != null)
                {
                    s.ReplaceComponent(
                        prereq,
                        WeaponFamiliarity.raceToFamiliarity[prereq.race].PrerequisiteFeature()
                    );
                }
            }
        }
    }
}

