using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    public class MasterChymist
    {
        static LibraryScriptableObject library => Main.library;

        public class PrerequisiteExtractsLevel : Prerequisite
        {
            // Token: 0x06009BE3 RID: 39907 RVA: 0x0027D6A8 File Offset: 0x0027B8A8
            public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
            {
                foreach (ClassData classData in unit.Progression.Classes)
                {
                    BlueprintSpellbook spellbook = classData.Spellbook;
                    if (spellbook != null && spellbook.IsAlchemist && unit.DemandSpellbook(classData.CharacterClass).MaxSpellLevel >= this.RequiredSpellLevel)
                    {
                        return true;
                    }
                }
                return false;
            }

            public static string AddOrdinal(int num)
            {
                if (num <= 0) return num.ToString();

                switch (num % 100)
                {
                    case 11:
                    case 12:
                    case 13:
                        return num + "th";
                }

                switch (num % 10)
                {
                    case 1:
                        return num + "st";
                    case 2:
                        return num + "nd";
                    case 3:
                        return num + "rd";
                    default:
                        return num + "th";
                }
            }

            // Token: 0x06009BE4 RID: 39908 RVA: 0x0027D758 File Offset: 0x0027B958
            public override string GetUIText()
            {
                StringBuilder stringBuilder = new StringBuilder();
                
                stringBuilder.Append($"Ability to create {AddOrdinal(this.RequiredSpellLevel)} level extracts");

                //stringBuilder.Append(" ");
                //stringBuilder.Append(UIStrings.Instance.Tooltips.SpellsOfLevel[this.RequiredSpellLevel]);
                return stringBuilder.ToString();
            }

            // Token: 0x04006B13 RID: 27411
            public int RequiredSpellLevel;
        }

        public class BrutalityDamageBonus : RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
        {
            public WeaponCategory[] weaponCategories;
            public ContextValue Value;

            public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
            {
                int num = Value.Calculate(this.Fact.MaybeContext);
                var weaponType = evt.Weapon.Blueprint.Type;

                if (weaponType.IsNatural || weaponCategories.Contains(weaponType.Category))
                {
                    evt.AddBonusDamage(num);
                }
            }

            public override void OnEventDidTrigger(RuleCalculateWeaponStats evt)
            {
            }
        }

        public static void makeAlchProgressions()
        {
            var alchemist = library.Get<BlueprintCharacterClass>("0937bec61c0dabc468428f496580c721");
            var alchProgression = library.Get<BlueprintProgression>("efd55ff9be2fda34981f5b9c83afe4f1");
            var bombs = library.Get<BlueprintFeature>("c59b2f256f5a70a4d896568658315b7d");

            var bombProgression = Helpers.CreateProgression("BombProgression",
                                                               bombs.Name,
                                                               bombs.Description,
                                                               "",
                                                               bombs.Icon,
                                                               FeatureGroup.None);

            //bombProgression.LevelEntries = new LevelEntry[] { };
            bombProgression.Classes = new BlueprintCharacterClass[] { alchemist };
            bombProgression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { };
            bombProgression.UIGroups = new UIGroup[] {  };

            var entries = new List<LevelEntry>();
            foreach(var e in alchProgression.LevelEntries)
            {
                if(e.Features.Contains(bombs))
                {
                    entries.Add(
                        Helpers.LevelEntry(e.Level, bombs)
                    );
                }
            }
            bombProgression.LevelEntries = entries.ToArray();

            alchProgression.LevelEntries[0].Features.Add(bombProgression);
        }

        public static BlueprintCharacterClass chymist;

        public static BlueprintFeatureSelection makeProgressionSelection()
        {
            var bombs = library.Get<BlueprintFeature>("c59b2f256f5a70a4d896568658315b7d");
            var bombProgression = Helpers.CreateProgression("ChymistBombProgression",
                                                               bombs.Name,
                                                               bombs.Description,
                                                               "",
                                                               bombs.Icon,
                                                               FeatureGroup.None);

            bombProgression.Classes = new BlueprintCharacterClass[] { chymist };
            bombProgression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { };
            bombProgression.UIGroups = new UIGroup[] { };
            {
                var entries = new List<LevelEntry>();
                for (int i = 1; i <= 10; i += 2)
                {
                    entries.Add(
                        Helpers.LevelEntry(i, bombs)
                    );
                }
                bombProgression.LevelEntries = entries.ToArray();
            }
            
            bombProgression.AddComponent(bombs.PrerequisiteFeature());

            var sneak = library.Get<BlueprintFeature>("9b9eac6709e1c084cb18c3a366e0ec87");
            var sneakProgression = Helpers.CreateProgression("ChymistSneakProgression",
                                                               sneak.Name,
                                                               sneak.Description,
                                                               "",
                                                               sneak.Icon,
                                                               FeatureGroup.None);

            sneakProgression.Classes = new BlueprintCharacterClass[] { chymist };
            sneakProgression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { };
            sneakProgression.UIGroups = new UIGroup[] { };

            {
                var entries = new List<LevelEntry>();
                for (int i = 1; i <= 10; i += 2)
                {
                    entries.Add(
                        Helpers.LevelEntry(i, sneak)
                    );
                }
                sneakProgression.LevelEntries = entries.ToArray();
            }

            sneakProgression.AddComponent(bombs.PrerequisiteNoFeature());

            var featureSelection = Helpers.CreateFeatureSelection(
                "MasterChymistProgressionSelection",
                "Bomb-Thrower",
                "The destructive power of bombs appeals to the violent urges of a master chymist. Levels in master chymist progress alchemist's bomb damage. If she did not have bomb abilty before she can advance sneak atack damage at the same rate as vivisectionist.",
                "",
                bombs.Icon,
                FeatureGroup.None);
            featureSelection.AllFeatures = new BlueprintFeature[] { bombProgression, sneakProgression };

            return featureSelection;
        }

        public static BlueprintFeature createFeatureBonusDuringMutagen(
            string name,
            string displayName,
            string description,
            BlueprintComponent[] components,
            string guid = "",
            UnityEngine.Sprite icon = null,
            FeatureGroup featureGroup = FeatureGroup.None
        )
        {
            var feature = Helpers.CreateFeature(
                name,
                displayName,
                description,
                guid,
                icon,
                featureGroup
            );

            var buff = Helpers.CreateBuff(
                $"{name}Buff",
                feature.Name,
                feature.Description,
                "",
                feature.Icon,
                null,
                components
            );
            buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var mutagens = new BlueprintFeature[]
            {
                library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea"), //mutagen
                library.Get<BlueprintFeature>("76c61966afdd82048911f3d63c6fe0bc"), //greater mutagen
                library.Get<BlueprintFeature>("6f5cb651e26bd97428523061b07ffc85"), //grand mutagen

                library.Get<BlueprintFeature>("e3f460ea61fcc504183c7d6818bbbf7a"), //cognatogen
                library.Get<BlueprintFeature>("18eb29676492e844eb5a55d1c855ce69"), //greater cognatogen
                library.Get<BlueprintFeature>("af4a320648eb5724889d6ff6255090b2"), //grand cognatogen
            };

            foreach (var m in mutagens)
            {
                var mutagenComponents = m.GetComponent<AddFacts>();

                foreach (var f in mutagenComponents.Facts)
                {

                    var mutagenBuff = Common.extractActions<ContextActionApplyBuff>((f as BlueprintAbility).GetComponent<AbilityEffectRunAction>().Actions.Actions)[0].Buff;
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuff(mutagenBuff, buff, feature);
                }
            }

            return feature;
        }

        public class EffectiveAlchemistLevelPrerequisite : Prerequisite
        {
            public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
            {
                int sum = 0;
                foreach(var c in classes)
                {
                    sum += Utils.GetClassLevel(unit, c);
                }

                foreach(var a in archetypes)
                {
                    sum += Utils.GetArchetypeLevel(unit, a.m_ParentClass, a);
                }

                return sum >= this.Level;
            }

            public override string GetUIText()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(
                    string.Format($"Effective alchemist level {Level}"));
                return stringBuilder.ToString();
            }

            public BlueprintCharacterClass[] classes;
            public BlueprintArchetype[] archetypes;

            public int Level = 1;

            public static Func<int, EffectiveAlchemistLevelPrerequisite> getBuilder(
                BlueprintCharacterClass[] classes,
                BlueprintArchetype[] archetypes)
            {
                return (level) => {
                    var prereq = Helpers.Create<EffectiveAlchemistLevelPrerequisite>();
                    prereq.classes = classes;
                    prereq.archetypes = archetypes;
                    prereq.Level = level;

                    return prereq;
                };
            }
        }

        public static void load()
        {
            //makeAlchProgressions();

            var ranger = library.Get<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var fighter = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var alchemist = library.Get<BlueprintCharacterClass>("0937bec61c0dabc468428f496580c721");

            var savesPrestigeLow = library.Get<BlueprintStatProgression>("dc5257e1100ad0d48b8f3b9798421c72");
            var savesPrestigeHigh = library.Get<BlueprintStatProgression>("1f309006cd2855e4e91a6c3707f3f700");

            chymist = Helpers.Create<BlueprintCharacterClass>();
            chymist.name = "MasterChymistClass";
            library.AddAsset(chymist, "");

            chymist.LocalizedName = Helpers.CreateString("MasterChymist.Name", "Master Chymist");
            chymist.LocalizedDescription = Helpers.CreateString("MasterChymist.Description",
                "When alchemists blithely use mutagens to turn themselves into hulking creatures of muscle and reflex, civilized folk often turn their heads and mutter that such transformations must have a price. For a few alchemists, that price is transformation into a master chymist, a creature able to take a monstrous brute form as an act of will. Master chymists become two personalities sharing a single body.Both the hulking “mutagenic form” of alchemical prowess and the original alchemist who created it think of themselves as the true form, and they must learn to work together to achieve their joint goals.More often than not, master chymists eventually become their mutagenic form, and the original alchemist's body and mind may only be brought forth when required by social custom or a need for obscurity and stealth arises. Unfortunately, the mutagenic form of a master chymist is often a more violent, unforgiving personality."
            );
            chymist.m_Icon = alchemist.Icon;
            chymist.SkillPoints = fighter.SkillPoints;
            chymist.HitDie = DiceType.D10;
            chymist.BaseAttackBonus = fighter.BaseAttackBonus;
            chymist.FortitudeSave = savesPrestigeHigh;
            chymist.ReflexSave = savesPrestigeHigh;
            chymist.WillSave = savesPrestigeLow;
            chymist.ClassSkills = new StatType[] { StatType.SkillAthletics, StatType.SkillMobility, StatType.SkillPerception, StatType.SkillKnowledgeWorld, StatType.SkillStealth, StatType.SkillPersuasion };
            chymist.IsDivineCaster = false;
            chymist.IsArcaneCaster = false;
            chymist.PrestigeClass = true;
            chymist.StartingGold = alchemist.StartingGold;
            chymist.PrimaryColor = alchemist.PrimaryColor;
            chymist.SecondaryColor = alchemist.SecondaryColor;
            chymist.RecommendedAttributes = new StatType[] { StatType.Intelligence, StatType.Strength };
            chymist.EquipmentEntities = alchemist.EquipmentEntities;
            chymist.MaleEquipmentEntities = alchemist.MaleEquipmentEntities;
            chymist.FemaleEquipmentEntities = alchemist.FemaleEquipmentEntities;
            chymist.StartingItems = alchemist.StartingItems;

            chymist.ComponentsArray = alchemist.ComponentsArray;

            chymist.AddComponent(Helpers.Create<SkipLevelsForSpellProgression>(s => s.Levels = new int[] { 1, 4, 8 }));
            chymist.AddComponent(Helpers.Create<PrerequisiteExtractsLevel>(p => p.RequiredSpellLevel = 3));

            var feralFeture = library.Get<BlueprintFeature>("fd5f7b37ab4301c48a88cc196ee5f0ce");
            chymist.AddComponent(feralFeture.PrerequisiteFeature());
            var mutagen = library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea");

            //hinterlander_class.AddComponent(Helpers.Create<CallOfTheWild.SpellbookMechanics.PrerequisiteDivineCasterTypeSpellLevel>(p => { p.RequiredSpellLevel = 3; p.Group = Prerequisite.GroupType.All; }));
            chymist.AddComponent(mutagen.PrerequisiteFeature());
            chymist.Progression = Helpers.CreateProgression("MasterChymistProgression",
                                                               chymist.Name,
                                                               chymist.Description,
                                                               "",
                                                               chymist.Icon,
                                                               FeatureGroup.None);

            var spellbookSelection = Helpers.CreateFeatureSelection("MasterChymistSpellbookSelection",
                                                                 "Master Chymist Spellbook Selection",
                                                                 "At every level except 1, 4 and 8, a master chymist gains new extracts per day as if she had also gained a level in alchemist. She does not, however, gain other benefits a character of that class would have gained, except for extracts per day and an increased effective caster level for extracts.",
                                                                 "",
                                                                 null,
                                                                 FeatureGroup.EldritchKnightSpellbook);
            spellbookSelection.Obligatory = true;
            Common.addSpellbooksToSpellSelection2("MasterChymist", 3, spellbookSelection, alchemist: true, arcane: false, divine: false, psychic: false);

            var progressionSelection = makeProgressionSelection();
            var simpleWeaponProf = library.Get<BlueprintFeature>("e70ecf1ed95ca2f40b754f1adb22bbdd");
            //Main.logger.Log($"length {simpleWeaponProf.GetComponent<AddProficiencies>().WeaponProficiencies.Length}");
            //Main.logger.Log($"count {new HashSet<WeaponCategory>(simpleWeaponProf.GetComponent<AddProficiencies>().WeaponProficiencies).Count}");
            var simpleWeapons = simpleWeaponProf.GetComponent<AddProficiencies>().WeaponProficiencies;
            //foreach(var w in simpleWeapons)
            //{
            //    Main.logger.Log($"{w}");
            //}
            var brutality = Helpers.CreateFeature(
                "ChymistBrutalityFeature",
                "Brutality",
                "At 3rd level, a master chymist's taste for violence leads her to strike more powerful blows with weapons easily mastered by her bestial mind. At 3rd level, a chymist in her mutagenic form deals +2 damage when attacking with simple weapons and natural attacks. This bonus increases to +4 at 7th level and to +6 at 9th level.",
                "",
                feralFeture.Icon,
                FeatureGroup.None,
                Helpers.Create<BrutalityDamageBonus>(
                    b =>
                    {
                        b.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                        b.weaponCategories = simpleWeapons;
                    }
                )
            );
            brutality.Ranks = 3;
            brutality.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureListRanks, featureList: new BlueprintFeature[] { brutality, brutality }));


            var advancedMutagen = Helpers.CreateFeatureSelection(
                "AdvancedMutagenSelection",
                "Advanced Mutagen",
                "At 2nd level, the mutagenic form of the master chymist continues to evolve and develop as she grows in power. The master chymist selects an advanced mutagen, a power that changes how her mutagen form works or can only be accessed in her mutagenic form. She gains additional advanced mutagens at 4th, 6th, 8th, and 10th level. The chymist cannot select the same advanced mutagen more than once.",
                "",
                null,
                FeatureGroup.None
            );
            var advancedMutagens = new List<BlueprintFeature>();

            var burly = createFeatureBonusDuringMutagen(
                "ChymistBurlyFeature",
                "Burly",
                "In her mutagenic form, the master chymist's heavy physical frame gives her an alchemical bonus on Strength checks, Constitution checks, and Strength-based skill checks as well as a bonus to CMB and CMD. The bonus is equal to half the master chymist's class level.",
                new BlueprintComponent[] {
                    Helpers.CreateAddContextStatBonus(StatType.AdditionalCMB, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateAddContextStatBonus(StatType.AdditionalCMD, ModifierDescriptor.UntypedStackable),
                    Common.createAbilityScoreCheckBonus(
                        Helpers.CreateContextValue(AbilityRankType.Default),
                        ModifierDescriptor.UntypedStackable, StatType.Strength),
                    Common.createAbilityScoreCheckBonus(
                        Helpers.CreateContextValue(AbilityRankType.Default),
                        ModifierDescriptor.UntypedStackable, StatType.Constitution),
                    Helpers.CreateAddContextStatBonus(StatType.SkillAthletics, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { chymist }, progression: ContextRankProgression.Div2)
                }
            );
            advancedMutagens.Add(burly);

            var nimble = createFeatureBonusDuringMutagen(
                "ChymistNimbleFeature",
                "Nimble",
                "The master chymist's lithe physical frame gives her an alchemical bonus on all Dexterity checks, Dexterity skill checks, and CMD, and a natural armor bonus to her Armor Class. The bonus is equal to half the master chymist's class level.",
                new BlueprintComponent[] {
                    Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.NaturalArmor),
                    Helpers.CreateAddContextStatBonus(StatType.AdditionalCMD, ModifierDescriptor.UntypedStackable),
                    Common.createAbilityScoreCheckBonus(
                        Helpers.CreateContextValue(AbilityRankType.Default),
                        ModifierDescriptor.UntypedStackable, StatType.Dexterity),
                    Helpers.CreateAddContextStatBonus(StatType.SkillMobility, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateAddContextStatBonus(StatType.SkillStealth, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateAddContextStatBonus(StatType.SkillThievery, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { chymist }, progression: ContextRankProgression.Div2)
                }
            );
            advancedMutagens.Add(nimble);

            var evasion = createFeatureBonusDuringMutagen(
                "ChymistEvasionFeature",
                "Evasion",
                "This mutagen functions as the rogue ability of the same name, except that it only applies in the chymist's mutagenic form.",
                new BlueprintComponent[] {
                    Helpers.Create<Evasion>(
                       e => {
                           e.SavingThrow = SavingThrowType.Reflex;
                       }
                    )
                }
            );
            advancedMutagens.Add(evasion);


            var mutagenAbilities = new List<BlueprintAbility>();
            var mutagenBuffs = new List<BlueprintBuff>();
            {
                var mutagens = new BlueprintFeature[]
                {
                library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea"), //mutagen
                library.Get<BlueprintFeature>("76c61966afdd82048911f3d63c6fe0bc"), //greater mutagen
                library.Get<BlueprintFeature>("6f5cb651e26bd97428523061b07ffc85"), //grand mutagen

                library.Get<BlueprintFeature>("e3f460ea61fcc504183c7d6818bbbf7a"), //cognatogen
                library.Get<BlueprintFeature>("18eb29676492e844eb5a55d1c855ce69"), //greater cognatogen
                library.Get<BlueprintFeature>("af4a320648eb5724889d6ff6255090b2"), //grand cognatogen
                };

                foreach (var m in mutagens)
                {
                    var mutagenComponents = m.GetComponent<AddFacts>();
                    foreach (var f in mutagenComponents.Facts)
                    {
                        mutagenAbilities.Add(f as BlueprintAbility);
                        var mutagenBuff = Common.extractActions<ContextActionApplyBuff>((f as BlueprintAbility).GetComponent<AbilityEffectRunAction>().Actions.Actions)[0].Buff;
                        mutagenBuffs.Add(mutagenBuff);
                    }
                }

                foreach (var m in mutagens)
                {
                    var mutagenComponents = m.GetComponent<AddFacts>();
                    foreach (var f in mutagenComponents.Facts)
                    {
                        var mutagenBuff = Common.extractActions<ContextActionApplyBuff>((f as BlueprintAbility).GetComponent<AbilityEffectRunAction>().Actions.Actions)[0].Buff;
                        var others = mutagenBuffs.Where(x => x != mutagenBuff).ToArray();
                        //Main.logger.Log($"other count: {others.Length}");
                        (f as BlueprintAbility).AddComponent(
                            Common.createAbilityExecuteActionOnCast(
                                Helpers.CreateActionList(
                                    Helpers.Create<CallOfTheWild.NewMechanics.ContextActionRemoveBuffs>(
                                        c => {
                                            c.Buffs = others;
                                            //c.ToCaster = true;
                                        }
                                    )
                                )
                            )
                        );
                    }
                }
            }

            //foreach (var b in mutagenBuffs)
            //{
            //    var others = mutagenBuffs.Where(x => x != b).ToArray();
            //    b.AddComponent(
            //        Helpers.CreateRunActions(
            //             Helpers.Create<CallOfTheWild.NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = others)
            //        )
            //    );
            //}

            var extendedMutagen = Helpers.CreateFeature(
                "ChymistExtendedMutagenFeature",
                "Extended Mutagen",
                "The duration of the master chymist's mutation is doubled.",
                "",
                null,
                FeatureGroup.None,
                Common.autoMetamagicOnAbilities(
                    Kingmaker.UnitLogic.Abilities.Metamagic.Extend,
                    mutagenAbilities.ToArray()
                )
            );
            advancedMutagens.Add(extendedMutagen);

            //var prereqBuilderNoMutation = EffectiveAlchemistLevelPrerequisite.getBuilder(
            //    new BlueprintCharacterClass[] { alchemist, chymist }
            //    //new BlueprintArchetype[] { MutationWarrior.archetype }
            //);

            var enlageBuff = library.Get<BlueprintBuff>("4f139d125bb602f48bfaec3d3e1937cb");
            var growthMutagen = createFeatureBonusDuringMutagen(
                "ChymistGrowthMutagenFeature",
                "Growth Mutagen",
                "When the chymist assumes her mutagenic form, she increases one size category, as if under the effects of an enlarge person spell. The character must have an effective alchemist level (alchemist level plus chymist level) of at least 16.",
                enlageBuff.Components
            );
            growthMutagen.AddComponent(
                EffectiveAlchemistLevelPrerequisite.getBuilder(
                    new BlueprintCharacterClass[] { alchemist, chymist },
                    new BlueprintArchetype[] { }
                )(16)
            );

            advancedMutagens.Add(growthMutagen);

            var dualMind = Helpers.CreateFeature(
                "ChymistDualMindFeature",
                "Dual Mind",
                "The chymist's alter ego gives her a +2 bonus on Will saving throws in her normal and mutagenic forms. If she is affected by an enchantment spell or effect and fails her saving throw, she can attempt it again 1 round later at the same DC; if she succeeds, she is free of the effect (as if she had made her original save).",
                "",
                null,
                FeatureGroup.None,
                Helpers.Create<CallOfTheWild.NewMechanics.SecondRollToRemoveBuffAfterOneRound>(
                    m =>
                    {
                        m.school = SpellSchool.Enchantment;
                        m.save_type = SavingThrowType.Will;
                    }
                ),
                Helpers.CreateAddStatBonus(StatType.SaveWill, 2, ModifierDescriptor.UntypedStackable)
            );
            advancedMutagens.Add(dualMind);

            var greaterMutagen = library.Get<BlueprintFeature>("76c61966afdd82048911f3d63c6fe0bc");
            var grandMutagen = library.Get<BlueprintFeature>("6f5cb651e26bd97428523061b07ffc85");
            var prereqBuilder = EffectiveAlchemistLevelPrerequisite.getBuilder(
                new BlueprintCharacterClass[] { alchemist, chymist },
                new BlueprintArchetype[] { MutationWarrior.archetype }
            );

            //replacing my own component, so smart!
            greaterMutagen.ReplaceComponent<MutationWarriorAlchemistClassLevelPrerequsite>(
                prereqBuilder(12)
            );
            grandMutagen.ReplaceComponent<MutationWarriorAlchemistClassLevelPrerequsite>(
                prereqBuilder(16)
            );
            advancedMutagens.Add(greaterMutagen);
            advancedMutagens.Add(grandMutagen);

            var furiousMutagen = Helpers.CreateFeature(
                "ChymistFuriousMutagenFeature",
                "Furious Mutagen",
                "The damage dice for the feral mutagen's bite and claw attacks increase by one die step. The character must have an effective alchemist level (alchemist level plus chymist level) of at least 11 to select this ability.",
                "",
                null,
                FeatureGroup.None,
                EffectiveAlchemistLevelPrerequisite.getBuilder(
                    new BlueprintCharacterClass[] { alchemist, chymist },
                    new BlueprintArchetype[] { }
                )(11)
            );

            var feralBuff = library.Get<BlueprintBuff>("fb890fa3374c38f42b08402688d984cf");
            {
                var clawComp = feralBuff.GetComponent<EmptyHandWeaponOverride>();
                feralBuff.RemoveComponent(clawComp);

                var oldBuff = Helpers.CreateBuff(
                    $"FeralSmallClawBuff",
                    "",
                    "",
                    "",
                    null,
                    null,
                    clawComp
                );
                oldBuff.SetBuffFlags(BuffFlags.HiddenInUi);

                var bigClaw = library.CopyAndAdd(clawComp.Weapon, "FeralBigClawWeapon", "");
                bigClaw.m_DamageDice = new DiceFormula(1, DiceType.D8);

                var newBuff = Helpers.CreateBuff(
                    $"FeralBigClawBuff",
                    "",
                    "",
                    "",
                    null,
                    null,
                    Common.createEmptyHandWeaponOverride(bigClaw)
                );
                newBuff.SetBuffFlags(BuffFlags.HiddenInUi);
                
                var clawConditional = Helpers.CreateConditional(Common.createContextConditionHasFact(furiousMutagen),
                    Common.createContextActionApplyBuff(newBuff, Helpers.CreateContextDuration(),
                        dispellable: false, is_child: true, is_permanent: true),
                    Common.createContextActionApplyBuff(oldBuff, Helpers.CreateContextDuration(),
                        dispellable: false, is_child: true, is_permanent: true)
                );

                Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(feralBuff, clawConditional);
            }
            
            {
                var biteComp = feralBuff.GetComponent<AddAdditionalLimb>();
                feralBuff.RemoveComponent(biteComp);

                var oldBuff = Helpers.CreateBuff(
                    $"FeralSmallBiteBuff",
                    "",
                    "",
                    "",
                    null,
                    null,
                    biteComp
                );
                oldBuff.SetBuffFlags(BuffFlags.HiddenInUi);

                var bigBite = library.CopyAndAdd(biteComp.Weapon, "FeralBigBiteWeapon", "");
                bigBite.m_DamageDice = new DiceFormula(1, DiceType.D10);

                var newBuff = Helpers.CreateBuff(
                    $"FeralBigBiteBuff",
                    "",
                    "",
                    "",
                    null,
                    null,
                    Common.createAddAdditionalLimb(bigBite)
                );
                newBuff.SetBuffFlags(BuffFlags.HiddenInUi);

                var clawConditional = Helpers.CreateConditional(Common.createContextConditionHasFact(furiousMutagen),
                    Common.createContextActionApplyBuff(newBuff, Helpers.CreateContextDuration(),
                        dispellable: false, is_child: true, is_permanent: true),
                    Common.createContextActionApplyBuff(oldBuff, Helpers.CreateContextDuration(),
                        dispellable: false, is_child: true, is_permanent: true)
                );

                Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuffNoRemove(feralBuff, clawConditional);
            }

            advancedMutagens.Add(furiousMutagen);
            advancedMutagen.AllFeatures = advancedMutagens.ToArray();

            var mutagenResource = library.Get<BlueprintAbilityResource>("3b163587f010382408142fc8a97852b6");
            var mutate = Helpers.CreateFeature(
                "ChymistMutateFeature",
                "Mutate",
                "At 1st level master chymist can use mutagen one additional time per day. She gains futher uses at level 5th, 8th and 10th.",
                "",
                null,
                FeatureGroup.None,
                Helpers.Create<IncreaseResourceAmount>(i => { i.Resource = mutagenResource; i.Value = 1; })
            );
            mutate.Ranks = 4;

            chymist.Progression.LevelEntries = new LevelEntry[]
            {
                Helpers.LevelEntry(1, progressionSelection, mutate),
                Helpers.LevelEntry(2, spellbookSelection, advancedMutagen),
                Helpers.LevelEntry(3, brutality),
                Helpers.LevelEntry(4, advancedMutagen),
                Helpers.LevelEntry(5, mutate),
                Helpers.LevelEntry(6, advancedMutagen),
                Helpers.LevelEntry(7, brutality),
                Helpers.LevelEntry(8, advancedMutagen, mutate),
                Helpers.LevelEntry(9, brutality),
                Helpers.LevelEntry(10, advancedMutagen, mutate),
            };

            chymist.Progression.Classes = new BlueprintCharacterClass[] { chymist };
            chymist.Progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { };
            chymist.Progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(spellbookSelection) };

            Helpers.RegisterClass(chymist);
        }
    }
}
