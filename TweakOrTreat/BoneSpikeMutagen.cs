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
    class BoneSpikeMutagen
    {
        static LibraryScriptableObject library => Main.library;
        static public void load()
        {
            var alchemist = library.Get<BlueprintCharacterClass>("0937bec61c0dabc468428f496580c721");
            var discovery = library.Get<BlueprintFeatureSelection>("cd86c437488386f438dcc9ae727ea2a6");
            var medicalDiscovery = library.Get<BlueprintFeatureSelection>("67f499218a0e22944abab6fe1c9eaeee");
            var selections = new List<BlueprintFeatureSelection>() {
                discovery, medicalDiscovery, Mindchemist.mindchemistDiscovery
            };

            var boneSpike = MasterChymist.createFeatureBonusDuringMutagen(
                "BoneSpikeMutagen",
                "Bone-Spike Mutagen",
                "When the alchemist imbibes a mutagen, he mutates his skeletal structure, causing the bones on his elbows, knuckles, spine, and shoulder blades to grow massive and pierce his skin, exposing themselves as large spikes. While the mutagen is in effect, the alchemist’s natural armor bonus granted by the mutagen increases by 2.",
                new BlueprintComponent[] {
                    Helpers.PrerequisiteClassLevel(alchemist, 6),
                    Helpers.CreateAddStatBonus(Kingmaker.EntitySystem.Stats.StatType.AC, 2, Kingmaker.Enums.ModifierDescriptor.NaturalArmor)
                }
            );

            foreach(var s in selections)
            {
                s.AllFeatures = s.AllFeatures.AddToArray(boneSpike);
            }
        }
    }
}
