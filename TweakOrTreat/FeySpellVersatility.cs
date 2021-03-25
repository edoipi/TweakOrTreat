using CallOfTheWild;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    class FeySpellVersatility
    {
        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var bardSpellList = library.Get<BlueprintSpellList>("25a5013493bdcf74bb2424532214d0c8");
            var wizardSpellList = library.Get<BlueprintSpellList>("ba0401fdeb4062f40a7aa95b6f07fe89");
            var witchSpellList = CallOfTheWild.Witch.witch_class.Spellbook.SpellList;

            var combined_spell_list = Common.combineSpellLists("FeySpellVersatilitySpellList",
                                                                (spell, spell_list, lvl) =>
                                                                {
                                                                    if (lvl > 4)
                                                                    {
                                                                        return false;
                                                                    }
                                                                    if(spell.School == SpellSchool.Enchantment)
                                                                    {
                                                                        return true;
                                                                    }
                                                                    if(spell.School == SpellSchool.Illusion)
                                                                    {
                                                                        return true;
                                                                    }
                                                                    if ((spell.SpellDescriptor & (SpellDescriptor.Curse)) != 0)
                                                                    {
                                                                        return true;
                                                                    }
                                                                    return false;
                                                                },
                                                                wizardSpellList, witchSpellList, bardSpellList);

            var ranger = library.Get<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            Common.excludeSpellsFromList(combined_spell_list, ranger.Spellbook.SpellList);

            var feySpellVersatility = Helpers.CreateFeature("FeySpellVersatilityFeature",
                                                            "Fey Spell Versatility (Ranger)",
                                                            "Choose a 1st-level spell, a 2nd-level spell, a 3rd-level spell, and a 4th-level spell from the bard, sorcerer/wizard, or witch spell list that is either from the enchantment or illusion school or a spell with the curse descriptor. Add those spells to your ranger spell list. Once chosen, these spells cannot be changed.",
                                                            "",
                                                            null,
                                                            FeatureGroup.Feat,
                                                            Helpers.Create<CallOfTheWild.NewMechanics.addSpellChoice>(a => { a.spell_book = ranger.Spellbook; a.spell_level = 1; a.spell_list = combined_spell_list; }),
                                                            Helpers.Create<CallOfTheWild.NewMechanics.addSpellChoice>(a => { a.spell_book = ranger.Spellbook; a.spell_level = 2; a.spell_list = combined_spell_list; }),
                                                            Helpers.Create<CallOfTheWild.NewMechanics.addSpellChoice>(a => { a.spell_book = ranger.Spellbook; a.spell_level = 3; a.spell_list = combined_spell_list; }),
                                                            Helpers.Create<CallOfTheWild.NewMechanics.addSpellChoice>(a => { a.spell_book = ranger.Spellbook; a.spell_level = 4; a.spell_list = combined_spell_list; }),
                                                            Helpers.PrerequisiteStatValue(StatType.Charisma, 13),
                                                            Common.createPrerequisiteClassSpellLevel(ranger, 1)
                                                            );

            var hunterFeySpellVersatility = library.CopyAndAdd<BlueprintFeature>(feySpellVersatility, "HunterFeySpellVersatilityFeature", "");
            hunterFeySpellVersatility.ReplaceComponent<PrerequisiteClassSpellLevel>(p => p.CharacterClass = CallOfTheWild.Hunter.hunter_class);
            hunterFeySpellVersatility.SetName("Fey Spell Versatility (Hunter)");
            foreach (var c in hunterFeySpellVersatility.GetComponents<CallOfTheWild.NewMechanics.addSpellChoice>())
            {
                var new_c = c.CreateCopy(asc => asc.spell_book = CallOfTheWild.Hunter.hunter_class.Spellbook);
                hunterFeySpellVersatility.ReplaceComponent(c, new_c);
            }
            library.AddFeats(feySpellVersatility, hunterFeySpellVersatility);
        }
    }
}
