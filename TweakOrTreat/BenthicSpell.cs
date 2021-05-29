using CallOfTheWild;
using CallOfTheWild.AdditionalSpellDescriptors;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CallOfTheWild.MetamagicFeats;

namespace TweakOrTreat
{
    class BenthicSpell
    {
        static LibraryScriptableObject library => Main.library;

        [HarmonyLib.HarmonyPatch(typeof(CallOfTheWild.MetamagicFeats), nameof(CallOfTheWild.MetamagicFeats.load))]
        static class MetamagicFeats_load
        {
            public static int BenthicMetamagic = 1024;
            internal static void Postfix()
            {
                var gudis = new List<(string, string)>() {
                    ("BenthicSpellFeature", "4778cf2eb80c44ceacb6aadec772b2c1"),
                    ("BenthicSpellFeatureMetamagicMasteryBuff", "b05412cedb4140edbb2a225c3ade9553"),
                    ("BenthicSpellFeatureMetamagicMasteryBuffToggleAbility", "96bcab9364dd48db84ffd6fc06bcf1ce"),
                    ("BenthicSpellFeatureMetamagicMasteryBuffToggleAbilityFeature", "aa97093a773144c78b071c90b73c231a"),
                    ("PsychicBenthicSpellFeature", "1cde712d0ff1458c9b67392181404536"),
                    ("PsychicBenthicSpellFeatureBuff", "66d7faa7edc04e33ab1f43043ed10a95"),
                    ("PsychicBenthicSpellFeatureBuffToggleAbility", "8cc15b686f82469ba9ef408010cc5806"),
                    ("OccultistImplementBenthicSpellFeatureMetamagicMasterBuff", "1995857f2e2a4ef6bb27950018c5a584"),
                    ("OccultistImplementBenthicSpellFeatureMetamagicMasterBuffToggleAbility", "f69471bc5f6a4c9aae2855c74030e9d4"),
                    ("OccultistImplementBenthicSpellFeatureMetamagicMasterBuffToggleAbilityFeature", "f3949f4ffd214c36868e32e15157699e"),
                    ("RelicHunterImplementBenthicSpellFeatureMetamagicMasterBuff", "569e776024df4aac89d62b86992ab554"),
                    ("RelicHunterImplementBenthicSpellFeatureMetamagicMasterBuffToggleAbility", "a7e8a2fe73544f058dd02dd213137181"),
                    ("RelicHunterImplementBenthicSpellFeatureMetamagicMasterBuffToggleAbilityFeature", "b5b9ffb29b364dd0b9b96b37b8a3dff6"),
                    ("MetaragerBenthicSpellFeature", "3865173189464432be98b3f9925f2085"),
                    ("BenthicSpellFeatureMetaRageBuff", "fc0aca3b62cf4687bf3863ed092c9434"),
                    ("BenthicSpellFeatureMetaRageAbility", "3ec49f570eec4e0b9eb73a82a25f4f12"),
                    ("BenthicSpellFeatureMetaRageFeat", "b8d3e3627a6b4b61a9ca6f9b23eea809"),
                    ("BenthicSpellFeature1Rod", "e3a444a5c4964c88a7d7e617707f61fa"),
                    ("BenthicSpellFeature1RodActivatableAbility", "951e5206a1154f6b9d81bc2428c76b64"),
                    ("BenthicSpellFeature1RodBuff", "e96e352721f84d548239ae5403d0707d"),
                    ("BenthicSpellFeature2Rod", "f34eeaea018d49f98de70a4a7d394dbe"),
                    ("BenthicSpellFeature2RodActivatableAbility", "6010d24c362d4918a89a6ecdc2f99ee3"),
                    ("BenthicSpellFeature2RodBuff", "f4fe2a8fdfdd4e6d9c4e286a0bf406c2"),
                    ("BenthicSpellFeature3Rod", "b46ec127b28b48a78514c57e70d38a29"),
                    ("BenthicSpellFeature3RodActivatableAbility", "7fccd58fd6874fc08830e57284436ef1"),
                    ("BenthicSpellFeature3RodBuff", "da4e138e3bf84829967bab3bee6aa77d"),
                };

                foreach (var entry in gudis)
                {
                    CallOfTheWild.Helpers.GuidStorage.addEntry(entry.Item1, entry.Item2);
                }

                //Type type = typeof(CallOfTheWild.Helpers.GuidStorage);
                //FieldInfo info = type.GetField("allow_guid_generation", BindingFlags.NonPublic | BindingFlags.Static);
                //info.SetValue(null, true);

                var feat = CallOfTheWild.Main.library.CopyAndAdd<BlueprintFeature>("a1de1e4f92195b442adb946f0e2b9d4e", "BenthicSpellFeature", "");
                feat.SetNameDescription(
                    "Metamagic (Benthic Spell)",
                    "You can modify a spell that deals acid, cold, electricity, or fire damage to deal damage through high-pressure water instead. The spell gains the water descriptor, and you can either replace the spell’s normal damage with bludgeoning damage or split the spell’s damage so that half is bludgeoning and half is of its normal type. Creatures with damage reduction apply their damage reduction to bludgeoning damage from a benthic spell, but the spell counts as bludgeoning and magic for the purposes of bypassing damage reduction. A benthic spell uses up a spell slot one level higher than the spell’s actual level."
                );

                feat.ReplaceComponent<AddMetamagicFeat>(a => a.Metamagic = (Metamagic)MetamagicFeats_load.BenthicMetamagic);

                var addToFeats = HarmonyLib.AccessTools.Method(typeof(CallOfTheWild.MetamagicFeats), "AddMetamagicToFeatSelection");
                addToFeats.Invoke(null, new object[] { feat });
                //AddMetamagicToFeatSelection(feat);

                //CallOfTheWild.Main.library.AddFeats(feat);
                //CallOfTheWild.Main.library.AddWizardFeats(feat);

                var spells = CallOfTheWild.Main.library.GetAllBlueprints().OfType<BlueprintAbility>().Where(
                    b => b.IsSpell
                    && b.EffectOnEnemy == AbilityEffectOnUnit.Harmful
                    && ((b.AvailableMetamagic & Metamagic.Maximize) != 0)
                    && (b.SpellDescriptor & (SpellDescriptor.Fire | SpellDescriptor.Cold | SpellDescriptor.Electricity | SpellDescriptor.Acid)) != 0
                ).Cast<BlueprintAbility>().ToArray();

                foreach (var s in spells)
                {
                    s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicFeats_load.BenthicMetamagic;
                    if (s.Parent != null)
                    {
                        s.AvailableMetamagic = s.AvailableMetamagic | (Metamagic)MetamagicFeats_load.BenthicMetamagic;
                    }
                }
            }
        }

