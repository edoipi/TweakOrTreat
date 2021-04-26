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
    public class UnbalancingTrick
    {
        static LibraryScriptableObject library => Main.library;
        static public void load()
        {
            var trip = library.Get<BlueprintFeature>("0f15c6f70d8fb2b49aa6cc24239cc5fa");
            var greaterTrip = library.Get<BlueprintFeature>("4cc71ae82bdd85b40b3cfe6697bb7949");

            var classes = new BlueprintCharacterClass[] {
                library.Get<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484"),
                library.Get<BlueprintCharacterClass>("c75e0971973957d4dbad24bc7957e4fb"),
                CallOfTheWild.Investigator.investigator_class
            };

            var slelections = new BlueprintFeatureSelection[] {
                library.Get<BlueprintFeatureSelection>("04430ad24988baa4daa0bcd4f1c7d118"), //slayer 2
                library.Get<BlueprintFeatureSelection>("43d1b15873e926848be2abf0ea3ad9a8"), //slayer 6
                library.Get<BlueprintFeatureSelection>("913b9cf25c9536949b43a2651b7ffb66"), //slayer 10
                library.Get<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93"), //rogue talents
                CallOfTheWild.Archetypes.Ninja.ninja_trick,
                CallOfTheWild.Investigator.investigator_talent_selection,
                CallOfTheWild.Investigator.extra_investigator_talent as BlueprintFeatureSelection
            };

            var replacementFeature = CallOfTheWild.Helpers.CreateFeature(
                "UnbalancingTrickReplacementFeature",
                "Unbalancing Trick and Character Level 6",
                "",
                "",
                trip.Icon,
                FeatureGroup.None
            );
            replacementFeature.HideInCharacterSheetAndLevelUp = true;
            replacementFeature.HideInUI = true;

            var unbalancingTrick = CallOfTheWild.Helpers.CreateFeature(
                "UnbalancingTrickRogueTalentFeature",
                "Unbalancing Trick",
                "The rogue gains Improved Trip as a bonus feat, even if she does not meet the prerequisites. At 6th level, she is treated as if she meets all the prerequisites of Greater Trip (although she must take the feat as normal to gain its benefits).",
                "",
                trip.Icon,
                FeatureGroup.RogueTalent,
                CallOfTheWild.Helpers.CreateAddFact(trip),
                CallOfTheWild.Helpers.CreateAddFeatureOnClassLevel(replacementFeature, 6, classes)
            );

            foreach (var prereq in greaterTrip.GetComponents<Prerequisite>().ToArray())
            {
                greaterTrip.ReplaceComponent(
                    prereq, 
                    CallOfTheWild.Helpers.Create<CallOfTheWild.PrerequisiteMechanics.PrerequsiteOrAlternative>(
                        p => 
                        {
                            p.base_prerequsite = prereq;
                            p.alternative_prerequsite = replacementFeature.PrerequisiteFeature();
                            p.Group = prereq.Group;
                        }
                    )
                );
            }

            foreach (var rt in slelections)
            {
                rt.AllFeatures = rt.AllFeatures.AddToArray(unbalancingTrick);
            }
        }
    }
}
