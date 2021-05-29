using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TweakOrTreat
{
    class ElementalMaster
    {
        static LibraryScriptableObject library => Main.library;

        static BlueprintSpellList makeElementalSpelllist(SpellDescriptor descriptor)
        {
            BlueprintSpellList wizardSeplllist = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");

            return Common.combineSpellLists(
                $"{descriptor}ElementalMasterSpelllist",
                (spell, spelllist, lvl) =>
                {
                    return (spell.SpellDescriptor & descriptor) != 0;
                },
                wizardSeplllist,
                Witch.witch_class.Spellbook.SpellList);
        }

        static public void load()
        {

            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "ElementalMasterArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Elemental Master");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Arcanists with an affinity for elemental forces sometimes focus on one and display its power in everything they do.");
            });
            Helpers.SetField(archetype, "m_ParentClass", CallOfTheWild.Arcanist.arcanist_class);
            library.AddAsset(archetype, "");

            var fireSpellList = makeElementalSpelllist(SpellDescriptor.Fire);
            var waterSpellList = makeElementalSpelllist(SpellDescriptor.Cold);
            var airSpellList = makeElementalSpelllist(SpellDescriptor.Electricity);
            var earthSpellList = makeElementalSpelllist(SpellDescriptor.Acid);

            var fireMovement = library.Get<BlueprintFeature>("f48c7d56a8a13af4d8e1cc9aae579b01");
            var waterMovement = library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8");
            var airMovement = library.Get<BlueprintFeature>("1ae6835b8f568d44c8deb911f74762e4");
            var earthMovement = library.Get<BlueprintFeature>("737ef897849327b45b88b83a797918c8");

            var fireIcon = Helpers.GetIcon("17cc794d47408bc4986c55265475c06f");
            var waterIcon = Helpers.GetIcon("7c692e90592257a4e901d12ae6ec1e41");
            var airIcon = Helpers.GetIcon("cd788df497c6f10439c7025e87864ee4");
            var earthIcon = Helpers.GetIcon("32393034410fb2f4d9c8beaa5c8c8ab7");

            var featureSet = new List<(String name, String oppositionName, SpellDescriptor descriptor, SpellDescriptor oppositionDescriptor,
                Sprite icon, Sprite oppositionIcon, BlueprintSpellList spelllist, BlueprintSpellList OppositionSelllist,  
                BlueprintFeature firstExploit, BlueprintFeature secondExploit, BlueprintFeature movement)>()
            {
                (
                    "Fire", "Water", 
                    SpellDescriptor.Fire, SpellDescriptor.Cold, 
                    fireIcon, waterIcon,
                    fireSpellList, waterSpellList, 
                    Arcanist.flame_arc, Arcanist.burning_flame, fireMovement
                ),
                (
                    "Water", "Fire", 
                    SpellDescriptor.Cold, SpellDescriptor.Fire, 
                    waterIcon, fireIcon,
                    waterSpellList, fireSpellList, 
                    Arcanist.ice_missile, Arcanist.icy_tomb, waterMovement
                ),
                (
                    "Air", "Earth", 
                    SpellDescriptor.Electricity, SpellDescriptor.Acid, 
                    airIcon, earthIcon,
                    airSpellList, earthSpellList, 
                    Arcanist.lightning_lance, Arcanist.dancing_electricity, airMovement
                ),
                (
                    "Earth", "Air",
                    SpellDescriptor.Acid, SpellDescriptor.Electricity, 
                    earthIcon, airIcon,
                    earthSpellList, airSpellList, 
                    Arcanist.acid_jet, Arcanist.lingering_acid, earthMovement
                ),
            };

            var featureSelection = Helpers.CreateFeatureSelection(
                "ElementalMasterProgressionSelection",
                "Elemental Focus",
                "At 1st level, the elemental master must select one element: air, earth, fire, or water. The arcanist can prepare one additional spell per day of each level she can cast, but it must have the elemental descriptor of her chosen element. In addition, any spell she prepares from the opposite elemental school (air opposes earth, fire opposes water) takes up two of her prepared spell slots. As he gains levels he receives additional abilities based on the element chosen.",
                "",
                null,
                FeatureGroup.SpecialistSchool);

            foreach(var e in featureSet)
            {
                var elementProg = Helpers.CreateProgression($"{e.name}ElementalMasterProgression",
                                                               $"Elemental Focus - {e.name}",
                                                               featureSelection.Description,
                                                               "",
                                                               e.icon,
                                                               FeatureGroup.None);

                elementProg.Classes = new BlueprintCharacterClass[] { Arcanist.arcanist_class };
                elementProg.UIDeterminatorsGroup = new BlueprintFeatureBase[] { };

                var extraSlot = Helpers.CreateFeature(
                    $"{e.name}ElementalMasterExtraSlot",
                    elementProg.Name,
                    $"The arcanist can prepare one additional spell per day of each level she can cast, but it must have the {e.descriptor} descriptor.",
                    "",
                    e.icon,
                    FeatureGroup.Domain,
                    Helpers.Create<AddSpecialSpellList>(
                        a =>
                        {
                            a.SpellList = e.spelllist;
                            a.CharacterClass = Arcanist.arcanist_class;
                        }
                    )
                );

                var oppositionSlot = Helpers.CreateFeature(
                    $"{e.name}ElementalMasterOppositionSlot",
                    $"Opposition element - {e.oppositionName}",
                    $"The arcanist must use two spell slots to prepare spells with {e.oppositionDescriptor} descriptor.",
                    "",
                    e.oppositionIcon,
                    FeatureGroup.Domain,
                    Helpers.Create<AddSpellListOppositionSchool>(
                        a =>
                        {
                            a.characterClass = Arcanist.arcanist_class;
                            a.spellList = e.OppositionSelllist;
                        }
                    )
                );
                e.firstExploit.HideInUI = false;

                elementProg.LevelEntries = new LevelEntry[] {
                    Helpers.LevelEntry(1, extraSlot, oppositionSlot),
                    Helpers.LevelEntry(3, e.firstExploit),
                    Helpers.LevelEntry(11, e.secondExploit),
                    Helpers.LevelEntry(15, e.movement),

                };
                elementProg.UIGroups = new UIGroup[] { Helpers.CreateUIGroup(extraSlot, e.firstExploit, e.secondExploit, e.movement) };

                featureSelection.AllFeatures = featureSelection.AllFeatures.AddToArray(elementProg);
            }


            var exploits = CallOfTheWild.Arcanist.arcane_exploits;
            if (!CallOfTheWild.Main.settings.balance_fixes)
            {
                archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, exploits), Helpers.LevelEntry(3, exploits), Helpers.LevelEntry(11, exploits), Helpers.LevelEntry(15, exploits) };
            }
            else
            {
                archetype.RemoveFeatures = new LevelEntry[] { Helpers.LevelEntry(1, exploits), Helpers.LevelEntry(4, exploits), Helpers.LevelEntry(10, exploits), Helpers.LevelEntry(16, exploits) };
            }

            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(1, featureSelection)
            };
            Arcanist.arcanist_class.Archetypes = Arcanist.arcanist_class.Archetypes.AddToArray(archetype);
        }
    }
}
