using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;
using Kingmaker.View.Animation;
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
        static void Prefix(ref bool forDollRoom)
        {
            forDollRoom = false;
        }
    }
    class Estoc
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var estoc = library.Get<BlueprintWeaponType>("d516765b3c2904e4a939749526a52a9a");
            var parameters = Helpers.GetField<WeaponVisualParameters>(estoc, "m_VisualParameters");
            Helpers.SetField(parameters, "m_WeaponAnimationStyle", WeaponAnimationStyle.SlashingOneHanded);
            Helpers.SetField(estoc, "m_VisualParameters", parameters);
        }
    }
}
