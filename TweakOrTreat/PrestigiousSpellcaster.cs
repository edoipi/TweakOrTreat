using Kingmaker.Blueprints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace TweakOrTreat
{
    class PrestigiousSpellcaster
    {
        static LibraryScriptableObject library => Main.library2;
        static internal void load()
        {
            string[] filePaths = Directory.GetFiles(UnityModManager.modsPath + @"/TweakOrTreat/PrestigiousSpellcaster/", "*.json", SearchOption.TopDirectoryOnly);
            var loadPrestigiousSpellCaster = HarmonyLib.AccessTools.TypeByName("ZFavoredClass.Core, ZFavoredClass").GetMethod("loadPrestigiousSpellCaster", BindingFlags.NonPublic | BindingFlags.Static);
            //Main.logger.Log($"");
            foreach (var fp in filePaths)
            {

                loadPrestigiousSpellCaster.Invoke(null, new object[] { fp });
            }
        }
    }
}
