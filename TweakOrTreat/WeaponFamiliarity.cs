using CallOfTheWild;
using CallOfTheWild.HoldingItemsMechanics;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CallOfTheWild.WeaponsFix;

namespace TweakOrTreat
{
    //[HarmonyLib.HarmonyPatch(typeof(Game), "LoadNewGame", new[] {typeof(BlueprintAreaPreset), typeof(SaveInfo) } )]
    //class LevelUpController_SetupNewCharacher_Patch
    //{
    //    static void Postfix(ref Game __instance)
    //    {
    //        __instance.State.PlayerState.MainCharacter.Value.Descriptor.IsEssentialForGame = false;
    //    }
    //}
    public class AddFactIfFeature : OwnedGameLogicComponent<UnitDescriptor>, IHandleEntityComponent<UnitEntityData>
    {
        public BlueprintFeature feature;
        public BlueprintUnitFact fact;
        [JsonProperty]
        public bool added;

        public override void OnTurnOn()
        {
            if (base.Owner.HasFact(feature))
            {
                base.Owner.AddFact(fact);
                //added = true;
            }
        }

        // Token: 0x06002518 RID: 9496 RVA: 0x0009A104 File Offset: 0x00098304
        public override void OnTurnOff()
        {
            //if (base.Owner.HasFact(feature))
            //{
                base.Owner.RemoveFact(fact);
            //}
        }

        // Token: 0x06002519 RID: 9497 RVA: 0x0009A174 File Offset: 0x00098374
        public void OnEntityCreated(UnitEntityData entity)
        {
            if (base.Owner.HasFact(feature))
            {
                base.Owner.AddFact(fact);
            }
        }

        // Token: 0x0600251A RID: 9498 RVA: 0x00002FA8 File Offset: 0x000011A8
        public void OnEntityRemoved(UnitEntityData entity)
        {
        }
        //public override void OnFactActivate()
        //{
        //    if(base.Owner.HasFact(feature))
        //    {
        //        base.OnFactActivate();
        //    }
        //}

        public override void OnRecalculate()
        {
            this.OnTurnOff();
            this.OnTurnOn();
        }
    }

    [ComponentName("Add proficiencies")]
    [AllowedOn(typeof(BlueprintUnit))]
    [AllowedOn(typeof(BlueprintUnitFact))]
    [AllowMultipleComponents]
    public class AddProficienciesIfFeature : OwnedGameLogicComponent<UnitDescriptor>, IHandleEntityComponent<UnitEntityData>
    {
        // Token: 0x06002517 RID: 9495 RVA: 0x0009A094 File Offset: 0x00098294
        public override void OnTurnOn()
        {
            if (base.Owner.HasFact(feature))
            {
                this.ArmorProficiencies.EmptyIfNull<ArmorProficiencyGroup>().ForEach(delegate (ArmorProficiencyGroup i)
                {
                    base.Owner.Proficiencies.Add(i);
                });
                this.WeaponProficiencies.EmptyIfNull<WeaponCategory>().ForEach(delegate (WeaponCategory i)
                {
                    base.Owner.Proficiencies.Add(i);
                });
            }
        }

        // Token: 0x06002518 RID: 9496 RVA: 0x0009A104 File Offset: 0x00098304
        public override void OnTurnOff()
        {
            //if (base.Owner.HasFact(feature))
            //{
                this.ArmorProficiencies.EmptyIfNull<ArmorProficiencyGroup>().ForEach(delegate (ArmorProficiencyGroup i)
                {
                    base.Owner.Proficiencies.Remove(i);
                });
                this.WeaponProficiencies.EmptyIfNull<WeaponCategory>().ForEach(delegate (WeaponCategory i)
                {
                    base.Owner.Proficiencies.Remove(i);
                });
            //}
        }

        // Token: 0x06002519 RID: 9497 RVA: 0x0009A174 File Offset: 0x00098374
        public void OnEntityCreated(UnitEntityData entity)
        {
            if (base.Owner.HasFact(feature))
            {
                this.ArmorProficiencies.EmptyIfNull<ArmorProficiencyGroup>().ForEach(delegate (ArmorProficiencyGroup i)
                {
                    entity.Descriptor.Proficiencies.Add(i);
                });
                this.WeaponProficiencies.EmptyIfNull<WeaponCategory>().ForEach(delegate (WeaponCategory i)
                {
                    entity.Descriptor.Proficiencies.Add(i);
                });
            }
        }

        // Token: 0x0600251A RID: 9498 RVA: 0x00002FA8 File Offset: 0x000011A8
        public void OnEntityRemoved(UnitEntityData entity)
        {
        }

        public BlueprintFeature feature;

        // Token: 0x04001982 RID: 6530
        public ArmorProficiencyGroup[] ArmorProficiencies;

