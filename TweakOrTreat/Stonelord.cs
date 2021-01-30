using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Kingdom;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;
using static Kingmaker.Kingdom.LeaderSlot;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;

namespace TweakOrTreat
{
    

    //creating channel energy right after cotw antipaladin so I can be sure all channel engine related things
    //are called before/after or whenever they should be
    [HarmonyLib.HarmonyPatch(typeof(CallOfTheWild.Antipaladin), "creatAntipaldinClass")]
    class CreateChannelEarth
    {
        public static BlueprintFeature channelEarth;

        static LibraryScriptableObject library => CallOfTheWild.Main.library;

        static void Postfix()
        {
            CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = UnityModManager.modsPath + @"/TweakOrTreat/Icons/";
            var iconHeal = CallOfTheWild.LoadIcons.Image2Sprite.Create(@"channel_earth.png");
            var iconHarm = CallOfTheWild.LoadIcons.Image2Sprite.Create(@"channel_earth_harm.png");
            CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = UnityModManager.modsPath + @"/CallOfTheWild/Icons/";

            var paladin = Helpers.GetClass("bfa11238e7ae3544bbeb4d0b92e897ec");
            var layOnHandsResource = library.Get<BlueprintAbilityResource>("9dedf41d995ff4446a181f143c3db98c");
            var earthSubtype = library.Get<BlueprintFeature>("e147258e5b7c40643893d80c9f2816e8");

            var contextRankConfig = Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { paladin }, progression: ContextRankProgression.OnePlusDiv2);
            var dcScaling = Common.createContextCalculateAbilityParamsBasedOnClasses(new BlueprintCharacterClass[] { paladin }, StatType.Charisma);
            channelEarth = Helpers.CreateFeature(
                "StonelordChannelEarthFeature",
                "Earth Channel",
                "At 4th level, a stonelord gains Elemental Channel (earth) as a bonus feat, which she may activate by spending two uses of her lay on hands ability, using her paladin level as her effective cleric level.",
                "f83b1d0f287747ed8104260084b94cda",
                iconHeal,
                FeatureGroup.None
            );

            var harm = ChannelEnergyEngine.createChannelEnergy(
                ChannelEnergyEngine.ChannelType.PositiveHarm,
                "StonelordChannelEarthHarm",
                "Earth Channel - Harm",
                "Channeling earth causes a burst that damages all creatures of the earth subtype in a 30 - foot radius centered on the stonelord. The amount of damage inflicted is equal to 1d6 points of damage plus 1d6 points of damage for every two stonelord levels beyond 1st (3d6 at 5th, and so on). Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1 / 2 the stonelord's level + the stonelord's Charisma modifier.",
                "f1cf7fb96724480bbfc9b1d1c5ba0097",
                contextRankConfig,
                dcScaling,
                Helpers.CreateResourceLogic(layOnHandsResource, amount: 2)
            );
            harm.SetIcon(iconHarm);

            harm.ReplaceComponent<AbilityEffectRunAction>(
                a =>
                {
                    a.Actions = a.Actions.ReplaceAction<Conditional>(c => {
                        c.IfFalse = c.IfFalse.ReplaceAction<Conditional>(c1 => {
                            var conditions = new Condition[] { Common.createContextConditionHasFact(earthSubtype) };
                            c1.ConditionsChecker = new ConditionsChecker() { Conditions = conditions, Operation = c1.ConditionsChecker.Operation };
                        });
                        c.IfTrue = c.IfTrue.ReplaceAction<Conditional>(c1 => {
                            var conditions = new Condition[] { c1.ConditionsChecker.Conditions[0],  Common.createContextConditionHasFact(earthSubtype) };
                            c1.ConditionsChecker = new ConditionsChecker() { Conditions = conditions, Operation = c1.ConditionsChecker.Operation };
                        });
                    });
                }
            );

            var heal = ChannelEnergyEngine.createChannelEnergy(
                ChannelEnergyEngine.ChannelType.PositiveHeal,
                "StonelordChannelEarthHeal",
                "Earth Channel - Heal",
                "Channeling earth causes a burst that heals all creatures of the earth subtype in a 30-foot radius centered on the stonelord. The amount of damage healed is equal to 1d6 points of damage plus 1d6 points of damage for every two stonelord levels beyond 1st (3d6 at 5th, and so on).",
                "732efcaf5bca4d7da9be0c587c065b46",
                contextRankConfig,
                dcScaling,
                Helpers.CreateResourceLogic(layOnHandsResource, amount: 2)
            );
            heal.SetIcon(iconHeal);

