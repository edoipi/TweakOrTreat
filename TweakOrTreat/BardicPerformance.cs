using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class BardicPerformance
    {
        static LibraryScriptableObject library => Main.library;
        static void addFastPerfromance(BlueprintArchetype archetype, String replacement)
        {
            BlueprintFeature moveAction = library.Get<BlueprintFeature>("36931765983e96d4bb07ce7844cd897e");
            BlueprintFeature swiftAction = library.Get<BlueprintFeature>("fd4ec50bc895a614194df6b9232004b9");

            var newMoveAction = library.CopyAndAdd(moveAction, archetype.name + moveAction.name, "");
            var newSwiftAction = library.CopyAndAdd(swiftAction, archetype.name + swiftAction.name, "");
            newMoveAction.SetDescription(newMoveAction.Description.Replace("a bard ", replacement));
            newSwiftAction.SetDescription(newSwiftAction.Description.Replace("a bard ", replacement));

            archetype.AddFeatures = archetype.AddFeatures.AddToArray(Helpers.LevelEntry(7, newMoveAction));
            archetype.AddFeatures = archetype.AddFeatures.AddToArray(Helpers.LevelEntry(13, newSwiftAction));

            archetype.GetParentClass().Progression.UIGroups = archetype.GetParentClass().Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(newMoveAction, newSwiftAction));
        }

        static internal void load()
        {
            BlueprintArchetype sensei = library.Get<BlueprintArchetype>("f8767821ec805bf479706392fcc3394c");
            BlueprintArchetype evangelist = CallOfTheWild.Archetypes.Evangelist.archetype;
            var oceansEcho = OceansEcho.archetype;

            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var monk = library.Get<BlueprintCharacterClass>("e8f21e5b58e0569468e420ebea456124");

            addFastPerfromance(sensei, "a sensei ");
            addFastPerfromance(evangelist, "an evangelist ");
            addFastPerfromance(oceansEcho, "an ocean’s echo ");
        }
    }
}
