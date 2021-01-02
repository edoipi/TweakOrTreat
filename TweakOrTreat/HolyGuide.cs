using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class HolyGuide
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var paladin = Helpers.GetClass("bfa11238e7ae3544bbeb4d0b92e897ec");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "HolyGuideArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Holy Guide");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A holy guide believes that it’s his sacred calling to clear the roads of bandits between towns as well as to escort travelers to safety. He must enforce the rule of law in the wilderness and help those that cannot defend themselves against the many dangers of the area.");
            });
            Helpers.SetField(archetype, "m_ParentClass", paladin);
            library.AddAsset(archetype, "");

            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = paladin.ClassSkills.AddToArray(StatType.SkillLoreNature);

            var smiteEvilResource = library.Get<BlueprintAbilityResource>("b4274c5bb0bf2ad4190eb7c44859048b");
            var selectionMercy = library.Get<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d");
            var favoredTerrainSelection = library.Get<BlueprintFeatureSelection>("a6ea422d7308c0d428a541562faedefd");
            var favoredTerrainImprove = library.Get<BlueprintFeatureSelection>("efa888832eae4e169f8ae285b0777b43");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(3, selectionMercy),
                Helpers.LevelEntry(6, selectionMercy),
            };

            var holyGuideFavoredTerrain = library.CopyAndAdd(favoredTerrainSelection, "HolyGuideFavoredTerrainSelection", "");
            holyGuideFavoredTerrain.SetDescription("At 3rd level, a holy guide chooses a favored terrain from the ranger favored terrains table. This otherwise functions like the ranger ability of the same name. This ability replaces the mercy gained at 3rd level. Every time a holy guide would be able to select another mercy, he can instead select another favored terrain and increase his bonuses for one existing favored terrain, just like a ranger.");

            var favoredTerrainUpgrade = Utils.CreateFeature("HolyGuideFavoredTerrainUpgrade", holyGuideFavoredTerrain.Name,
                holyGuideFavoredTerrain.Description,
                "", null, FeatureGroup.Racial,
                new BlueprintComponent[]
                {
                    Helpers.Create<CallOfTheWild.EvolutionMechanics.addSelection>( c =>
                    {
                        c.selection = holyGuideFavoredTerrain;
                    }),
                    Helpers.Create<CallOfTheWild.EvolutionMechanics.addSelection>( c =>
                    {
                        c.selection = favoredTerrainImprove;
                    }),
                    Common.createPrerequisiteArchetypeLevel(paladin, archetype, 1)
                }
            );

            selectionMercy.AllFeatures = selectionMercy.AllFeatures.AddToArray(favoredTerrainUpgrade);

            var ability = library.CopyAndAdd<BlueprintAbility>("f1c8ec6179505714083ed9bd47599268", "HolyGuideTactician", ""); //TacticalLeaderFeatShareAbility
            // let's keep "Tactician" name since this arcgetypes ability has no name
            // no evil allies clause as well since I'm feeling lazy
            ability.SetNameDescription("Tactician", " At 6th level, a holy guide gains a teamwork feat as a bonus feat. He must meet the prerequisites for this feat. As a standard action, He can expend one use of smite evil to grant this feat to all allies within 30 feet who can see and hear him. Allies retain the use of this bonus feat for 3 rounds plus 1 round for every 2 levels the holy guide possesses. Allies do not need to meet the prerequisites of this bonus feat.");
            ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", new BlueprintCharacterClass[] { paladin }));
            ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = smiteEvilResource);

            var teamworkFeat = Helpers.CreateFeatureSelection(
                "HolyGuideTeamworkFeatFeatureSelection",
                "Teamwork Feat",
                ability.Description,
                "",
                ability.Icon,
                FeatureGroup.None,
                Helpers.CreateAddFact(ability)
            );

            // get teamwork feats defined to be shared by tactical leader
            var teamworkFeats = library.Get<BlueprintBuff>("a603a90d24a636c41910b3868f434447").GetComponent<CallOfTheWild.TeamworkMechanics.AddFactsFromCasterIfHasBuffs>().facts.Cast<BlueprintFeature>().ToArray();

            // add component creating toggle abilities
            foreach (var tf in teamworkFeats)
            {
                var addComp = tf.GetComponent<AddFeatureIfHasFact>().CreateCopy(a => a.CheckedFact = teamworkFeat);
                tf.AddComponent(addComp);
            }

            // same selection as "TeamworkFeat"
            teamworkFeat.AllFeatures = library.Get<BlueprintFeatureSelection>("d87e2f6a9278ac04caeb0f93eff95fcb").AllFeatures;

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(3, holyGuideFavoredTerrain),
                Helpers.LevelEntry(6, teamworkFeat),
            };
            paladin.Archetypes = paladin.Archetypes.AddToArray(archetype);

            paladin.Progression.UIGroups = paladin.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(selectionMercy, holyGuideFavoredTerrain, teamworkFeat)
            );
            //foreach (var group in paladin.Progression.UIGroups)
            //{
            //    if (group.Features.Contains(selectionMercy))
            //    {
            //        group.Features.Add(holyGuideFavoredTerrain);
            //    }
            //}
        }
    }
}

