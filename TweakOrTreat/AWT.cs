using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CallOfTheWild.Helpers;
using static Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite;

namespace TweakOrTreat
{
    // very unusual place for this patch - I am unable to directly patch AdvancedFighterOptions class at all -
    // it cannot be initialized by Harmony and apparently initialization is required for a patch
    // initialization most likely does not work because of some of the static fields in the class
    // here however, inside Call of the Wild "Load" I am able to explicitly initialize it
    // and tweak a little bit one of its fields
    [HarmonyLib.HarmonyPatch(typeof(CallOfTheWild.SkillUnlocks), "load")]
    class patch_advanced_weapon_training
    {
        static void Postfix()
        {
            try
            {
                var t = HarmonyLib.AccessTools.TypeByName("CallOfTheWild.AdvancedFighterOptions, CallOfTheWild");
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(t.TypeHandle);
                FieldInfo info = HarmonyLib.AccessTools.Field(t, "group_training_map");
                object value = info.GetValue(null);
                var groups = value as Dictionary<WeaponFighterGroup, BlueprintFeature>;
                groups.Add((WeaponFighterGroup)100, Warpriest.arsenal_chaplain_weapon_training);
                info.SetValue(null, groups);

                GuidStorage.addEntry("100WarriorSpiritEnchantmentAbility", "534d488b71234fa3b1c243f3b192ca22");
                GuidStorage.addEntry("100WarriorSpiritEnchantmentAbilityFeature", "3c8cfb2446b64181974863fe92dd71df");
            }
            catch(Exception e)
            {
                Main.logger.Log(String.Format("Error while attempting to patch Advanced Weapon Training {0}", e));
            }
        }
    }
    class AWT
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var warpriestWeaponTraining = CallOfTheWild.Warpriest.arsenal_chaplain_weapon_training;  
            var focusedWeapon = library.Get<BlueprintFeatureSelection>("786bde5345a548408fade70b60a70482");

            var archatypeListFeature = Helpers.CreateFeature("FocusedWeaponArchetypeExtensionFeature",
                                                               "",
                                                               "",
                                                               "",
                                                               null,
                                                               FeatureGroup.None);
            archatypeListFeature.AddComponent(Helpers.Create<ContextRankConfigArchetypeList>(c => c.archetypes = new BlueprintArchetype[] { CallOfTheWild.Warpriest.arsenal_chaplain, Myrmidarch.archetype }));
            archatypeListFeature.HideInCharacterSheetAndLevelUp = true;

            foreach (var f in focusedWeapon.AllFeatures)
            {
                var reqs = f.GetComponents<PrerequisiteFeature>();

                if(reqs.Count() == 1)
                {
                    f.ReplaceComponent<PrerequisiteFeature>(
                        p => p.Group = GroupType.Any
                    );
                }

                f.AddComponent(Helpers.PrerequisiteFeature(warpriestWeaponTraining, true));
                
                f.ReplaceComponent<ContextRankConfig>(
                    c =>
                    {
                        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueTypeExtender.SummClassLevelWithArchetypes.ToContextRankBaseValueType());
                        Helpers.SetField(c, "m_Feature", archatypeListFeature);
                        Helpers.SetField(c, "m_Class", Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(CallOfTheWild.Warpriest.warpriest_class, Myrmidarch.magus));
                    }
                );
            }
        }
    }
}