        static public void load()
        {
            { 
                var original = HarmonyLib.AccessTools.Method(typeof(UIUtilityTexts), "GetMetamagicList");
                var patch = HarmonyLib.AccessTools.Method(typeof(UIUtilityTexts_GetMetamagicList_Patch), "Postfix");
                Main.harmony.Patch(original, postfix: new HarmonyLib.HarmonyMethod(patch));
            }
        }



        //[HarmonyLib.HarmonyPatch(typeof(RuleCastSpell), "OnTrigger")]
        //static class RuleCastSpell_OnTrigger_Patch
        //{
        //    internal static void Postfix(RuleCastSpell __instance, RulebookEventContext context)
        //    {
        //        var context2 = __instance.Context;

        //        if (context2?.AbilityBlueprint == null || context2?.Params == null)
        //        {
        //            return;
        //        }

        //        if (context2.SourceAbility.IsSpell &&
        //            (context2.Params.Metamagic & (Metamagic)MetamagicFeats_load.BenthicMetamagic) != 0)
        //        {
        ////            context2.RemoveSpellDescriptor(SpellDescriptor.Fire);
        ////            context2.RemoveSpellDescriptor(SpellDescriptor.Cold);
        ////            context2.RemoveSpellDescriptor(SpellDescriptor.Acid);
        ////            context2.RemoveSpellDescriptor(SpellDescriptor.Electricity);
        //            context2.AddSpellDescriptor((SpellDescriptor)ExtraSpellDescriptor.Water);
        //            return;
        //        }
        //    }
        //}

