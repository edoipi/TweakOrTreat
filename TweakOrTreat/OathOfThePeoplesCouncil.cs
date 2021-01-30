using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class OathOfThePeoplesCouncil
    {
        static LibraryScriptableObject library => Main.library;
        public static BlueprintArchetype archetype;
        static internal void load()
        {
            var paladin = Helpers.GetClass("bfa11238e7ae3544bbeb4d0b92e897ec");
            archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "OathOfThePeoplesCouncilArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Oath of the People's Council");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "The Eagle Knights and the people of Andoran hold their elected officials to extremely high standards of conduct. Paladins swearing the oath of the People’s Council serve the common good by finding and thwarting those who abuse their authority. Many seek specifically to emulate Talmandor, the agathion patron of Andoran.");
            });
            Helpers.SetField(archetype, "m_ParentClass", paladin);

            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = paladin.ClassSkills.AddToArray(StatType.SkillPerception).RemoveFromArray(StatType.SkillKnowledgeArcana);

            library.AddAsset(archetype, "");

            var smiteEvilFeature = library.Get<BlueprintFeature>("3a6db57fce75b0244a6a5819528ddf26");
            var markOfJustice = library.Get<BlueprintFeature>("9f13fdd044ccb8a439f27417481cb00e");
            var smiteEvilExtra = library.Get<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137");
            var holyChampion = library.Get<BlueprintFeature>("eff3b63f744868845a2f511e9929f0de");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, smiteEvilFeature),
                Helpers.LevelEntry(4, smiteEvilExtra),
                Helpers.LevelEntry(7, smiteEvilExtra),
                Helpers.LevelEntry(10, smiteEvilExtra),
                Helpers.LevelEntry(11, markOfJustice),
                Helpers.LevelEntry(13, smiteEvilExtra),
                Helpers.LevelEntry(16, smiteEvilExtra),
                Helpers.LevelEntry(19, smiteEvilExtra),
                Helpers.LevelEntry(20, holyChampion),
            };

            var trueStrike = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00");
            var findTraps = library.Get<BlueprintAbility>("4709274b2080b6444a3c11c6ebbe2404");
            var seeInvisibility = library.Get<BlueprintAbility>("30e5dc243f937fc4b95d2f8f4e1b7ff3");
            var trueSeeing = library.Get<BlueprintAbility>("b3da3fbee6a751d4197e446c7e852bcb");

            var trueStrikeFeature = OathAgainstChaos.abilityIntoFeature(trueStrike, 1);
            var findTrapsFeature = OathAgainstChaos.abilityIntoFeature(findTraps, 2);
            var seeInvisibilityFeature = OathAgainstChaos.abilityIntoFeature(seeInvisibility, 3);
            var trueSeeingFeature = OathAgainstChaos.abilityIntoFeature(trueSeeing, 4);

            var resource = library.CopyAndAdd<BlueprintFeature>("b92bfc201c6a79e49afd0b5cfbfc269f", "OathOfThePeoplesCouncilResource", "");
            resource.SetName("Stirring Monologue");
            resource.SetDescription("At 1st level, the paladin can deliver a stirring monologue on the ideals of justice and fairness, motivating allies and persuading others. This functions as bardic performance as per a bard of her paladin level using Perform (oratory). All the effects are language-dependent even if they would not normally be. The paladin gains the following performances at the indicated levels: inspire courage (1st), inspire competence (4th), fascinate (7th), dirge of doom (10th), inspire greatness (13th), and frightening tune (16th).");
            resource.ReplaceComponent<IncreaseResourcesByClass>(i => {
                i.CharacterClass = paladin;
                i.BaseValue = 2;
            });
            resource.HideInCharacterSheetAndLevelUp = false;
            resource.HideInUI = false;

            var resScaling = library.Get<BlueprintAbilityResource>("e190ba276831b5c4fa28737e5e49e6a6");
            var amount = Helpers.GetField(resScaling, "m_MaxAmount");
            BlueprintCharacterClass[] classes = Helpers.GetField<BlueprintCharacterClass[]>(amount, "ClassDiv").AddToArray(paladin);
            Helpers.SetField(amount, "Class", classes);
            Helpers.SetField(amount, "Archetypes", new BlueprintArchetype[] { archetype });
            Helpers.SetField(resScaling, "m_MaxAmount", amount);

            var bard = library.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f");
            var inspireCourageFeature = library.Get<BlueprintFeature>("acb4df34b25ca9043a6aba1a4c92bc69");
            //var inspireCourageBuff = library.Get<BlueprintBuff>("6d6d9e06b76f5204a8b7856c78607d5d");
            var inspireCompetenceFeature = library.Get<BlueprintFeature>("6d3fcfab6d935754c918eb0e004b5ef7");
            //var inspireHeroicsFeature = library.Get<BlueprintFeature>("199d6fa0de149d044a8ab622a542cc79");

            ClassToProgression.addClassToFeat(paladin, new BlueprintArchetype[] { archetype }, ClassToProgression.DomainSpellsType.NoSpells, inspireCourageFeature, bard);
            ClassToProgression.addClassToFeat(paladin, new BlueprintArchetype[] { archetype }, ClassToProgression.DomainSpellsType.NoSpells, inspireCompetenceFeature, bard);

            BlueprintFeature fascinateFeature;
            { 
                var fascinateAbility = library.CopyAndAdd<BlueprintActivatableAbility>("993908ad3fb81f34ba0ed168b7c61f58", "OathOfThePeoplesCouncilFascinateToggleAbility", "");
                var fascianteArea = library.CopyAndAdd<BlueprintAbilityAreaEffect>("a4fc1c0798359974e99e1d790935501d", "OathOfThePeoplesCouncilFascianteArea", "");
                fascianteArea.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(c => c.CharacterClass = paladin);
                var fascinateBuff = library.CopyAndAdd<BlueprintBuff>("555930f121b364a4e82670b433028728", "OathOfThePeoplesCouncilFascianteBuff", "");
                fascinateBuff.SetDescription(fascinateAbility.Description);
                fascinateBuff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = fascianteArea);
                fascinateAbility.Buff = fascinateBuff;

                fascinateFeature = Common.ActivatableAbilityToFeature(fascinateAbility, false);
            }
            

            var dirgeFeature = library.Get<BlueprintFeature>("1d48ab2bded57a74dad8af3da07d313a");
            var inspireGreatnessFeature = library.Get<BlueprintFeature>("9ae0f32c72f8df84dab023d1b34641dc");

            BlueprintFeature frighteningTuneFeature;// = library.Get<BlueprintFeature>("cfd8940869a304f4aa9077415f93febe");
            {
                var tuneAbility = library.CopyAndAdd<BlueprintActivatableAbility>("e312b7e8cfac00d4692337f29580c9f3", "OathOfThePeoplesCouncilTuneToggleAbility", "");
                var tuneArea = library.CopyAndAdd<BlueprintAbilityAreaEffect>("55c526a79761a3c48a3cc974a09bfef7", "OathOfThePeoplesCouncilTuneArea", "");
                tuneArea.ReplaceComponent<ContextCalculateAbilityParamsBasedOnClass>(c => c.CharacterClass = paladin);
                var tuneBuff = library.CopyAndAdd<BlueprintBuff>("6d0a82635b9167a4584ff74f5cd50315", "OathOfThePeoplesCouncilTuneBuff", "");
                tuneBuff.SetDescription(tuneAbility.Description);
                tuneBuff.ReplaceComponent<AddAreaEffect>(a => a.AreaEffect = tuneArea);
                tuneAbility.Buff = tuneBuff;

                frighteningTuneFeature = Common.ActivatableAbilityToFeature(tuneAbility, false);
            }

            //ClassToProgression.addClassToFeat(paladin, new BlueprintArchetype[] { archetype }, ClassToProgression.DomainSpellsType.NoSpells, frighteningTuneFeature, bard);

            var layOnHands1 = library.Get<BlueprintAbility>("8d6073201e5395d458b8251386d72df1");
            var layOnHands2 = library.Get<BlueprintAbility>("caae1dc6fcf7b37408686971ee27db13");
            var layOnHands3 = library.Get<BlueprintAbility>("8337cea04c8afd1428aad69defbfc365");
            var healBase = library.Get<BlueprintAbility>("ae3b3f4d7d9841059df91f578c726070");

            var abilitiesToMax = new List<BlueprintAbility> { layOnHands1, layOnHands2, layOnHands3 };
            abilitiesToMax.AddRange(healBase.Variants);

            var maximize = Common.autoMetamagicOnAbilities(Kingmaker.UnitLogic.Abilities.Metamagic.Maximize,
                abilitiesToMax.ToArray()
            );

            //"affected" so I guess only buffing performances
            var capstone = Helpers.CreateFeature(
                "ChampionOfAndoranFeature",
                "Champion of Andoran",
                "At 20th level, an oathbound paladin becomes a champion of the people of Andoran. Her DR increases to 10/evil. Whenever she uses her stirring monologue, allies (including the paladin herself ) affected by the monologue gain SR 30 against spells with the evil descriptor or spell-like abilities cast by evil creatures. Weapons wielded by those affected by her stirring monologue are treated as good, silver, and cold iron for the purpose of overcoming damage reduction. In addition, whenever the oathbound paladin channels positive energy or uses lay on hands to heal a creature, she heals it the maximum possible amount.",
                "",
                null,
                FeatureGroup.None,
                Common.createAlignmentDR(10, Kingmaker.Enums.Damage.DamageAlignment.Evil),
                maximize
            );

            var courageBuff = library.Get<BlueprintBuff>("6d6d9e06b76f5204a8b7856c78607d5d");
            var competenceBuff = library.Get<BlueprintBuff>("1fa5f733fa1d77743bf54f5f3da5a6b1");
            var greatnessBuff = library.Get<BlueprintBuff>("ec38c2e60d738584983415cb8a4f508d");

            var spellResistBuff = Helpers.CreateBuff(
                "ChampionOfAndoranSpellResistBuff",
                "",
                "",
                "",
                null,
                null,
                Helpers.Create<SpellResistanceAgainstAlignment>(
                    s =>
                    {
                        s.Alignment = Kingmaker.Enums.AlignmentComponent.Evil;
                        s.Value = Common.createSimpleContextValue(30);
                    }
                ),
                Common.createAddOutgoingMaterial(Kingmaker.Enums.Damage.PhysicalDamageMaterial.Silver | Kingmaker.Enums.Damage.PhysicalDamageMaterial.ColdIron),
                Common.createAddOutgoingAlignment(Kingmaker.Enums.Damage.DamageAlignment.Good)
            );
            spellResistBuff.SetBuffFlags(BuffFlags.HiddenInUi);

            Utils.addContextActionApplyBuffOnCasterFactsToActivatedAbilityBuffNoRemove(courageBuff, spellResistBuff, capstone);
            Utils.addContextActionApplyBuffOnCasterFactsToActivatedAbilityBuffNoRemove(competenceBuff, spellResistBuff, capstone);
            Utils.addContextActionApplyBuffOnCasterFactsToActivatedAbilityBuffNoRemove(greatnessBuff, spellResistBuff, capstone);

            var auraOfTruthBuff = Helpers.CreateBuff(
                "AuraOfTruthBuff",
                "",
                "",
                "",
                null,
                null,
                Common.createSavingThrowBonusAgainstSchool(1, ModifierDescriptor.UntypedStackable, Kingmaker.Blueprints.Classes.Spells.SpellSchool.Illusion)
            );
            auraOfTruthBuff.SetBuffFlags(BuffFlags.HiddenInUi);

            var auraOFTruthFeature = Common.createAuraEffectFeature(
                "Aura of Truth",
                "At 11th level, the paladin gains +2 bonus on saving throws against spells of illusion school. Allies within 30 feet gain +1 bonus on such saves.",
                null,
                auraOfTruthBuff,
                30.Feet(),
                Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>())
            );
            auraOFTruthFeature.AddComponent(Common.createSavingThrowBonusAgainstSchool(1, ModifierDescriptor.UntypedStackable, Kingmaker.Blueprints.Classes.Spells.SpellSchool.Illusion));

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, resource, inspireCourageFeature),
                Helpers.LevelEntry(4, inspireCompetenceFeature, trueStrikeFeature),
                Helpers.LevelEntry(7, fascinateFeature, findTrapsFeature),
                Helpers.LevelEntry(10, dirgeFeature, seeInvisibilityFeature),
                Helpers.LevelEntry(11, auraOFTruthFeature),
                Helpers.LevelEntry(13, inspireGreatnessFeature, trueSeeingFeature),
                Helpers.LevelEntry(16, frighteningTuneFeature),
                Helpers.LevelEntry(20, capstone)
            };
            paladin.Archetypes = paladin.Archetypes.AddToArray(archetype);


            paladin.Progression.UIGroups = paladin.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(inspireCourageFeature, inspireCompetenceFeature, fascinateFeature, dirgeFeature, inspireGreatnessFeature, frighteningTuneFeature),
                Helpers.CreateUIGroup(trueStrikeFeature, findTrapsFeature, seeInvisibilityFeature, trueSeeingFeature)
            );

            foreach (var group in paladin.Progression.UIGroups)
            {
                if (group.Features.Contains(markOfJustice))
                {
                    group.Features.Add(capstone);
                    group.Features.Add(auraOFTruthFeature);
                }
            }

            var discordantVocieFeature = library.Get<BlueprintFeature>("8064adc641c74e4cb821ce048ecd83a2");
            discordantVocieFeature.AddComponent(Common.createPrerequisiteArchetypeLevel(paladin, archetype, 8, any: true));
        }
    }
}
