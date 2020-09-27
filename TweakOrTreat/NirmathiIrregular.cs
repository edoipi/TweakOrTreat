using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class NirmathiIrregular
    {
        static LibraryScriptableObject library => Main.library;

        static internal void load()
        {
            var ranger = Helpers.GetClass("cda0615668a6df14eb36ba19ee881af6");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "NirmathiIrregularArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Nirmathi Irregular");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description",
                    "The forested country of Nirmathas is known for its rangers—skilled trackers, marksmen, and silent stalkers under the leafy canopy. Nirmathas has no formal military, for her people love their freedom and are reluctant to take orders from anyone, so her defense falls to individual scouts and small groups of allied commandos. Many of Nirmathas’s rangers take the guide or skirmisher archetypes (see the Advanced Player’s Guide), but some focus on the magic of stealth. These irregular troops fight against the frequent invasions by Molthuni soldiers, striking quickly and melting into the green shadows as soon as their opponents rally themselves for a counterattack.");
            });
            Helpers.SetField(archetype, "m_ParentClass", ranger);
            library.AddAsset(archetype, "");

            var favoriteEnemySelection = library.Get<BlueprintFeature>("16cc2c937ea8d714193017780e7d4fc6");

            archetype.RemoveFeatures = new LevelEntry[] {
            };

            var paladin_class = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");

            //var airSpells = library.Get<BlueprintFeature>("77d5b7b66eae8714fa541b0739ab50c1");
            //var addSpellsORiginal = airSpells.GetComponent<AddSpecialSpellList>();
            //var addSpellsNew = Helpers.Create<AddSpecialSpellList>();
            //addSpellsNew.CharacterClass = paladin_class;
            //addSpellsNew.SpellList = addSpellsORiginal.SpellList;
            //airSpells.AddComponent(addSpellsNew);
            //airSpells.HideInUI = false;
            //airSpells.HideInCharacterSheetAndLevelUp = false;

            var airDomainProgressionDrood = library.Get<BlueprintProgression>("3aef017b78329db4fa53fe8560069886");
            var airDomainProgressionCleric = library.Get<BlueprintProgression>("750bfcd133cd52f42acbd4f7bc9cc365");
            //airDomainProgression.HideInUI = false;
            //airDomainProgression.HideInCharacterSheetAndLevelUp = false;
            //airDomainProgression.Classes = airDomainProgression.Classes.AddToArray(paladin_class);
            //airDomainProgression.Archetypes = airDomainProgression.Archetypes.AddToArray();


            var hunter = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("fec08c1a3187da549abd6b85f27e4432");
            //hunter.AddFeatures = hunter.AddFeatures.AddToArray(Helpers.LevelEntry(1, airDomainProgression));
            var druidClass = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("610d836f3a3a9ed42a4349b62f002e96");
            var DefenderOfTheTrueWorld = ResourcesLibrary.TryGetBlueprint<BlueprintArchetype>("782c46afacd88d448afba6178471a744");

            DefenderOfTheTrueWorld.AddFeatures = DefenderOfTheTrueWorld.AddFeatures.AddToArray(Helpers.LevelEntry(1, airDomainProgressionDrood));

            ClassToProgression.addClassToFact(paladin_class, 
                new BlueprintArchetype[] { hunter }, 
                ClassToProgression.DomainSpellsType.SpecialList, airDomainProgressionDrood, druidClass);

            hunter.AddFeatures = hunter.AddFeatures.AddToArray(Helpers.LevelEntry(1, airDomainProgressionCleric));

            CallOfTheWild.Archetypes.SacredServant.archetype.AddFeatures = CallOfTheWild.Archetypes.SacredServant.archetype.AddFeatures.AddToArray(Helpers.LevelEntry(1, airDomainProgressionCleric));

            ranger.Spellbook.CantripsType = CantripsType.Orisions;

            var cleric = library.Get<BlueprintCharacterClass>("67819271767a9dd4fbfd4ae700befea0");
            var cleric_domain = library.Get<BlueprintFeatureSelection>("48525e5da45c9c243a343fc6545dbdb9");
            //domain_selection = library.CopyAndAdd(cleric_domain, "SacredServantDomainSelection", "");
            ClassToProgression.addClassToDomains(archetype.GetParentClass(), new BlueprintArchetype[] { archetype }, ClassToProgression.DomainSpellsType.SpecialList, cleric_domain, cleric);
            airDomainProgressionCleric.Groups = airDomainProgressionCleric.Groups.AddToArray(FeatureGroup.Domain, FeatureGroup.ClericSecondaryDomain);
            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(2, airDomainProgressionCleric),
                
            };

            ranger.Archetypes = ranger.Archetypes.AddToArray(archetype);
        }
    }
}