        //[HarmonyLib.HarmonyPatch(typeof(RuleCalculateAbilityParams), HarmonyLib.MethodType.Constructor)]
        //[HarmonyLib.HarmonyPatch(new[] { typeof(UnitEntityData), typeof(BlueprintScriptableObject), typeof(Spellbook) })]
        //public class RuleCalculateAbilityParams_patch
        //{
        //    internal static void Postfix(RuleCalculateAbilityParams __instance)
        //    {
        //        var metamagicAbility = UnityEngine.Object.Instantiate(__instance.Blueprint);
        //        //var tempList = new List<BlueprintAbility>() { (BlueprintAbility)__instance.Blueprint };
        //        //Main.logger.Log($"Has: {tempList.HasItem((BlueprintAbility)metamagicAbility)}");

        //        FieldInfo blueprint = typeof(RuleCalculateAbilityParams).GetField("Blueprint", BindingFlags.Public | BindingFlags.Instance);
        //        blueprint.SetValue(__instance, metamagicAbility);

        //        //metamagicAbility.ReplaceComponent<SpellDescriptorComponent>(s => s.Descriptor = s.Descriptor | (SpellDescriptor)ExtraSpellDescriptor.Water);
        //        var descriptorComponent = metamagicAbility.GetComponent<SpellDescriptorComponent>();
        //        //Main.logger.Log($"Metamagic: {__instance.m_MetamagicData.MetamagicMask}");
        //        if (descriptorComponent != null)
        //        {
        //            var descriptor = new SpellDescriptorWrapper(descriptorComponent.Descriptor.Value);
        //            metamagicAbility.ReplaceComponent<SpellDescriptorComponent>(s => s.Descriptor = descriptor);
        //        }
        //    }
        //}

        //[HarmonyLib.HarmonyPatch(typeof(RuleCalculateAbilityParams), HarmonyLib.MethodType.Constructor)]
        //[HarmonyLib.HarmonyPatch(new[] { typeof(UnitEntityData), typeof(AbilityData) })]
        //public class RuleCalculateAbilityParams_patch2
        //{
        //    static List<(Metamagic metamagic, SpellDescriptor descriptor)> metamagicMap = new List<(Metamagic, SpellDescriptor)>()
        //    {
        //        ((Metamagic)MetamagicFeats_load.BenthicMetamagic, (SpellDescriptor)ExtraSpellDescriptor.Water),
        //        ((Metamagic)MetamagicExtender.ElementalAcid, SpellDescriptor.Acid),
        //        ((Metamagic)MetamagicExtender.ElementalCold, SpellDescriptor.Cold),
        //        ((Metamagic)MetamagicExtender.ElementalElectricity, SpellDescriptor.Electricity),
        //        ((Metamagic)MetamagicExtender.ElementalFire, SpellDescriptor.Fire),
        //    };

        //    public static void changeDescriptors(RuleCalculateAbilityParams __instance)
        //    {
        //        var descriptorComponent = __instance.Blueprint.GetComponent<SpellDescriptorComponent>();
        //        //Main.logger.Log($"Metamagic: {__instance.m_MetamagicData?.MetamagicMask}");
        //        if (descriptorComponent != null)
        //        {
        //            var descriptor = descriptorComponent.Descriptor.Value;

        //            //__instance.m_MetamagicData
        //            foreach (var entry in metamagicMap)
        //            {
        //                if (__instance.HasMetamagic(entry.metamagic))
        //                {
        //                    var elementalMask = SpellDescriptor.Fire | SpellDescriptor.Cold | SpellDescriptor.Electricity | SpellDescriptor.Acid;
        //                    descriptor &= ~elementalMask;
        //                    descriptor |= entry.descriptor;

        //                    descriptorComponent.Descriptor = new SpellDescriptorWrapper(descriptor);
        //                    //metamagicAbility.ReplaceComponent<SpellDescriptorComponent>(s => s.Descriptor = descriptor);
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    internal static void Postfix(RuleCalculateAbilityParams __instance)
        //    {
        //        changeDescriptors(__instance);
        //    }
        //}

