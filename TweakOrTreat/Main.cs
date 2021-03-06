﻿using UnityModManagerNet;
using System;
using System.Reflection;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using CallOfTheWild;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.View.Animation;

namespace TweakOrTreat
{
    internal class Main
    {
        public static bool quickerBardicPerformance;
        public static bool twoHandedPreview;
        public static bool slashingFencing;
        public static bool noHandInTheAir;
        public static bool noTwoHandedRapier;
        public static bool advisorUseMaxStat;
        public static bool spellSlotsFromPermanentBonusOnly;
        public static bool reduceSpellSlotsFromAbilityDrain;
        internal class Settings
        {
            internal Settings()
            {

                using (StreamReader settings_file = File.OpenText("Mods/TweakOrTreat/settings.json"))
                using (JsonTextReader reader = new JsonTextReader(settings_file))
                {
                    JObject jo = (JObject)JToken.ReadFrom(reader);

                    quickerBardicPerformance = (bool)jo["quicker_bardic_performance"];
                    twoHandedPreview = (bool)jo["two_handed_preview"];
                    slashingFencing = (bool)jo["slashing_fencing"];
                    noHandInTheAir = (bool)jo["no_hand_in_the_air"];
                    noTwoHandedRapier = (bool)jo["no_two_handed_rapier"];
                    advisorUseMaxStat = (bool)jo["advisor_use_max_stat"];
                    spellSlotsFromPermanentBonusOnly = (bool)jo["spell_slots_from_permanent_bonus_only"];
                    reduceSpellSlotsFromAbilityDrain = (bool)jo["reduce_spell_slots_from_ability_drain"];
#if DEBUG
                    advisorUseMaxStat = true;
#endif
                }
            }
        }

        static internal Settings settings = new Settings();
        internal static UnityModManagerNet.UnityModManager.ModEntry.ModLogger logger;
        internal static HarmonyLib.Harmony harmony;
        internal static LibraryScriptableObject library;
        internal static LibraryScriptableObject library2;

        static readonly Dictionary<Type, bool> typesPatched = new Dictionary<Type, bool>();
        static readonly List<String> failedPatches = new List<String>();
        static readonly List<String> failedLoading = new List<String>();

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void DebugLog(string msg)
        {
            if (logger != null) logger.Log(msg);
        }
        internal static void DebugError(Exception ex)
        {
            if (logger != null) logger.Log(ex.ToString() + "\n" + ex.StackTrace);
        }
        internal static bool enabled;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;

                harmony = new HarmonyLib.Harmony(modEntry.Info.Id);

                //Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "cecil");
                //Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "./mmdump");

                harmony.PatchAll(Assembly.GetExecutingAssembly());

