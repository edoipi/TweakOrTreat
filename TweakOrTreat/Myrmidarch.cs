using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace TweakOrTreat
{
    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AddEldritchArcher: OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            this.Owner.Ensure<UnitPartMagus>()?.EldritchArcher.Retain();
        }

        public override void OnTurnOff()
        {
            this.Owner.Ensure<UnitPartMagus>()?.EldritchArcher.Release();
        }
    }
    class Myrmidarch
    {
        static LibraryScriptableObject library => Main.library;
        public static BlueprintArchetype archetype;
        public static BlueprintCharacterClass magus;
        static internal void load()
        {
            magus = Helpers.GetClass("45a4607686d96a1498891b3286121780");
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MyrmidarchArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Myrmidarch");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The myrmidarch is a skilled specialist, using magic to supplement and augment his martial mastery. Less inclined to mix the two than a typical magus, the myrmidarch seeks supremacy with blade, bow, and armor.");
            });
            Helpers.SetField(archetype, "m_ParentClass", magus);
            library.AddAsset(archetype, "");

            var wizardSpellList = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
            var kensaiSpellbook = library.Get<BlueprintSpellbook>("682545e11e5306c45b14ca78bcbe3e62");
            var myrmidiarchSpellbook = library.CopyAndAdd(kensaiSpellbook, "MyrmidiarchSpellbook", "");
            myrmidiarchSpellbook.AddComponent(
                Helpers.Create<AddCustomSpells>(
                    a =>
                    {
                        a.CasterLevel = 19;
                        a.Count = 6;
                        a.SpellList = wizardSpellList;
                        a.MaxSpellLevel = 6;
                    }
                )
            );

            foreach (var s in new BlueprintFeatureSelection[] {
                Common.EldritchKnightSpellbookSelection,
                Common.ArcaneTricksterSelection,
                Common.MysticTheurgeArcaneSpellbookSelection,
                Hinterlander.spellbook_selection,
                DawnflowerAnchorite.spellbook_selection})
            {
                foreach (var f in s.AllFeatures)
                {
                    if (f.GetComponents<PrerequisiteClassSpellLevel>().Where(c => c.CharacterClass == magus).Count() > 0)
                    {
                        f.AddComponent(Common.prerequisiteNoArchetype(archetype));
                    }
                }
            }

            Common.addReplaceSpellbook(Common.EldritchKnightSpellbookSelection, myrmidiarchSpellbook, "EldritchKnightMyrmidiarch",
                                       Common.createPrerequisiteClassSpellLevel(magus, 3),
                                       Common.createPrerequisiteArchetypeLevel(archetype, 1));

            Common.addReplaceSpellbook(Common.ArcaneTricksterSelection, myrmidiarchSpellbook, "ArcaneTricksterMyrmidiarch",
                                      Common.createPrerequisiteClassSpellLevel(magus, 2),
                                       Common.createPrerequisiteArchetypeLevel(archetype, 1));

            Common.addReplaceSpellbook(Common.MysticTheurgeArcaneSpellbookSelection, myrmidiarchSpellbook, "MysticTheurgeMyrmidiarch",
                                       Common.createPrerequisiteClassSpellLevel(magus, 2),
                                       Common.createPrerequisiteArchetypeLevel(archetype, 1));

            Common.addReplaceSpellbook(Hinterlander.spellbook_selection, myrmidiarchSpellbook, "HinterlanderMyrmidiarch",
                                       Common.createPrerequisiteClassSpellLevel(magus, 1),
                                       Common.createPrerequisiteArchetypeLevel(archetype, 1));

            Common.addReplaceSpellbook(DawnflowerAnchorite.spellbook_selection, myrmidiarchSpellbook, "DawnflowerAnchoriteMyrmidiarch",
                                       Common.createPrerequisiteClassSpellLevel(magus, 2),
                                       Common.createPrerequisiteArchetypeLevel(archetype, 1));

            archetype.ReplaceSpellbook = myrmidiarchSpellbook;

            var spellStrike = library.Get<BlueprintFeature>("be50f4e97fff8a24ba92561f1694a945");
            var spellRecall = library.Get<BlueprintFeature>("61fc0521e9992624e9c518060bf89c0f");
            var spellRecallGreater = library.Get<BlueprintFeature>("0ef6ec1c2fdfc204fbd3bff9f1609490");
            var arcana = library.Get<BlueprintFeature>("e9dc4dfc73eaaf94aae27e0ed6cc9ada");
            var oldFighterTraining = library.Get<BlueprintFeature>("2b636b9e8dd7df94cbd372c52237eebf");
            var improvedSpellCombat = library.Get<BlueprintFeature>("836879fcd5b29754eb664a090bd6c22f");
            var greaterSpellCombat = library.Get<BlueprintFeature>("379887a82a7248946bbf6d0158663b5e");
            var trueMagus = library.Get<BlueprintFeature>("789c7539ba659174db702e18d7c2d330");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(4, spellRecall),
                Helpers.LevelEntry(6, arcana),
                Helpers.LevelEntry(8, improvedSpellCombat),
                Helpers.LevelEntry(10, oldFighterTraining),
                Helpers.LevelEntry(11, spellRecallGreater),
                Helpers.LevelEntry(12, arcana),
                Helpers.LevelEntry(14, greaterSpellCombat),
                Helpers.LevelEntry(18, arcana),
                Helpers.LevelEntry(20, trueMagus),
            };

            var eldritchArcherRangedSpellCombat = library.Get<BlueprintFeature>("8b68a5b8223beed40b137885116c408f");
            var spellCombatAbility = library.Get<BlueprintActivatableAbility>("8898a573e8a8a184b8186dbc3a26da74");
            spellCombatAbility.Group = ActivatableAbilityGroupExtension.ExtraGroup1.ToActivatableAbilityGroup();

            var rangedSepllstrikeBuff = Helpers.CreateBuff(
                "MyrmidiarchEldritchArcherBuff",
                "Ranged Spellstrike",
                "At 4th level, a myrmidarch can use spellstrike to cast a single-target touch attack ranged spell and deliver it through a ranged weapon attack. Even if the spell can normally affect multiple targets, only a single missile, ray, or effect accompanies the attack.\n" +
                "At 11th level, a myrmidarch can use a ranged weapon for spell combat.",
                "",
                eldritchArcherRangedSpellCombat.Icon,
                null,
                Helpers.Create<AddEldritchArcher>()
            );

            var rangedSpellstrikeAbility = Helpers.CreateActivatableAbility(
                "MyrmidiarchEldritchArcherAbility",
                rangedSepllstrikeBuff.Name,
                rangedSepllstrikeBuff.Description,
                "",
                rangedSepllstrikeBuff.Icon,
                rangedSepllstrikeBuff,
                AbilityActivationType.Immediately,
                CommandType.Free,
                null
            );
            rangedSpellstrikeAbility.DeactivateImmediately = true;
            rangedSpellstrikeAbility.Group = ActivatableAbilityGroupExtension.ExtraGroup1.ToActivatableAbilityGroup();

            var rangedSpellstrikeFeature = Helpers.CreateFeature(
                "MyrmidiarchEldritchArcherFeature",
                rangedSpellstrikeAbility.Name,
                rangedSpellstrikeAbility.Description,
                "",
                rangedSpellstrikeAbility.Icon,
                FeatureGroup.None,
                Helpers.CreateAddFact(rangedSpellstrikeAbility)
            );
            library.Get<BlueprintFeature>("6aa84ca8918ac604685a3d39a13faecc").AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.FeatureReplacement>(f => f.replacement_feature = rangedSpellstrikeFeature)); //ranged spellstrike

            var rangedSpellCombatFeature = Helpers.CreateFeature(
                "MyrmidiarchRangedSpellCombatFeature",
                "Ranged Spell Combat",
                rangedSpellstrikeAbility.Description,
                "",
                rangedSpellstrikeAbility.Icon,
                FeatureGroup.None,
                Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroupExtension.ExtraGroup1.ToActivatableAbilityGroup())
            );

            var fighter = Helpers.GetClass("48ac8db94d5de7645906c7d0ad3bcfbd");
            var fighter1 = Helpers.CreateFeature(
                "MyridiarchFighter1",
                "Fighter Training",
                "At 7th level, a myrmidarch counts his magus level –3 as his fighter level for the purpose of qualifying for feats (if he has levels in fighter, these levels stack). At 10th level, the myrmidarch treats his magus levels as fighter levels for the purposes of fighter training.",
                "",
                oldFighterTraining.Icon,
                FeatureGroup.None,
                Common.createClassLevelsForPrerequisites(fighter, magus, summand: -3)
            );
            var fighter2 = Helpers.CreateFeature(
                "MyridiarchFighter2",
                "Fighter Training",
                "At 7th level, a myrmidarch counts his magus level –3 as his fighter level for the purpose of qualifying for feats (if he has levels in fighter, these levels stack). At 10th level, the myrmidarch treats his magus levels as fighter levels for the purposes of fighter training.",
                "",
                oldFighterTraining.Icon,
                FeatureGroup.None,
                Common.createClassLevelsForPrerequisites(fighter, magus),
                Common.createRemoveFeatureOnApply(fighter1)
            );

            var armorTraining = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");
            var advancedArmorTraining = library.TryGet<BlueprintFeature>("3e65a6725026458faabc9d0c2748974c") ?? armorTraining;

            var advancedWeaponTraining = library.Get<BlueprintFeatureSelection>("b8cecf4e5e464ad41b79d5b42b76b399");
            var weaponTrainingRankup = library.Get<BlueprintFeatureSelection>("5f3cc7b9a46b880448275763fe70c0b0");

            var armoredJuggernaut = library.Get<BlueprintFeature>("9b81b76e2b3741a5a8d1b77a8107e909");

            var archatypeListFeature = Helpers.CreateFeature("ArmoredJuggernautArchetypeExtensionFeature",
                                                               "",
                                                               "",
                                                               "",
                                                               null,
                                                               FeatureGroup.None);
            archatypeListFeature.AddComponent(Helpers.Create<ContextRankConfigArchetypeList>(c => c.archetypes = new BlueprintArchetype[] { archetype }));
            archatypeListFeature.HideInCharacterSheetAndLevelUp = true;
            foreach (var comp in armoredJuggernaut.GetComponents<CallOfTheWild.WeaponTrainingMechanics.AddFeatureOnArmor>())
            {
                comp.feature.ReplaceComponent<ContextRankConfig>(
                    c =>
                    {
                        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType());
                        Helpers.SetField(c, "m_Feature", archatypeListFeature);
                        Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(Myrmidarch.magus));
                    }
                );
            }

            var armorSpecialization = library.Get<BlueprintFeatureSelection>("6cf910d80ba143e19f9196b7450a3832");
            foreach(var feature in armorSpecialization.AllFeatures)
            {
                feature.GetComponent<CallOfTheWild.WeaponTrainingMechanics.AddFeatureOnArmor>().feature.ReplaceComponent<ContextRankConfig>(
                    c =>
                    {
                        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType());
                        Helpers.SetField(c, "m_Feature", archatypeListFeature);
                        Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(Myrmidarch.magus));
                    }
                );
            }

            var armoredConfidence = library.Get<BlueprintFeature>("5db3b92deb084af39d33c6f52634f180");
            foreach (var comp in armoredConfidence.GetComponents<CallOfTheWild.WeaponTrainingMechanics.AddFeatureOnArmor>())
            {
                comp.feature.ReplaceComponent<ContextRankConfig>(
                    c =>
                    {
                        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType());
                        Helpers.SetField(c, "m_Feature", archatypeListFeature);
                        Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(Myrmidarch.magus));
                    }
                );
            }

            var criticalDeflection = library.Get<BlueprintFeature>("5db3b92deb084af39d33c6f52634f180");
            criticalDeflection.GetComponent<CallOfTheWild.WeaponTrainingMechanics.AddFeatureOnArmor>().feature.ReplaceComponent<ContextRankConfig>(
                    c =>
                    {
                        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType());
                        Helpers.SetField(c, "m_Feature", archatypeListFeature);
                        Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(Myrmidarch.magus));
                    }
                );

            var fighterArmorMastery = library.Get<BlueprintFeature>("ae177f17cfb45264291d4d7c2cb64671");
            //why make new when fighters one is identical except minor detail?
            //var capstone = Helpers.CreateFeature(
            //    "MyrmidiarchArmorMastery",
            //    "Armor Mastery",
            //    "At 7th level, a myrmidarch counts his magus level –3 as his fighter level for the purpose of qualifying for feats (if he has levels in fighter, these levels stack). At 10th level, the myrmidarch treats his magus levels as fighter levels for the purposes of fighter training.",
            //    "",
            //    oldFighterTraining.Icon,
            //    FeatureGroup.None,
            //    Helpers.Create<WeaponTrainingMechanics.AddFeatureOnArmor>(a => { a.feature = feature; a.required_armor = armor_types; }
            //);

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(4, rangedSpellstrikeFeature),
                Helpers.LevelEntry(6, advancedWeaponTraining),
                Helpers.LevelEntry(7, fighter1),
                Helpers.LevelEntry(8, armorTraining),
                Helpers.LevelEntry(10, fighter2),
                Helpers.LevelEntry(11, rangedSpellCombatFeature),
                Helpers.LevelEntry(12, advancedWeaponTraining, weaponTrainingRankup),
                Helpers.LevelEntry(14, advancedArmorTraining),
                Helpers.LevelEntry(18, advancedWeaponTraining, weaponTrainingRankup),
                Helpers.LevelEntry(20, fighterArmorMastery),
            };

            magus.Archetypes = magus.Archetypes.AddToArray(archetype);

            foreach (var group in magus.Progression.UIGroups)
            {
                if (group.Features.Contains(spellStrike))
                {
                    group.Features.Add(rangedSpellstrikeFeature);
                }
                if (group.Features.Contains(greaterSpellCombat))
                {
                    group.Features.Add(rangedSpellCombatFeature);
                }
            }
            magus.Progression.UIGroups = magus.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(fighter1, fighter2),
                Helpers.CreateUIGroup(armorTraining, advancedArmorTraining, fighterArmorMastery)
            );
        }
    }
}
