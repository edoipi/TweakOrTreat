using CallOfTheWild;
using Kingmaker.Kingdom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.Kingdom.LeaderSlot;

namespace TweakOrTreat
{
    [HarmonyLib.HarmonyPatch(typeof(LeaderState), "GetCharacterStat")]
    class LeaderState_GetCharacterStat_Patch
    {
        [HarmonyLib.HarmonyReversePatch]
        [HarmonyLib.HarmonyPatch(typeof(LeaderState), "GetCharacterStat")]
        public static int Original(object instance, LeaderState.Leader unit, bool withPenalty)
        {
            // its a stub so it has no initial content
            throw new NotImplementedException("It's a stub");
        }

        static bool Prefix(ref LeaderState __instance, MethodBase __originalMethod, ref int __result, LeaderState.Leader unit, bool withPenalty)
        {
            var stats = new StatTypeAttr[] {
                StatTypeAttr.Strength, StatTypeAttr.Dexterity, StatTypeAttr.Constitution,
                StatTypeAttr.Intelligence, StatTypeAttr.Wisdom, StatTypeAttr.Charisma
            };
            __result = 0;
            var originalStat = __instance.CharacterStatType;
            foreach (var stat in stats)
            {
                Helpers.SetField(__instance.Slot, "m_CharacterStat", stat);
                var res = Original(__instance, unit, withPenalty);
                __result = Math.Max(__result, res);
            }
            Helpers.SetField(__instance.Slot, "m_CharacterStat", (StatTypeAttr)originalStat);
            return false;
        }

        static bool Prepare()
        {
            return Main.advisorUseMaxStat;
        }
    }

    class Advisors
    {
    }
}
