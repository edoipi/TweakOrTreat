using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class Mindchemist
    {
        static LibraryScriptableObject library => Main.library;
        public static BlueprintFeatureSelection mindchemistDiscovery;
        static internal void load()
        {
            var alchemist = Helpers.GetClass("0937bec61c0dabc468428f496580c721");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MindchemistArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Mindchemist");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While most alchemists use mutagens to boost their physical ability at the cost of mental ability, some use alchemy for the opposite purpose—to boost the power of the mind and memory. A mindchemist can reach incredible levels of mental acuity, but suffers lingering debilitating effects to his physique.");
            });
            Helpers.SetField(archetype, "m_ParentClass", alchemist);
            library.AddAsset(archetype, "");

            var mutagen = library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea");
            var discoveries = library.Get<BlueprintFeatureSelection>("cd86c437488386f438dcc9ae727ea2a6");
            var poisonResistance = library.Get<BlueprintFeature>("c9022272c87bd66429176ce5c597989c");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, mutagen), //mutagen
                //in place of poison use and to disallow stacking with archetypes that it should not stack with
                Helpers.LevelEntry(2, poisonResistance),
                Helpers.LevelEntry(2, discoveries),
                Helpers.LevelEntry(4, discoveries),
                Helpers.LevelEntry(6, discoveries),
                Helpers.LevelEntry(8, discoveries),
                Helpers.LevelEntry(10, discoveries),
                Helpers.LevelEntry(12, discoveries),
                Helpers.LevelEntry(14, discoveries),
                Helpers.LevelEntry(16, discoveries),
                Helpers.LevelEntry(18, discoveries),
                Helpers.LevelEntry(20, discoveries),
            };

            var perfectRecall = Helpers.CreateFeature(
                "PerfectRecallfeature",
                "Perfect Recall",
                "At 2nd level, a mindchemist has honed his memory. When making a Knowledge check, he may add his Intelligence bonus on the check a second time. Thus, a mindchemist with 5 ranks in Knowledge (world) and a +2 Intelligence bonus has a total skill bonus of +9 (5 + 2 + 2) using this ability.",
                "",
                null,
                FeatureGroup.None,
                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeArcana, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeWorld, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                Helpers.CreateAddContextStatBonus(StatType.SkillLoreNature, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                Helpers.CreateAddContextStatBonus(StatType.SkillLoreReligion, ModifierDescriptor.UntypedStackable, rankType: AbilityRankType.StatBonus),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.StatBonus, stat: StatType.Intelligence, min: 0, type: AbilityRankType.StatBonus),
                Helpers.Create<RecalculateOnStatChange>(r => r.Stat = StatType.Intelligence)
            );

            
            mindchemistDiscovery = library.CopyAndAdd(discoveries, "MindchemistDiscovery", "");
            mindchemistDiscovery.SetName("Mindchemist Discovery");
            mindchemistDiscovery.SetDescription(mindchemistDiscovery.Description + " A mindchemist may select Skill Focus (Trickery), Lore (Religion), Knowledge (Arcana) or Knowledge (World) in place of a discovery.");
            var tickeryFocus = Helpers.GetSkillFocus(StatType.SkillThievery);
            var religionFocus = Helpers.GetSkillFocus(StatType.SkillLoreReligion);
            var arcanaFocus = Helpers.GetSkillFocus(StatType.SkillKnowledgeArcana);
            var worldFocus = Helpers.GetSkillFocus(StatType.SkillKnowledgeWorld);

            mindchemistDiscovery.AllFeatures = mindchemistDiscovery.AllFeatures.AddToArray(mutagen, tickeryFocus, religionFocus, arcanaFocus, worldFocus);

            var cognatogen = library.Get<BlueprintFeature>("e3f460ea61fcc504183c7d6818bbbf7a");
            var mutagenResource = library.Get<BlueprintAbilityResource>("3b163587f010382408142fc8a97852b6");

            var cognatogenWithResource = Helpers.CreateFeature(
                "CognatogenWithResource",
                cognatogen.Name,
                cognatogen.Description,
                "",
                cognatogen.Icon,
                FeatureGroup.None,
                Helpers.CreateAddFact(cognatogen),
                Helpers.CreateAddAbilityResource(mutagenResource)
             );

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, cognatogenWithResource),
                Helpers.LevelEntry(2, perfectRecall),
                Helpers.LevelEntry(2, mindchemistDiscovery),
                Helpers.LevelEntry(4, mindchemistDiscovery),
                Helpers.LevelEntry(6, mindchemistDiscovery),
                Helpers.LevelEntry(8, mindchemistDiscovery),
                Helpers.LevelEntry(10, mindchemistDiscovery),
                Helpers.LevelEntry(12, mindchemistDiscovery),
                Helpers.LevelEntry(14, mindchemistDiscovery),
                Helpers.LevelEntry(16, mindchemistDiscovery),
                Helpers.LevelEntry(18, mindchemistDiscovery),
                Helpers.LevelEntry(20, mindchemistDiscovery),               
            };
            alchemist.Archetypes = alchemist.Archetypes.AddToArray(archetype);
            //var persistantMutagen = library.Get<BlueprintFeature>("75ba281feb2b96547a3bfb12ecaff052");
            var grandDiscoverySelection = library.Get<BlueprintFeature>("2729af328ab46274394cedc3582d6e98");
            //alchemist.Progression.UIGroups = alchemist.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(cognatogenWithResource, persistantMutagen, grandDiscoverySelection));
            foreach(var group in alchemist.Progression.UIGroups)
            {
                if(group.Features.Contains(grandDiscoverySelection))
                {
                    group.Features.Add(cognatogenWithResource);
                }
            }
        }
    }
}
