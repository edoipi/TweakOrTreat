using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace TweakOrTreat
{
    class SylvanTrickster
    {
        public static AbilityRankType FeatureRank { get; private set; }

        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var rouge = Helpers.GetClass("299aa766dee3cbf4790da4efb8c72484");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "SylvanTricksterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Sylvan Trickster");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Sylvan tricksters are rogues who model themselves after the mischievous fey of legend. Though not spellcasters, sylvan tricksters learn minor magical abilities reminiscent of those favored by fey creatures. The journey for many sylvan tricksters began in childhood, learning at an elder’s knee the stories of the fey and the secrets of how to resist their tricks. Sylvan tricksters often live in areas where the boundary with the First World is thin, and a few have even been there— voluntarily or not.");
            });
            Helpers.SetField(archetype, "m_ParentClass", rouge);
            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = rouge.ClassSkills.RemoveFromArray(StatType.SkillKnowledgeWorld).AddToArray(StatType.SkillLoreNature);
            library.AddAsset(archetype, "");

            //var mutagen = library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea");
            var trapfinding = library.Get<BlueprintFeature>("dbb6b3bffe6db3547b31c3711653838e");
            var rougeTalent = library.Get<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93");
            var uncannyDodge = library.Get<BlueprintFeature>("3c08d842e802c3e4eb19d15496145709");
            var improvedUncannyDodge = library.Get<BlueprintFeature>("485a18c05792521459c7d06c63128c79");
            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, trapfinding),
                Helpers.LevelEntry(2, rougeTalent),
                Helpers.LevelEntry(4, rougeTalent, uncannyDodge),
                Helpers.LevelEntry(6, rougeTalent),
                Helpers.LevelEntry(8, rougeTalent, improvedUncannyDodge),
                Helpers.LevelEntry(10, rougeTalent),
                Helpers.LevelEntry(12, rougeTalent),
                Helpers.LevelEntry(14, rougeTalent),
                Helpers.LevelEntry(16, rougeTalent),
                Helpers.LevelEntry(18, rougeTalent),
                Helpers.LevelEntry(20, rougeTalent),
            };

            var resistNaturesLure = library.Get<BlueprintFeature>("ad6a5b0e1a65c3540986cf9a7b006388");
            var woodlandStride = library.Get<BlueprintFeature>("11f4072ea766a5840a46e6660894527d");
            var tricksterTalent = library.CopyAndAdd(rougeTalent, "SylvanTricksterRougeTalent", "");
            tricksterTalent.SetIcon(woodlandStride.Icon);
            tricksterTalent.SetDescription("A sylvan trickster can select a witch hex in place of a rogue talent. At 10th level, the sylvan trickster can select a hex or major hex in place of a rogue talent. At 18th level, a sylvan trickster can select a hex, major hex, or grand hex in place of a rogue talent. She cannot select any hex more than once.");
            var hexSelection = Helpers.CreateFeatureSelection("sylvantricksterWitchHexselection", "Witch Hex",
                "A sylvan trickster can select a witch hex in place of a rogue talent. At 10th level, the sylvan trickster can select a hex or major hex in place of a rogue talent. At 18th level, a sylvan trickster can select a hex, major hex, or grand hex in place of a rogue talent. She cannot select any hex more than once.",
                "", null, FeatureGroup.None);

            //some hex icons are from CotW and not base game
            CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = UnityModManager.modsPath + @"/CallOfTheWild/Icons/";
            var hexEngine = new CallOfTheWild.HexEngine(new BlueprintCharacterClass[] { rouge }, StatType.Intelligence, archetype: archetype);
            BlueprintFeature healing = hexEngine.createHealing(
                "SylvanTricksterHealingHex", Witch.healing.Name, Witch.healing.Description,
                "", "", "", "", "", ""
            );
            BlueprintFeature beastOfIllOmen = hexEngine.createBeastOfIllOmen(
                "SylvanTricksterBeastOfIllOmenHex", Witch.beast_of_ill_omen.Name, Witch.beast_of_ill_omen.Description,
                "", "", ""
            );
            BlueprintFeature slumber = hexEngine.createSlumber(
                "SylvanTricksterSlumberHex", Witch.slumber_hex.Name, Witch.slumber_hex.Description,
                "", "", ""
            );
            BlueprintFeature misfortune = hexEngine.createMisfortune(
                "SylvanTricksterMisfortuneHex", Witch.misfortune_hex.Name, Witch.misfortune_hex.Description,
                "", "", "", ""
            );
            BlueprintFeature fortune = hexEngine.createFortuneHex(
                "SylvanTricksterFortuneHex", Witch.fortune_hex.Name, Witch.fortune_hex.Description,
                "", "", "", ""
            );
            BlueprintFeature flight = hexEngine.CreateFlightHex(
                "SylvanTrickster"+"Flight"+"Hex", Witch.flight_hex.Name, Witch.flight_hex.Description
            );
            flight.AddComponent(Helpers.PrerequisiteClassLevel(rouge, 5));
            BlueprintFeature iceplant = hexEngine.createIceplantHex(
                "SylvanTrickster" + "Iceplant" + "Hex", Witch.iceplant_hex.Name, Witch.iceplant_hex.Description,
                ""
            );
            BlueprintFeature murksight = hexEngine.createMurksightHex(
                "SylvanTrickster" + "Murksight" + "Hex", Witch.murksight_hex.Name, Witch.murksight_hex.Description,
                ""
            );
            BlueprintFeature ameliorating = hexEngine.createAmeliorating(
                "SylvanTrickster" + "Ameliorating" + "Hex", Witch.ameliorating.Name, Witch.ameliorating.Description,
                "", "", "", "", "", ""
            );
            BlueprintFeature evilEye = hexEngine.createEvilEye(
                "SylvanTrickster" + "EvilEye" + "Hex", Witch.evil_eye.Name, Witch.evil_eye.Description,
                "", "", "", "", "", "", "", ""
            );
            BlueprintFeature summerHeat = hexEngine.createSummerHeat(
                "SylvanTrickster" + "SummerHeat" + "Hex", Witch.summer_heat.Name, Witch.summer_heat.Description,
                "", "", "", "", ""
            );
            BlueprintFeature cackle = hexEngine.createCackle(
                "SylvanTrickster" + "Cackle" + "Hex", Witch.cackle.Name, Witch.cackle.Description,
                "", "", "", "",
                "SylvanTrickster" + "CreateCackleToggleAbility"
            );
            BlueprintFeature ward = hexEngine.createWardHex(
                "SylvanTrickster" + "Ward" + "Hex", Witch.ward.Name, Witch.ward.Description
            );
            BlueprintFeature swampsGrasp = hexEngine.createSwampsGrasp(
                "SylvanTrickster" + "SwampGrasp" + "Hex", Witch.swamps_grasp.Name, Witch.swamps_grasp.Description
            );
            //major hexes
            BlueprintFeature majorAmeliorating = hexEngine.createMajorAmeliorating(
                "SylvanTrickster" + "MajorAmerliorating" + "Hex", Witch.major_ameliorating.Name, Witch.major_ameliorating.Description,
                "", "", "", "", "", ""
            );
            BlueprintFeature majorHealing = hexEngine.createMajorHealing(
                "SylvanTrickster" + "MajorHealing" + "Hex", Witch.major_healing.Name, Witch.major_healing.Description,
                "", "", "", "", "", ""
            );
            BlueprintFeature animalSkin = hexEngine.createAnimalSkin(
                "SylvanTrickster" + "AnimalSkin" + "Hex", Witch.animal_skin.Name, Witch.animal_skin.Description,
                "", "", "", ""
            );
            BlueprintFeature agony = hexEngine.createAgony(
                "SylvanTrickster" + "Agony" + "Hex", Witch.agony.Name, Witch.agony.Description,
                "", "", "", ""
            );
            BlueprintFeature beastGift = hexEngine.createBeastGift(
                "SylvanTrickster" + "BeastGift" + "Hex", Witch.beast_gift.Name, Witch.beast_gift.Description,
                "", "", "", ""
            );
            BlueprintFeature harrowingCurse = hexEngine.createHarrowingCurse(
                "SylvanTrickster" + "HarrowingCurse" + "Hex", Witch.harrowing_curse.Name, Witch.harrowing_curse.Description,
                "", "", ""
            );
            BlueprintFeature iceTomb = hexEngine.createIceTomb(
                "SylvanTrickster" + "IceTomb" + "Hex", Witch.ice_tomb.Name, Witch.ice_tomb.Description,
                "", "", "", ""
            );
            BlueprintFeature regenerativeSinew = hexEngine.createRegenerativeSinew(
                "SylvanTrickster" + "RegenerativeSinew" + "Hex", Witch.regenerative_sinew.Name, Witch.regenerative_sinew.Description,
                "", "", "", "", ""
            );
            BlueprintFeature retribution = hexEngine.createRetribution(
                "SylvanTrickster" + "Retribution" + "Hex", Witch.retribution.Name, Witch.retribution.Description,
                "", "", ""
            );
            BlueprintFeature restlessSlumber = hexEngine.createRestlessSlumber(
                "SylvanTrickster" + "RestlessSlumber" + "Hex", Witch.restless_slumber.Name, Witch.restless_slumber.Description
            );
            restlessSlumber.AddComponent(Helpers.PrerequisiteFeature(slumber));
            // grand hexes
            BlueprintFeature animalServant = hexEngine.createAnimalServant(
                "SylvanTrickster" + "AnimalServant" + "Hex", Witch.animal_servant.Name, Witch.animal_servant.Description,
                "", "", "", ""
            );
            BlueprintFeature deathCurse = hexEngine.createDeathCurse(
                "SylvanTrickster" + "DeathCurse" + "Hex", Witch.death_curse.Name, Witch.death_curse.Description,
                "", "", "", ""
            );
            BlueprintFeature layToRest = hexEngine.createLayToRest(
                "SylvanTrickster" + "LayToRest" + "Hex", Witch.lay_to_rest.Name, Witch.lay_to_rest.Description,
                "", "", ""
            );
            BlueprintFeature lifeGiver = hexEngine.createLifeGiver(
                "SylvanTrickster" + "LifeGiver" + "Hex", Witch.life_giver.Name, Witch.life_giver.Description,
                "", "", ""
            );
            BlueprintFeature eternalSlumber = hexEngine.createEternalSlumber(
                "SylvanTrickster" + "EternalSlumber" + "Hex", Witch.eternal_slumber.Name, Witch.eternal_slumber.Description,
                "", "", "", ""
            );

            hexSelection.AllFeatures = new BlueprintFeature[] {
                healing, beastOfIllOmen, slumber, misfortune, fortune, flight,
                iceplant, murksight, ameliorating, evilEye, summerHeat, cackle,
                ward, swampsGrasp,
                majorAmeliorating, majorHealing, animalSkin, agony, beastGift,
                harrowingCurse, iceTomb, regenerativeSinew, retribution, restlessSlumber,
                animalServant, deathCurse, layToRest, lifeGiver, eternalSlumber
            };
            tricksterTalent.AllFeatures = tricksterTalent.AllFeatures.AddToArray(hexSelection);

            //not sure if there should be rogue version
            //var extraHexFeat = library.CopyAndAdd<BlueprintFeatureSelection>("5d3b5b72afb940d4b9aab740d8925b53", "SylvanTricksterExtraHexFeat", "");
            //extraHexFeat.AllFeatures = hexSelection.AllFeatures.ToArray();
            //extraHexFeat.RemoveComponents<PrerequisiteClassLevel>();
            //extraHexFeat.AddComponent(
            //    Helpers.PrerequisiteFeature()
            //);
            //library.AddFeats(extraHexFeat);

            var arcaneMediumArmor = library.Get<BlueprintFeature>("b24897e082896654c8dd64c8fb677363");
            var feyResistance = Helpers.CreateFeature("SylvanTricksterFeyResistance", "Fey Resistance",
                "At 8th level, a sylvan trickster gains DR 2/cold iron. At 11th level and every 3 levels thereafter, this damage reduction increases by 2 (to a maximum of DR 10/cold iron at 20th level).",
                "", arcaneMediumArmor.Icon, FeatureGroup.None,
                Helpers.Create<Kingmaker.UnitLogic.FactLogic.AddDamageResistancePhysical>(
                    a =>
                    {
                        a.Material = PhysicalDamageMaterial.ColdIron;
                        a.BypassedByMaterial = true;
                        a.Value.ValueType = ContextValueType.Rank;
                        a.Value.Value = 2;
                    }
                )
            );
            feyResistance.Ranks = 10;
            feyResistance.AddComponent(
                Helpers.CreateContextRankConfig(
                    baseValueType: ContextRankBaseValueType.FeatureRank,
                    feature: feyResistance,
                    progression: ContextRankProgression.MultiplyByModifier,
                    stepLevel: 2
                )
            );

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(2, tricksterTalent),
                Helpers.LevelEntry(4, tricksterTalent, resistNaturesLure),
                Helpers.LevelEntry(6, tricksterTalent),
                Helpers.LevelEntry(8, tricksterTalent, feyResistance),
                Helpers.LevelEntry(10, tricksterTalent),
                Helpers.LevelEntry(11, feyResistance),
                Helpers.LevelEntry(12, tricksterTalent),
                Helpers.LevelEntry(14, tricksterTalent, feyResistance),
                Helpers.LevelEntry(16, tricksterTalent),
                Helpers.LevelEntry(17, feyResistance),
                Helpers.LevelEntry(18, tricksterTalent),
                Helpers.LevelEntry(20, tricksterTalent, feyResistance),
            };
            rouge.Archetypes = rouge.Archetypes.AddToArray(archetype);
            //var persistantMutagen = library.Get<BlueprintFeature>("75ba281feb2b96547a3bfb12ecaff052");
            //var grandDiscoverySelection = library.Get<BlueprintFeature>("2729af328ab46274394cedc3582d6e98");
            rouge.Progression.UIGroups = rouge.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(tricksterTalent),
                Helpers.CreateUIGroup(resistNaturesLure, feyResistance)
            );
            
        }
    }
}