        //[HarmonyLib.HarmonyPatch(typeof(RuleCalculateAbilityParams), nameof(RuleCalculateAbilityParams.AddMetamagic))]
        //public class RuleCalculateAbilityParams_AddMetamagic
        //{
        //    internal static void Postfix(RuleCalculateAbilityParams __instance)
        //    {
        //        RuleCalculateAbilityParams_patch2.changeDescriptors(__instance);
        //    }
        //}

        //[HarmonyLib.HarmonyPatch()]
        //static class RulebookSubscribersList_patch
        //{
        //    static MethodInfo TargetMethod()
        //    {
        //        return typeof(RulebookSubscribersList<IRulebookHandler<RuleCalculateAbilityParams>, RuleCalculateAbilityParams>).GetMethod("OnEventAboutToTrigger", BindingFlags.Public | BindingFlags.Instance);
        //    }

        //    static bool isPriority(IRulebookHandler<RuleCalculateAbilityParams> x)
        //    {
        //        return typeof(AutoMetamagic).IsInstanceOfType(x)
        //            || typeof(MetamagicOnNextSpell).IsInstanceOfType(x)
        //            || typeof(MetamagicRodMechanics).IsInstanceOfType(x)
        //            || typeof(ReplaceAbilityParamsWithContext).IsInstanceOfType(x);
        //    }

        //    public static void OnEventAboutToTrigger(RulebookSubscribersList<IRulebookHandler<RuleCalculateAbilityParams>, RuleCalculateAbilityParams> __instance, RulebookEvent evt)
        //    {
        //        //Main.logger.Log($"I am running ");
        //        __instance.Executing = true;
        //        try
        //        {
        //            int count = __instance.List.Count;
        //            var newList = new List<IRulebookHandler<RuleCalculateAbilityParams>>();
        //            for (int i = 0; i < count; i++)
        //            {
        //                newList.Add(__instance.List[i]);
        //            }
        //            newList.Sort(
        //                delegate (IRulebookHandler<RuleCalculateAbilityParams> x, IRulebookHandler<RuleCalculateAbilityParams> y)
        //                {
        //                    bool a = isPriority(x);
        //                    bool b = isPriority(y);

        //                    if (a == b)
        //                        return 0;

        //                    if (a)
        //                        return -1;

        //                    return 1;
        //                }
        //            );

        //            foreach (var rulebookHandler in newList)
        //            {
        //                if (rulebookHandler != null)
        //                {
        //                    try
        //                    {
        //                        rulebookHandler.OnEventAboutToTrigger((RuleCalculateAbilityParams)((object)evt));
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        UberDebug.LogException(ex);
        //                    }
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            __instance.Executing = false;
        //            __instance.Cleanup();
        //        }
        //    }

        //    internal static bool Prefix(RulebookSubscribersList<IRulebookHandler<RuleCalculateAbilityParams>, RuleCalculateAbilityParams> __instance, RulebookEvent evt)
        //    {
        //        OnEventAboutToTrigger(__instance, evt);
        //        return false;
        //    }
        //}




