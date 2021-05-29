using CallOfTheWild;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CallOfTheWild.Common;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static TweakOrTreat.MasterChymist;

namespace TweakOrTreat
{
    public class OppositionSpellListUnitPart : UnitPart
    {
        [JsonProperty]
        Dictionary<Spellbook, List<BlueprintSpellList>> oppositions = new Dictionary<Spellbook, List<BlueprintSpellList>>();

        public void AddOpposition(Spellbook spellbook, BlueprintSpellList spellList)
        {
            List<BlueprintSpellList> spellLists;
            if (!oppositions.TryGetValue(spellbook, out spellLists))
            {
                spellLists = new List<BlueprintSpellList>();
                oppositions.Add(spellbook, spellLists);
            }
            spellLists.Add(spellList);
        }

        public void RemoveOpposition(Spellbook spellbook, BlueprintSpellList spellList)
        {
            List<BlueprintSpellList> spellLists;
            if (oppositions.TryGetValue(spellbook, out spellLists))
            {
                spellLists.Remove(spellList);
                if (spellLists.Count == 0)
                    oppositions.Remove(spellbook);
            }
        }

        public bool IsOpposed(Spellbook spellbook, BlueprintAbility spell)
        {
            if (!oppositions.ContainsKey(spellbook))
            {
                return false;
            }

            foreach (var spellList in oppositions[spellbook])
            {
                if (spellList.Contains(spell))
                {
                    return true;
                }
            }

            return false;
        }

        
    }

    public static class OppostitionExtension {
        public static bool isOpposed(this Spellbook spellbook, BlueprintAbility spell)
        {
            if (spellbook.m_SpecialLists.Exists((BlueprintSpellList p) => p.Contains(spell)))
            {
                return false;
            }
            var owner = spellbook.Owner;
            return owner.Ensure<OppositionSpellListUnitPart>().IsOpposed(spellbook, spell);
        }
    }

    public class AddSpellListOppositionSchool : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintSpellList spellList;
        public BlueprintCharacterClass characterClass;
        public override void OnFactActivate()
        {
            if (base.Owner.GetSpellbook(this.characterClass) == null)
            {
                return;
            }
            var spellbook = base.Owner.DemandSpellbook(this.characterClass);
            this.Owner.Ensure<OppositionSpellListUnitPart>().AddOpposition(spellbook, spellList);
        }


