using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic;

using System.Linq;
using UnityModManagerNet;
using System.Reflection;
using System;
using Kingmaker;
using UnityEngine.SceneManagement;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.ResourceLinks;
using System.Collections.Generic;
using Kingmaker.Blueprints.CharGen;
using UnityEngine;
using Kingmaker.Blueprints.Root;

namespace TweakOrTreat
{
    //[Harmony12.HarmonyPatch(typeof(Kingmaker.UnitLogic.UnitProgressionData), 
    //    "GetClassLevel", new Type[] { typeof(Kingmaker.Blueprints.Classes.BlueprintCharacterClass) })]
    //class Patch1
    //{
    //    /*static int Postfix(int level)
    //    {
    //        Main.logger.Log("Postfix in action, current level is "+level);
    //        return level;
    //    }*/
    //    static void Postfix(ref int __result, ref Kingmaker.UnitLogic.UnitProgressionData __instance, BlueprintCharacterClass characterClass)
    //    {
    //        /*Main.logger.Log("Postfix in action, current level is " + __result);
    //        Main.logger.Log("class is " + characterClass.AssetGuid);*/
    //        if (characterClass.AssetGuid == "dddd788b65d340b1a577db64bca14264")
    //        {
    //            Main.logger.Log("Mocking eldritch level");
    //            __result = __instance.CharacterLevel;
    //        }
    //    }
    //}

    class RemoveFeature : OwnedGameLogicComponent<Kingmaker.UnitLogic.UnitDescriptor>
    {
        // Token: 0x060001AA RID: 426 RVA: 0x0000C9BC File Offset: 0x0000ABBC
        public override void OnFactActivate()
        {
            base.Owner.RemoveFact(this.Feature);
        }

        // Token: 0x060001AB RID: 427 RVA: 0x0000C9CF File Offset: 0x0000ABCF
        public override void OnTurnOn()
        {
            base.Owner.RemoveFact(this.Feature);
        }

        // Token: 0x060001AC RID: 428 RVA: 0x0000C9E2 File Offset: 0x0000ABE2
        public override void OnTurnOff()
        {
            base.Owner.AddFact(this.Feature, null, null);
        }

        // Token: 0x040000B1 RID: 177
        public BlueprintUnitFact Feature;
    }
    class Core
    {
        static public BlueprintCharacterClass edritchHeritageClass;
        static public BlueprintFeatureSelection bloodline_selection;
        static public BlueprintFeature eldritchHeritageFeat;

