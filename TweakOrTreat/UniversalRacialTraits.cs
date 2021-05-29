using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class UniversalRacialTraits
    {
        static LibraryScriptableObject library => Main.library;
        static string feyThoughtsName = "Fey Thoughts";
        static string feyThoughtsDesc = "The character sees the world more like a native of the First World. " +
            "Select two of the following skills: Mobility, Bluff, Diplomacy, Stealth, Lore (nature), Perception, Trickery, or Use Magic Device. The selected skills are always class skills for the character.";
        static BlueprintFeatureSelection classSkillsFeatureSelection;

        public static BlueprintFeature makeFeyThoughts(string suffix, BlueprintComponent[] components)
        {
            var feature = Helpers.CreateFeature(
                $"FeyThoughtsBaseFeature{suffix}",
                feyThoughtsName,
                feyThoughtsDesc,
                "",
                null,
                FeatureGroup.None,
                Helpers.Create<CallOfTheWild.EvolutionMechanics.addSelection>(c =>
                {
                    c.selection = classSkillsFeatureSelection;
                }),
                Helpers.Create<CallOfTheWild.EvolutionMechanics.addSelection>(c =>
                {
                    c.selection = classSkillsFeatureSelection;
                })
            );
            feature.AddComponents(components);

            return feature;
        }

        static internal void load()
        {
            var skills = new List<StatType>() {
                StatType.SkillMobility, StatType.CheckBluff, StatType.CheckDiplomacy, StatType.SkillStealth,
                StatType.SkillLoreNature, StatType.SkillPerception, StatType.SkillThievery, StatType.SkillUseMagicDevice
            };

            var otherSkills = new List<StatType>() {
                StatType.CheckBluff, StatType.CheckDiplomacy
            };

            var classSkills = new List<BlueprintFeature>();

            foreach(var skill in skills)
            {
                var feature = Helpers.CreateFeature(
                    $"FeyThoughts{skill}Feature",
                    $"{feyThoughtsName}: {LocalizedTexts.Instance.Stats.GetText(skill)}",
                    feyThoughtsDesc,
                    "",
                    null,
                    FeatureGroup.None
                );

                if(!otherSkills.Contains(skill))
                {
                    feature.AddComponent(Helpers.Create<AddClassSkill>(a => a.Skill = skill));
                }
                else
                {
                    feature.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.AddBonusToSkillCheckIfNoClassSkill>(a => { a.skill = StatType.SkillPersuasion; a.check = skill; }));
                }

                classSkills.Add(feature);
            }

            //classSkillsArray = classSkills.ToArray();

            classSkillsFeatureSelection = Helpers.CreateFeatureSelection(
                "FeyThougthsFeatureSelection",
                feyThoughtsName,
                feyThoughtsDesc,
                "",
                null,
                FeatureGroup.AasimarHeritage
            );
            classSkillsFeatureSelection.AllFeatures = classSkills.ToArray();
        }
    }
}
