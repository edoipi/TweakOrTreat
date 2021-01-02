using CallOfTheWild;
using Derring_Do;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class VirtuosoBravo
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            
            Main.logger.Log("Derring-Do found");

            var paladin = Helpers.GetClass("bfa11238e7ae3544bbeb4d0b92e897ec");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "VirtuosoBravoArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Virtuos Bravo");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Although no less a beacon of hope and justice than other paladins, virtuous bravos rely on their wit and grace rather than might and strong armor.");
            });
            Helpers.SetField(archetype, "m_ParentClass", paladin);
            archetype.RemoveSpellbook = true;
            library.AddAsset(archetype, "");

            var smiteEvilFeature = library.Get<BlueprintFeature>("3a6db57fce75b0244a6a5819528ddf26");
            var paladinProficiencies = library.Get<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb");
            var selectionMercy = library.Get<BlueprintFeature>("02b187038a8dce545bb34bbfb346428d");
            var auraOfJusticeFeature = library.Get<BlueprintFeature>("9f13fdd044ccb8a439f27417481cb00e");
            var holyChampion = library.Get<BlueprintFeature>("eff3b63f744868845a2f511e9929f0de");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, smiteEvilFeature, paladinProficiencies),
                Helpers.LevelEntry(3, selectionMercy),
                Helpers.LevelEntry(6, selectionMercy),
                Helpers.LevelEntry(9, selectionMercy),
                Helpers.LevelEntry(11, auraOfJusticeFeature),
                Helpers.LevelEntry(12, selectionMercy),
                Helpers.LevelEntry(15, selectionMercy),
                Helpers.LevelEntry(18, selectionMercy),
                Helpers.LevelEntry(20, holyChampion),
            };

            var virtuosoSmite = library.CopyAndAdd(smiteEvilFeature, "VirtuosoSmiteEvilFeature", "");
            var virtuosoSmiteBuff = library.CopyAndAdd<BlueprintBuff>("b6570b8cbb32eaf4ca8255d0ec3310b0", "VirtuosoSmiteEvilBuff", "");
            var virtuosoSmiteAbility = library.CopyAndAdd<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec", "VirtuosoSmiteEvilAbility", "");
            virtuosoSmiteBuff.RemoveComponents<ACBonusAgainstTarget>();
            virtuosoSmite.ReplaceComponent<AddFacts>(af => af.Facts = new BlueprintUnitFact[]{ virtuosoSmiteAbility });
            virtuosoSmite.SetDescription("When using smite evil, a virtuous bravo doesn’t gain a deflection bonus to AC.");

            virtuosoSmiteAbility.ReplaceComponent<AbilityEffectRunAction>(
                aera =>
                {
                    var conditional = aera.Actions.Actions[0] as Conditional;
                    var newConditional = Helpers.Create<Conditional>(
                        c =>
                        {
                            c.ConditionsChecker = conditional.ConditionsChecker;
                            c.IfFalse = conditional.IfFalse;
                            c.IfTrue = new ActionList();

                            c.IfTrue.Actions = Common.changeAction<ContextActionApplyBuff>(conditional.IfTrue.Actions,
                                b => b.Buff = virtuosoSmiteBuff);
                        }
                    );
                    aera.Actions = new ActionList();
                    aera.Actions.Actions = new GameAction[]{ newConditional };
                }
            );

            var virtousProficiency = library.CopyAndAdd(paladinProficiencies, "VirtuosoProficiencies", "");

            virtousProficiency.ReplaceComponent<AddFacts>(c => c.Facts = c.Facts.RemoveFromArray(library.Get<BlueprintFeature>("1b0f68188dcc435429fb87a022239681"))); //heavy armor proficiency
            virtousProficiency.ReplaceComponent<AddFacts>(c => c.Facts = c.Facts.RemoveFromArray(library.Get<BlueprintFeature>("cb8686e7357a68c42bdd9d4e65334633"))); //shields proficiency
            virtousProficiency.AddComponent(Common.createAddArmorProficiencies(ArmorProficiencyGroup.Buckler));
            virtousProficiency.SetNameDescription(
                "Weapon and Armor Proficiency",
                "Virtuous bravos aren’t proficient with heavy armor or shields (except for bucklers)."
            );
            

            var preciseStrikeBuff = library.CopyAndAdd<BlueprintBuff>("8fa7914d1d734478b9f863e7a514427e", "PreciseStrikeBuffVirtuousBravo", "");
            preciseStrikeBuff.ReplaceComponent<AddBonusPrecisionDamageToSwashbucklerWeapons>(a => a.swashbuckler_class = paladin);

            var preciseStrikeAbility = library.CopyAndAdd<BlueprintAbility>("349bd539fb6741c1b32a3b6164362559", "PreciseStrikeAbilityVirtuousBravo", "");
            preciseStrikeAbility.ReplaceComponent<AbilityEffectRunAction>(
                aera =>
                {
                    aera.Actions = new ActionList();
                    aera.Actions.Actions = new GameAction[] {
                        Common.createContextActionApplyBuff(preciseStrikeBuff, Helpers.CreateContextDuration(), dispellable: false, duration_seconds: 4)
                    };
                }
            );
            var preciseStrikeFeature = library.CopyAndAdd<BlueprintFeature>("03b1034003bc4d8083e03c83604484ab", "PreciseStrikeFeatureVirtuousBravo", "");
            
            preciseStrikeFeature.ReplaceComponent<AddFacts>(a => a.Facts = new BlueprintUnitFact[] { preciseStrikeAbility });
            //preciseStrikeAbility.ReplaceComponent<AddBonusPrecisionDamageToSwashbucklerWeapons>(a => a.swashbuckler_class = paladin);
            var component = preciseStrikeFeature.ComponentsArray[1] as AddBonusPrecisionDamageToSwashbucklerWeapons;
            var clone = UnityEngine.Object.Instantiate(component);
            clone.swashbuckler_class = paladin;
            preciseStrikeFeature.ComponentsArray[1] = clone;

            var nimbleOriginal = library.TryGet<BlueprintFeature>("221d8fee280b48eda3a871fe96c32eb1");
            var nimble = library.CopyAndAdd(nimbleOriginal, "NimbleVirtuousBravo", "");
            var nimbleBonus = library.CopyAndAdd<BlueprintFeature>("c9fc9521fe41460aa0a52c0a8405c811", "NimbleBonusVirtuousBravo", "");
            nimbleBonus.ReplaceComponent<ContextRankConfig>(
                Helpers.CreateContextRankConfig(
                    baseValueType: ContextRankBaseValueType.ClassLevel, 
                    classes: new BlueprintCharacterClass[] { paladin },
                    progression: ContextRankProgression.DelayedStartPlusDivStep,
                    startLevel: 3, stepLevel: 4
                )
            );
            nimble.ReplaceComponent<SwashbucklerNoArmorOrLightArmorNimbleFeatureUnlock>(
                s => s.NewFact = nimbleBonus
            );

            var masterStrike = library.Get<BlueprintFeature>("72dcf1fb106d5054a81fd804fdc168d3");
            var desc = "At 20th level, a virtuous bravo becomes a master at dispensing holy justice with her blade. When the virtuous bravo confirms a critical hit with a light or one-handed piercing melee weapon, she can slay the target. The target receives a Fortitude save to negate the death effect. On a success, the target is instead stunned for 1 round (it still takes damage). The DC of this save is equal to 10 + 1/2 the virtuous bravo’s paladin level + her Charisma modifier. Once a creature has been the target of a bravo’s holy strike, regardless of whether or not it succeeds at the save, that creature is immune to that bravo’s holy strike for 24 hours. Creatures that are immune to critical hits are also immune to this ability.";

            var cooldown = Helpers.CreateBuff(
                "HolyStrikeVirtousBravoCooldownBuff",
                "Holy Strike Cooldown",
                desc,
                "",
                masterStrike.Icon,
                null
            );
            cooldown.Stacking = StackingType.Stack;
            var applyCooldown = Helpers.CreateConditional(
                Common.createContextConditionHasBuffFromCaster(cooldown), null,
                Common.createContextActionApplyBuff(cooldown, Helpers.CreateContextDuration(1, DurationRate.Days), dispellable: false)
            );

            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");
            var applyStun = Common.createContextActionApplyBuff(stunned, Helpers.CreateContextDuration(1), dispellable: false/*, duration_seconds: 7*/);

            var slayAction = Helpers.CreateConditional(
                Common.createContextConditionHasBuffFromCaster(cooldown), null,
                Helpers.CreateActionSavingThrow(SavingThrowType.Fortitude, Helpers.CreateConditionalSaved(applyStun, Helpers.Create<ContextActionKillTarget>()), applyCooldown)
            );

            var holyStrike = Helpers.CreateFeature(
                "HolyStrikeVirtousBravoFeature",
                "Holy Strike",
                desc,
                "",
                masterStrike.Icon,
                FeatureGroup.None,
                Helpers.Create<AddInitiatorAttackWithWeaponTrigger>(a =>
                    {
                        a.OnlyHit = true;
                        a.CriticalHit = true;
                        a.DuelistWeapon = true;
                        a.Action = Helpers.CreateActionList(slayAction);
                    }
                ),
                Common.createContextCalculateAbilityParamsBasedOnClassesWithArchetypes(
                    new BlueprintCharacterClass[] { paladin },
                    new BlueprintArchetype[] { archetype },
                    StatType.Charisma
                )
            );

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, virtuosoSmite, virtousProficiency, Derring_Do.Swashbuckler.swashbuckler_finesse),
                Helpers.LevelEntry(3, nimble),
                Helpers.LevelEntry(4, Derring_Do.Swashbuckler.panache,
                    Derring_Do.Swashbuckler.dodging_panache_deed, 
                    Derring_Do.Swashbuckler.menacing_swordplay_deed, 
                    Derring_Do.Swashbuckler.opportune_parry_and_riposte_deed,
                    preciseStrikeFeature, 
                    Derring_Do.Swashbuckler.swashbuckler_initiative_deed),
                Helpers.LevelEntry(7, nimble),
                Helpers.LevelEntry(11, nimble,
                    Derring_Do.Swashbuckler.bleeding_wound_deed,
                    Derring_Do.Swashbuckler.evasive_deed,
                    Derring_Do.Swashbuckler.subtle_blade_deed,
                    Derring_Do.Swashbuckler.superior_feint_deed,
                    Derring_Do.Swashbuckler.swashbucklers_grace_deed,
                    Derring_Do.Swashbuckler.targeted_strike_deed),
                Helpers.LevelEntry(15, nimble),
                Helpers.LevelEntry(19, nimble),
                Helpers.LevelEntry(20, holyStrike)
            };
            paladin.Archetypes = paladin.Archetypes.AddToArray(archetype);

            foreach (var group in paladin.Progression.UIGroups)
            {
                if (group.Features.Contains(smiteEvilFeature))
                {
                    group.Features.Add(virtuosoSmite);
                }
                if (group.Features.Contains(holyChampion))
                {
                    group.Features.Add(holyStrike);
                }
            }
            paladin.Progression.UIDeterminatorsGroup = paladin.Progression.UIDeterminatorsGroup.AddToArray(virtousProficiency);
        }
    }
}