        static internal void load()
        {
            var library = Main.library;

            BlueprintFeature moveAction = library.Get<BlueprintFeature>("36931765983e96d4bb07ce7844cd897e");
            BlueprintFeature swiftAction = library.Get<BlueprintFeature>("fd4ec50bc895a614194df6b9232004b9");

            BlueprintArchetype sensei = library.Get<BlueprintArchetype>("f8767821ec805bf479706392fcc3394c");
            BlueprintArchetype evangelist = library.Get<BlueprintArchetype>("c420c8bf100742cf943a1dac561d9f57");

            LevelEntry level7 = new LevelEntry();
            level7.Level = 7;
            level7.Features.Add(moveAction);

            LevelEntry level13 = new LevelEntry();
            level13.Level = 13;
            level13.Features.Add(swiftAction);

            sensei.AddFeatures = sensei.AddFeatures.AddToArray(level7);
            sensei.AddFeatures = sensei.AddFeatures.AddToArray(level13);

            evangelist.AddFeatures = evangelist.AddFeatures.AddToArray(level7);
            evangelist.AddFeatures = evangelist.AddFeatures.AddToArray(level13);
            //CustomizationOptions

            BlueprintFeature dualTalent = Helpers.CreateFeature("DualTalent", "Dual Talent", 
                "Some humans are uniquely skilled at maximizing their natural gifts. These humans pick two ability scores and gain a +2 racial bonus in each of those scores. This racial trait replaces the +2 bonus to any one ability score, the bonus feat, and the skilled traits.", 
                "bb26aef1399042fc9c62d24dff2ba5dc", null, FeatureGroup.Racial, new BlueprintComponent[]
            {
                Helpers.Create<PrerequisiteFeature>(delegate(PrerequisiteFeature c)
                {
                    c.Feature = library.Get<BlueprintFeature>("247a4068296e8be42890143f451b4b45"); // feat
                }),
                Helpers.Create<RemoveFeature>(delegate(RemoveFeature c)
                {
                    c.Feature = library.Get<BlueprintFeature>("247a4068296e8be42890143f451b4b45"); // feat
                }),
                Helpers.Create<PrerequisiteFeature>(delegate(PrerequisiteFeature c)
                {
                    c.Feature = library.Get<BlueprintFeature>("3adf9274a210b164cb68f472dc1e4544"); // skilled
                }),
                Helpers.Create<RemoveFeature>(delegate(RemoveFeature c)
                {
                    c.Feature = library.Get<BlueprintFeature>("3adf9274a210b164cb68f472dc1e4544"); // skilled
                }),
                StatType.Dexterity.CreateAddStatBonus(2, ModifierDescriptor.Racial),
            });

            BlueprintRace human = library.Get<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4");
            
            //human.Size = Size.Small;
            //var feat = library.Get<BlueprintFeature>("247a4068296e8be42890143f451b4b45"); // feat
            //var noSelection = Helpers.Create<NoSelectionIfAlreadyHasFeature>();
            //noSelection.Features = new BlueprintFeature[] { dualTalent };
            //noSelection.AnyFeatureFromSelection = true;
            //feat.AddComponent(noSelection);
            //RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(human, 1, new List<BlueprintFeature>() { dualTalent });

            BlueprintFeature dualTalent2 = Helpers.CreateFeature("DualTalent2", "Dual Talent",
                "Some humans are uniquely skilled at maximizing their natural gifts. These humans pick two ability scores and gain a +2 racial bonus in each of those scores. This racial trait replaces the +2 bonus to any one ability score, the bonus feat, and the skilled traits.",
                "69729a2a79e640ffa238bb489d6b2eb0", null, FeatureGroup.Racial, new BlueprintComponent[]
            {
                Helpers.Create<PrerequisiteFeature>(delegate(PrerequisiteFeature c)
                {
                    c.Feature = library.Get<BlueprintFeature>("26a668c5a8c22354bac67bcd42e09a3f"); // feat
                }),
                Helpers.Create<RemoveFeature>(delegate(RemoveFeature c)
                {
                    c.Feature = library.Get<BlueprintFeature>("26a668c5a8c22354bac67bcd42e09a3f"); // feat
                }),
                StatType.Dexterity.CreateAddStatBonus(2, ModifierDescriptor.Racial),
            });

            BlueprintRace halfelf = library.Get<BlueprintRace>("b3646842ffbd01643ab4dac7479b20b0");
            //RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(halfelf, 1, new List<BlueprintFeature>() { dualTalent2 });

            //var wizard_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");

            //edritchHeritageClass = Helpers.Create<BlueprintCharacterClass>();
            //edritchHeritageClass.name = "EldritchHeritageClass";
            //library.AddAsset(edritchHeritageClass, "dddd788b65d340b1a577db64bca14264");

            //edritchHeritageClass.LocalizedName = Helpers.CreateString("EldritchHeritage.Name", "EldritchHeritage");
            //edritchHeritageClass.LocalizedDescription = Helpers.CreateString("EldritchHeritage.Description",
            //    "aaaaaaaaaaaaaaaaaaaaaaa"
            //    );
            //edritchHeritageClass.m_Icon = wizard_class.Icon;
            //edritchHeritageClass.SkillPoints = wizard_class.SkillPoints;
            //edritchHeritageClass.HitDie = wizard_class.HitDie;
            //edritchHeritageClass.BaseAttackBonus = wizard_class.BaseAttackBonus;
            //edritchHeritageClass.FortitudeSave = wizard_class.ReflexSave;
            //edritchHeritageClass.ReflexSave = wizard_class.ReflexSave;
            //edritchHeritageClass.WillSave = wizard_class.WillSave;
            ////arcanist_class.Spellbook = createArcanistSpellbook();
            //edritchHeritageClass.ClassSkills = wizard_class.ClassSkills.AddToArray(StatType.SkillUseMagicDevice);
            //edritchHeritageClass.IsDivineCaster = false;
            //edritchHeritageClass.IsArcaneCaster = true;
            //edritchHeritageClass.StartingGold = wizard_class.StartingGold;
            //edritchHeritageClass.PrimaryColor = wizard_class.PrimaryColor;
            //edritchHeritageClass.SecondaryColor = wizard_class.SecondaryColor;
            //edritchHeritageClass.RecommendedAttributes = new StatType[] { StatType.Intelligence, StatType.Charisma };
            //edritchHeritageClass.NotRecommendedAttributes = new StatType[0];
            //edritchHeritageClass.EquipmentEntities = wizard_class.EquipmentEntities;
            //edritchHeritageClass.MaleEquipmentEntities = wizard_class.MaleEquipmentEntities;
            //edritchHeritageClass.FemaleEquipmentEntities = wizard_class.FemaleEquipmentEntities;
            //edritchHeritageClass.ComponentsArray = wizard_class.ComponentsArray;
            //edritchHeritageClass.StartingItems = wizard_class.StartingItems;

            //edritchHeritageClass.Progression = wizard_class.Progression;

            //edritchHeritageClass.AddComponent(Helpers.PrerequisiteClassLevel(edritchHeritageClass, 21));
            //edritchHeritageClass.HideIfRestricted = true;

            //bloodline_selection = library.CopyAndAdd<BlueprintFeatureSelection>("24bef8d1bee12274686f6da6ccbc8914", "EldritchHeritageBloodlineSelectionFeatureSelection", "");
            //List<BlueprintFeature> bloodlines = new List<BlueprintFeature>();

            //bloodline_selection.SetNameDescription("Bloodline",
            //                                "A blood arcanist selects one bloodline from those available through the sorcerer bloodline class feature. The blood arcanist gains the bloodline arcana and bloodline powers of that bloodline, treating her arcanist level as her sorcerer level. The blood arcanist does not gain bonus feats, or bonus spells from her bloodline.");

            //foreach (var b in bloodline_selection.AllFeatures)
            //{
            //    bloodlines.Add(Common.removeEntriesFromProgression(b as BlueprintProgression, "Arcanist" + b.name, f => f.name.Contains("ClassSkill") || f.name.Contains("SpellLevel") || f.name.Contains("Arcana")));
            //}

            //foreach (var b in bloodlines)
            //{
            //    b.Groups = new FeatureGroup[] { FeatureGroup.Feat };
            //    b.IsClassFeature = false;


            //}
            //bloodline_selection.AllFeatures = bloodlines.ToArray();

            //List<BlueprintFeature> powers = new List<BlueprintFeature>();

            //foreach (var b in bloodline_selection.AllFeatures)
            //{
            //    var b2 = (BlueprintProgression)b;
            //    BlueprintFeatureBase feature = null;// = b2.LevelEntries[0].Features[0];
            //    foreach(var entry in b2.LevelEntries)
            //    {
            //        if(entry.Level == 9)
            //        {
            //            feature = entry.Features[0];
            //        }
            //    }
            //    if(feature != null) {
            //        powers.Add((BlueprintFeature)feature);
            //    }
            //}
            //bloodline_selection.AllFeatures = powers.ToArray();

            //bloodline_selection.Group = FeatureGroup.Feat;
            //edritchHeritageClass.Progression.LevelEntries.AddToArray(Helpers.LevelEntry(1, bloodline_selection));
            //edritchHeritageClass.Progression.UIDeterminatorsGroup = edritchHeritageClass.Progression.UIDeterminatorsGroup.AddToArray(bloodline_selection);


            //ClassToProgression.addClassToDomains(edritchHeritageClass, new BlueprintArchetype[] {}, ClassToProgression.DomainSpellsType.NoSpells, bloodline_selection);
            //Helpers.RegisterClass(edritchHeritageClass);

            //library.AddFeats(bloodline_selection);
        }
    }
}