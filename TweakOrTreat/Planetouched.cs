using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TweakOrTreat
{
    //    [HarmonyLib.HarmonyPatch(typeof(UnitDescriptor), "get_OriginalSize")]
    //    class UnitDescriptor_get_OriginalSize_Patch
    //    {
    //        static void Postfix(ref Size __result, ref UnitDescriptor __instance)
    //        {
    //            if (__instance == null)
    //            {
    //                return;
    //            }

    //            if (__instance.HasFact(Planetouched.smallSize))
    //            {
    //                __result = Size.Small;
    //            }
    //        }
    //    }

    //    [HarmonyLib.HarmonyPatch(typeof(Kingmaker.UI.LevelUp.CharBDollStyler), "HandleChooseElement")]
    //    class CharBDollStyler_HandleChooseElement_Patch
    //    {
    //        public static void Postfix()
    //        {
    //            var controller = Game.Instance?.UI?.CharacterBuildController?.LevelUpController;
    //            var preset = controller?.Doll?.RacePreset;
    //            Main.logger.Log("I did something");
    //            if (preset == null)
    //            {
    //                Main.logger.Log("No preset");
    //                return;
    //            }

    //            var sizedPreset = preset as BlueprintRaceVisualPresetWithSize;
    //            Main.logger.Log("Some preset");
    //            if (sizedPreset?.size != null)
    //            {
    //                Main.logger.Log("Size preset");
    //                controller.Unit.AddFact(sizedPreset.size);
    //                controller.Preview.AddFact(sizedPreset.size);
    //                //controller.Unit.State.Size = controller.Unit.OriginalSize;
    //                //Main.logger.Log("New size "+ controller.Unit.State.Size);
    //            } else
    //            {
    //                foreach(var v in Planetouched.raceResizeMap.Values) {
    //                    controller.Unit.RemoveFact(v);
    //                    controller.Preview.RemoveFact(v);
    //                }
    //            }
    //            controller.Unit.State.Size = controller.Unit.OriginalSize;
    //            controller.Preview.State.Size = controller.Preview.OriginalSize;


    //            Main.logger.Log("New size unit " + controller.Unit.State.Size);
    //            Main.logger.Log("New size prev " + controller.Preview.State.Size);
    //            Helpers.SetField(controller, "m_RecalculatePreview", true);
    //            typeof(LevelUpController).GetMethod("UpdatePreview", BindingFlags.NonPublic | BindingFlags.Instance)
    //                        .Invoke(controller, new object[] { });
    //            Main.logger.Log("New size unit " + controller.Unit.State.Size);
    //            Main.logger.Log("New size prev " + controller.Preview.State.Size);
    //        }
    //    }

    //    [HarmonyLib.HarmonyPatch(typeof(DollState), "Validate")]
    //    class DollState_Updated_Patch
    //    {
    //        static void Postfix()
    //        {
    //            Main.logger.Log("new patch");
    //            //CharBDollStyler_HandleChooseElement_Patch.Postfix();
    //        }
    //    }

    //    //[HarmonyLib.HarmonyPatch(typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController), "SetupNewCharacher")]
    //    //class LevelUpController_SetupNewCharacher_Patch
    //    //{
    //    //    static void Postfix(ref Kingmaker.UnitLogic.Class.LevelUp.LevelUpController __instance)
    //    //    {

    //    //        __instance.Unit.State.Size = __instance.Unit.OriginalSize;
    //    //    }
    //    //}
    //    //[Harmony12.HarmonyPatch(typeof(Kingmaker.UnitLogic.Class.LevelUp.LevelUpController), "UpdatePreview")]
    //    //class LevelUpController_SetupNewCharacher_Patch
    //    //{
    //    //    static void Postfix(ref Kingmaker.UnitLogic.Class.LevelUp.LevelUpController __instance)
    //    //    {
    //    //        var preset = __instance?.Doll?.RacePreset;
    //    //        if (preset == null)
    //    //        {
    //    //            return;
    //    //        }

    //    //        var sizedPreset = preset as BlueprintRaceVisualPresetWithSize;

    //    //        if (sizedPreset != null)
    //    //        {
    //    //            __instance.Unit.AddFact(sizedPreset.size);
    //    //            __instance.Unit.State.Size = __instance.Unit.OriginalSize;
    //    //        }
    //    //    }
    //    //}

    public class BlueprintRaceVisualPresetWithSize : BlueprintRaceVisualPreset
    {
        public BlueprintFeature size;
        public BlueprintRaceVisualPresetWithSize()
        {

        }
    }

    class Planetouched
    {
        static LibraryScriptableObject library => Main.library;
        public static BlueprintFeature smallSize;
        public static Dictionary<BlueprintRace, BlueprintFeature> raceResizeMap = new Dictionary<BlueprintRace, BlueprintFeature>();

        static void addPresets(BlueprintRace destRace, BlueprintRace sourceRace)
        {
            var presets = new BlueprintRaceVisualPreset[sourceRace.Presets.Length];
            var skin = destRace.Presets[0].Skin;

            for (int i = 0; i < sourceRace.Presets.Length; i++)
            {
                var sizedPreset = ScriptableObject.CreateInstance<BlueprintRaceVisualPresetWithSize>();
                var src = sourceRace.Presets[i];

                sizedPreset.RaceId = destRace.RaceId;
                sizedPreset.MaleSkeleton = src.MaleSkeleton;
                sizedPreset.FemaleSkeleton = src.FemaleSkeleton;
                //Main.logger.Log("Source size " + sourceRace.Size);
                if (sourceRace.Size < Size.Medium)
                    sizedPreset.size = raceResizeMap[destRace];
                sizedPreset.Skin = skin;
                sizedPreset.name = destRace.name + sourceRace.name + "Preset" + i;

                var guid = Helpers.MergeIds(destRace.AssetGuid, src.AssetGuid);

                library.AddAsset(sizedPreset, guid);

                presets[i] = sizedPreset;
            }

            destRace.Presets = destRace.Presets.AddToArray(presets);
        }

        static void makeSmallRace(BlueprintRace race)
        {
            var raceName = race.Name.ToLower();
            var desc = string.Format("Not all {0}s are descended from humans. Non-human {0}s have the same statistics as human {0}s with the exception of size. Those born of small race are themselves small.", raceName);
            var small = Helpers.CreateFeature("SmallSize" + raceName, "Small Size",
                desc,
                "", null, FeatureGroup.Racial, new BlueprintComponent[] { Helpers.CreateAddFact(smallSize) });

            raceResizeMap[race] = small;
        }

        static internal void load()
        {
            BlueprintRace aasimar = library.Get<BlueprintRace>("b7f02ba92b363064fb873963bec275ee");
            BlueprintRace tiefling = library.Get<BlueprintRace>("5c4e42124dc2b4647af6e36cf2590500");

            BlueprintRace halfling = library.Get<BlueprintRace>("b0c3ef2729c498f47970bb50fa1acd30");
            BlueprintRace gnome = library.Get<BlueprintRace>("ef35a22c9a27da345a4528f0d5889157");
            BlueprintRace halforc = library.Get<BlueprintRace>("1dc20e195581a804890ddc74218bfd8e");
            BlueprintRace dwarf = library.Get<BlueprintRace>("c4faf439f0e70bd40b5e36ee80d06be7");

            var races = new BlueprintRace[] { halfling, gnome, halforc, dwarf };

            smallSize = Helpers.CreateFeature("SmallSizeGeneric", "Small Size",
                "",
                "", null, FeatureGroup.Racial, new BlueprintComponent[] { });
            smallSize.HideInCharacterSheetAndLevelUp = true;
            smallSize.HideInUI = true;

            makeSmallRace(aasimar);
            makeSmallRace(tiefling);

            foreach (var race in races)
            {
                addPresets(aasimar, race);
                addPresets(tiefling, race);
            }
        }
    }
}
