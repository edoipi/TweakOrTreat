using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class KineticEnhancement
    {
        static LibraryScriptableObject library => Main.library;
        static public void load()
        {
            var kineticEnancment = library.CopyAndAdd(CallOfTheWild.WizardDiscoveries.knowledge_is_power, "KineticEnhancementPsiTechFeature", "");
            kineticEnancment.SetNameDescription(
                "Kinetic Enhancement",
                "You can generate kinetic energy to aid yourself in close-quarters scrapes. You add your Intelligence modifier on combat maneuver checks and to your CMD. You also add your Intelligence modifier on all Strength checks."
            );
            kineticEnancment.RemoveComponents<PrerequisiteClassLevel>();

            var ampSelection = Helpers.CreateFeatureSelection(
                "PsiTechSelection",
                "Psi-Tech Discoveries",
                "A psychic can learn a psi-tech discovery in place of a phrenic amplification or a feat.",
                "",
                null,
                FeatureGroup.None
            );
            ampSelection.AllFeatures = new BlueprintFeature[] { kineticEnancment };

            var featSelection = library.CopyAndAdd(ampSelection, "PsiTechFeatSelection", "");
            featSelection.AddComponent(Helpers.PrerequisiteClassLevel(CallOfTheWild.Psychic.psychic_class, 1));
            library.AddFeats(featSelection);

            var amps = new List<BlueprintFeatureSelection>() {
                CallOfTheWild.Psychic.phrenic_amplification,
                CallOfTheWild.Psychic.extra_phrenic_amplification,
                library.Get<BlueprintFeatureSelection>("b9ac4b0f1326464e8c3161f41b0743e3"), //detective extra
                CallOfTheWild.Investigator.phrenic_dabbler
            };

            foreach(var a in amps)
            {
                a.AllFeatures = a.AllFeatures.AddToArray(ampSelection);
            }
        }
    }
}
