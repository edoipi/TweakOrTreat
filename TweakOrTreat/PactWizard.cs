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
    class PactWizard
    {
        static LibraryScriptableObject library => Main.library;
        public static BlueprintFeatureSelection mindchemistDiscovery;
        static internal void load()
        {
            var wizard = Helpers.GetClass("ba34257984f4c41408ce1dc2004e342e");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "PactWizardArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Pact Wizard");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While the art of wizardry is usually a scholar’s pursuit, there are those who seek mastery of arcane power without tedious study and monotonous research. Motivated by foolish ambition, such individuals turn to the greatest enigmas of the cosmos in the hopes of attaining greater power. Though few successfully attract the attention of these forces, those who do receive phenomenal arcane power for their efforts, but become the dutiful playthings and servants of the forces with which they consort.");
            });
            Helpers.SetField(archetype, "m_ParentClass", wizard);
            library.AddAsset(archetype, "");

            var wizardFeat = library.Get<BlueprintFeature>("8c3102c2ff3b69444b139a98521a4899");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, wizardFeat),
                Helpers.LevelEntry(5, wizardFeat),
                Helpers.LevelEntry(10, wizardFeat),
                Helpers.LevelEntry(15, wizardFeat),
                Helpers.LevelEntry(20, wizardFeat),
            };

            var patron = Helpers.CreateFeatureSelection(
                "PactWizardPatronSelection",
                "Patron Spells",
                "At 1st level, a pact wizard must select a patron. This functions like the witch class ability of the same name, except the pact wizard automatically adds his patron’s spells to his spellbook instead of to his familiar. In addition, the pact wizard can expend any prepared spell in order to spontaneously cast one of his patron’s spells of the same level or lower.",
                "",
                CallOfTheWild.Witch.witch_patrons.Icon,
                FeatureGroup.None
            );

            
            archetype.AddFeatures = new LevelEntry[] {
                //Helpers.LevelEntry(1, cognatogenWithResource),
            };
            wizard.Archetypes = wizard.Archetypes.AddToArray(archetype);
            //var persistantMutagen = library.Get<BlueprintFeature>("75ba281feb2b96547a3bfb12ecaff052");
            //alchemist.Progression.UIGroups = alchemist.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(cognatogenWithResource, persistantMutagen, grandDiscoverySelection));
            //foreach (var group in alchemist.Progression.UIGroups)
            //{
            //    if (group.Features.Contains(grandDiscoverySelection))
            //    {
            //        group.Features.Add(cognatogenWithResource);
            //    }
            //}
        }
    }
}

