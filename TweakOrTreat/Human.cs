using CallOfTheWild;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UI._ConsoleUI.CharGen.Phases.AbilityScores;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TweakOrTreat
{
    public class RemoveSelectableBonus : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        public BlueprintFeatureSelection selection;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
        }

        public override void OnFactActivate()
        {
            try
            {
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    levelUp.State.CanSelectRaceStat = false;
                    levelUp.State.SelectedRaceStat = null;
                    typeof(LevelUpController).GetMethod("RemoveAction", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(typeof(SelectRaceStat))
                        .Invoke(levelUp, new object[] { null });
                    typeof(LevelUpController).GetMethod("UpdatePreview", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(levelUp, new object[] {});
                }
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }

    public class RemoveSkillPoint : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        public BlueprintFeatureSelection selection;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
        }

        public override void OnFactActivate()
        {
            try
            {
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    levelUp.State.ExtraSkillPoints--;
                }
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }

        public override void OnFactDeactivate()
        {
            try
            {
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    levelUp.State.ExtraSkillPoints++;
                }
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }


    public class RemoveSelection : OwnedGameLogicComponent<UnitDescriptor>, ILevelUpCompleteUIHandler
    {
        public BlueprintFeatureSelection selection;

        public void HandleLevelUpComplete(UnitEntityData unit, bool isChargen)
        {
        }

        public override void OnFactActivate()
        {
            try
            {
                var levelUp = Game.Instance.UI.CharacterBuildController.LevelUpController;
                if (Owner == levelUp.Preview || Owner == levelUp.Unit)
                {
                    levelUp.State.Selections.RemoveAll(s => s.Selection == selection);
                }
            }
            catch (Exception e)
            {
                Main.logger.Error(e.ToString());
            }
        }
    }

    class Human
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            BlueprintRace human = library.Get<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4");
            var feat = library.Get<BlueprintFeatureSelection>("247a4068296e8be42890143f451b4b45"); // feat
            human.Features = human.Features.Where(f => f != feat).ToArray();
            var humanFeat = library.CopyAndAdd(feat, "HumanExtraFeat", "");
            //new feat selection in place of original one, for safety
            human.Features = human.Features.AddToArray<BlueprintFeatureBase>(humanFeat);

            var bonusFeatComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = humanFeat;
                }),
                Helpers.Create<RemoveFeatureOnApply>( c =>
                {
                    c.Feature = humanFeat;
                }),
                Helpers.Create<RemoveSelection>(c =>
                {
                    c.selection = humanFeat;
                }),
            };
            var skilledComponents = new BlueprintComponent[] {
                Helpers.Create<PrerequisiteFeature>(c =>
                {
                    c.Feature = library.Get<BlueprintFeature>("3adf9274a210b164cb68f472dc1e4544"); // skilled
                }),
                Helpers.Create<RemoveFeatureOnApply>(c =>
                {
                    c.Feature = library.Get<BlueprintFeature>("3adf9274a210b164cb68f472dc1e4544"); // skilled
                }),
            };
            var changedComponent = library.Get<BlueprintFeature>("3adf9274a210b164cb68f472dc1e4544").GetComponent<CallOfTheWild.NewMechanics.AddSkillPointOnEvenLevels>();
            if (changedComponent == null)
            {
                //Main.logger.Log("we got null");
                skilledComponents = skilledComponents.AddToArray(
                    Helpers.Create<RemoveSkillPoint>()
                );
            }
            //Main.logger.Log("we past getting null");
            var statBonuses = new List<BlueprintFeature>();

            foreach(var stat in StatTypeHelper.Attributes)
            {
                var name = Enum.GetName(typeof(StatType), stat);
                var blank = '\u200B';
                var space = "";
                for(int i=0; i< (int)stat; i++)
                {
                    space += blank;
                }
                var feature = Helpers.CreateFeature(name+"dualtalent", "Dual Talent "+ space + name + " Bonus",
                    "Some humans are uniquely skilled at maximizing their natural gifts. These humans pick two ability scores and gain a +2 racial bonus in each of those scores. This racial trait replaces the +2 bonus to any one ability score, the bonus feat, and the skilled traits.",
                    "", null, FeatureGroup.Domain, 
                    stat.CreateAddStatBonus(2, ModifierDescriptor.Racial)
                );
                statBonuses.Add(feature);
            }
            var dualTalentSelection = Helpers.CreateFeatureSelection("DualTalentselection", "Dual Talent",
                "Some humans are uniquely skilled at maximizing their natural gifts. These humans pick two ability scores and gain a +2 racial bonus in each of those scores. This racial trait replaces the +2 bonus to any one ability score, the bonus feat, and the skilled traits.",
                "", null, FeatureGroup.Domain);
            dualTalentSelection.AllFeatures = statBonuses.ToArray();

            var dualTalent = Utils.CreateFeature("DualTalent", "Dual Talent",
                "Some humans are uniquely skilled at maximizing their natural gifts. These humans pick two ability scores and gain a +2 racial bonus in each of those scores. This racial trait replaces the +2 bonus to any one ability score, the bonus feat, and the skilled traits.",
                "", null, FeatureGroup.Racial, 
                bonusFeatComponents,
                skilledComponents,
                new BlueprintComponent[]
                {
                    Helpers.Create<RemoveSelectableBonus>(),
                
                    Helpers.Create<CallOfTheWild.EvolutionMechanics.addSelection>( c =>
                    {
                        c.selection = dualTalentSelection;
                    }),
                    Helpers.Create<CallOfTheWild.EvolutionMechanics.addSelection>( c =>
                    {
                        c.selection = dualTalentSelection;
                    }),
                }
            );

            var exoticWeaponProficiency = library.Get<BlueprintFeatureSelection>("9a01b6815d6c3684cb25f30b8bf20932");
            var martialWeaponProficiency = library.Get<BlueprintFeature>("203992ef5b35c864390b4e4a1e200629");

            var militaryTraditionDescription = "Several human cultures raise all children (or all children of a certain social class) to serve in the military or defend themselves with force of arms. They gain proficiency with up to two martial or exotic weapons appropriate to their culture. This racial trait replaces the bonus feat trait.";

            var militaryTraditionSelection = library.CopyAndAdd(exoticWeaponProficiency, "MilitaryTraditionSelection", "");
            militaryTraditionSelection.AllFeatures = militaryTraditionSelection.AllFeatures.AddToArray(martialWeaponProficiency);
            militaryTraditionSelection.Group = FeatureGroup.Domain;
            militaryTraditionSelection.SetDescription(militaryTraditionDescription);
            militaryTraditionSelection.SetName("Military Tradition");

            var militaryTradition = Utils.CreateFeature("MilitaryTradition", "Military Tradition",
                militaryTraditionDescription,
                "", null, FeatureGroup.Racial,
                bonusFeatComponents,
                new BlueprintComponent[]
                {
                    Helpers.Create<CallOfTheWild.EvolutionMechanics.addSelection>( c =>
                    {
                        c.selection = militaryTraditionSelection;
                    }),
                    Helpers.Create<CallOfTheWild.EvolutionMechanics.addSelection>( c =>
                    {
                        c.selection = militaryTraditionSelection;
                    }),
                }
            );

            var giantAncestry = Utils.CreateFeature("GiantAncestery", "Giant Ancestery",
                "Humans with ogre or troll ancestry end up having hulking builds and asymmetrical features. Such humans gain a +1 bonus on combat maneuver checks and to CMD, but a –2 penalty on Stealth checks. This racial trait replaces skilled",
                "", null, FeatureGroup.Racial,
                skilledComponents,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddStatBonus(StatType.AdditionalCMB, 1, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateAddStatBonus(StatType.AdditionalCMD, 1, ModifierDescriptor.UntypedStackable),
                    Helpers.CreateAddStatBonus(StatType.SkillStealth, -2, ModifierDescriptor.UntypedStackable)
                }
            );

            var awarness = Utils.CreateFeature("Awareness", "Awareness",
                "Humans raised within monastic traditions or communities that encourage mindfulness seem to shrug off many dangers more easily than other humans. They gain a +1 racial bonus on all saving throws and concentration checks. This racial trait replaces humans’ bonus feat.",
                "", null, FeatureGroup.Racial,
                bonusFeatComponents,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddStatBonus(StatType.SaveWill, 1, ModifierDescriptor.Racial),
                    Helpers.CreateAddStatBonus(StatType.SaveReflex, 1, ModifierDescriptor.Racial),
                    Helpers.CreateAddStatBonus(StatType.SaveFortitude, 1, ModifierDescriptor.Racial),
                    Helpers.Create<ConcentrationBonus>(c => c.Value = 1),
                }
            );

            var keenSenses = library.Get<BlueprintFeature>("9c747d24f6321f744aa1bb4bd343880d");
            var heartOfTheFey = Utils.CreateFeature("HeartoftheFey", "Heart of the Fey",
                "Fey-touched humans enjoy senses and reactions superior to those of their kin. These humans receive Keen Senses, gain a +1 racial bonus on Reflex and Will saves, and treat Lore (nature) and Perception as class skills. This racial trait replaces skilled.",
                "", null, FeatureGroup.Racial,
                skilledComponents,
                new BlueprintComponent[]
                {
                    Helpers.CreateAddStatBonus(StatType.SaveWill, 1, ModifierDescriptor.Racial),
                    Helpers.CreateAddStatBonus(StatType.SaveReflex, 1, ModifierDescriptor.Racial),
                    Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillLoreNature),
                    Helpers.Create<AddClassSkill>(a => a.Skill = StatType.SkillPerception),
                    Helpers.CreateAddFact(keenSenses)  
                }
            );

            var persuasive = library.Get<BlueprintFeature>("86d93a5891d299d4983bdc6ef3987afd");
            var powerfulPresence = Utils.CreateFeature("PowerfulPresence", "Powerful Presence",
                "Humans with a regal bearing and strong personal magnetism can apply their presence more forcefully than others. A human with this trait treats her Charisma score as 2 points higher for the purpose of meeting feat prerequisites. In addition, humans with this racial trait gain Persuasive as a bonus feat. This racial trait replaces the bonus feat trait.",
                "", null, FeatureGroup.Racial,
                bonusFeatComponents,
                new BlueprintComponent[]
                {
                    Helpers.Create<ReplaceStatForPrerequisites>(r =>
                        {
                            r.OldStat = StatType.Charisma;
                            r.NewStat = StatType.Charisma;
                            r.Policy = ReplaceStatForPrerequisites.StatReplacementPolicy.Summand;
                            r.SpecificNumber = 2;
                        }
                    ),
                    Helpers.CreateAddFact(persuasive)
                }
            );

            var spellPenetration = library.Get<BlueprintFeature>("ee7dc126939e4d9438357fbd5980d459");
            var unstoppableMagic = library.CopyAndAdd(spellPenetration, "UnstoppableMagic", "");
            unstoppableMagic.AddComponents(bonusFeatComponents);
            unstoppableMagic.SetName("Unstoppable Magic");
            unstoppableMagic.SetDescription("Humans from civilizations built upon advanced magic, such as Geb or Nex, are educated in a variety of ways to accomplish their magical goals. They gain a +2 racial bonus on caster level checks against spell resistance. This racial trait replaces the bonus feat trait.");

            //var elvenWeapons = library.Get<BlueprintFeature>("03fd1e043fc678a4baf73fe67c3780ce");
            //var dwarvenWeapons = library.Get<BlueprintFeature>("a1619e8d27fe97c40ba443f6f8ab1763");
            //var orcWeapons = library.Get<BlueprintFeature>("6ab6c271d1558344cbc746350243d17d");
            //var drowWeapons = library.Get<BlueprintFeature>("10d0be4122534a9eb41173e88b3e8cc7");
            //var adoptiveParentage = Helpers.CreateFeatureSelection("AdoptiveParentage", "Adoptive Parentage",
            //    "Humans are sometimes orphaned and adopted by other races. Choose one humanoid race without the human subtype. You start play with that race’s weapon familiarity racial trait. This racial trait replaces the bonus feat trait.",
            //    "", null, FeatureGroup.Racial,
            //    bonusFeatComponents
            //);
            //adoptiveParentage.AllFeatures = adoptiveParentage.AllFeatures.AddToArray(
            //    elvenWeapons,
            //    dwarvenWeapons,
            //    orcWeapons,
            //    drowWeapons
            //);

            RacesUnleashed.RacialTraits.AddAlternativeRacialTraitsSelection(human, 2, new List<BlueprintFeature>() {
                dualTalent,
                militaryTradition,
                giantAncestry,
                awarness,
                heartOfTheFey,
                powerfulPresence,
                unstoppableMagic,
                //adoptiveParentage
            });
        }
    }
}
