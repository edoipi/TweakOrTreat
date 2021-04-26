using HarmonyLib;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    [HarmonyLib.HarmonyPatch(typeof(Spellbook), nameof(Spellbook.GetSpellsPerDay))]
    [HarmonyLib.HarmonyAfter("CallOfTheWild")]
    class Spellbook_GetSpellsPerDay
    {
        static bool Prepare()
        {
            if (!Main.spellSlotsFromPermanentBonusOnly)
                return false;
            if (Main.reduceSpellSlotsFromAbilityDrain)
                filter = filterWithDrain;
            else
                filter = filterWithoutDrain;
            return true;
        }

        static Func<ModifiableValue.Modifier, bool> filter;

        static bool filterWithDrain(ModifiableValue.Modifier m)
        {
            return filterWithoutDrain(m) || m.ModDescriptor == ModifierDescriptor.StatDrain;
        }

        static bool filterWithoutDrain(ModifiableValue.Modifier m)
        {
            return m.IsRacial() || m.ItemSource != null || m.ModDescriptor == ModifierDescriptor.Inherent;
        }

        static int permanentBonus(ModifiableValueAttributeStat stat)
        {
            var permanentValue = stat.ApplyModifiersFiltered(
                stat.CalculateBaseValue(stat.BaseValue),
                filter
            );
            return permanentValue / 2 - 5;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var getBonusIndex = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("get_Bonus"));

            codes[getBonusIndex] = new CodeInstruction(
                System.Reflection.Emit.OpCodes.Call,
                new Func<ModifiableValueAttributeStat, int>(permanentBonus).Method
            );

            //foreach(var code in codes)
            //{
            //    if(code.opcode == System.Reflection.Emit.OpCodes.Callvirt)
            //        Main.logger.Log($"operand: {code.operand.ToString()}");
            //}

            return codes.AsEnumerable();
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(LevelUpHelper), nameof(LevelUpHelper.GetIntelligenceSkillPoints))]
    class LevelUpHelper_GetIntelligenceSkillPoints
    {
        static bool Prepare()
        {
            return false;
        }

        static int permanentValue(ModifiableValueAttributeStat stat)
        {
            var permanentValue = stat.ApplyModifiersFiltered(
                stat.CalculateBaseValue(stat.BaseValue),
                m => m.IsRacial() || (m.ModDescriptor == ModifierDescriptor.Inherent && m.ItemSource == null)
            );
            return permanentValue;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var getBonusIndex = codes.FindIndex(x => x.opcode == System.Reflection.Emit.OpCodes.Callvirt && x.operand.ToString().Contains("CalculatePermanentValueWithoutEnhancement"));

            codes[getBonusIndex] = new CodeInstruction(
                System.Reflection.Emit.OpCodes.Call,
                new Func<ModifiableValueAttributeStat, int>(permanentValue).Method
            );

            return codes.AsEnumerable();
        }
    }

    class SpellbookFix
    {
    }
}
