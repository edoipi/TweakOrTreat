using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite;

namespace TweakOrTreat
{
    class WildStalker
    {
        static LibraryScriptableObject library => Main.library;

        static internal void load()
        {
            var ranger = Helpers.GetClass("cda0615668a6df14eb36ba19ee881af6");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "WildStalkerArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Wild Stalker");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description",
                    "Civilization grows stronger and more decadent with each passing year. It tears into unclaimed wilderness and destroys the fragile ecology in its constant push for expansion and exploitation. The wild stalker forsakes the bonds of community and lives in the trackless wilds far from others of his kind, or perhaps grew up there, never knowing of civilization as anything more than his enemy. He drives pioneers back to civilization and strives to keep the land unspoiled.");
            });
            Helpers.SetField(archetype, "m_ParentClass", ranger);
            library.AddAsset(archetype, "");

            var favoriteEnemySelection = library.Get<BlueprintFeature>("16cc2c937ea8d714193017780e7d4fc6");
            var favoriteEnemyRankUp = library.Get<BlueprintFeature>("c1be13839472aad46b152cf10cf46179"); 

            var rangerStyleSelection2 = library.Get<BlueprintFeature>("c6d0da9124735a44f93ac31df803b9a9");
            var rangerStyleSelection6 = library.Get<BlueprintFeature>("61f82ba786fe05643beb3cd3910233a8");
            var rangerStyleSelection10 = library.Get<BlueprintFeature>("78177315fc63b474ea3cbb8df38fafcd");
            
            var huntersBondSelection = library.Get<BlueprintFeature>("b705c5184a96a84428eeb35ae2517a14");

            var quarry = library.Get<BlueprintFeature>("385260ca07d5f1b4e907ba22a02944fc");
            var improvedQuarry = library.Get<BlueprintFeature>("25e009b7e53f86141adee3a1213af5af");
            var masterHunter = library.Get<BlueprintFeature>("9d53ef63441b5d84297587d75f72fc17");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, favoriteEnemySelection),
                Helpers.LevelEntry(2, rangerStyleSelection2),
                Helpers.LevelEntry(4, huntersBondSelection),
                Helpers.LevelEntry(5, favoriteEnemySelection, favoriteEnemyRankUp),
                Helpers.LevelEntry(6, rangerStyleSelection6),
                Helpers.LevelEntry(10, favoriteEnemySelection, favoriteEnemyRankUp, rangerStyleSelection10),
                Helpers.LevelEntry(11, quarry),
                Helpers.LevelEntry(14, rangerStyleSelection10),
                Helpers.LevelEntry(15, favoriteEnemySelection, favoriteEnemyRankUp),
                Helpers.LevelEntry(18, rangerStyleSelection10),
                Helpers.LevelEntry(19, improvedQuarry),
                Helpers.LevelEntry(20, favoriteEnemySelection, favoriteEnemyRankUp, masterHunter),
            };

            var strongSenses = Helpers.CreateFeature(
                "StrongSensesfeature",
                "Strong Senses",
                " At 1st level, a wild stalker’s life among the wild has sharpened his senses. He gains a +2 bonus on Perception checks. " +
                " This bonus increases by +1 at 4th level and for every four levels after 4th (to a maximum of +7 at 17th level).",
                "",
                null,
                FeatureGroup.None,
                Helpers.CreateAddContextStatBonus(StatType.SkillPerception, ModifierDescriptor.UntypedStackable),
                Helpers.CreateContextRankConfig(
                    baseValueType: ContextRankBaseValueType.ClassLevel, 
                    classes: new BlueprintCharacterClass[] { ranger },
                    archetype: archetype,
                    progression: ContextRankProgression.StartPlusDivStep, 
                    stepLevel: 4, 
                    startLevel: -4
                )
            );
            strongSenses.ReapplyOnLevelUp = true;

            var barbarian = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("f7d7eb166b3dd594fb330d085df41853");
            var barbarianLevel = Helpers.CreateFeature(
                "WIldStalkerBarbarianLevel",
                "",
                "",
                "",
                null,
                FeatureGroup.None,
                Common.createClassLevelsForPrerequisites(barbarian, ranger, summand: -3)
            );
            barbarianLevel.HideInUI = true;
            barbarianLevel.HideInCharacterSheetAndLevelUp = true;

            var uncannyDodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            var rageFeature = library.Get<BlueprintFeature>("2479395977cfeeb46b482bc3385f4647");

            var fakeBarbarian = library.Get<BlueprintCharacterClass>("91f4c9284ca442a48beb010dc5778c06");

            var rageResource = library.Get<BlueprintAbilityResource>("24353fcf8096ea54684a72bf58dedbc9");
            var rageOfTheWild = Helpers.CreateFeature(
                "RageOfTheWild",
                "Rage of the Wild",
                "At 4th level, a wild stalker gains the rage ability as the barbarian class feature, but its barbarian level is considered to be his ranger level –3.",
                "",
                rageFeature.Icon,
                FeatureGroup.None,
                Helpers.CreateAddFact(rageFeature),
                Helpers.CreateAddFact(barbarianLevel),
                Helpers.Create<IncreaseResourcesByClassOnly>(
                    i =>
                    {
                        i.CharacterClass = ranger;
                        i.Resource = rageResource;
                        i.levelAdjustment = -3;
                        i.levelMult = 2;
                        i.BaseValue = 2;
                    }
                ),
                Helpers.Create<CallOfTheWild.FakeClassLevelMechanics.AddFakeClassLevel>(
                    a => 
                    {
                        a.fake_class = fakeBarbarian;
                        a.value = Helpers.CreateContextValue(AbilityRankType.Default);
                    }
                ),
                Helpers.CreateContextRankConfig(
                    ContextRankBaseValueType.ClassLevel, 
                    classes: new BlueprintCharacterClass[] { ranger },
                    progression: ContextRankProgression.BonusValue,
                    stepLevel: -3
                )
            );
            rageOfTheWild.HideInCharacterSheetAndLevelUp = true;

            //var amount = Helpers.GetField(rageResource, "m_MaxAmount");
            //Helpers.SetField(amount, "Class", Helpers.GetField<BlueprintCharacterClass[]>(amount, "Class").AddToArray(ranger));
            //Helpers.SetField(amount, "Archetypes", Helpers.GetField<BlueprintArchetype[]>(amount, "Archetypes").AddToArray(archetype));
            //Helpers.SetField(rageResource, "m_MaxAmount", amount);

            //var rageBuff = library.Get<BlueprintBuff>("da8ce41ac3cd74742b80984ccc3c9613");
            //ClassToProgression.addClassToBuff(ranger, new BlueprintArchetype[] { archetype }, rageBuff, barbarian);

            //fake barbarian already added to relevant abilities
            
            var ragePowerSelection = library.Get<BlueprintFeatureSelection>("28710502f46848d48b3f0d6132817c4e");
            var ragePowers = library.CopyAndAdd<BlueprintFeatureSelection>(ragePowerSelection, "RagePowerswildstalker", "");
            ragePowers.SetDescription("At 5th level, a wild stalker ranger gains a single rage power, as the barbarian class feature. He gains another rage power each four levels after 5th (to a maximum of four rage powers at 17th level).");

            //foreach (var f in ragePowers.AllFeatures)
            //{
            //    ClassToProgression.addClassToFeat(ranger, new BlueprintArchetype[] { archetype }, ClassToProgression.DomainSpellsType.NoSpells, f, barbarian);
            //}

            ////buffs form rage powers that should scale with staler level, ripped straight from cotw 
            ////and not double checked that these are all that need to be updated
            //BlueprintBuff[] buffsToUpdateScaling = new BlueprintBuff[]{
            //    library.Get<BlueprintBuff>("ec7db4946877f73439c4ee661f645452"), //beast totem ac buff
            //    library.Get<BlueprintBuff>("3858dd3e9a94f0b41abdc58387d68ccf"), //guarded stance
            //    library.Get<BlueprintBuff>("5b5e566167a3f2746b7d3a26bc8a50a6"), //guarded stance protect vitals
            //    library.Get<BlueprintBuff>("b209f567dc78a1440aad52d4138c5f27"), //reflexive dodge
            //    library.Get<BlueprintBuff>("0c6e198a78210954c9fe245a26b0c315"), //deadly accuracy
            //    library.Get<BlueprintBuff>("9ec69854596674a4ba40802e6337894d"), //inspire ferocity buff
            //    library.Get<BlueprintBuff>("c6271b3183c48d54b8defd272bea0665"), //lethal stance
            //    library.Get<BlueprintBuff>("a8a733d2605c66548b652f312ea4dbf3"), //reckless stance
            //    NewRagePowers.greater_celestial_totem_buff,
            //    NewRagePowers.superstition_buff,
            //    NewRagePowers.ghost_rager_buff,
            //    NewRagePowers.witch_hunter_buff
            //};
            //buffsToUpdateScaling = buffsToUpdateScaling.AddToArray(NewRagePowers.energy_resistance_buff);

            ////unlike skald adjustments here we need to add archetype and change type to acount only for archetyped levels
            //foreach (var b in buffsToUpdateScaling)
            //{
            //    var c = b.GetComponent<ContextRankConfig>();
            //    BlueprintCharacterClass[] classes = Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class");
            //    classes = classes.AddToArray(ranger);
            //    Helpers.SetField(c, "m_Class", classes);
            //    Helpers.SetField(c, "Archetype", archetype);
            //    Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueType.SummClassLevelWithArchetype);
            //}

            //var renewedVigor = library.Get<BlueprintAbility>("5a25185dbf75a954580a1248dc694cfc");
            //var contextRankConfigs = renewedVigor.GetComponents<ContextRankConfig>();
            //foreach (var c in contextRankConfigs)
            //{
            //    var t = Helpers.GetField<ContextRankBaseValueType>(c, "m_BaseValueType");
            //    if (t == ContextRankBaseValueType.ClassLevel)
            //    {
            //        BlueprintCharacterClass[] classes = Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class");
            //        classes = classes.AddToArray(ranger);
            //        Helpers.SetField(c, "m_Class", classes);
            //        Helpers.SetField(c, "Archetype", archetype);
            //        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueType.SummClassLevelWithArchetype);
            //    }
            //}

            //var howlScaling = NewRagePowers.terrifying_howl_ability.GetComponent<CallOfTheWild.NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>();
            //howlScaling.CharacterClasses = howlScaling.CharacterClasses.AddToArray(ranger);
            //howlScaling.archetypes = howlScaling.archetypes.AddToArray(archetype);

            var ragePowersOrSkills = library.CopyAndAdd<BlueprintFeatureSelection>(ragePowers, "WildTalent", "");
            ragePowersOrSkills.SetDescription("At 7th level, a wild stalker can either take a rage power, or gains a +2 insight bonus into any one of the following skills: " +
                "Mobility, Perception, Stealth, or Lore (Nature). The wild stalker can gain one of these two benefits again every four levels after 7th (to a maximum of 4 times at 19th level).");
            ragePowersOrSkills.SetName("Wild Talent");

            var stats = new (StatType, String)[] {
                (StatType.SkillMobility, "Mobility"),
                (StatType.SkillPerception, "Perception"),
                (StatType.SkillStealth, "Stealth"),
                (StatType.SkillLoreNature, "Lore (Nature)")
            };
            foreach(var statWithName in stats)
            {
                var skillBonus = Helpers.CreateFeature(
                    "WildStalker" + new string(statWithName.Item2
                        .Where(c => !(c == ' ' || c == '(' || c == ')'))
                        .ToArray()) + "Bonus",
                    "Wild Talent - " + statWithName.Item2,
                    "At 7th level, a wild stalker can either take a rage power, or gains a +2 insight bonus into any one of the following skills: " +
                    "Mobility, Perception, Stealth, or Lore (Nature). The wild stalker can gain one of these two benefits again every four levels after 7th (to a maximum of 4 times at 19th level).",
                    "",
                    null,
                    FeatureGroup.None,
                    Helpers.CreateAddStatBonus(statWithName.Item1, 2, ModifierDescriptor.Insight)
                );
                ragePowersOrSkills.AllFeatures = ragePowersOrSkills.AllFeatures.AddToArray(skillBonus);
            }

            var greaterRage = library.Get<BlueprintFeature>("ce49c579fe0bcc647a32c96929fae982");
            var tirelessRage = library.Get<BlueprintFeature>("ca9343d75a83a2745a22fa11c383153a");
            var mightyRage = library.Get<BlueprintFeature>("06a7e5b60020ad947aed107d82d1f897");

            archetype.AddFeatures = new LevelEntry[] {
                //Helpers.LevelEntry(1, rageOfTheWild),

                Helpers.LevelEntry(1, strongSenses),
                Helpers.LevelEntry(2, uncannyDodge),
                Helpers.LevelEntry(4, rageOfTheWild),
                Helpers.LevelEntry(5, ragePowers),
                Helpers.LevelEntry(7, ragePowersOrSkills),
                Helpers.LevelEntry(9, ragePowers),
                Helpers.LevelEntry(11, greaterRage, ragePowersOrSkills),
                Helpers.LevelEntry(13, ragePowers),
                Helpers.LevelEntry(15, ragePowersOrSkills),
                Helpers.LevelEntry(17, ragePowers),
                Helpers.LevelEntry(19, tirelessRage, ragePowersOrSkills),
                Helpers.LevelEntry(20, mightyRage),
            };

            var extraRagePowerSelection = library.Get<BlueprintFeature>("0c7f01fbbe687bb4baff8195cb02fe6a");
            var prereq = extraRagePowerSelection.GetComponent<PrerequisiteFeature>();
            prereq.Group = GroupType.Any;
            extraRagePowerSelection.AddComponent(Helpers.PrerequisiteFeature(ragePowers, true));

            ranger.Archetypes = ranger.Archetypes.AddToArray(archetype);

            ranger.Progression.UIGroups = ranger.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(rageOfTheWild, greaterRage, tirelessRage, mightyRage),
                Helpers.CreateUIGroup(ragePowers, ragePowersOrSkills)
            );
        }
    }
}