            heal.ReplaceComponent<AbilityEffectRunAction>(
                a =>
                {
                    a.Actions = a.Actions.ReplaceAction<Conditional>(c => {
                        c.IfFalse = c.IfFalse.ReplaceAction<Conditional>(c1 => {
                            var conditions = new Condition[] { Common.createContextConditionHasFact(earthSubtype) };
                            c1.ConditionsChecker = new ConditionsChecker() { Conditions = conditions, Operation = c1.ConditionsChecker.Operation };
                        });
                        c.IfTrue = c.IfTrue.ReplaceAction<Conditional>(c1 => {
                            var conditions = new Condition[] { c1.ConditionsChecker.Conditions[0], Common.createContextConditionHasFact(earthSubtype) };
                            c1.ConditionsChecker = new ConditionsChecker() { Conditions = conditions, Operation = c1.ConditionsChecker.Operation };
                        });
                    });
                }
            );

            var harmBase = Common.createVariantWrapper("StonelordChannelEarthHarmBase", "3b9bc8cd4a7a4ac18e875c446cbbb57a", harm);
            var healBase = Common.createVariantWrapper("StonelordChannelEarthHealBase", "a1d32012874a43b6a40472fa7f0e1e87", heal);

            ChannelEnergyEngine.storeChannel(harm, channelEarth, ChannelEnergyEngine.ChannelType.PositiveHarm);
            ChannelEnergyEngine.storeChannel(heal, channelEarth, ChannelEnergyEngine.ChannelType.PositiveHeal);

            channelEarth.AddComponent(Helpers.CreateAddFacts(harmBase, healBase));
        }
    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class ReplaceAnimalCompanion : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnFactActivate()
        {
            var addPetOld = Owner.Progression.Features.SelectFactComponents<AddPet>().FirstOrDefault<AddPet>();
            var oldPet = addPetOld?.SpawnedPet;

            Owner.RemoveFact(old);
            Owner.AddFact(update);
            Owner.Progression.Features.CallFactComponents<AddPet>(delegate (AddPet i)
            {
                i.TryUpdatePet();
            });

            
            if (oldPet != null && addPetOld.Pet == old.GetComponent<AddPet>().Pet)
            {
                var items = new List<ItemEntity>();
                foreach (var slot in oldPet.Body.EquipmentSlots)
                {
                    if(slot.HasItem && slot.CanRemoveItem())
                    {
                        items.Add(slot.Item);
                        slot.RemoveItem();
                    }
                }
                oldPet.Descriptor.SetMaster(null);
                oldPet.Destroy();

                var newPet = Owner.Pet;
                if (newPet != null)
                {
                    foreach (var item in items)
                    {
                        foreach (var slot in newPet.Body.EquipmentSlots)
                        {
                            if (!slot.HasItem && slot.CanInsertItem(item))
                            {
                                slot.InsertItem(item);
                                break;
                            }
                        }
                    }
                }
            }
            
        }

