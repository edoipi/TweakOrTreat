using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class OceansEcho
    {
        static LibraryScriptableObject library => Main.library;
        public static BlueprintArchetype archetype;
        static internal void load()
        {
            var oracle = CallOfTheWild.Oracle.oracle_class;
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "OceansEchoArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Ocean's Echo");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Although many merfolk claim deep connections to both art and the natural world, a rare few merfolk can manipulate the forces of nature and weave them into song. An ocean’s echo is a merfolk gifted with the powers of an oracle and a singing voice that evokes the legendary tales of merfolk virtuosos.");
            });
            Helpers.SetField(archetype, "m_ParentClass", oracle);
            library.AddAsset(archetype, "");

            var inspireCourageFeature = library.Get<BlueprintFeature>("acb4df34b25ca9043a6aba1a4c92bc69");
            var inspireCourageBuff = library.Get<BlueprintBuff>("6d6d9e06b76f5204a8b7856c78607d5d");
            inspireCourageBuff.ReplaceComponent<ContextRankConfig>(c =>
                {
                    var archetypesFeature = Helpers.GetField<BlueprintFeature>(c, "m_Feature");
                    var archetypesComponent = archetypesFeature.GetComponent<ContextRankConfigArchetypeList>();
                    archetypesComponent.archetypes = archetypesComponent.archetypes.AddToArray(archetype);
                    Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(oracle));
                }
            );

            var inspireCompetenceFeature = library.Get<BlueprintFeature>("6d3fcfab6d935754c918eb0e004b5ef7");
            var inspireCompetenceBuff = library.Get<BlueprintBuff>("1fa5f733fa1d77743bf54f5f3da5a6b1");
            inspireCompetenceBuff.ReplaceComponent<ContextRankConfig>(c =>
                {
                    var archetypesFeature = Helpers.GetField<BlueprintFeature>(c, "m_Feature");
                    var archetypesComponent = archetypesFeature.GetComponent<ContextRankConfigArchetypeList>();
                    archetypesComponent.archetypes = archetypesComponent.archetypes.AddToArray(archetype);
                    Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(oracle));
                }
            );
            
            var inspireHeroicsFeature = library.Get<BlueprintFeature>("199d6fa0de149d044a8ab622a542cc79");

            var resource = library.CopyAndAdd<BlueprintFeature>("b92bfc201c6a79e49afd0b5cfbfc269f", "OceansEchoResource", "");
            resource.SetName("Inspiring Song");
            resource.SetDescription("The voice of an ocean’s echo provides inspiration to allies. This ability is identical to bardic performance (using Perform [sing] only), allowing her to inspire courage at 1st level, inspire competence at 3rd level, and inspire heroics at 15th level, as a bard of the ocean’s echo’s level. It is usable a total number of rounds per day equal to her level + her Charisma modifier (minimum 1).");
            resource.ReplaceComponent<IncreaseResourcesByClass>(i => {
                i.CharacterClass = oracle;
                i.BaseValue = 0;
            });
            resource.HideInCharacterSheetAndLevelUp = false;
            resource.HideInUI = false;

            var bonusSpells = new Dictionary<int, BlueprintFeature>();
             
            var spellToConvert = new (int, BlueprintAbility)[] {
                (4, library.Get<BlueprintAbility>("c3893092a333b93499fd0a21845aa265")), //SoundBurst
                (8, library.Get<BlueprintAbility>("f09453607e683784c8fca646eec49162")), //Shout
                (10, library.Get<BlueprintAbility>("d38aaf487e29c3d43a3bffa4a4a55f8f")), //SongOfDiscord
                (12, library.Get<BlueprintAbility>("fd0d3840c48cafb44bb29e8eb74df204")), //ShoutGreater
                (14, CallOfTheWild.NewSpells.song_of_discord_greater), 
            };

            foreach(var (level, spell) in spellToConvert)
            {
                var spellFeature = Helpers.CreateFeature(
                    "OceansEchoBonusSpell" + spell.name,
                    spell.Name,
                    "At 2nd level, and every two levels thereafter, an oracle learns an additional spell derived from her mystery.\n"
                    + spell.Name + ": " + spell.Description,
                    "",
                    spell.Icon,
                    FeatureGroup.None,
                    spell.CreateAddKnownSpell(oracle, level / 2)
                );

                bonusSpells[level] = spellFeature;
            }

            var oceansEchoMysteries = library.CopyAndAdd(CallOfTheWild.Oracle.oracle_mysteries, "OceansEchoOracleMysteries", "");
            oceansEchoMysteries.AllFeatures = new BlueprintFeature[] { };
            
            foreach(var mysteryFeature in CallOfTheWild.Oracle.oracle_mysteries.AllFeatures)
            {
                var mystery = mysteryFeature as BlueprintProgression;
                var oceansEchoMystery = library.CopyAndAdd(mystery, "OceansEcho" + mystery.name, "");
                oceansEchoMystery.LevelEntries = new LevelEntry[mystery.LevelEntries.Length];

                for(int i = 0; i < mystery.LevelEntries.Length; i++)
                {
                    if (bonusSpells.ContainsKey(mystery.LevelEntries[i].Level))
                    {
                        var spell = bonusSpells[mystery.LevelEntries[i].Level];
                        oceansEchoMystery.UIGroups[0].Features.Add(spell);
                        oceansEchoMystery.LevelEntries[i] = Helpers.LevelEntry(mystery.LevelEntries[i].Level, spell);
                    }
                    else
                    {
                        oceansEchoMystery.LevelEntries[i] = mystery.LevelEntries[i];
                    }
                }

                oceansEchoMysteries.AllFeatures = oceansEchoMysteries.AllFeatures.AddToArray(oceansEchoMystery);
                mystery.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.FeatureReplacement>(f => f.replacement_feature = oceansEchoMystery));
            }

            var classSkills = Helpers.CreateFeature(
                "OceansEchoClassSkillsFeature",
                "Class Skills",
                "An ocean’s echo adds Lore (nature) to her list of class skills.",
                "",
                null,
                FeatureGroup.None,
                Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillLoreNature)
            );

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, 
                    CallOfTheWild.Oracle.revelation_selection, 
                    CallOfTheWild.Oracle.oracle_mysteries,
                    CallOfTheWild.Oracle.mystery_skills
                ),
                Helpers.LevelEntry(3, CallOfTheWild.Oracle.revelation_selection),
                Helpers.LevelEntry(15, CallOfTheWild.Oracle.revelation_selection)
            };

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, classSkills, oceansEchoMysteries, inspireCourageFeature, resource),
                Helpers.LevelEntry(3, inspireCompetenceFeature),
                Helpers.LevelEntry(15, inspireHeroicsFeature),
            };
            oracle.Archetypes = oracle.Archetypes.AddToArray(archetype);

            oracle.Progression.UIDeterminatorsGroup = oracle.Progression.UIDeterminatorsGroup.AddToArray(oceansEchoMysteries, classSkills);
            oracle.Progression.UIGroups = oracle.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(inspireCourageFeature, inspireCompetenceFeature, inspireHeroicsFeature)
            );

            var discordantVocieFeature = library.Get<BlueprintFeature>("8064adc641c74e4cb821ce048ecd83a2");
            discordantVocieFeature.AddComponent(Common.createPrerequisiteArchetypeLevel(oracle, archetype, 8, any: true));
        }
    }
}
