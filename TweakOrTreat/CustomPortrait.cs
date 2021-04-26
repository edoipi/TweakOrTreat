using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TweakOrTreat
{
    [HarmonyLib.HarmonyPatch(typeof(CustomPortraitsManager), nameof(CustomPortraitsManager.LoadPortrait))]
    class Game_PauseBind_Patch
    {
        static bool Prepare()
        {
            return true;
        }

        static bool Prefix(ref CustomPortraitsManager __instance, ref Sprite __result, string portraitPath, Sprite baseSprite, bool force)
        {
            Sprite result;
            try
            {
                if (!force)
                {
                    Sprite sprite = __instance.m_LoadedPortraits.Get(portraitPath, null);
                    if (sprite)
                    {
                        return sprite;
                    }
                }
                if (File.Exists(portraitPath))
                {
                    byte[] data = File.ReadAllBytes(portraitPath);
                    int width = baseSprite.texture.width;
                    int height = baseSprite.texture.height;
                    Texture2D texture2D = new Texture2D(width, height, TextureFormat.DXT5, false);
                    texture2D.LoadImage(data);
                    Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)width, (float)height), new Vector2(0f, 0f), 200f);
                    __instance.m_LoadedPortraits[portraitPath] = sprite;
                    result = sprite;
                }
                else
                {
                    UberDebug.LogChannel("Disk", "File not found: " + portraitPath, Array.Empty<object>());
                    result = null;
                }
            }
            catch (Exception ex)
            {
                UberDebug.LogChannel("Disk", "The process upload failed: " + ex.ToString(), Array.Empty<object>());
                result = null;
            }
            __result = result;
            return false;
        }
    }
    class CustomPortrait
    {
    }
}
