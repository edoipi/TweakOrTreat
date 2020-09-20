using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class ExtraDiscovery
    {
        static LibraryScriptableObject library = Main.library;

        static internal void makeExtraDicovery(BlueprintFeatureSelection discovery)
        {
            var extraDiscovery = library.CopyAndAdd(discovery, "Extra"+discovery.name, "");
            extraDiscovery.Group = FeatureGroup.Feat;
            extraDiscovery.SetName("Extra "+extraDiscovery.Name);
            extraDiscovery.SetDescription("You gain one additional discovery. You must meet all of the prerequisites for this discovery.");
            extraDiscovery.AddComponent(Helpers.PrerequisiteFeature(discovery));

            library.AddFeats(extraDiscovery);
        }

        static internal void load()
        {
            var discovery = library.Get<BlueprintFeatureSelection>("cd86c437488386f438dcc9ae727ea2a6");
            var medicalDiscovery = library.Get<BlueprintFeatureSelection>("67f499218a0e22944abab6fe1c9eaeee");

            makeExtraDicovery(discovery);
            makeExtraDicovery(medicalDiscovery);
            makeExtraDicovery(Mindchemist.mindchemistDiscovery);
        }
    }
}