        public override void OnFactDeactivate()
        {
            if (base.Owner.GetSpellbook(this.characterClass) == null)
            {
                return;
            }
            var spellbook = base.Owner.DemandSpellbook(this.characterClass);
            this.Owner.Ensure<OppositionSpellListUnitPart>().RemoveOpposition(spellbook, spellList);
        }
    }



    [HarmonyLib.HarmonyPatch(typeof(SpellItemBase), nameof(SpellItemBase.IsTwoSlots))]
    class SpellItemBase_IsTwoSlots
    {
        static void Postfix(SpellItemBase __instance, ref bool __result)
        {
            __result = __result || Game.Instance.UI.SpellBookController.CurrentSpellbook.isOpposed(__instance.Spell);
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(Spellbook), nameof(Spellbook.CalcSlotsCost))]
    class Spellbook_CalcSlotsCost
    {
        static void Postfix(Spellbook __instance, ref int __result, BlueprintAbility spell)
        {
            if (__instance.isOpposed(spell))
            {
                __result = 2;
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetAvailableForCastSpellCount))]
    class Spellbook_GetAvailableForCastSpellCount
    {
        static void Postfix(Spellbook __instance, ref int __result, AbilityData spell)
        {
            if (__instance.isOpposed(spell.Blueprint))
            {
                __result /= 2;
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetMemorizeSlots))]
    class Spellbook_GetMemorizeSlots
    {
        static bool isOpposed(bool prev, Spellbook sp, BlueprintAbility spell)
        {
            return prev || sp.isOpposed(spell);
        }

        static IEnumerable<HarmonyLib.CodeInstruction> Transpiler(IEnumerable<HarmonyLib.CodeInstruction> instructions)
        {
            foreach(var code in instructions)
            {
                if (code.opcode == System.Reflection.Emit.OpCodes.Callvirt && code.operand.ToString().Contains("Contains")) {
                    yield return code;
                    yield return new HarmonyLib.CodeInstruction(
                        System.Reflection.Emit.OpCodes.Ldarg_0
                    );
                    yield return new HarmonyLib.CodeInstruction(
                        System.Reflection.Emit.OpCodes.Ldarg_2
                    );
                    yield return new HarmonyLib.CodeInstruction(
                        System.Reflection.Emit.OpCodes.Call,
                        new Func<bool, Spellbook, BlueprintAbility, bool>(isOpposed).Method
                    );
                }
                else
                {
                    yield return code;
                }
            }
        }
    }


    [HarmonyLib.HarmonyPatch(typeof(Spellbook), nameof(Spellbook.Memorize))]
    class Spellbook_Memorize
    {
        static bool isOpposed(bool prev, Spellbook sp, AbilityData spell)
        {
            return prev || sp.isOpposed(spell.Blueprint);
        }

        static IEnumerable<HarmonyLib.CodeInstruction> Transpiler(IEnumerable<HarmonyLib.CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.opcode == System.Reflection.Emit.OpCodes.Callvirt && code.operand.ToString().Contains("Contains"))
                {
                    yield return code;
                    yield return new HarmonyLib.CodeInstruction(
                        System.Reflection.Emit.OpCodes.Ldarg_0
                    );
                    yield return new HarmonyLib.CodeInstruction(
                        System.Reflection.Emit.OpCodes.Ldarg_1
                    );
                    yield return new HarmonyLib.CodeInstruction(
                        System.Reflection.Emit.OpCodes.Call,
                        new Func<bool, Spellbook, AbilityData, bool>(isOpposed).Method
                    );
                }
                else
                {
                    yield return code;
                }
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(LevelUpRecommendationEx), nameof(LevelUpRecommendationEx.SpellPriority))]
    class LevelUpRecommendationEx_SpellPriority
    {
        static void Postfix(ref RecommendationPriority __result, BlueprintScriptableObject blueprint, LevelUpState levelUpState)
        {
            BlueprintAbility blueprintAbility = blueprint as BlueprintAbility;
            if (blueprintAbility == null || blueprintAbility.Type != AbilityType.Spell)
            {
                return;
            }

            ClassData classData = levelUpState.Unit.Progression.GetClassData(levelUpState.SelectedClass);
            BlueprintSpellbook blueprintSpellbook = (classData != null) ? classData.Spellbook : null;
            if (blueprintSpellbook != null)
            {
                Spellbook spellbook = levelUpState.Unit.DemandSpellbook(blueprintSpellbook);
                if(spellbook.m_SpecialLists.Exists((BlueprintSpellList p) => p.Contains(blueprintAbility)))
                {
                    __result = RecommendationPriority.Good;
                    return;
                }

                if(spellbook.isOpposed(blueprintAbility))
                {
                    __result = RecommendationPriority.Bad;
                    return;
                }
            }
        }
    }

    
    class VoidSchool
    {
        static LibraryScriptableObject library => Main.library;

        public static string spellListToString(BlueprintSpellList spellList)
        {
            var voidSpellsString = "";

            foreach (var level in spellList.SpellsByLevel.Skip(1))
            {
                voidSpellsString += PrerequisiteExtractsLevel.AddOrdinal(level.SpellLevel) + " - ";
                voidSpellsString += String.Join(", ", level.Spells.Select(s => s.Name)) + "\n";
            }

            return voidSpellsString;
        }

        public static string dumpWizardSpells()
        {
            BlueprintSpellList spellList = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
            var voidSpellsString = "";

            foreach (var level in spellList.SpellsByLevel.Skip(1))
            {
                //voidSpellsString += PrerequisiteExtractsLevel.AddOrdinal(level.SpellLevel) + " - ";
                //voidSpellsString += String.Join(", ", level.Spells.Select(s => s.Name)) + "\n";
                foreach(var spell in level.Spells)
                {
                    Main.logger.Log($"new SpellId(\"{spell.AssetGuid}\", {level.SpellLevel}), // {spell.Name}");
                }
            }

            return voidSpellsString;
        }

        static public void load()
        {
            //dumpWizardSpells();
            var schoolSelection = library.Get<BlueprintFeatureSelection>("5f838049069f1ac4d804ce0862ab5110");
            var wizard = Helpers.GetClass("ba34257984f4c41408ce1dc2004e342e");
            var arcanist = CallOfTheWild.Arcanist.arcanist_class;
            //var earthDomainSpellList = library.Get<BlueprintSpellList>("df3bc5bda7deb9d46b0f177db3bb7876");

            var voidSpells = new Common.ExtraSpellList(
                new SpellId("3e9d1119d43d07c4c8ba9ebfd1671952", 1), //hurricane bow
                new SpellId("ef768022b0785eb43a18969903c537c4", 1), //shield
                new SpellId("2c38da66e5a599347ac95b3294acbe00", 1), //true strike
                new SpellId(CallOfTheWild.NewSpells.haunting_mists.AssetGuid, 2),
                new SpellId("89940cde01689fb46946b2f8cd7b66b7", 2), // invisibility
                new SpellId("30e5dc243f937fc4b95d2f8f4e1b7ff3", 2), // see invisibility
                new SpellId("92681f181b507b34ea87018e8f7a528a", 3), // dispel
                new SpellId(CallOfTheWild.SpiritualWeapons.twilight_knife.AssetGuid, 3),
                new SpellId("cf6c901fb7acc904e85c63b342e9c949", 4), // confusion
                new SpellId("eabf94e4edc6e714cabd96aa69f8b207", 5), // mind fog
                new SpellId("f0f761b808dc4b149b08eaf44b99f633", 6), // dispel
                new SpellId("b3da3fbee6a751d4197e446c7e852bcb", 6), // true seeing
                new SpellId("2b044152b3620c841badb090e01ed9de", 7), // insanity
                new SpellId("df2a0ba6b6dcecf429cbb80a56fee5cf", 8), // mind blank
                new SpellId(CallOfTheWild.NewSpells.orb_of_the_void.AssetGuid, 8),
                new SpellId(CallOfTheWild.NewSpells.time_stop.AssetGuid, 9),
                new SpellId("1f01a098d737ec6419aedc4e7ad61fdd", 9) //foresight
            ).createSpellList("VoidSchoolSpellList", "");

            var fireSpells = new Common.ExtraSpellList(
                new SpellId("4783c3709a74a794dbe7c8e7e0b1b038", 1), //burning hands
                new SpellId("42a65895ba0cb3a42b6019039dd2bff1", 2), //molten orb
                new SpellId("21ffef7791ce73f468b6fca4d9371e8b", 2), // resist energy
                new SpellId("cdb106d53c65bbc4086183d54c3b97c7", 2), // scorching ray
                new SpellId(CallOfTheWild.NewSpells.fiery_shiriken.AssetGuid, 2), 
                new SpellId("1724061e89c667045a6891179ee2e8e7", 2), // summon monster 2

                new SpellId("7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), // resist communal
                new SpellId("2d81362af43aeac4387a3d4fced489c3", 3), //fireball
                new SpellId("d2f116cfe05fcdd4a94e80143b67046f", 3), //protection from energy
                new SpellId(CallOfTheWild.NewSpells.flame_arrow.AssetGuid, 3), 

                new SpellId("f72f8f03bf0136c4180cd1d70eb773a5", 4), // controlled fireball
                new SpellId("5e826bcdfde7f82468776b55315b2403", 4), // dragons breath
                new SpellId("690c90a82bf2e58449c6b541cb8ea004", 4), // elemental body 1
                new SpellId(CallOfTheWild.NewSpells.fire_shield.AssetGuid, 4),
                new SpellId("e48638596c955a74c8a32dbc90b518c1", 4), // obsidian flow
                new SpellId("76a629d019275b94184a1a8733cac45e", 4), // protection from energy communal
                new SpellId("7ed74a3ec8c458d4fb50b192fd7be6ef", 4), // summon monster 4
                new SpellId(CallOfTheWild.NewSpells.wall_of_fire.AssetGuid, 4),
                new SpellId("16ce660837fb2544e96c3b7eaad73c63", 4), // volcanic storm

                new SpellId("6d437be73b459594ab103acdcae5b9e2", 5), // elemental body 2
                new SpellId("ebade19998e1f8542a1b55bd4da766b3", 5), // firesnake
                new SpellId("630c8b85d9f07a64f917d79cb5905741", 5), // summon monster 5

                new SpellId("459e6d5aab080a14499e13b407eb3b85", 6), // elemental body 3
                new SpellId("093ed1d67a539ad4c939d9d05cfe192c", 6), // sirocco
                new SpellId("7d700cdf260d36e48bb7af3a8ca5031f", 6), // tar pool
                new SpellId("e740afbab0147944dab35d83faa0ae1c", 6), // summon monster 6

                new SpellId("98734a2665c18cd4db71878b0532024a", 7), // firebrand
                new SpellId("376db0590f3ca4945a8b6dc16ed14975", 7), // elemental body 4
                new SpellId("ab167fd8203c1314bac6568932f1752f", 7), // summon monster 7

                new SpellId("d3ac756a229830243a72e84f3ab050d0", 8), // summon monster 8
                new SpellId(CallOfTheWild.NewSpells.incendiary_cloud.AssetGuid, 8),

                new SpellId(CallOfTheWild.NewSpells.meteor_swarm.AssetGuid, 9),
                new SpellId("08ccad78cac525040919d51963f9ac39", 9) //fiery body
            ).createSpellList("FireSchoolSpellList", "");

            var airSpells = new Common.ExtraSpellList(
                new SpellId("ab395d2335d3f384e99dddee8562978f", 1), // Shocking Grasp

                new SpellId("21ffef7791ce73f468b6fca4d9371e8b", 2), // Resist Energy
                new SpellId("861f589c9c1e4ddfa433cfe1a3c6103e", 2), // Aggressive Thundercloud
                new SpellId("1724061e89c667045a6891179ee2e8e7", 2), // Summon Monster II

                new SpellId("d2cff9243a7ee804cb6d5be47af30c73", 3), // Lightning Bolt
                new SpellId("d2f116cfe05fcdd4a94e80143b67046f", 3), // Protection From Energy
                new SpellId("7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), // Resist Energy, Communal
                new SpellId("e70ed627e4ea48ceb44bb9c314d99994", 3), // Fly
                new SpellId("57cb4fc531204af6864eaf4154f14ae4", 3), // Cloak of Winds

                new SpellId("5e826bcdfde7f82468776b55315b2403", 4), // Dragon's Breath
                new SpellId("690c90a82bf2e58449c6b541cb8ea004", 4), // Elemental Body I
                new SpellId("76a629d019275b94184a1a8733cac45e", 4), // Protection from Energy, Communal
                new SpellId("f09453607e683784c8fca646eec49162", 4), // Shout
                new SpellId("7ed74a3ec8c458d4fb50b192fd7be6ef", 4), // Summon Monster IV

                new SpellId("6d437be73b459594ab103acdcae5b9e2", 5), // Elemental Body II
                new SpellId("630c8b85d9f07a64f917d79cb5905741", 5), // Summon Monster V
                new SpellId("fcf79679d60a419ba240702fe57ba1c3", 5), // Suffocation
                new SpellId("3433e2f772c647df9bf0ac09d0a483d9", 5), // Overland Flight
                new SpellId("1b97b369427646d88af852555d610fa3", 5), // Fickle Winds

                new SpellId("645558d63604747428d55f0dd3a4cb58", 6), // Chain Lightning
                new SpellId("459e6d5aab080a14499e13b407eb3b85", 6), // Elemental Body III
                new SpellId("093ed1d67a539ad4c939d9d05cfe192c", 6), // Sirocco
                new SpellId("e740afbab0147944dab35d83faa0ae1c", 6), // Summon Monster VI

                new SpellId("376db0590f3ca4945a8b6dc16ed14975", 7), // Elemental Body IV
                new SpellId("ab167fd8203c1314bac6568932f1752f", 7), // Summon Monster VII
                new SpellId("fc8b00640b5d409abce2f12678e9a9ac", 7), // Fly, Mass
                new SpellId("14255e04fe8e470eb0594498a8af84f8", 7), // Scouring Winds

                new SpellId("fd0d3840c48cafb44bb29e8eb74df204", 8), // Shout, Greater
                new SpellId("7cfbefe0931257344b2cb7ddc4cdff6f", 8), // Stormbolts
                new SpellId("d3ac756a229830243a72e84f3ab050d0", 8), // Summon Monster VIII

                new SpellId("b660fdaeaff44b53af80245ac73affaf", 9), // Winds of Vengeance
                new SpellId("ef1fb31492974822a1306796edb22f1b", 9) // Suffocation, Mass
            ).createSpellList("AirSchoolSpellList", "");

            var earthSpells = new Common.ExtraSpellList(
                new SpellId("95810d2829895724f950c8c4086056e7", 1), // Corrosive Touch
                new SpellId("95851f6e85fe87d4190675db0419d112", 1), // Grease
                new SpellId("85067a04a97416949b5d1dbf986d93f3", 1), // Stone Fist
                new SpellId("9a46dfd390f943647ab4395fc997936d", 2), // Acid Arrow
                new SpellId("29ccc62632178d344ad0be0865fd3113", 2), // Create Pit
                new SpellId("ce7dad2b25acf85429b6c9550787b2d9", 2), // Glitterdust
                new SpellId("42a65895ba0cb3a42b6019039dd2bff1", 2), // Molten Orb
                new SpellId("21ffef7791ce73f468b6fca4d9371e8b", 2), // Resist Energy
                new SpellId("5181c2ed0190fc34b8a1162783af5bf4", 2), // Stone Call
                new SpellId("1724061e89c667045a6891179ee2e8e7", 2), // Summon Monster II

                new SpellId("d2f116cfe05fcdd4a94e80143b67046f", 3), // Protection From Energy
                new SpellId("7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), // Resist Energy, Communal
                new SpellId("46097f610219ac445b4d6403fc596b9f", 3), // Spiked Pit
                new SpellId("68a9e6d7256f1354289a39003a46d826", 3), // Stinking Cloud

                new SpellId("1407fb5054d087d47a4c40134c809f12", 4), // Acid Pit
                new SpellId("5e826bcdfde7f82468776b55315b2403", 4), // Dragon's Breath
                new SpellId("690c90a82bf2e58449c6b541cb8ea004", 4), // Elemental Body I
                new SpellId("e48638596c955a74c8a32dbc90b518c1", 4), // Obsidian Flow
                new SpellId("76a629d019275b94184a1a8733cac45e", 4), // Protection from Energy, Communal
                new SpellId("c66e86905f7606c4eaa5c774f0357b2b", 4), // Stoneskin
                new SpellId("7ed74a3ec8c458d4fb50b192fd7be6ef", 4), // Summon Monster IV

                new SpellId("c543eef6d725b184ea8669dd09b3894c", 5), // Acidic Spray
                new SpellId("6d437be73b459594ab103acdcae5b9e2", 5), // Elemental Body II
                new SpellId("f63f4d1806b78604a952b3958892ce1c", 5), // Hungry Pit
                new SpellId("7c5d556b9a5883048bf030e20daebe31", 5), // Stoneskin, Communal
                new SpellId("630c8b85d9f07a64f917d79cb5905741", 5), // Summon Monster V
                new SpellId("e0360073ef70072d0c862db39d67da1a", 5), // Corrosive Consumption

                new SpellId("dbf99b00cd35d0a4491c6cc9e771b487", 6), // Acid Fog
                new SpellId("459e6d5aab080a14499e13b407eb3b85", 6), // Elemental Body III
                new SpellId("e243740dfdb17a246b116b334ed0b165", 6), // Stone to Flesh
                new SpellId("e740afbab0147944dab35d83faa0ae1c", 6), // Summon Monster VI
                new SpellId("7d700cdf260d36e48bb7af3a8ca5031f", 6), // Tar Pool

                new SpellId("8c29e953190cc67429dc9c701b16b7c2", 7), // Caustic Eruption
                new SpellId("376db0590f3ca4945a8b6dc16ed14975", 7), // Elemental Body IV
                new SpellId("ab167fd8203c1314bac6568932f1752f", 7), // Summon Monster VII

                new SpellId("d3ac756a229830243a72e84f3ab050d0", 8), // Summon Monster VIII
                new SpellId("6c5bdb1d98ed4b7496b4263243edd15e", 8), // Iron Body

                new SpellId("01300baad090d634cb1a1b2defe068d6", 9) // Clashing Rocks
            ).createSpellList("EarthSchoolSpellList", "");

            var waterSpells = new Common.ExtraSpellList(
                new SpellId("4acf55ad7fec47478ded06d9e929fa06", 1), // Obscuring Mist

                new SpellId("b6010dda6333bcf4093ce20f0063cd41", 2), // Frigid Touch
                new SpellId("21ffef7791ce73f468b6fca4d9371e8b", 2), // Resist Energy
                new SpellId("1724061e89c667045a6891179ee2e8e7", 2), // Summon Monster II

                new SpellId("d2f116cfe05fcdd4a94e80143b67046f", 3), // Protection From Energy
                new SpellId("7bb0c402f7f789d4d9fae8ca87b4c7e2", 3), // Resist Energy, Communal
                new SpellId("95574c36b2b4487499757994c8d2d472", 3), // Sleet Storm

                new SpellId("5e826bcdfde7f82468776b55315b2403", 4), // Dragon's Breath
                new SpellId("690c90a82bf2e58449c6b541cb8ea004", 4), // Elemental Body I
                new SpellId("fcb028205a71ee64d98175ff39a0abf9", 4), // Ice Storm
                new SpellId("76a629d019275b94184a1a8733cac45e", 4), // Protection from Energy, Communal
                new SpellId("fbec7a5a3ec14e6488fa13ebdf6f7f33", 4), // Solid Fog
                new SpellId("7ed74a3ec8c458d4fb50b192fd7be6ef", 4), // Summon Monster IV

                new SpellId("548d339ba87ee56459c98e80167bdf10", 5), // Cloudkill
                new SpellId("e7c530f8137630f4d9d7ee1aa7b1edc0", 5), // Cone of Cold
                new SpellId("6d437be73b459594ab103acdcae5b9e2", 5), // Elemental Body II
                new SpellId("65e8d23aef5e7784dbeb27b1fca40931", 5), // Icy Prison
                new SpellId("630c8b85d9f07a64f917d79cb5905741", 5), // Summon Monster V

                new SpellId("5ef85d426783a5347b420546f91a677b", 6), // Cold Ice Strike
                new SpellId("459e6d5aab080a14499e13b407eb3b85", 6), // Elemental Body III
                new SpellId("e740afbab0147944dab35d83faa0ae1c", 6), // Summon Monster VI
                new SpellId("a878ed50be344fd8990721bbec85e658", 6), // Fluid Form
                new SpellId("48d635c409ab422a8ea2d36b2b51ffe9", 6), // Freezing Sphere

                new SpellId("376db0590f3ca4945a8b6dc16ed14975", 7), // Elemental Body IV
                new SpellId("ab167fd8203c1314bac6568932f1752f", 7), // Summon Monster VII
                new SpellId("f88eb4ba88694047b8cf321ebddfbdef", 7), // Ice Body

                new SpellId("08323922485f7e246acb3d2276515526", 8), // Horrid Wilting
                new SpellId("17696c144a0194c478cbe402b496cb23", 8), // Polar Ray
                new SpellId("7ef49f184922063499b8f1346fb7f521", 8), // Seamantle
                new SpellId("d3ac756a229830243a72e84f3ab050d0", 8), // Summon Monster VIII

                new SpellId("1852a9393a23d5741b650a1ea7078abc", 9), // Icy Prison, Mass
                new SpellId("d8144161e352ca846a73cf90e85bf9ac", 9) // Tsunami
            ).createSpellList("WaterSchoolSpellList", "");

            var voidProgression = Helpers.CreateProgression(
                "VoidSchoolProgression",
                "Elemental School - Void",
                "A wizard who specializes in the void element gains a number of school powers and one bonus spell slot of each level that the wizard can cast, from 1st on up. This bonus spell slot must be used to prepare a spell from the void elemental school’s spell list. Unlike a normal arcane school, the void elemental school requires the wizard to select a single element (air, earth, fire, or water) as his opposition school. A wizard must expend two spell slots to prepare a spell from his opposed elemental school as normal. He does not need to select a second opposition school.\n Void spell list:\n" + spellListToString(voidSpells),
                "",
                //Helpers.GetIcon("4737294a66c91b844842caee8cf505c8"),
                Helpers.GetIcon("567801abe990faf4080df566fadcd038"),
                FeatureGroup.None
            );

            var oppositionSelection = library.Get<BlueprintFeatureSelection>("6c29030e9fea36949877c43a6f94ff31");
            foreach(var f in oppositionSelection.AllFeatures)
            {
                f.AddComponent(voidProgression.PrerequisiteNoFeature());
            }

            var fireOpposition = Helpers.CreateFeature(
                "FireOppositonSchoolFeature",
                "Opposition Elemental School - Fire",
                "The fire elementalist sees a world around him that is made to burn, and he can bring that fire to consume his foes. He has also learned that fire can purify and protect, if properly controlled.\nFire spell list:\n" + spellListToString(fireSpells),
                "",
                Helpers.GetIcon("c3724cfbe98875f4a9f6d1aabd4011a6"),
                FeatureGroup.None,
                Helpers.Create<AddSpellListOppositionSchool>(
                    a =>
                    {
                        a.characterClass = wizard;
                        a.spellList = fireSpells;
                    }
                ),
                Helpers.Create<AddSpellListOppositionSchool>(
                    a =>
                    {
                        a.characterClass = arcanist;
                        a.spellList = fireSpells;
                    }
                ),
                voidProgression.PrerequisiteFeature(true)
            );

            var airOpposition = Helpers.CreateFeature(
                "AirOppositonSchoolFeature",
                "Opposition Elemental School - Air",
                "The air elementalist uses the forces of the wind, sky, clouds, and lightning to confuse and destroy his foes, all while flying through the air with ease.\nAir spell list:\n" + spellListToString(airSpells),
                "",
                Helpers.GetIcon("d7d18ce5c24bd324d96173fdc3309646"),
                FeatureGroup.None,
                Helpers.Create<AddSpellListOppositionSchool>(
                    a =>
                    {
                        a.characterClass = wizard;
                        a.spellList = airSpells;
                    }
                ),
                Helpers.Create<AddSpellListOppositionSchool>(
                    a =>
                    {
                        a.characterClass = arcanist;
                        a.spellList = airSpells;
                    }
                ),
                voidProgression.PrerequisiteFeature(true)
            );

            var earthOpposition = Helpers.CreateFeature(
                "EarthOppositonSchoolFeature",
                "Opposition Elemental School - Earth",
                "The earth elementalist draws power from the stone around him, shaping it, shattering it, and bending it to his will. He can use it to defend himself or cause it to rise up and crush his foes.\nEarth spell list:\n" + spellListToString(earthSpells),
                "",
                Helpers.GetIcon("b6a604dab356ac34788abf4ad79449ec"),
                FeatureGroup.None,
                Helpers.Create<AddSpellListOppositionSchool>(
                    a =>
                    {
                        a.characterClass = wizard;
                        a.spellList = earthSpells;
                    }
                ),
                Helpers.Create<AddSpellListOppositionSchool>(
                    a =>
                    {
                        a.characterClass = arcanist;
                        a.spellList = earthSpells;
                    }
                ),
                voidProgression.PrerequisiteFeature(true)
            );

            var waterOpposition = Helpers.CreateFeature(
                "WaterOppositonSchoolFeature",
                "Opposition Elemental School - Water",
                "The water elementalist draws magic from the ocean depths. His power is fluid as well, crushing foes in mighty waves or wearing them down through timeless erosion.\nWater spell list:\n" + spellListToString(waterSpells),
                "",
                Helpers.GetIcon("c451fde0aec46454091b70384ea91989"),
                FeatureGroup.None,
                Helpers.Create<AddSpellListOppositionSchool>(
                    a =>
                    {
                        a.characterClass = wizard;
                        a.spellList = waterSpells;
                    }
                ),
                Helpers.Create<AddSpellListOppositionSchool>(
                    a =>
                    {
                        a.characterClass = arcanist;
                        a.spellList = waterSpells;
                    }
                ),
                voidProgression.PrerequisiteFeature(true)
            );

            var oppositionSchools = new BlueprintFeature[] { fireOpposition, airOpposition, earthOpposition, waterOpposition };

            oppositionSelection.AllFeatures = oppositionSelection.AllFeatures.AddToArray(oppositionSchools);

            voidProgression.Classes = new BlueprintCharacterClass[] { wizard, arcanist };
            voidProgression.UIDeterminatorsGroup = new BlueprintFeatureBase[] {
                oppositionSelection
            }.AddToArray(oppositionSchools);

            var oppositionResearch = CallOfTheWild.WizardDiscoveries.opposition_research;
            var arcanistOppositionResearch = ArcaneDiscoveryExploit.arcanistOppositionResearch;

            foreach(var opp in oppositionSchools)
            {
                var feature = Helpers.CreateFeature(
                    $"OppositionReasearch{opp.name}Feature",
                    $"Opposition Research: {opp.Name.Substring(11)}",
                    oppositionResearch.Description,
                    "",
                    opp.Icon,
                    FeatureGroup.None,
                    opp.PrerequisiteFeature(),
                    Helpers.Create<RemoveFeatureOnApply>(f => f.Feature = opp)
                );
                oppositionResearch.ReplaceComponent<PrerequisiteFeaturesFromList>(
                    p =>
                    {
                        p.Features = p.Features.AddToArray(opp);
                    }
                );
                arcanistOppositionResearch.ReplaceComponent<PrerequisiteFeaturesFromList>(
                    p =>
                    {
                        p.Features = p.Features.AddToArray(opp);
                    }
                );
                oppositionResearch.AllFeatures = oppositionResearch.AllFeatures.AddToArray(feature);
                arcanistOppositionResearch.AllFeatures = arcanistOppositionResearch.AllFeatures.AddToArray(feature);
            }

            var voidAwarness20 = Helpers.CreateFeature(
                "VoidAwarness20Feature",
                "Void Awareness", 
                "Your ability to recognize the void allows your body to react to magical manifestations before you’re even aware of them. You gain a +2 insight bonus on saving throws against spells and spell-like abilities. This bonus increases by +1 for every five wizard levels you possess. At 20th level, whenever you would be affected by a spell or spell-like ability that allows a saving throw, you can roll twice to save against the effect and take the better result.",
                "",
                Helpers.GetIcon("4093d5a0eb5cae94e909eb1e0e1a6b36"),
                FeatureGroup.None,
                Helpers.Create<ModifyD20>(m => { m.Rule = RuleType.SavingThrow; m.RollsAmount = 1; m.TakeBest = true; })
            );
            voidAwarness20.HideInCharacterSheetAndLevelUp = true;

            var voidAwarness = library.CopyAndAdd<BlueprintFeature>("255a5e42f16cf7c4bb0261e7afaef17d", "VoidAwarnessFeature", "");
            voidAwarness.SetNameDescription("Void Awareness", "Your ability to recognize the void allows your body to react to magical manifestations before you’re even aware of them. You gain a +2 insight bonus on saving throws against spells and spell-like abilities. This bonus increases by +1 for every five wizard levels you possess. At 20th level, whenever you would be affected by a spell or spell-like ability that allows a saving throw, you can roll twice to save against the effect and take the better result.");
            voidAwarness.SetIcon(voidAwarness20.Icon);
            voidAwarness.ReplaceComponent<AddSpecialSpellList>(
                a =>
                {
                    a.SpellList = voidSpells;
                    a.CharacterClass = wizard;
                    
                }
            );
            voidAwarness.AddComponent(
                Helpers.Create<AddSpecialSpellList>(
                    a =>
                    {
                        a.SpellList = voidSpells;
                        a.CharacterClass = arcanist;
                    }
                )
            );
            voidAwarness.AddComponents(
                Helpers.Create<Hardy>(
                    h =>
                    {
                        h.ModifierDescriptor = Kingmaker.Enums.ModifierDescriptor.Insight;
                        h.SpellDescriptor = SpellDescriptor.None;
                        h.Value = 1;
                        h.Bonus = Helpers.CreateContextValueRank();
                    }
                ),
                Helpers.CreateContextRankConfig(
                    ContextRankBaseValueType.ClassLevel, ContextRankProgression.StartPlusDivStep, 
                    classes: new BlueprintCharacterClass[] { wizard, arcanist },
                    stepLevel: 5
                ),
                Helpers.CreateAddFeatureOnClassLevel(voidAwarness20, 20, new BlueprintCharacterClass[] { wizard, arcanist }, null)
            
            );
            voidAwarness.HideInUI = false;
            voidAwarness.HideInCharacterSheetAndLevelUp = false;

            var weaknessResource = library.CopyAndAdd<BlueprintAbilityResource>("870a9cc29d8d0e945b7fbd7926378197", "RevealWeaknessResurce", "");
            weaknessResource.m_MaxAmount.Class = new BlueprintCharacterClass[] { wizard, arcanist };
            var revealWeaknessBuff = library.CopyAndAdd<BlueprintBuff>("cc383a9eaae4d2b45a925d442b367b54", "RevealWeaknessBuff", "");
            revealWeaknessBuff.SetNameDescription("Reveal Weakness", "When you activate this school power as a standard action, you select a foe within 30 feet. That creature takes a penalty to its AC and on saving throws equal to 1/2 your wizard level (minimum –1) for 1 round. You can use this ability a number of times per day equal to 3 + your Intelligence bonus.");
            revealWeaknessBuff.Components = new BlueprintComponent[] {
                Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.UntypedStackable, multiplier: -1),
                Helpers.CreateAddContextStatBonus(StatType.SaveFortitude, ModifierDescriptor.UntypedStackable, multiplier: -1),
                Helpers.CreateAddContextStatBonus(StatType.SaveReflex, ModifierDescriptor.UntypedStackable, multiplier: -1),
                Helpers.CreateAddContextStatBonus(StatType.SaveWill, ModifierDescriptor.UntypedStackable, multiplier: -1),
                Helpers.CreateContextRankConfig(
                    ContextRankBaseValueType.ClassLevel, 
                    ContextRankProgression.Div2, 
                    classes: new BlueprintCharacterClass[] { wizard, arcanist },
                    min: 1
                )
            };
            var applyDebuff = Common.createContextActionApplyBuff(
                revealWeaknessBuff, 
                Helpers.CreateContextDuration(1, Kingmaker.UnitLogic.Mechanics.DurationRate.Rounds), 
                is_from_spell: true, 
                is_permanent: false, 
                dispellable: true);

            var revealWeaknessAbility = library.CopyAndAdd<BlueprintAbility>("4d9bf81b7939b304185d58a09960f589", "RevealWeaknessAbility", "");
            revealWeaknessAbility.SetNameDescription(revealWeaknessBuff.Name, revealWeaknessBuff.Description);
            revealWeaknessAbility.CanTargetPoint = false;
            revealWeaknessAbility.Range = AbilityRange.Medium;
            revealWeaknessAbility.SpellResistance = false;
            revealWeaknessAbility.LocalizedDuration = Helpers.oneRoundDuration;

            revealWeaknessAbility.Components = new BlueprintComponent[] {
                Helpers.CreateRunActions(applyDebuff),
                weaknessResource.CreateResourceLogic()
            };

            var revealWeakness = Helpers.CreateFeature(
                "RevealWeaknessFeature",
                revealWeaknessBuff.Name,
                revealWeaknessBuff.Description,
                "",
                revealWeaknessBuff.Icon,
                FeatureGroup.None,
                revealWeaknessAbility.CreateAddFact(),
                weaknessResource.CreateAddAbilityResource()
            );

            var buff = Helpers.CreateBuff(
                "AuraOfPrescienceNonAreaBuff",
                "Aura of Prescience",
                "At 8th level, you can emit a 30-foot aura of void energy for a number of rounds per day equal to your wizard level. Allies within this aura gain a +2 insight bonus on ability checks, attack rolls, damage rolls, saving throws, and skill checks. These rounds do not need to be consecutive.",
                "",
                Helpers.GetIcon("4737294a66c91b844842caee8cf505c8"),
                null,
                //Common.createPrefabLink("6e01d9f56e260ea4088836571d0e6404"),
                Helpers.CreateAddStatBonus(StatType.AdditionalAttackBonus, 2, ModifierDescriptor.Insight),
                Helpers.CreateAddStatBonus(StatType.AdditionalDamage, 2, ModifierDescriptor.Insight),
                Helpers.CreateAddStatBonus(StatType.SaveFortitude, 2, ModifierDescriptor.Insight),
                Helpers.CreateAddStatBonus(StatType.SaveReflex, 2, ModifierDescriptor.Insight),
                Helpers.CreateAddStatBonus(StatType.SaveWill, 2, ModifierDescriptor.Insight),
                Helpers.Create<BuffAllSkillsBonusAbilityValue>(b => { b.Value = 2; b.Descriptor = ModifierDescriptor.Insight; }),
                Helpers.Create<BuffAbilityRollsBonus>(
                    b =>
                    {
                        b.Value = 2;
                        b.Descriptor = ModifierDescriptor.Insight;
                        b.AffectAllStats = true;
                        b.Multiplier = 1;
                    }
                )
            );
            
            var toggle = Common.createToggleAreaEffect(
                buff, 
                30.Feet(), 
                Helpers.CreateConditionsCheckerAnd(),
                AbilityActivationType.WithUnitCommand,
                UnitCommand.CommandType.Standard,
                Common.createPrefabLink("79cd602c3311fda459f1e7c62d7ec9a1"), 
                null
                //null
            );

            toggle.Group = ActivatableAbilityGroup.None;
            //toggle.AddComponent(performance_resource.CreateActivatableResourceLogic(ResourceSpendType.NewRound));
            toggle.DeactivateIfCombatEnded = true;
            toggle.DeactivateIfOwnerDisabled = true;
            toggle.Buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var auraOfPrescience = Common.ActivatableAbilityToFeature(toggle, false);
            var auraOfPrescienceResource = library.CopyAndAdd<BlueprintAbilityResource>("ccd9239740802bd4eab4cb751467205d", "AuraOfPrescienceResurce", "");
            auraOfPrescienceResource.m_MaxAmount.Class = new BlueprintCharacterClass[] { wizard, arcanist };
            toggle.AddComponent(auraOfPrescienceResource.CreateActivatableResourceLogic(ResourceSpendType.NewRound));
            toggle.AddComponent(auraOfPrescienceResource.CreateAddAbilityResource());

            voidProgression.LevelEntries = new LevelEntry[] {
                Helpers.LevelEntry(1, voidAwarness, revealWeakness, oppositionSelection),
                Helpers.LevelEntry(8, auraOfPrescience)
            };

            voidProgression.UIGroups = new UIGroup[] {
                Helpers.CreateUIGroup(revealWeakness, auraOfPrescience)
            };

            var acanistSchoolSelection = library.Get<BlueprintFeatureSelection>("78f8b53b5c524051b2800a5347f1fa29");
            schoolSelection.AllFeatures = schoolSelection.AllFeatures.AddToArray(voidProgression);
            acanistSchoolSelection.AllFeatures = acanistSchoolSelection.AllFeatures.AddToArray(voidProgression);
        }
    }
}