                //Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "");
            }
            catch (Exception ex)
            {
                DebugError(ex);
                throw ex;
            }
            return true;
        }
        
        public static void dumpWithNonexisting(string guid_file_name)
        {
            Type type = typeof(CallOfTheWild.Helpers.GuidStorage);
            FieldInfo info = type.GetField("guids_in_use", BindingFlags.NonPublic | BindingFlags.Static);
            object value = info.GetValue(null);
            var guids_in_use = value as Dictionary<string, string>;
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(guid_file_name))
            {
                foreach (var pair in guids_in_use)
                {
                    sw.WriteLine(pair.Key + '\t' + pair.Value + '\t');
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        [HarmonyLib.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
        [HarmonyLib.HarmonyAfter("RacesUnleashed", "DerringDo")]
        static class LibraryScriptableObject_LoadDictionary_Patch
        {
            static void Postfix(LibraryScriptableObject __instance)
            {
                var self = __instance;
                if (Main.library != null) return;
                Main.library = self;
                try
                {
                    Main.DebugLog("Loading Tweak Or Treat");

                    //CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = @"./Mods/TweakOrTreat/Icons/";
#if DEBUG                
                    bool allow_guid_generation = true;
#else
                    bool allow_guid_generation = false; //no guids should be ever generated in release
#endif
                    CallOfTheWild.Helpers.GuidStorage.load(Properties.Resources.blueprints, allow_guid_generation);

                    //Bullshit.load();

                    WeaponFamiliarity.load();
                    Overrun.load();

                    UniversalRacialTraits.load();

                    OceansEcho.load();
                    

                    Mindchemist.load();

                    BoneSpikeMutagen.load();

                    MutationWarrior.load();
                    ExtraDiscovery.load();
                    WildStalker.load();
                    //NirmathiIrregular.load();
                    ArcaneDiscoveryExploit.load();

                    Halfling.load();
                    Human.load();

                    Ki.load();
                    SylvanTrickster.load();

                    var nimbleOriginal = library.TryGet<BlueprintFeature>("221d8fee280b48eda3a871fe96c32eb1");
                    if (nimbleOriginal != null)
                        VirtuosoBravo.load();

                    HolyGuide.load();
                    OathAgainstChaos.load();
                    Myrmidarch.load();
                    AWT.load();
                    Stonelord.load();
                    OathOfThePeoplesCouncil.load();

                    Halcyon.load();
                    AncientLorekeeper.load();

                    //PactWizard.load();

                    if (quickerBardicPerformance)
                        BardicPerformance.load();

                    FeySpellVersatility.load();
                    Aaasimar.load();
                    Elf.load();
                    Dwarf.load();

                    Siegebreaker.load();

                    UnbalancingTrick.load();

                    MasterChymist.load();

                    KineticEnhancement.load();

                    VoidSchool.load();

                    BenthicSpell.load();

                    ElementalMaster.load();

                    //Planetouched.load();

                    //var bastard_sword_type = library.Get<BlueprintWeaponType>("d2fe2c5516b56f04da1d5ea51ae3ddfe");
                    //WeaponVisualParameters bs_visuals = GetField<WeaponVisualParameters>(bastard_sword_type, "m_VisualParameters");
                    //SetField(bs_visuals, "m_WeaponAnimationStyle", WeaponAnimationStyle.SlashingOneHanded);

#if DEBUG
                    string guid_file_name = @"./Mods/TweakOrTreat/blueprints.txt";
                    //Main.logger.Log("Dumping bluprints to "+guid_file_name);
                    dumpWithNonexisting(guid_file_name);
                    //CallOfTheWild.Helpers.GuidStorage.dump(guid_file_name);
#endif
                    CallOfTheWild.Helpers.GuidStorage.dump(@"./Mods/TweakOrTreat/loaded_blueprints.txt");
                }
                catch (Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }

        //public static T GetField<T>(object obj, string name)
        //{
        //    return (T)HarmonyLib.AccessTools.Field(obj.GetType(), name).GetValue(obj);
        //}

        //public static void SetField(object obj, string name, object value)
        //{
        //    HarmonyLib.AccessTools.Field(obj.GetType(), name).SetValue(obj, value);
        //}

        [HarmonyLib.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
        [HarmonyLib.HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary", new Type[0])]
        [HarmonyLib.HarmonyAfter("RacesUnleashed", "DerringDo", "ZFavoredClass")]
        [HarmonyLib.HarmonyPriority(HarmonyLib.Priority.Low)]
        static class LibraryScriptableObject_LoadDictionary_Patch2
        {
            static void Postfix(LibraryScriptableObject __instance)
            {
                var self = __instance;
                if (Main.library2 != null) return;
                Main.library2 = self;
                try
                {
                    Main.DebugLog("Loading Tweak Or Treat part 2");
                    string guid_file_name = @"./Mods/TweakOrTreat/blueprints.txt";

#if DEBUG
                    bool allow_guid_generation = true;
#else
                    bool allow_guid_generation = false; //no guids should be ever generated in release
#endif
                    var fileContent = File.ReadAllText(guid_file_name);
                    //Main.logger.Log(fileContent);
                    CallOfTheWild.Helpers.GuidStorage.load(fileContent, allow_guid_generation);

                    HalfElf.load();

                    HalfOrc.load();

                    var multitalented = library.TryGet<BlueprintFeatureSelection>("e6a72a23e75545bc9a57a6b94ffc8b69");
                    if (multitalented != null) {
                        HeirloomWeapon.load();

                        PrestigiousSpellcaster.load();
                    }
#if DEBUG

                    CallOfTheWild.Helpers.GuidStorage.dump(guid_file_name);
#endif
                    CallOfTheWild.Helpers.GuidStorage.dump(@"./Mods/TweakOrTreat/loaded_blueprints.txt");
                }
                catch (Exception ex)
                {
                    Main.DebugError(ex);
                }
            }
        }

        internal static Exception Error(String message)
        {
            logger?.Log(message);
            return new InvalidOperationException(message);
        }
    }
}

