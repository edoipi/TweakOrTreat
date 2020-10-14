using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Kingmaker.View.Animation;
using Kingmaker.View.Equipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    [Harmony12.HarmonyPatch(typeof(ItemEntityWeapon), "GetAnimationStyle")]
    class ItemEntityWeapon_GetAnimationStyle_Patch
    {
        static bool Prepare()
        {
            return Main.twoHandedPreview;
        }

        static void Prefix(ref bool forDollRoom)
        {
            forDollRoom = false;
        }
    }

    [Harmony12.HarmonyPatch(typeof(UnitViewHandsEquipment), "get_ActiveOffHandWeaponStyle")]
    class UnitViewHandsEquipment_ActiveOffHandWeaponStyle_Patch
    {
        static bool Prepare()
        {
            return Main.noHandInTheAir;
        }

        static void Postfix(UnitViewHandsEquipment __instance, ref WeaponAnimationStyle __result)
        {
            if(__instance.ActiveMainHandWeaponStyle != WeaponAnimationStyle.Fist && __result == WeaponAnimationStyle.Fist)
            {
                __result = WeaponAnimationStyle.None;
            }
        }
    }

    [Harmony12.HarmonyPatch(typeof(WeaponVisualParameters), "get_AnimStyle")]
    class WeaponVisualParameters_get_AnimStyle_Patch
    {
        static bool Prepare()
        {
            return Main.slashingFencing;
        }

        static void Postfix(ref WeaponAnimationStyle __result)
        {
            if ( __result == WeaponAnimationStyle.Fencing)
            {
                __result = WeaponAnimationStyle.SlashingOneHanded;
            }
        }
    }

    [Harmony12.HarmonyPatch(typeof(BlueprintWeaponType), "get_IsOneHandedWhichCanBeUsedWithTwoHands")]
    class BlueprintWeaponType_IsOneHandedWhichCanBeUsedWithTwoHands
    {
        static bool Prepare()
        {
            return true;
        }

        static void Postfix(BlueprintWeaponType __instance, ref bool __result)
        {
            if (__instance.Category == Kingmaker.Enums.WeaponCategory.Rapier)
            {
                __result = false;
            }
        }
    }

    class Estoc
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            //var estoc = library.Get<BlueprintWeaponType>("d516765b3c2904e4a939749526a52a9a");
            //var parameters = Helpers.GetField<WeaponVisualParameters>(estoc, "m_VisualParameters");
            //Helpers.SetField(parameters, "m_WeaponAnimationStyle", WeaponAnimationStyle.SlashingOneHanded);
            //Helpers.SetField(estoc, "m_VisualParameters", parameters);
        }
    }
}
