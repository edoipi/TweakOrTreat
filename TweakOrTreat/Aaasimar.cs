using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class PrerequisiteFeaturesFromListWithDescription : PrerequisiteFeaturesFromList
    {
        public string description;
        public override string GetUIText()
        {
            return description;
        }
    }
    class Aaasimar
    {
        static List<StatType> checks = new List<StatType>() { StatType.CheckBluff, StatType.CheckDiplomacy, StatType.CheckIntimidate };
        static bool isSkillOrCheck(StatType stat)
        {
            return StatTypeHelper.IsSkill(stat) || checks.Contains(stat);
        }

        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            //var burningArc = library.Get<BlueprintFeature>("e3e1bc1cab8e8d24583d24d7b6c2b900");
            //var AgathionHeritage = library.Get<BlueprintFeature>("4285d0c6b57444c46a302899e0149b09");
            //var AngelHeritage = library.Get<BlueprintFeature>("ceedc840b113c3348a2f32b434df5fef");
            //var ArchonHeritage = library.Get<BlueprintFeature>("72c04df144dbb644583184a7828c69b9");
            //var AzataHeritage = library.Get<BlueprintFeature>("0f9170125bc1bac478c62ef1433fa1ec");
            //var ClassicHeritage = library.Get<BlueprintFeature>("3b545be3f5cc9fd4081937c226360625");
            //var GarudaHeritage = library.Get<BlueprintFeature>("b3494115041bdc1428b4443bf6bf68c3");
            //var PeriHeritage = library.Get<BlueprintFeature>("4ce39942e203de74198e3d2fd0608b96");
            //var 
            //var heritages = new List<BlueprintFeature>()
            //{
            //    AgathionHeritage, AngelHeritage, ArchonHeritage, AzataHeritage, ClassicHeritage, 
            //};

            var heritagesBlueprint = library.Get<BlueprintFeatureSelection>("67aabcbce8f8ae643a9d08a6ca67cabd");
            var spellLikeList = new List<BlueprintFeature>();
            var skilledList = new List<BlueprintFeature>();
            foreach (var heritage in heritagesBlueprint.AllFeatures)
            {
                var spellLike = heritage.GetComponent<AddFacts>();
                heritage.RemoveComponent(spellLike);
                var spellLikeFeature = Helpers.CreateFeature(
                    "SpellLikeAbilityFeature" + heritage.name,
                    "",
                    "",
                    "",
                    null,
                    FeatureGroup.None,
                    spellLike
                );
                spellLikeFeature.HideInCharacterSheetAndLevelUp = true;
                spellLikeFeature.HideInUI = true;

                spellLikeList.Add(spellLikeFeature);
                heritage.AddComponent(Helpers.CreateAddFact(spellLikeFeature));

                var skilledComponents = heritage.GetComponents<AddStatBonus>().Where(c => isSkillOrCheck(c.Stat));
                //Main.logger.Log($"skilled length: {skilledComponents.Count()}");
                //Main.logger.Log($"skilled length2: {skilledComponents.ToArray().Length}");

                var skilledFeature = Helpers.CreateFeature(
                    "SkilledFeature" + heritage.name,
                    "",
                    "",
                    "",
                    null,
                    FeatureGroup.None
                );

                foreach (var c in skilledComponents)
                {
                    //Main.logger.Log($"skilled feature length: {skilledFeature.ComponentsArray.Length}");
                    skilledFeature.AddComponent(c);
                    //Main.logger.Log($"skilled feature length: {skilledFeature.ComponentsArray.Length}");
                }
                heritage.RemoveComponents<AddStatBonus>(c => isSkillOrCheck(c.Stat));
                //skilledFeature.AddComponents(skilledComponents);
                //skilledFeature.AddComponent(skilledComponents.First());
                skilledFeature.HideInCharacterSheetAndLevelUp = false;
                skilledFeature.HideInUI = false;

                skilledList.Add(skilledFeature);
                heritage.AddComponent(Helpers.CreateAddFact(skilledFeature));
            }

            var spellLikeComponentsList = new List<BlueprintComponent> {
                Helpers.Create<PrerequisiteFeaturesFromListWithDescription>(c =>
                {
                    c.Features = spellLikeList.ToArray();
                    c.Group = Prerequisite.GroupType.All;
                    c.description = "Spell-Like Ability";
                })
            };
            foreach (var s in spellLikeList)
            {
                spellLikeComponentsList.Add(
                    Helpers.Create<RemoveFeatureOnApply>(c =>
                    {
                        c.Feature = s;
                    })
                );
            }

            var skilledComponentList = new List<BlueprintComponent> {
                Helpers.Create<PrerequisiteFeaturesFromListWithDescription>(c =>
                {
                    c.Features = skilledList.ToArray();
                    c.Group = Prerequisite.GroupType.All;
                    c.description = "Skilled";
                })
            };
            foreach (var s in skilledList)
            {
                skilledComponentList.Add(
                    Helpers.Create<RemoveFeatureOnApply>(c =>
                    {
                        c.Feature = s;
                    })
                );
            }

            var spellPenetration = library.Get<BlueprintFeature>("ee7dc126939e4d9438357fbd5980d459");
            var crusadingMagic = library.CopyAndAdd(spellPenetration, "CrusadingMagic", "");
            crusadingMagic.AddComponents(spellLikeComponentsList);
            crusadingMagic.AddComponents(skilledComponentList);
            crusadingMagic.AddComponent(
                Helpers.CreateAddStatBonus(StatType.SkillKnowledgeArcana, 2, Kingmaker.Enums.ModifierDescriptor.Racial)
            );
            crusadingMagic.SetName("Crusading Magic");
            crusadingMagic.SetDescription("Many aasimars feel obligated to train to defend the world against fiends such as the invaders from the Worldwound. These aasimars gain a +2 racial bonus on caster level checks to overcome spell resistance and on Knowledge (planes) checks. This racial trait replaces the skilled and spell-like ability racial traits.");

            var aasimar = library.Get<BlueprintRace>("b7f02ba92b363064fb873963bec275ee");
            RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(aasimar, 1, new List<BlueprintFeature>()
            {
                crusadingMagic
            });
        }
    }
}
