using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    //[HarmonyLib.HarmonyPatch(typeof(Game), "PauseBind")]
    //class Game_PauseBind_Patch
    //{
    //    static bool Prepare()
    //    {
    //        return false;
    //    }

    //    static bool Prefix(ref Game __instance)
    //    {
    //        Game.Instance.IsPaused = !Game.Instance.IsPaused;
    //        return false;
    //    }
    //}
    class Bullshit
    {
        public static ContextActionApplyBuff[] GetAbilityContextActionApplyBuffs(BlueprintAbility Ability)
        {
            return Ability
                .GetComponents<AbilityEffectRunAction>()
                .SelectMany(c => c.Actions.Actions.OfType<ContextActionApplyBuff>()
                    .Concat(c.Actions.Actions.OfType<ContextActionConditionalSaved>()
                        .SelectMany(a => a.Failed.Actions.OfType<ContextActionApplyBuff>()))
                    .Concat(c.Actions.Actions.OfType<Conditional>()
                        .SelectMany(a => a.IfTrue.Actions.OfType<ContextActionApplyBuff>()
                            .Concat(a.IfFalse.Actions.OfType<ContextActionApplyBuff>()))))
                .Where(c => c.Buff != null).ToArray();
        }

        public static DurationRate[] getAbilityBuffDurations(BlueprintAbility Ability)
        {
            var applyBuffs = GetAbilityContextActionApplyBuffs(Ability);
            return applyBuffs.Select(a => a.UseDurationSeconds ? DurationRate.Rounds : a.DurationValue.Rate).ToArray();
        }

        public static void printDurations(BlueprintAbility Ability)
        {
            var durations = getAbilityBuffDurations(Ability);
            Main.logger.Log(Ability.Name);
            foreach(var d in durations)
            {
                Main.logger.Log(d.ToString());
            }
        }

        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var delayPoison = library.Get<BlueprintAbility>("b48b4c5ffb4eab0469feba27fc86a023");
            var deathWard = library.Get<BlueprintAbility>("0413915f355a38146bc6ad40cdf27b3f");
            var trueStrike = library.Get<BlueprintAbility>("2c38da66e5a599347ac95b3294acbe00");

            printDurations(delayPoison);
            printDurations(deathWard);
            printDurations(trueStrike);
        }
    }
}