        // Token: 0x0400587C RID: 22652
        public BlueprintUnitFact old;
        public BlueprintUnitFact update;
    }

    class Stonelord
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var paladin = Helpers.GetClass("bfa11238e7ae3544bbeb4d0b92e897ec");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "StonelordArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Stonelord");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "A stonelord is a devoted sentinel of dwarven enclaves, drawing the power of the earth and ancient stone to protect her people.");
            });
            Helpers.SetField(archetype, "m_ParentClass", paladin);
            archetype.RemoveSpellbook = true;
            library.AddAsset(archetype, "");

            var auraOfJusticeFeature = library.Get<BlueprintFeature>("9f13fdd044ccb8a439f27417481cb00e");
            { 
                var smiteEvil = library.Get<BlueprintFeature>("3a6db57fce75b0244a6a5819528ddf26");
                var smiteEvilExtra = library.Get<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137");
                var divineGrace = library.Get<BlueprintFeature>("8a5b5e272e5c34e41aa8b4facbb746d3");
                var selectionMercy = library.Get<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d");
                var divineHealth = library.Get<BlueprintFeature>("41d1d0de15e672349bf4262a5acf06ce");
                var channelPositive = library.Get<BlueprintFeature>("cb6d55dda5ab906459d18a435994a760");

                var weaponBond = library.Get<BlueprintFeature>("1c7cdc1605554954f838d85bbdd22d90");
                var weaponBond2 = library.Get<BlueprintFeature>("c8db0772b7059ec4eabe55b7e0e79824");
                var weaponBond3 = library.Get<BlueprintFeature>("d2f45a2034d4f7643ba1a450bc5c4c06");
                var weaponBond4 = library.Get<BlueprintFeature>("6d73f49b602e29a43a6faa2ea1e4a425");
                var weaponBond5 = library.Get<BlueprintFeature>("f17c3ba33bb44d44782cb3851d823011");
                var weaponBond6 = library.Get<BlueprintFeature>("b936ee90c070edb46bd76025dc1c5936");

                var weaponBondExtra = library.Get<BlueprintFeature>("5a64de5435667da4eae2e4c95ec87917");

                
                var holyChampion = library.Get<BlueprintFeature>("eff3b63f744868845a2f511e9929f0de");

                archetype.RemoveFeatures = new LevelEntry[] {
                    Helpers.LevelEntry(1, smiteEvil),
                    Helpers.LevelEntry(2, divineGrace),
                    Helpers.LevelEntry(3, selectionMercy, divineHealth),
                    Helpers.LevelEntry(4, smiteEvilExtra, channelPositive),
                    Helpers.LevelEntry(5, weaponBond),
                    Helpers.LevelEntry(7, smiteEvilExtra),
                    Helpers.LevelEntry(8, weaponBond2),
                    Helpers.LevelEntry(9, selectionMercy, weaponBondExtra),
                    Helpers.LevelEntry(10, smiteEvilExtra),
                    Helpers.LevelEntry(11, weaponBond3, auraOfJusticeFeature),
                    Helpers.LevelEntry(12, selectionMercy),
                    Helpers.LevelEntry(13, smiteEvilExtra, weaponBondExtra),
                    Helpers.LevelEntry(14, weaponBond4),
                    Helpers.LevelEntry(15, selectionMercy),
                    Helpers.LevelEntry(16, smiteEvilExtra),
                    Helpers.LevelEntry(17, weaponBond5, weaponBondExtra),
                    //Helpers.LevelEntry(18, selectionMercy), //maybe not that one
                    Helpers.LevelEntry(19, smiteEvilExtra),
                    Helpers.LevelEntry(20, holyChampion, weaponBond6),
                };
            }

            var elementalSmall = library.CopyAndAdd<BlueprintUnit>("a940662426e68524680bc06e1794fa8d", "StonelordElementalSmall", "");
            var elementalMedium = library.CopyAndAdd<BlueprintUnit>("11d8e4b048acc0e4c8e42e76b8ab869d", "StonelordElementalMedium", "");
            var elementalLarge = library.CopyAndAdd<BlueprintUnit>("6345d091fc79e0840b14b908a4e65d4d", "StonelordElementalLarge", "");
            var elementalHuge = library.CopyAndAdd<BlueprintUnit>("3e447739c6b1e2d41b301ee477e41ba7", "StonelordElementalHuge", "");
            var elementalGreater = library.CopyAndAdd<BlueprintUnit>("55f39411dc3c9ef43aa61c2d7fe3bfc9", "StonelordElementalGreater", "");
            var elementalElder = library.CopyAndAdd<BlueprintUnit>("672433d2f2e99764db2eadc6f595a2ba", "StonelordElementalElder", "");

            var elementals = new BlueprintUnit[]{
                elementalSmall,
                elementalMedium,
                elementalLarge,
                elementalHuge,
                elementalGreater,
                elementalElder
            };

            var playerFaction = library.Get<BlueprintFaction>("72f240260881111468db610b6c37c099");

            foreach (var elemental in elementals)
            {
                elemental.Alignment = Alignment.LawfulGood;
                elemental.AddFacts = elemental.AddFacts.AddToArray(CallOfTheWild.Hunter.celestial_template);
                elemental.AddComponent(Helpers.Create<AllowDyingCondition>());
                elemental.AddComponent(Helpers.Create<AddResurrectOnRest>());
                elemental.Faction = playerFaction;
            }

            var addElementals = new List<BlueprintFeature>();

            foreach (var elemental in elementals)
            {
                var addElemental = Helpers.CreateFeature(
                "StoneServant"+elemental.name,
                "Stone Servant",
                "At 5th level, a stonelord may call a Small earth elemental to her side, as a paladin calls her mount. This earth elemental is Lawful Good in alignment and possesses the celestial template, and it increases in size as the stonelord gains levels, becoming Medium at 8th level, Large at 11th level, Huge at 14th level, Greater at 17th level, and Elder at 20th level.",
                "",
                Helpers.GetIcon("5181c2ed0190fc34b8a1162783af5bf4"), //stone call
                FeatureGroup.AnimalCompanion,
                library.Get<BlueprintFeature>("126712ef923ab204983d6f107629c895").ComponentsArray
            );
                addElemental.IsClassFeature = true;
                //addSmallElemental.ReapplyOnLevelUp = true;

                addElemental.ReplaceComponent<AddPet>(a => { a.Pet = elemental; a.UpgradeLevel = 100; a.LevelRank = null; });

                addElementals.Add(addElemental);
            }

            var companionSelection = Helpers.CreateFeatureSelection(
                addElementals[0].name+"Selection",
                addElementals[0].Name,
                addElementals[0].Description,
                "",
                addElementals[0].Icon,
                FeatureGroup.AnimalCompanion
            );
            companionSelection.AllFeatures = new BlueprintFeature[] { addElementals[0] };

            var replaceCompanions = new List<BlueprintFeature>();

            foreach (var pair in addElementals.Zip(addElementals.Skip(1), (a, b) => Tuple.Create(a, b)))
            {
                var previous = pair.Item1;
                var next = pair.Item2;

                var replaceCompanion = Helpers.CreateFeature(
                    "ReplaceCompanion"+previous.name+next.name,
                    "Stone Servant",
                    "At 5th level, a stonelord may call a Small earth elemental to her side, as a paladin calls her mount. This earth elemental is Lawful Good in alignment and possesses the celestial template, and it increases in size as the stonelord gains levels, becoming Medium at 8th level, Large at 11th level, Huge at 14th level, Greater at 17th level, and Elder at 20th level.",
                    "",
                    Helpers.GetIcon("5181c2ed0190fc34b8a1162783af5bf4"), //stone call
                    FeatureGroup.AnimalCompanion,
                    Helpers.Create<ReplaceAnimalCompanion>(
                        r =>
                        {
                            r.old = previous;
                            r.update = next;
                        }
                    )
                );

                replaceCompanions.Add(replaceCompanion);
            }

            var earthSubtype = library.Get<BlueprintFeature>("e147258e5b7c40643893d80c9f2816e8");
            var constructType = library.Get<BlueprintFeature>("fd389783027d63343b4a5634bd81645f");
            BlueprintFeature stonestrike;
            BlueprintFeature stonebane;
            BlueprintBuff stonestrikeBuff;
            ContextActionApplyBuff stonestrikeAction;
            BlueprintAbilityResource stonestrikeResource;
            {
                var icon = Helpers.GetIcon("f069b6557a2013544ac3636219186632");
                var name = "Stonestrike";
                var description = "Once per day per paladin level, a stonelord can draw upon the power of the living rock. As a swift action, she treats her melee attacks until the beginning of her next turn (whether armed or unarmed) as magical and adamantine, with a +1 bonus on attack and damage rolls, as well as on combat maneuver checks and CMD. This bonus increases by +1 at 5th level and every 5 levels thereafter";

                stonestrikeBuff = Helpers.CreateBuff(
                    name + "Buff",
                    name,
                    description,
                    "",
                    icon,
                    null,
                    Helpers.CreateAddContextStatBonus(StatType.AdditionalAttackBonus, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateAddContextStatBonus(StatType.AdditionalDamage, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateAddContextStatBonus(StatType.AdditionalCMD, ModifierDescriptor.UntypedStackable),
                    //Helpers.CreateAddContextStatBonus(StatType.AdditionalCMB, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { paladin },
                                                    progression: ContextRankProgression.OnePlusDivStep, stepLevel: 5),
                    Common.createAddOutgoingMagic(),
                    Common.createAddOutgoingMaterial(Kingmaker.Enums.Damage.PhysicalDamageMaterial.Adamantite)
                );

                stonestrikeAction = Helpers.CreateApplyBuff(stonestrikeBuff, Helpers.CreateContextDuration(1), fromSpell: false, dispellable: false);

                stonestrikeResource = Helpers.CreateAbilityResource(name+"Resource", "", "", "", null);
                stonestrikeResource.SetIncreasedByLevel(0, 1, new BlueprintCharacterClass[] { paladin });

                var baneIcon = Helpers.GetIcon("7ddf7fbeecbe78342b83171d888028cf");
                var bane = library.CopyAndAdd<BlueprintWeaponEnchantment>("eebb4d3f20b8caa43af1fed8f2773328", "StonebaneEnchantment", "");
                Helpers.SetField(bane, "m_EnchantName", Helpers.CreateString(bane.name + ".Name", "Stonebane"));
                bane.ReplaceComponent<WeaponConditionalEnhancementBonus>(
                    w =>
                    {
                        w.Conditions = Helpers.CreateConditionsCheckerOr(
                            Common.createContextConditionHasFact(earthSubtype),
                            Common.createContextConditionHasFact(constructType)
                        );
                    }
                );
                bane.ReplaceComponent<WeaponConditionalDamageDice>(
                    w =>
                    {
                        w.Conditions = Helpers.CreateConditionsCheckerOr(
                            Common.createContextConditionHasFact(earthSubtype),
                            Common.createContextConditionHasFact(constructType)
                        );
                    }
                );
                var baneBuff = Helpers.CreateBuff(
                    "StonebaneBuff",
                    "Stonebane",
                    "At 11th level, when using stonestrike, a stonelord’s attack gains the bane weapon special ability against creatures with the earth subtype and constructs.",
                    "",
                    baneIcon,
                    null,
                    Common.createBuffEnchantWornItem(bane)
                );
                baneBuff.SetBuffFlags(BuffFlags.HiddenInUi);
                stonebane = Helpers.CreateFeature(
                    "StonebaneFeature",
                    "Stonebane",
                    "At 11th level, when using stonestrike, a stonelord’s attack gains the bane weapon special ability against creatures with the earth subtype and constructs.",
                    "",
                    baneIcon,
                    FeatureGroup.None
                );
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(stonestrikeBuff, baneBuff, stonebane);

                var ability = Helpers.CreateAbility(
                    name+"Ability",
                    stonestrikeBuff.Name,
                    stonestrikeBuff.Description,
                    "",
                    stonestrikeBuff.Icon,
                    AbilityType.Supernatural,
                    CommandType.Swift,
                    AbilityRange.Personal,
                    Helpers.oneRoundDuration,
                    "",
                    Helpers.CreateRunActions(stonestrikeAction),
                    stonestrikeResource.CreateResourceLogic()
                );
                ability.setMiscAbilityParametersSelfOnly();

                stonestrike = Common.AbilityToFeature(ability, false);
                stonestrike.AddComponent(stonestrikeResource.CreateAddAbilityResource());
            }

            BlueprintFeature heartstone;
            {
                var icon = Helpers.GetIcon("c66e86905f7606c4eaa5c774f0357b2b");
                var name = "Heartstone";
                var description = "At 2nd level, a stonelord’s flesh becomes progressively rockier. She gains a +1 natural armor bonus to AC and DR/adamantine equal to 1/2 her paladin level. The natural armor bonus increases by +1 at 6th level, and every four levels thereafter, to a maximum of +5 at 18th level.";

                var DR = Helpers.CreateFeature(
                    name + "DRFeature",
                    name,
                    description,
                    "",
                    icon,
                    FeatureGroup.None,
                    Common.createMaterialDR(Helpers.CreateContextValue(AbilityRankType.Default), Kingmaker.Enums.Damage.PhysicalDamageMaterial.Adamantite),
                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: new BlueprintCharacterClass[] { paladin },
                                                    progression: ContextRankProgression.Div2)
                );
                DR.HideInUI = true;
                DR.HideInCharacterSheetAndLevelUp = true;

                heartstone = Helpers.CreateFeature(
                    name+"Feature",
                    name,
                    description,
                    "",
                    icon,
                    FeatureGroup.None,
                    Helpers.CreateAddContextStatBonus(StatType.AC, ModifierDescriptor.NaturalArmor),
                    Common.createAddFeatureIfHasFact(DR, DR, true)
                );
                heartstone.Ranks = 5;
                heartstone.ReapplyOnLevelUp = true;
                heartstone.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureRank, feature: heartstone));
            }

            BlueprintFeature stoneblood1;
            BlueprintFeature stoneblood2;
            BlueprintFeature stoneblood3;
            {
                var icon = Helpers.GetIcon("19d2ea3d212681946920c7b70bb44492");
                var name = "Stoneblood";
                var description = "At 3rd level, a stonelord’s vitals begin to calcify and her blood transforms into liquid stone. She adds her paladin level on checks to stabilize at negative hit points and gains a 25% chance to ignore a critical hit or precision damage. This does not stack with fortification armor or similar effects. At 9th level, this chance increases to 50% and she becomes immune to petrification. At 15th level, this chance increases to 75% and she becomes immune to bleed and blood drain effects.";

                stoneblood1 = Helpers.CreateFeature(
                    name + "1Feature",
                    name,
                    description,
                    "",
                    icon,
                    FeatureGroup.None,
                    Common.createAddFortification(25)
                );

                var pertificationImmunity = library.Get<BlueprintFeature>("b625283fc6eb72c47a2fc5e2a3ff9eb4");
                stoneblood2 = Helpers.CreateFeature(
                    name + "2Feature",
                    name,
                    description,
                    "",
                    icon,
                    FeatureGroup.None,
                    Common.createAddFortification(50),
                    Helpers.CreateAddFact(pertificationImmunity)
                );

                var bleedImmunity = library.Get<BlueprintBuff>("3f6038d75ccffaa40b338f4b13f9e4b6");
                stoneblood3 = Helpers.CreateFeature(
                    name + "3Feature",
                    name,
                    description,
                    "",
                    icon,
                    FeatureGroup.None,
                    Common.createAddFortification(75),
                    Helpers.CreateAddFact(Common.buffToFeature(bleedImmunity))
                );
            }

            BlueprintFeature phaseStrike;
            {
                var icon = Helpers.GetIcon("a26c23a887a6f154491dc2cefdad2c35");
                var name = "Phase Strike";
                var description = "At 12th level, a stonelord’s stonestrike may pass through stone and metal as if they weren’t there. By spending 2 uses of her stonestrike ability, she may ignore any cover less than total cover provided by stone or metal, and she ignores any AC bonus from stone or metal armor or shields as if wielding a brilliant energy weapon. A phase strike cannot damage constructs, objects, or creatures with the earth subtype, but unlike a brilliant energy weapon, it can harm undead.";

                var brilliant = library.CopyAndAdd<BlueprintWeaponEnchantment>("66e9e299c9002ea4bb65b6f300e43770", "PhaseStrikeEnchantment", "");
                Helpers.SetField(brilliant, "m_EnchantName", Helpers.CreateString(brilliant.name + ".Name", name));
                Helpers.SetField(brilliant, "m_Description", Helpers.CreateString(brilliant.name + ".Description", description));
                brilliant.ReplaceComponent<MissAgainstFactOwner>(
                    m =>
                    {
                        m.Facts = new BlueprintUnitFact[]
                        {
                            earthSubtype, constructType
                        };
                    }
                );

                var brilliantBuff = Helpers.CreateBuff(
                    "PhaseStrikeBuff",
                    name,
                    description,
                    "",
                    icon,
                    null,
                    Common.createBuffEnchantWornItem(brilliant)
                );
                var brilliantAction = Helpers.CreateApplyBuff(brilliantBuff, Helpers.CreateContextDuration(1), fromSpell: false, dispellable: false);

                var ability = Helpers.CreateAbility(
                    "PhaseStrikeAbility",
                    brilliantBuff.Name,
                    brilliantBuff.Description,
                    "",
                    brilliantBuff.Icon,
                    AbilityType.Supernatural,
                    CommandType.Swift,
                    AbilityRange.Personal,
                    Helpers.oneRoundDuration,
                    "",
                    Helpers.CreateRunActions(stonestrikeAction, brilliantAction),
                    stonestrikeResource.CreateResourceLogic(amount: 2)
                );
                ability.setMiscAbilityParametersSelfOnly();

                phaseStrike = Common.AbilityToFeature(ability, false);
            }

            BlueprintFeature stoneBody;
            {
                var icon = Helpers.GetIcon("4aa7942c3e62a164387a73184bca3fc1");
                var name = "Stone Body";
                var description = "At 20th level, a stonelord's body transforms into living stone. She becomes immune to paralysis, poison, and stunning. She is also no longer subject to critical hits or precision-based damage.";

                stoneBody = Helpers.CreateFeature(
                    "StoneBodyFeature",
                    name,
                    description,
                    "",
                    icon,
                    FeatureGroup.None,
                    Helpers.Create<AddImmunityToCriticalHits>(),
                    Helpers.Create<AddImmunityToPrecisionDamage>(),
                    Common.createBuffDescriptorImmunity(SpellDescriptor.Poison | SpellDescriptor.Paralysis | SpellDescriptor.Stun),
                    Common.createAddConditionImmunity(UnitCondition.Paralyzed),
                    Common.createAddConditionImmunity(UnitCondition.Stunned)
                );
            }

            var defender = library.Get<BlueprintCharacterClass>("d5917881586ff1d4d96d5b7cebda9464");
            var stance = library.CopyAndAdd<BlueprintFeature>("2a6a2f8e492ab174eb3f01acf5b7c90a", "StonelordStance", "");
            var stancePower = library.Get<BlueprintFeatureSelection>("2cd91c501bda80b47ac2df0d51b02973");
            stance.AddComponents(
                Helpers.Create<CallOfTheWild.EvolutionMechanics.addSelection>(a => a.selection = stancePower),
                Common.createClassLevelsForPrerequisites(paladin, defender)
            );

            var stanceResource = library.Get<BlueprintAbilityResource>("879118b25a0e1984d9693b364741ee7e");
            var amount = Helpers.GetField(stanceResource, "m_MaxAmount");
            BlueprintCharacterClass[] classes = Helpers.GetField<BlueprintCharacterClass[]>(amount, "Class").AddToArray(paladin);
            Helpers.SetField(amount, "Class", classes);
            Helpers.SetField(amount, "Archetypes", new BlueprintArchetype[] { archetype });
            Helpers.SetField(stanceResource, "m_MaxAmount", amount);

            var renewedDefence = library.Get<BlueprintAbility>("ce50ad6751d7eec44a888c78b249050e");
            renewedDefence.ReplaceContextRankConfig(
                c =>
                {
                    if(c.IsBasedOnClassLevel)
                    {
                        Helpers.SetField(c, "m_BaseValueType", ContextRankBaseValueType.SummClassLevelWithArchetype);
                        var classArray = Helpers.GetField<BlueprintCharacterClass[]>(c, "m_Class").AddToArray(paladin);
                        Helpers.SetField(c, "m_Class", classArray);
                        Helpers.SetField(c, "Archetype", archetype);
                    }
                }
            );

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, stonestrike),
                Helpers.LevelEntry(2, heartstone),
                Helpers.LevelEntry(3, stoneblood1),
                Helpers.LevelEntry(4, stance, CreateChannelEarth.channelEarth),
                Helpers.LevelEntry(5, companionSelection),
                Helpers.LevelEntry(6, heartstone),
                Helpers.LevelEntry(8, replaceCompanions[0], stancePower),
                Helpers.LevelEntry(9, stoneblood2),
                Helpers.LevelEntry(10, heartstone),
                Helpers.LevelEntry(11, replaceCompanions[1], stonebane),
                Helpers.LevelEntry(12, phaseStrike, stancePower),
                Helpers.LevelEntry(14, replaceCompanions[2], heartstone),
                Helpers.LevelEntry(15, stoneblood3),
                Helpers.LevelEntry(16, stancePower),
                Helpers.LevelEntry(17, replaceCompanions[3]),
                Helpers.LevelEntry(18, heartstone),
                Helpers.LevelEntry(20, replaceCompanions[4], stoneBody, stancePower),
            };
            paladin.Archetypes = paladin.Archetypes.AddToArray(archetype);

            paladin.Progression.UIGroups = paladin.Progression.UIGroups.AddToArray(
                Helpers.CreateUIGroup(replaceCompanions.ToArray().AddToArray(companionSelection)),
                Helpers.CreateUIGroup(stoneblood1, stoneblood2, stoneblood3, stoneBody),
                Helpers.CreateUIGroup(stonestrike, stonebane, phaseStrike),
                Helpers.CreateUIGroup(stance, stancePower)
            );
            foreach (var group in paladin.Progression.UIGroups)
            {
                if (group.Features.Contains(auraOfJusticeFeature))
                {
                    group.Features.Add(CreateChannelEarth.channelEarth);
                }
            }
        }
    }
}