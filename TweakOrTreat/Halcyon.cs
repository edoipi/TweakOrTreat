using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    public class SpellWithMaxLevel : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        [JsonProperty]
        bool applied;
        public BlueprintSpellList spellList;
        public int maxSpellLevel;
        public BlueprintSpellbook spellBook;
        public int count;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
        }

        public override void OnFactActivate()
        {
            try
            {
                var levelUp = Game.Instance.UI.CharacterBuildController?.LevelUpController;
                if (Owner == levelUp?.Preview || Owner == levelUp?.Unit)
                {
                    levelUp.State.DemandSpellSelection(spellBook, spellList).SetExtraSpells(count, maxSpellLevel);
                    applied = true;
                }

            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    class Halcyon
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var druid = Helpers.GetClass("610d836f3a3a9ed42a4349b62f002e96");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "HalcyonDruidArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Halcyon Druid");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Druids of the Halcyon Circle embrace the teachings of Old-Mage Jatembe, combining them with traditional druidic practices. Though they revere the natural world, halcyon druids are less focused on emulating its inhabitants. Instead, like Old-Mage Jatembe, they treat with beings from the Outer Sphere. As a sign of their allegiance, these druids wear fantastical masks depicting celestials, fiends, and— most often—the bestial agathions. \nHalcyon druids are peacekeepers, mediating between people and nature, people and the spirit world, and different groups of people.Yet when necessary, they use their magic to fight enemies of peace, especially demons and demon cultists. \nMost halcyon druids serve Nantambu and the surrounding villages—making, memorizing, and arbitrating the unwritten pacts between them.Some halcyon druids, however, are drawn farther afield.They may do so to forge new connections between tribes and villages, to spread the message of peace, or to fight demonic outbreaks.");
            });
            Helpers.SetField(archetype, "m_ParentClass", druid);
            library.AddAsset(archetype, "");

            var goodDomainSpells = library.Get<BlueprintSpellList>("dc242eb60eed94a4eb0640d773780090");
            var halcyonSpelllist = Common.combineSpellLists("HalcyonDruidSpellList", druid.Spellbook.SpellList, goodDomainSpells);

            var halcyonSpellbook = library.CopyAndAdd(druid.Spellbook, "HalcyonDruidSpellbook", "");
            halcyonSpellbook.SpellList = halcyonSpelllist;

            archetype.ReplaceSpellbook = halcyonSpellbook;

            foreach (var s in new BlueprintFeatureSelection[] {
                Common.MysticTheurgeDivineSpellbookSelection,
                Hinterlander.spellbook_selection,
                DawnflowerAnchorite.spellbook_selection,
                HolyVindicator.spellbook_selection})
            {
                foreach (var f in s.AllFeatures)
                {
                    if (f.GetComponents<PrerequisiteClassSpellLevel>().Where(c => c.CharacterClass == druid).Count() > 0)
                    {
                        f.AddComponent(Common.prerequisiteNoArchetype(archetype));
                    }
                }
            }

            Common.addMTDivineSpellbookProgression(druid, halcyonSpellbook, "MysticTheurgeHalcyonDruidProgression",
                                                    Common.createPrerequisiteArchetypeLevel(archetype, 1),
                                                    Common.createPrerequisiteClassSpellLevel(druid, 2)
                                                    );

            Common.addReplaceSpellbook(Hinterlander.spellbook_selection, halcyonSpellbook, "HinterlanderHalcyonDruid",
                                       Common.createPrerequisiteClassSpellLevel(druid, 1),
                                       Common.createPrerequisiteArchetypeLevel(archetype, 1));

            Common.addReplaceSpellbook(DawnflowerAnchorite.spellbook_selection, halcyonSpellbook, "DawnflowerAnchoriteHalcyonDruid",
                                       Common.createPrerequisiteClassSpellLevel(druid, 2),
                                       Common.createPrerequisiteArchetypeLevel(archetype, 1));

            Common.addReplaceSpellbook(HolyVindicator.spellbook_selection, halcyonSpellbook, "HolyVindicatorHalcyonDruid",
                                       Common.createPrerequisiteClassSpellLevel(druid, 1),
                                       Common.createPrerequisiteArchetypeLevel(archetype, 1));

            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = druid.ClassSkills.AddToArray(StatType.SkillPersuasion, StatType.SkillKnowledgeArcana, StatType.SkillLoreReligion);

            var bond = library.Get<BlueprintFeature>("3830f3630a33eba49b60f511b4c8f2a8");
            var summons = library.Get<BlueprintFeature>("b296531ffe013c8499ad712f8ae97f6b");
            var natureSense = library.Get<BlueprintFeature>("3a859e435fdd6d343b80d4970a7664c1");

            var resistNat = library.Get<BlueprintFeature>("ad6a5b0e1a65c3540986cf9a7b006388");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, bond, summons, natureSense, Wildshape.druid_wildshapes_progression),
                Helpers.LevelEntry(4, resistNat)
            };

            var wizardSpellList = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");

            //var naturalArcanaX = Helpers.CreateFeature(
            //    "NaturalArcana",
            //    "Natural Arcana",
            //    "At 4th level a halcyon druid learns all wizard catrips. At 6th level and every 2 levels thereafter, she chooses two spells from the wizard/sorcerer spell list and adds them to her druid spell list. The chosen spells must be at least 1 level lower than the highest level spell she can currently cast. At 20th level, the halcyon druid can choose wizard/sorcerer spells of any level.",
            //    "",
            //    null,
            //    FeatureGroup.None,
            //    Helpers.Create<SpellWithMaxLevel>(
            //        s =>
            //        {
            //            s.spellBook = halcyonSpellbook;
            //            s.spellList = wizardSpellList;
            //            s.count = 2;
            //            s.maxSpellLevel = 2;
            //        }
            //    )
            //);

            //var cantrips = Common.createCantrips(
            //    "NaturalArcanaCantrips",
            //    "Natural Arcana",
            //    "At 4th level a halcyon druid learns all wizard catrips. At 6th level and every 2 levels thereafter, she chooses two spells from the wizard/sorcerer spell list and adds them to her druid spell list. The chosen spells must be at least 1 level lower than the highest level spell she can currently cast. At 20th level, the halcyon druid can choose wizard/sorcerer spells of any level.",
            //    library.Get<BlueprintFeature>("55edf82380a1c8540af6c6037d34f322").Icon,
            //    "",
            //    druid,
            //    StatType.Wisdom,
            //    wizardSpellList.SpellsByLevel[0].Spells.Except(druid.Spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()).ToArray()
            //);

            //foreach(var cartrip in wizardSpellList.SpellsByLevel[0].Spells.Except(druid.Spellbook.SpellList.SpellsByLevel[0].Spells.ToArray()))
            //{
            //    cantrips.AddComponent(
            //        cartrip.CreateAddKnownSpell(druid, 0)
            //    );
            //}

            var naturalArcana = new Dictionary<int, BlueprintFeature>();
            var arcanaDesc = "At 4th level and every 2 levels thereafter, a halcyon druid chooses two spells from the wizard/sorcerer spell list and adds them to her druid spell list. The chosen spells must be at least 1 level lower than the highest level spell she can currently cast. At 20th level, the halcyon druid can choose wizard/sorcerer spells of any level.";
            var arcanaIcon = library.Get<BlueprintFeature>("55edf82380a1c8540af6c6037d34f322").Icon;
            for (int level = 4, i=1; level <= 18; level+=2, i++)
            {
                naturalArcana[level] = Helpers.CreateFeature(
                    "NaturalArcana"+level,
                    "Natural Arcana",
                    arcanaDesc,
                    "",
                    arcanaIcon,
                    FeatureGroup.None,
                    Helpers.Create<SpellWithMaxLevel>(
                        s =>
                        {
                            s.spellBook = halcyonSpellbook;
                            s.spellList = wizardSpellList;
                            s.count = 2;
                            s.maxSpellLevel = i;
                        }
                    )
                );
            }

            naturalArcana[20] = Helpers.CreateFeature(
                "NaturalArcana" + 20,
                "Natural Arcana",
                arcanaDesc,
                "",
                arcanaIcon,
                FeatureGroup.None,
                Helpers.Create<SpellWithMaxLevel>(
                    s =>
                    {
                        s.spellBook = halcyonSpellbook;
                        s.spellList = wizardSpellList;
                        s.count = 2;
                        s.maxSpellLevel = 9;
                    }
                )
            );

            var spont = Helpers.CreateFeature(
                "HalcyonDruidSpontaneousCasting",
                "Spontaneous Casting",
                "A halcyon druid adds all spells from the Good cleric domain to her druid spell list, and she can focus stored spell energy into spells from the Good domain that she hasn’t prepared ahead of time. She can lose a prepared spell to cast any spell of the same level or lower from the Good domain.",
                "",
                library.Get<BlueprintAbility>("eee384c813b6d74498d1b9cc720d61f4").Icon,
                FeatureGroup.None
            );


            var spellsArray = Common.createSpelllistsForSpontaneousConversion(goodDomainSpells.SpellsByLevel.Select(x => x.Spells.FirstOrDefault()).Where(x => x != null).ToArray());

            for (int i = 0; i < spellsArray.Length; i++)
            {
                spont.AddComponent(Common.createSpontaneousSpellConversion(druid, spellsArray[i].ToArray()));
            }

            var resource = Helpers.CreateAbilityResource("HalcyonDruidItemBondResource", "", "", "", null);
            resource.SetFixedResource(1);

            var bondAbilities = new List<BlueprintAbility>();
            var itemBond = library.CopyAndAdd<BlueprintFeature>("2fb5e65bd57caa943b45ee32d825e9b9", "HalcyonDruidItemBondFeature", "");
            foreach (var f in itemBond.GetComponent<AddFacts>().Facts)
            {
                var a = library.CopyAndAdd<BlueprintAbility>(f.AssetGuid, "HalcyonDruid" + f.name, "");
                a.ReplaceComponent<AbilityResourceLogic>(ab => ab.RequiredResource = resource);
                a.SetDescription("A halcyon druid forms a powerful bond with a mask, which functions identically to a wizard’s bonded object except that it can be used to cast druid spells (including those gained from class abilities) instead of wizard spells.");

                bondAbilities.Add(a);
            }
            
            itemBond.SetDescription(bondAbilities[0].Description);
            itemBond.ReplaceComponent<AddFacts>(a => a.Facts = bondAbilities.ToArray());
            itemBond.ReplaceComponent<AddAbilityResources>(a => a.Resource = resource);

            var peacekeeper = Helpers.CreateFeature(
                "PeacekeeperFeature",
                "Peacekeeper",
                "A halcyon druid adds half her class level (minimum 1) to Diplomacy and Knowledge (world) checks.",
                "",
                null,
                FeatureGroup.None,
                Helpers.CreateAddContextStatBonus(StatType.CheckDiplomacy, ModifierDescriptor.None),
                Helpers.CreateAddContextStatBonus(StatType.SkillKnowledgeWorld, ModifierDescriptor.None),
                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { druid }, progression: ContextRankProgression.Div2, min: 1)
            );

            var outsider = library.Get<BlueprintFeature>("9054d3988d491d944ac144e27b6bc318");
            var resist = Helpers.CreateFeature(
                "ResistFiendishInfluenceFeature",
                "Resist Fiendish Influence",
                "At 4th level, a halcyon druid gains a +4 bonus on saving throws against the spell-like and supernatural abilities of outsiders with the evil subtype and spells with the evil descriptor.",
                "",
                null,
                FeatureGroup.None,
                Common.createContextSavingThrowBonusAgainstFact(outsider,
                                                                AlignmentComponent.Evil,
                                                                Common.createSimpleContextValue(4),
                                                                ModifierDescriptor.UntypedStackable),
                Common.createContextSavingThrowBonusAgainstDescriptor(4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Evil)
            );

            var beastShape = library.Get<BlueprintAbility>("940a545a665194b48b722c1f9dd78d53");
            var bResource = Helpers.CreateAbilityResource("BeastShapeResource", "", "", "", null);
            bResource.SetFixedResource(1);
            var bFeature = Helpers.CreateFeature(
                "EmbodyMaskFeature",
                "Embody Mask",
                "At 13th level, a halcyon druid wearing her bonded mask can embody the spirit it represents. This ability requires a standard action to activate and functions as per beast shape IV.",
                "",
                null,
                FeatureGroup.None,
                Helpers.CreateAddAbilityResource(bResource)
            //Helpers.CreateAddFact(bAbility)
            );
            foreach(var variant in beastShape.Variants)
            {
                var bAbility = Common.convertToSuperNatural(variant, "HalcyonDuid", new BlueprintCharacterClass[] { druid }, StatType.Wisdom, bResource);

                bFeature.AddComponent(Helpers.CreateAddFact(bAbility));
            }

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, spont, itemBond, peacekeeper),
                Helpers.LevelEntry(4, resist),
                Helpers.LevelEntry(13, bFeature)
            };

            foreach(var entry in naturalArcana)
            {
                archetype.AddFeatures = archetype.AddFeatures.AddToArray(
                    Helpers.LevelEntry(entry.Key, entry.Value)
                );
            }

            druid.Archetypes = druid.Archetypes.AddToArray(archetype);

            foreach (var group in druid.Progression.UIGroups)
            {
                if (group.Features.Contains(natureSense))
                {
                    group.Features.Add(peacekeeper);
                }
            }
            druid.Progression.UIGroups = druid.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(naturalArcana.Values.ToArray().AddToArray(spont)),
                Helpers.CreateUIGroup(itemBond, bFeature)
            );
        }
    }
}
