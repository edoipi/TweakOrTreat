using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    //[HarmonyLib.HarmonyPatch(typeof(SpellSelectionData), "CanSelectAnything")]
    //class Spellbook_Patch
    //{
    //    static bool Prepare()
    //    {
    //        return true;
    //    }

    //    private static bool SpellbookContainsSpell(Spellbook spellbook, int ii, BlueprintAbility sb)
    //    {
    //        return spellbook.GetKnownSpells(ii).FirstOrDefault((AbilityData a) => a.Blueprint == sb) != null;
    //    }

    //    static void Postfix(SpellSelectionData __instance, UnitDescriptor unit, bool __result)
    //    {
    //        if (__result == true)
    //            return;
    //        Main.logger.Log("Was false");
    //        Spellbook spellbook = unit.Spellbooks.FirstOrDefault((Spellbook s) => s.Blueprint == __instance.Spellbook);
    //        if (spellbook == null)
    //        {
    //            return;
    //        }
    //        Main.logger.Log("Sepllbook aint null");
    //        if (__instance.ExtraSelected != null && __instance.ExtraSelected.Length != 0)
    //        {
    //            Main.logger.Log("Something extra to select");
    //            if (__instance.ExtraSelected.HasItem((BlueprintAbility i) => i == null))
    //            {
    //                Main.logger.Log("dunno what it is");
    //                if (__instance.ExtraMaxLevel < 1)
    //                    return;
    //                Main.logger.Log("Max level is at least 1");
    //                if (__instance.SpellList.SpellsByLevel[1].SpellsFiltered.HasItem((BlueprintAbility sb) => !SpellbookContainsSpell(spellbook, 1, sb)))
    //                {
    //                    __result = true;
    //                    return;
    //                }
    //                Main.logger.Log("did not change result");
    //            }
    //        }
    //    }
    //}

    [HarmonyLib.HarmonyPatch(typeof(ApplySpellbook), "Apply")]
    class ApplySpellbook_Apply_Patch
    {
        static bool Prepare()
        {
            return true;
        }

        static void Postfix(LevelUpState state, UnitDescriptor unit)
        {
            //state.DemandSpellSelection(CallOfTheWild.Oracle.oracle_class.Spellbook, CallOfTheWild.Witch.witch_class.Spellbook.SpellList).SetExtraSpells(1, 1);\
            //Main.logger.Log("To learn length: " + unit.Ensure<LearnSpellUnitPart>().toLearn.Count);
            foreach(var learn in unit.Ensure<LearnSpellUnitPart>().toLearn)
            {
                //Main.logger.Log($"Leraning count: {learn.count}, max level: {learn.maxSpellLevel}");
                state.DemandSpellSelection(learn.spellBook, learn.spellList).SetExtraSpells(learn.count, learn.maxSpellLevel);
            }
            unit.Ensure<LearnSpellUnitPart>().toLearn.Clear();
        }
    }

    public class LearnSpellUnitPart : UnitPart {
        //[JsonProperty]
        public List<LearnSpells> toLearn = new List<LearnSpells>();
    }

    public class LearnSpells : OwnedGameLogicComponent<UnitDescriptor>
    {
        [JsonProperty]
        bool applied;
        public BlueprintSpellList spellList;
        public int maxSpellLevel;
        public BlueprintSpellbook spellBook;
        public int count;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
            //applied = true;
            //Owner.Ensure<LearnSpellUnitPart>().toLearn.Remove(this);
        }

        public override void OnFactActivate()
        {
            //if (applied)
            //    return;
            var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
            if (Owner == levelUp.Preview || Owner == levelUp.Unit)
            {
                Owner.Ensure<LearnSpellUnitPart>().toLearn.Add(this);
            }
        }
    }

    //public class SpellWithMaxLevelWithAdjustment : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    //{
    //    [JsonProperty]
    //    bool applied;
    //    public BlueprintSpellList spellList;
    //    public int maxSpellLevel;
    //    public BlueprintSpellbook spellBook;
    //    public int count;
    //    //public int adjustment;

    //    public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
    //    {
    //    }

    //    public override void OnFactActivate()
    //    {
    //        try
    //        {
    //            var levelUp = Game.Instance.UI.CharacterBuildController?.LevelUpController;
    //            if (Owner == levelUp?.Preview || Owner == levelUp?.Unit)
    //            {
    //                //var spellSelection = 
    //                Main.logger.Log("Selections num A: "+ levelUp.State.SpellSelections.Count);
    //                var spellSelection = levelUp.State.DemandSpellSelection(spellBook, spellList);
    //                Main.logger.Log("Selections num B: " + levelUp.State.SpellSelections.Count);
    //                int existingNewSpells = spellSelection.LevelCount[maxSpellLevel]?.SpellSelections.Length ?? 0;
    //                spellSelection.SetExtraSpells(1, maxSpellLevel);
    //                Main.logger.Log("Selections num C: " + levelUp.State.SpellSelections.Count);
    //                //Game.Instance.UI.CharacterBuildController.Spells.RefreshSpellCollection(levelUp.State.SpellSelections);
    //                //Game.Instance.UI.CharacterBuildController.Spells.OnChangeCurrentCollection();
    //                //Game.Instance.UI.CharacterBuildController.Spells.SetupSelector();
    //                Main.logger.Log("Selections num D: " + levelUp.State.SpellSelections.Count);
    //                applied = true;
    //            }

    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error(e);
    //        }
    //    }
    //}
    class AncientLorekeeper
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var oracle = CallOfTheWild.Oracle.oracle_class;
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "AncientLorekeeperArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Ancient Lorekeeper");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The ancient lorekeeper is a repository for all the beliefs and vast knowledge of an elven people. She shows a strong interest in and understanding of histories and creation legends at a young age, and as she matures her calling to serve as the memory of her long-lived people becomes clear to all who know her. An ancient lorekeeper has the following class features.");
            });
            Helpers.SetField(archetype, "m_ParentClass", oracle);
            library.AddAsset(archetype, "");

            

            var lorekeeperMysteries = library.CopyAndAdd(CallOfTheWild.Oracle.oracle_mysteries, "AncientLorekeeperMysteries", "");
            lorekeeperMysteries.AllFeatures = new BlueprintFeature[] { };

            foreach (var mysteryFeature in CallOfTheWild.Oracle.oracle_mysteries.AllFeatures)
            {
                var mystery = mysteryFeature as BlueprintProgression;
                var oceansEchoMystery = library.CopyAndAdd(mystery, "AncientLorekeeper" + mystery.name, "");
                oceansEchoMystery.LevelEntries = new LevelEntry[1];

                for (int i = 0; i < mystery.LevelEntries.Length; i++)
                {
                    if (mystery.LevelEntries[i].Level == 20)
                    {
                        oceansEchoMystery.LevelEntries[0] = mystery.LevelEntries[i];
                    }
                }

                lorekeeperMysteries.AllFeatures = lorekeeperMysteries.AllFeatures.AddToArray(oceansEchoMystery);
                mystery.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.FeatureReplacement>(f => f.replacement_feature = oceansEchoMystery));
            }

            var classSkills = Helpers.CreateFeature(
                "AncientLorekeeperClassSkillsFeature",
                "Class Skills",
                " An ancient lorekeeper adds Knowledge (arcane) and Knowledge (world) to her list of class skills.",
                "",
                null,
                FeatureGroup.None,
                Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillKnowledgeArcana),
                Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillKnowledgeWorld)
            );

            var newWizardSpellList = library.CopyAndAdd<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89", "LevelAdjustedWizardSpellList", "");
            var newSpellsByLevel = new SpellLevelList[newWizardSpellList.SpellsByLevel.Length+1];
            for (int i = 0; i < newWizardSpellList.SpellsByLevel.Length; i++)
            {
                newSpellsByLevel[i+1] = new SpellLevelList(newWizardSpellList.SpellsByLevel[i].SpellLevel + 1);
                newSpellsByLevel[i+1].SpellLevel = newWizardSpellList.SpellsByLevel[i].SpellLevel + 1;
                newSpellsByLevel[i+1].Spells = newWizardSpellList.SpellsByLevel[i].Spells;
            }

            var notCantrips = new List<BlueprintAbility>();

            foreach(var cantrip in newSpellsByLevel[1].Spells)
            {
                var notCantrip = library.CopyAndAdd(cantrip, cantrip.name+"NotCantrip", "");
                notCantrip.RemoveComponents<CantripComponent>();
                notCantrips.Add(notCantrip);
            }
            newSpellsByLevel[1].Spells = notCantrips;

            newSpellsByLevel[0] = newWizardSpellList.SpellsByLevel[0];
            newWizardSpellList.SpellsByLevel = newSpellsByLevel;

            var elvenArcana = new Dictionary<int, BlueprintFeature>();
            var arcanaDesc = "At 2nd level, an ancient lorekeeper’s mastery of elven legends and philosophy has allowed her to master one spell used by elven wizards. She selects one spell from the sorcerer/wizard spell list that is at least one level lower than the highest-level oracle spell she can cast. The ancient lorekeeper gains this as a bonus spell known. The spell is treated as one level higher than its true level for all purposes. The ancient lorekeeper may choose an additional spell at 4th, 6th, 8th, 10th, 12th, 14th, 16th, and 18th levels. This ability replaces the bonus spells she would normally gain at these levels from her chosen mystery.";
            var arcanaIcon = library.Get<BlueprintFeature>("55edf82380a1c8540af6c6037d34f322").Icon;
            for (int level = 2, i = 1; level <= 18; level += 2, i++)
            {
                elvenArcana[level] = Helpers.CreateFeature(
                    "ElvenArcana" + level,
                    "Elven Arcana",
                    arcanaDesc,
                    "",
                    arcanaIcon,
                    FeatureGroup.None,
                    Helpers.Create<LearnSpells>(
                        s =>
                        {
                            s.spellBook = oracle.Spellbook;
                            s.spellList = newWizardSpellList;
                            s.count = 1;
                            s.maxSpellLevel = i;
                        }
                    )
                );
            }

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1,
                    CallOfTheWild.Oracle.oracle_mysteries,
                    CallOfTheWild.Oracle.mystery_skills
                ),
            };
            

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, classSkills, lorekeeperMysteries),
            };
            oracle.Archetypes = oracle.Archetypes.AddToArray(archetype);

            foreach (var entry in elvenArcana)
            {
                archetype.AddFeatures = archetype.AddFeatures.AddToArray(
                    Helpers.LevelEntry(entry.Key, entry.Value)
                );
            }

            oracle.Progression.UIDeterminatorsGroup = oracle.Progression.UIDeterminatorsGroup.AddToArray(lorekeeperMysteries, classSkills);
            //oracle.Progression.UIGroups = oracle.Progression.UIGroups.AddToArray(
            //    Helpers.CreateUIGroup(inspireCourageFeature, inspireCompetenceFeature, inspireHeroicsFeature)
            //);
            
        }
    }
}

