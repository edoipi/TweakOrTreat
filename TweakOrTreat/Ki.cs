using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    [HarmonyLib.HarmonyPatch(typeof(CallOfTheWild.Archetypes.Ninja), "createKiPool")]
    class Ninja_createKiPool_Patch
    {
        static bool Prepare()
        {
            return true;
        }

        //replace ki resource that will be used to create ninja talents with monk one and update it where it was already used
        static void Postfix()
        {
            CallOfTheWild.Archetypes.Ninja.ki_resource = CallOfTheWild.Main.library.Get<BlueprintAbilityResource>("9d9c90a9a1f52d04799294bf91c80a82");
            
            var extraAttack = CallOfTheWild.Main.library.Get<BlueprintAbility>("f00816f78c1c4615a2a6338ad7966276");
            extraAttack.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = CallOfTheWild.Archetypes.Ninja.ki_resource);

            var speed = CallOfTheWild.Main.library.Get<BlueprintAbility>("64cbbff39cd74f09afb0481d57b7e3ed");
            speed.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = CallOfTheWild.Archetypes.Ninja.ki_resource);

            CallOfTheWild.Archetypes.Ninja.ki_pool.ReplaceComponent<AddAbilityResources>(
                a => a.Resource = CallOfTheWild.Archetypes.Ninja.ki_resource
            );
        }
    }

    //I know what the problem with patching is but I'm too lazy to overcome it properly
    [HarmonyLib.HarmonyPatch(typeof(CallOfTheWild.SkillUnlocks), "load")]
    class MonkKiPowers_load_Patch
    {
        static bool Prepare()
        {
            return true;
        }

        //replace ki resource that will be used to create ninja talents with monk one and update it where it was already used
        static void Postfix()
        {
            try
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CallOfTheWild.MonkKiPowers).TypeHandle);
                CallOfTheWild.MonkKiPowers.cha_resource = CallOfTheWild.MonkKiPowers.wis_resource;
            }
            catch (Exception e)
            {
                Main.logger.Log(String.Format("Error while attempting to patch monk ki powers {0}", e));
            }
        }
    }

    public class IncreaseResourcesByClassOnly : OwnedGameLogicComponent<UnitDescriptor>, IResourceAmountBonusHandler, IUnitSubscriber
    {
        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            int classLevel = base.Owner.Progression.GetClassLevel(this.CharacterClass);
            classLevel += levelAdjustment;
            classLevel = (int)( classLevel * levelMult);

            if (base.Fact.Active && resource == this.Resource)
            {
                bonus += classLevel + this.BaseValue;
            }
        }
        
        public BlueprintAbilityResource Resource;
        public BlueprintCharacterClass CharacterClass;
        public int BaseValue;
        public double levelMult = 1;
        public int levelAdjustment = 0;
    }

    public class IncreaseResourcesByStatOnly : OwnedGameLogicComponent<UnitDescriptor>, IResourceAmountBonusHandler, IUnitSubscriber
    {
        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            ModifiableValueAttributeStat stat = base.Owner.Stats.GetStat<ModifiableValueAttributeStat>(this.Stat);
            int num = (stat != null) ? stat.Bonus : 0;
            num /= statDiv;

            if (base.Fact.Active && resource == this.Resource)
            {
                bonus += num + this.BaseValue;
            }
        }

        public BlueprintAbilityResource Resource;
        public StatType Stat;
        public int BaseValue;
        public int statDiv = 1;
    }

    class Ki
    {
        static LibraryScriptableObject library => Main.library;
        public static void load()
        {
            var kiResource = library.Get<BlueprintAbilityResource>("9d9c90a9a1f52d04799294bf91c80a82");
            var rogue = library.Get<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");

            var amount = Helpers.GetField(kiResource, "m_MaxAmount");
            var classes = Helpers.GetField<BlueprintCharacterClass[]>(amount, "ClassDiv").AddToArray(rogue);
            Helpers.SetField(amount, "ClassDiv", classes);
            Helpers.SetField(amount, "ArchetypesDiv", new BlueprintArchetype[] {
                CallOfTheWild.Archetypes.Ninja.archetype
            });
            Helpers.SetField(amount, "IncreasedByStat", false);
            Helpers.SetField(kiResource, "m_MaxAmount", amount);

            var kiWisdomBonus = Helpers.CreateFeature("KiWisdomBonus", "Ki Poll Wisdom Bonus", "", "", null, FeatureGroup.None,
                Helpers.Create<IncreaseResourcesByStatOnly>(i =>
                    {
                        i.Stat = StatType.Wisdom;
                        i.Resource = kiResource;
                    }
                )
            );
            kiWisdomBonus.HideInCharacterSheetAndLevelUp = true;
            kiWisdomBonus.HideInUI = true;

            var kiCharismaBonus = Helpers.CreateFeature("KiCharismaBonus", "Ki Poll Charisma Bonus", "", "", null, FeatureGroup.None,
                Helpers.Create<IncreaseResourcesByStatOnly>(i =>
                    {
                        i.Stat = StatType.Charisma;
                        i.Resource = kiResource;
                    }
                )
            );
            kiCharismaBonus.HideInCharacterSheetAndLevelUp = true;
            kiCharismaBonus.HideInUI = true;

            var warpriestExtraAttack = library.Get<BlueprintAbility>("62366dad39ba406e97d8d3b2c26b8330");
            warpriestExtraAttack.ReplaceComponent<AbilityResourceLogic>(c => c.RequiredResource = kiResource);
            var warpriestKiArmor = library.Get<BlueprintAbility>("f6b4568a63bb4993b4657b01bac447f7");
            warpriestKiArmor.ReplaceComponent<AbilityResourceLogic>(c => c.RequiredResource = kiResource);
            CallOfTheWild.Warpriest.sacred_fist_ki_pool.ReplaceComponent<AddAbilityResources>(
                a => a.Resource = kiResource
            );
            CallOfTheWild.Warpriest.sacred_fist_ki_pool.AddComponents(
                Helpers.Create<FactSinglify>(
                    f =>
                    {
                        f.NewFacts = new BlueprintUnitFact[] { kiWisdomBonus };
                    }
                ),
                Helpers.Create<IncreaseResourcesByClassOnly>(
                    i =>
                    {
                        i.CharacterClass = CallOfTheWild.Warpriest.warpriest_class;
                        i.Resource = kiResource;
                        i.levelAdjustment = -3;
                        i.levelMult = 0.5;
                    }
                )
            );

            var kiPowerFeature = library.Get<BlueprintFeature>("e9590244effb4be4f830b1e3fffced13");
            kiPowerFeature.AddComponent(
                Helpers.Create<FactSinglify>(
                    f =>
                    {
                        f.NewFacts = new BlueprintUnitFact[] { kiWisdomBonus };
                    }
                )
            );

            CallOfTheWild.Archetypes.Ninja.ki_pool.AddComponent(
                Helpers.Create<FactSinglify>(
                    f =>
                    {
                        f.NewFacts = new BlueprintUnitFact[] { kiCharismaBonus };
                    }
                )
            );

            var scaledKiPowerFeature = library.Get<BlueprintFeature>("ae98ab7bda409ef4bb39149a212d6732");
            scaledKiPowerFeature.ReplaceComponent<AddAbilityResources>(
                a => a.Resource = kiResource
            );
            scaledKiPowerFeature.AddComponent(
                Helpers.Create<FactSinglify>(
                    f =>
                    {
                        f.NewFacts = new BlueprintUnitFact[] { kiCharismaBonus };
                    }
                )
            );

            var scaledExtraAttack = library.Get<BlueprintAbility>("ca948bb4ce1a2014fbf4d8d44b553074");
            scaledExtraAttack.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = kiResource);

            var scaledPowerSelection = library.Get<BlueprintFeatureSelection>("4694f6ac27eaed34abb7d09ab67b4541");
            foreach(var feature in scaledPowerSelection.AllFeatures)
            {
                var addFact = feature.GetComponents<AddFacts>().FirstOrDefault();
                if(addFact != null)
                {
                    var ability = addFact.Facts.FirstOrDefault();
                    if(ability != null)
                    {
                        ability.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = kiResource);
                    }
                }
            }

            var scaledDragonSelection = library.Get<BlueprintFeatureSelection>("f9042eed12dac2745a2eb7a9a936906b");
            foreach (var progression in scaledDragonSelection.AllFeatures)
            {
                var addFact = progression.GetComponent<AddFacts>();
                var furyFeature = addFact.Facts.First();
                var furyFeatureAddFact = furyFeature.GetComponent<AddFacts>();
                var furyAbility = furyFeatureAddFact.Facts.First();
                furyAbility.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = kiResource);

                var weaponEntry = (progression as BlueprintProgression).LevelEntries.First().Features.First();
                var weaponAbility = weaponEntry.GetComponent<AddFacts>().Facts.First();
                weaponAbility.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = kiResource);
            }

            var kiPoolRouge = Helpers.CreateFeature("KiPoolRouge", "Ki Pool",
                "A rogue with this talent gains a small ki pool. This ki pool is similar to a ninja’s ki pool, but the rogue’s ki pool does not grant any extra attacks. The rogue gains a number of ki points equal to her Wisdom modifier. These ki points replenish at the start of each day. She can spend a ki point to gain a +10-foot bonus to movement until the end of her turn.", 
                "", null, FeatureGroup.None,
                Helpers.Create<FactSinglify>(
                    f =>
                    {
                        f.NewFacts = new BlueprintUnitFact[] { kiWisdomBonus };
                    }
                ),
                kiResource.CreateAddAbilityResource()
            );

            //keeping swift action as ninja 
            var kiSpeedBuff = library.CopyAndAdd<BlueprintBuff>("9ea4ec3dc30cd7940a372a4d699032e7", "RougeKiSpeedBoostBuff", "");
            kiSpeedBuff.ReplaceComponent<BuffMovementSpeed>(b => b.Value = 10);
            var kiSpeedBurst = library.CopyAndAdd<BlueprintAbility>("8c98b8f3ac90fa245afe14116e48c7da", "RougeKiSpeedBoostAbility", "");

            var action = Common.changeAction<ContextActionApplyBuff>(
                kiSpeedBurst.GetComponent<AbilityEffectRunAction>().Actions.Actions,
                    c =>
                    {
                        c.Buff = kiSpeedBuff;
                        c.DurationValue = Helpers.CreateContextDuration(1, DurationRate.Rounds);
                    }
            );
            kiSpeedBurst.ReplaceComponent<AbilityEffectRunAction>(Helpers.CreateRunActions(action));
            kiSpeedBurst.ReplaceComponent<AbilityResourceLogic>(a => a.RequiredResource = kiResource);
            kiSpeedBurst.SetNameDescription(
                "Ki Speed Burst",
                "A rogue with this ki power can spend 1 point from his ki pool as a swift action to grant himself a sudden burst of speed. This increases the rogue's base land speed by 10 feet for 1 round."
            );
            kiPoolRouge.AddComponent(Helpers.CreateAddFacts(kiSpeedBurst));

            var rougeTalent = library.Get<BlueprintFeatureSelection>("c074a5d615200494b8f2a9c845799d93");
            rougeTalent.AllFeatures = rougeTalent.AllFeatures.AddToArray(kiPoolRouge);

            var ninjaTrick = Helpers.CreateFeatureSelection("NinjaTrickrougetalent", "Ninja Trick",
                "A rogue with this talent can choose a trick from the ninja trick list. The rogue cannot choose a ninja trick with the same name as a rogue talent. The rogue can choose but cannot use talents that require ki points, unless she has a ki pool. A rogue can pick this talent more than once.", 
                "", null, FeatureGroup.None);
            
            var ninjaTrickFeatures = new List<BlueprintFeature>();

            foreach(var trick in CallOfTheWild.Archetypes.Ninja.ninja_trick.AllFeatures)
            {
                if (!rougeTalent.AllFeatures.Contains(trick))
                {
                    ninjaTrickFeatures.Add(trick);
                }
            }

            ninjaTrick.AllFeatures = ninjaTrickFeatures.ToArray();
            rougeTalent.AllFeatures = rougeTalent.AllFeatures.AddToArray(ninjaTrick);
        }
    }
}
