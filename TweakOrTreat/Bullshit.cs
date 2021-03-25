using Kingmaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    [HarmonyLib.HarmonyPatch(typeof(Game), "PauseBind")]
    class Game_PauseBind_Patch
    {
        static bool Prepare()
        {
            return false;
        }

        static bool Prefix(ref Game __instance)
        {
            Game.Instance.IsPaused = !Game.Instance.IsPaused;
            return false;
        }
    }
    class Bullshit
    {

    }
}