        private static void UpdateForType(Type type, BaseDamage source, BaseDamage destination)
        {
            FieldInfo[] myObjectFields = type.GetFields(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo fi in myObjectFields)
            {
                fi.SetValue(destination, fi.GetValue(source));
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(RulePrepareDamage), "OnTrigger")]
        static class RulePrepareDamage_OnTrigger_Patch
        {
            internal static bool Prefix(RulePrepareDamage __instance, RulebookEventContext context)
            {
                var context2 = Helpers.GetMechanicsContext()?.SourceAbilityContext;
                if (context2 == null)
                {
                    var source_buff = (__instance.Reason?.Item as ItemEntityWeapon)?.Blueprint.GetComponent<CallOfTheWild.NewMechanics.EnchantmentMechanics.WeaponSourceBuff>()?.buff;

                    if (source_buff != null)
                    {
                        context2 = __instance.Initiator.Buffs?.GetBuff(source_buff)?.MaybeContext?.SourceAbilityContext;
                    }
                }
                if (context2 == null)
                {
                    return true;
                }
                if (context2.SourceAbility.IsSpell &&
                    (context2.Params.Metamagic & (Metamagic)MetamagicFeats_load.BenthicMetamagic) != 0)
                {
                    var newGuys = new List<BaseDamage>();
                    foreach (BaseDamage item in __instance.DamageBundle)
                    {
                        if (item is EnergyDamage)
                        {
                            var bludgeDamage = new PhysicalDamage(DiceFormula.Zero, Kingmaker.Enums.Damage.PhysicalDamageForm.Bludgeoning);
                            bludgeDamage.Enchantment = 1;
                            bludgeDamage.EnchantmentTotal = 1;

                            UpdateForType(typeof(BaseDamage), item, bludgeDamage);

                            newGuys.Add(bludgeDamage);
                        }
                    }

                    __instance.DamageBundle.Remove(
                        d => d is EnergyDamage
                    );
                    
                    foreach (var d in newGuys)
                    {
                        __instance.DamageBundle.Add(d);
                    }

                    return true;
                }
                return true;
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(CallOfTheWild.MetamagicFeats), "calculateNewMetamagicCost")]
        static class MetamagicFeats_calculateNewMetamagicCost_Patch
        {
            internal static bool Prefix(Metamagic metamagic, ref int __result)
            {
                if (metamagic == (Metamagic)MetamagicFeats_load.BenthicMetamagic)
                {
                    __result = 1;
                    return false;
                }
                return true;
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MetamagicHelper), "DefaultCost")]
        static class MetamagicHelper_DefaultCost_Patch
        {
            internal static bool Prefix(Metamagic metamagic, ref int __result)
            {
                if (metamagic == (Metamagic)MetamagicFeats_load.BenthicMetamagic)
                {
                    __result = 1;
                    return false;
                }
                return true;
            }
        }

        //[HarmonyLib.HarmonyPatch()]
        //static class ExcludeWithElemental
        //{
        //    static MethodInfo TargetMethod()
        //    {
        //        var t = HarmonyLib.AccessTools.TypeByName("CallOfTheWild.SpellManipulationMechanics.ActionBarGroupSlot__SetToggleAdditionalSpells__Patch, CallOfTheWild");
        //        var original = HarmonyLib.AccessTools.Method(t, "getMetamagicDataForSpecifiedCost");

        //        return original;
        //    }

        //    internal static bool Prefix(List<Metamagic> metamagics, ref MetamagicData __result, out bool add_more)
        //    {
        //        add_more = false;
        //        Main.logger.Log("Im in");
        //        Main.logger.Log($"{metamagics.Count(m => (m & (Metamagic)MetamagicFeats.MetamagicExtender.Elemental) != 0)}");
        //        Main.logger.Log($"{metamagics.Count(m => (m & (Metamagic)MetamagicFeats_load.BenthicMetamagic) != 0)}");
        //        if (metamagics.Count(m => (m & (Metamagic)MetamagicFeats.MetamagicExtender.Elemental) != 0) > 0
        //            && metamagics.Count(m => (m & (Metamagic)MetamagicFeats_load.BenthicMetamagic) != 0) > 0)
        //        {
        //            __result = null;
        //            return false;
        //        }
        //        return true;
        //    }
        //}

        [HarmonyLib.HarmonyPatch(typeof(MetamagicHelper), "SpellIcon")]
        static class MetamagicHelper_SpellIcon_Patch
        {
            internal static bool Prefix(Metamagic metamagic, ref Sprite __result)
            {
                if (metamagic == (Metamagic)MetamagicFeats_load.BenthicMetamagic)
                {
                    __result = UIRoot.Instance.SpellBookColors.MetamagicHeighten;
                    return false;
                }
                return true;
            }
        }

        //[HarmonyLib.HarmonyPatch(typeof(UIUtilityTexts), "GetMetamagicList")]
        static class UIUtilityTexts_GetMetamagicList_Patch
        {
            static void Postfix(Metamagic mask, ref string __result)
            {
                string extra_metamagic = "";

                if ((mask & (Metamagic)MetamagicFeats_load.BenthicMetamagic) != 0)
                {
                    extra_metamagic += "Benthic, ";
                }

                if (extra_metamagic.Length > 2)
                {
                    extra_metamagic = extra_metamagic.Substring(0, extra_metamagic.Length - 2);
                }
                if (!__result.Empty())
                {
                    __result += ", ";
                }

                __result += extra_metamagic;
            }
        }

    }
}
