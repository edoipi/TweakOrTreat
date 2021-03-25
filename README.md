Requires Call of the Wild and Races Unleashed to run, optionally Favored Class for some alternate racial traits

Warning: from version 0.2.0 Call of the Wild version required is 1.112. What is more any Wild Stalker that is at least level 4 should be respecced to make rage powers work properly.

Features:
- Mindchemist Alchemist archetype - alchemist with cognatogen instead of mutagen
- Mutation Warrior Fighter archetype - fighter with mutagen
- Wild Stalker Ranger archetype - ranger with rage
- Ocean's Echo Oracle archetype - oracle with bardic performance
- Sylvan Trickter Rogue archetype - rogue with hexes
- Virtuous Bravo paladin - swashbuckler paladin, requires Derring-Do mod to work
- Holy Guide paladin - tiny archetype giving favored terrain and teamwork feat for mercies
- Oath Against Chaos paladin - smite chaos instead of smite evil
- Myrmidiarch magus - more fighter-like magus
- Stonelord paladin - defensive dwarven paladin
- Oath of the People's Council paladin - paladin mixed with bard
- Halcyon Druid - druid with access to arcane
- Ancient Lorekeeper Oracle - oracle with arcane spells
- Alternative racial triats for halfling: fleet of foot
- Alternative racial traits for human: Awarness, Dual Talent, Giant Ancestry, Heart of the Fey, Military Tradition, Powerful Presence, Unstoppable Magic
- Alternative racial traits for half-elf: Dual Minded, Multidisciplined(requires Favored Class)
- Extra Discovery feat
- Arcane Discovery arcanist exploit
- Unification of ki between classes
- Ki pool Rogue Talent(keep in mind it was never made into unchained, so it is questionable if it should be here)
- Rogue can select Ninja Tricks
- Arsenal Chaplain gets acces to Advanced Weapon Training - might be overpowered so be careful
- Extra deities with nice domains: Ragathiel, Tsukiyo and Gruhastha
- Fey Spell Versatility for ranger and hunter
optional:
- move and swift action bardic performance for ocean's echo oracle, sensei monk and evangelist cleric
- hold weapons in two hands in inventory preview - experimental
- replace estoc and rapier animation with one handed slashing weapon animation(still bad but in some ways looks better)
- if holding weapon in one hand with other hand empty, character does not keep the empty hand high in the air
- advisors use their highest stat instead of the one normally used at their current role(disabled by default)
Details:
Mindchemist's Perfect Recall affects both "Knowledge" and "Lore" skills.
Mindchemist loses first instance of poison resistance instead of poison use that does not exist in this game.
This makes it so it does not stack with any other archetype right now, which is actually correct per pnp.
Mutation Warrior stacks with alchemist in a following way: he stacks completely for the purpose of both qualifying
and scaling for discoveries, but only those that can be selected by his Mutagen Discovery. His full level adds duration
to the mutagen but only after he gets his mutagen at level 3(I might change that, but I am not sure which way I prefer it).
Mutation Warrior does not qualify for Extra Discovery.
Wild Stalker required some changes adapting it to the game engine and to actually make it work since noone proof read 
it before publishing.
There is no vision/darkness system in the game so everyone receives 2 perception bonus level 1.
Quarry, greater quarry and capstone were all useless as they depend of features traded out, so they got replaced
with greater rage, tireless rage and mighty rage.
While the text mentions getting 8 rage powers total the listed progression didn't match(one of the powers would be at level 21)
and the progression was very uneven, so I changed it to be every other level.
From version 0.2.0 rage resource and rage powers scale with ranger level - 3 as they are supposed to thanks to Holic's fake level implementation.
Ocean' Echo has no race requirements. Instead of pied piping she gets greater song of discord.
Sylvan Trickster receives nothing in place of trapfinding(oh well...).
Virtuous Bravo capstone has only instant kill option implemented.
Oath Against Chaos has some spells replaced. Additionally he can use Mark of Justice to grant either smite evil or smite chaos, but granting smite evil costs additional lay on hands charges.
Myrmidiarch gets access to both Advanced Weapon Training and Advanced Armor Training. What is more he can select those options instead of new weapon groups or new armor training just as fighter can. The level 11th ability is replaced by ranged spell combat. He receives extra toggle that changes both spell strike and spell combat between ranged and melee modes.
Stonelord is not limited to dwarf-only. It always receives full bonuses regardless of contact with ground. I have not implemented ability to take 5-foot step in stance.
Oath of the People's Council paladin at 11th level gets aura giving straight bonus against all illusion spells.
Halcyon Druid at 13th level gets simple single-cast beast shape IV. He has no alginment requirements - I have found mention of this on srd which is weird give flavour.
Ancient Lorekeeper is not race restricted.

Arcane Discovery exploit is not accessible for exploiter wizard.
Unification of ki between classes: it makes it so every monk type, sacred fist warpriest, ninja and rogue with ki pool talent use the same resource, so multiclass characters can use their ki on abilities from any of those classes. When it comes to getting bonus to ki pool from stat you get it from all the stats that your classes allow, but only once per stat. For example monk/ninja will have pool scaling with wisdom and charisma, but scaled fist/ ninja will scale only with charisma bonus and not charisma bonus*2.
Sensei, evangelist and ocean's echo  get move and swift bardic performance at level 7 and 13, as I believe it should be per pnp rule.
This component can be turned off in settings.
Credits:
Holic92 both for Call of the Wild and direct help. This mod capitalizes a lot on Call of the Wild by directly using functions made by Holic and as reference to see how things can be done. Without it it would most likely never have been made.
Spacehamster for Races Unleashed and tutoring.
Zappastuff aka ThyWoof for the template project that helped a lot with starting this mod.
Eddie for Derring-Do mod.