        // Token: 0x04001983 RID: 6531
        public WeaponCategory[] WeaponProficiencies;
    }

    class WeaponFamiliarity
    {
        static LibraryScriptableObject library => Main.library;

        public static BlueprintFeature halflingWeaponFamiliarity;
        public static BlueprintFeature gnomeWeaponFamiliarity;
        static internal void load()
        {
            var martialWeaponProficiency = library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629");

            var elf = library.Get<BlueprintRace>("25a5878d125338244896ebd3238226c8");
            var elvenWeaponFamiliarity = library.Get<BlueprintFeature>("03fd1e043fc678a4baf73fe67c3780ce");
            var dwarf = library.Get<BlueprintRace>("c4faf439f0e70bd40b5e36ee80d06be7");
            var dwarvenWeaponFamiliarity = library.Get<BlueprintFeature>("a1619e8d27fe97c40ba443f6f8ab1763");
            var halforc = library.Get<BlueprintRace>("1dc20e195581a804890ddc74218bfd8e");
            var orcWeaponFamiliarity = library.Get<BlueprintFeature>("6ab6c271d1558344cbc746350243d17d");
            halflingWeaponFamiliarity = Helpers.CreateFeature(
                    "HalflingWeaponFamiliarityFeature",
                    "Halfling Weapon Familiarity",
                    "Halflings treat any weapon with the word “halfling” in its name as a martial weapon.",
                    "",
                    Helpers.GetIcon("25da2dc95ed4a6b419608c678f2a9cc3"),
                    FeatureGroup.Racial
                );
            var halfling = library.Get<BlueprintRace>("b0c3ef2729c498f47970bb50fa1acd30");
            halfling.Features = halfling.Features.AddToArray(halflingWeaponFamiliarity);
            gnomeWeaponFamiliarity = Helpers.CreateFeature(
                    "GnomeWeaponFamiliarityFeature",
                    "Gnome Weapon Familiarity",
                    "Gnomes treat any weapon with the word “gnome” in its name as a martial weapon.",
                    "",
                    Helpers.GetIcon("018ad48ffd3460d47900491656d2ff26"),
                    FeatureGroup.Racial
                );
            var gnome = library.Get<BlueprintRace>("ef35a22c9a27da345a4528f0d5889157");
            gnome.Features = gnome.Features.AddToArray(gnomeWeaponFamiliarity);

            var raceToFamiliarity = new Dictionary<BlueprintRace, BlueprintFeature>() {
                {elf, elvenWeaponFamiliarity},
                {dwarf, dwarvenWeaponFamiliarity},
                {halforc, orcWeaponFamiliarity},
                {halfling, halflingWeaponFamiliarity},
                {gnome, gnomeWeaponFamiliarity},
            };

            var newAddProfs = new List<BlueprintComponent>();
            foreach(var c in martialWeaponProficiency.GetComponents<AddProficiencies>())
            {
                if(c.RaceRestriction)
                {
                    if(raceToFamiliarity.ContainsKey(c.RaceRestriction))
                    {
                        var newAddProf = new AddProficienciesIfFeature();
                        newAddProf.ArmorProficiencies = c.ArmorProficiencies;
                        newAddProf.WeaponProficiencies = c.WeaponProficiencies;
                        newAddProf.feature = raceToFamiliarity[c.RaceRestriction];
                        //newAddProf.IsReapplying = true;

                        newAddProfs.Add(newAddProf);
                    }
                }
            }
            martialWeaponProficiency.ReapplyOnLevelUp = true;
            martialWeaponProficiency.RemoveComponents<AddProficiencies>(c => c.RaceRestriction);

            //var dwarvenWaraxeProficiency = library.Get<BlueprintFeature>("bd0d7feca087d2247b12965c1467790c");

            //var waraxeCategory = library.Get<BlueprintWeaponType>("a6925f5f897801449a648d865637e5a0");
            var dwarvenWaraxeProficiency = Helpers.CreateFeature("DwarvenWaraxeFullProfFeature", "", "", "", null, FeatureGroup.Racial);
            dwarvenWaraxeProficiency.HideInUI = true;
            dwarvenWaraxeProficiency.AddComponent(
                Helpers.Create<FullProficiency>(fp => { fp.category = WeaponCategory.DwarvenWaraxe;  })
            );


            martialWeaponProficiency.RemoveComponents<FullProficiency>();
            //martialWeaponProficiency.RemoveComponents<CanHoldIn1Hand>();
            martialWeaponProficiency.AddComponent(
                Helpers.Create<AddFactIfFeature>(
                    a =>
                    {
                        a.fact = dwarvenWaraxeProficiency;
                        a.feature = dwarvenWeaponFamiliarity;
                    }
                )
            );

            martialWeaponProficiency.AddComponents(newAddProfs);
        }
    }
}
