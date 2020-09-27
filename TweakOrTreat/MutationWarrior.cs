using CallOfTheWild;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    public class MutationWarriorAlchemistClassLevelPrerequsite : Prerequisite
    {
        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit, LevelUpState state)
        {
            return Utils.GetClassLevel(unit, this.alchemistClass) + 
                Utils.GetArchetypeLevel(unit, this.fighterClass, this.mutationWarriorArchetype) >= this.Level;
        }

        public override string GetUIText()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(
                string.Format("{0} {1}: {2}",
                this.alchemistClass.Name,
                UIStrings.Instance.Tooltips.Level,
                this.Level));
            return stringBuilder.ToString();
        }
        
        public BlueprintCharacterClass alchemistClass;
        public BlueprintCharacterClass fighterClass;
        public BlueprintArchetype mutationWarriorArchetype;

        public int Level = 1;

        public static Func<int, MutationWarriorAlchemistClassLevelPrerequsite> getBuilder(
            BlueprintCharacterClass alchemistClass,
            BlueprintCharacterClass fighterClass,
            BlueprintArchetype mutationWarriorArchetype) {
            return (level) => {
                var prereq = Helpers.Create<MutationWarriorAlchemistClassLevelPrerequsite>();
                prereq.alchemistClass = alchemistClass;
                prereq.fighterClass = fighterClass;
                prereq.mutationWarriorArchetype = mutationWarriorArchetype;
                prereq.Level = level;

                return prereq;
            };
        }
    }

    class MutationWarrior
    {
        static void addMutagenScaling(BlueprintFeature mutagen, BlueprintCharacterClass fighter, BlueprintFeature warriorMutagen)
        {
            var spell_level_comps = mutagen.GetComponents<SpellLevelByClassLevel>().ToArray();
            mutagen.RemoveComponents<SpellLevelByClassLevel>();
            foreach (var slc in spell_level_comps)
            {
                mutagen.AddComponent(Helpers.Create<CallOfTheWild.NewMechanics.SpellLevelByClassLevel>(s =>
                {
                    s.Ability = slc.Ability;
                    s.Class = slc.Class;
                    s.ExtraClass = fighter;
                    s.ExtraFeatureToCheck = warriorMutagen;
                }));
            }
        }

        static LibraryScriptableObject library => Main.library;
        static internal void load()
        {
            var fighter = Helpers.GetClass("48ac8db94d5de7645906c7d0ad3bcfbd");
            var archetype = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "MutationWarriorArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Mutation Warrior");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "While most fighters rely on physical fitness and rigorous training to achieve martial superiority, a few prefer to create and imbibe dangerous concoctions that mutate them into fearsome creatures.");
            });
            Helpers.SetField(archetype, "m_ParentClass", fighter);
            library.AddAsset(archetype, "");

            var alchemist = Helpers.GetClass("0937bec61c0dabc468428f496580c721");
            var mutagen = library.Get<BlueprintFeature>("cee8f65448ce71c4b8b8ca13751dd8ea");
            var greaterMutagen = library.Get<BlueprintFeature>("76c61966afdd82048911f3d63c6fe0bc");
            var grandMutagen = library.Get<BlueprintFeature>("6f5cb651e26bd97428523061b07ffc85");
            var feralMutagen = library.Get<BlueprintFeature>("fd5f7b37ab4301c48a88cc196ee5f0ce");
            var nauseatingFlesh = library.Get<BlueprintFeature>("de112af9600e0e446af0c7ef0707e1c8");
            var preserveOrgans = library.Get<BlueprintFeature>("76b4bb8e54f3f5c418f421684c76ef4e");
            var feralWings = library.Get<BlueprintFeature>("78197196e096c6e4eaed5c62fa108b52");
            var spontaneousHealing = library.Get<BlueprintFeature>("2bc1ee626a69667469ab5c1698b99956");

            var discoveries = new BlueprintFeature[] {
                greaterMutagen,
                grandMutagen,
                feralMutagen,
                nauseatingFlesh,
                preserveOrgans,
                spontaneousHealing,
                feralWings
            };

            var prereqBuilder = MutationWarriorAlchemistClassLevelPrerequsite.getBuilder(alchemist, fighter, archetype);
            foreach(var feature in discoveries)
            {
                var levelPrereq = feature.GetComponent<PrerequisiteClassLevel>();
                if (levelPrereq == null)
                    continue;

                feature.RemoveComponents<PrerequisiteClassLevel>();
                feature.AddComponent(prereqBuilder(levelPrereq.Level));
            }

            var mutagenDiscovery = Helpers.CreateFeatureSelection(
                "MutagenDiscovery",
                "Mutagen Discovery",
                "At 7th level and every 4 levels thereafter, the mutation warrior can choose one of the following alchemist discoveries " +
                "to augment his abilities: feral mutagen, grand mutagen, greater mutagen, nauseating flesh, preserve organs, spontaneous healing, " +
                "feral wings. The mutagen warrior uses his fighter level as his effective alchemist level for the purpose of these discoveries.",
                "",
                null,
                FeatureGroup.None
            );

            mutagenDiscovery.AllFeatures = discoveries;

            var warriorMutagen = Helpers.CreateFeature(
                "MuTagenmutationwarrior",
                mutagen.Name,
                "At 3rd level, a mutation warrior discovers how to create a mutagen that he can imbibe in order to heighten his physical prowess at the cost of his personality. This ability functions as the alchemist’s mutagen ability, using his fighter level as his alchemist level.\n" + mutagen.Description,
                "",
                mutagen.Icon,
                FeatureGroup.None,
                Helpers.CreateAddFact(mutagen)
            );

            addMutagenScaling(mutagen, fighter, warriorMutagen);
            addMutagenScaling(greaterMutagen, fighter, warriorMutagen);
            addMutagenScaling(grandMutagen, fighter, warriorMutagen);

            var spontaneousHeaingResource = library.Get<BlueprintAbilityResource>("0b417a7292b2e924782ef2aab9451816");

            var amount = Helpers.GetField(spontaneousHeaingResource, "m_MaxAmount");
            BlueprintCharacterClass[] classes = Helpers.GetField<BlueprintCharacterClass[]>(amount, "ClassDiv").AddToArray(fighter);
            Helpers.SetField(amount, "ClassDiv", classes);
            Helpers.SetField(amount, "ArchetypesDiv", new BlueprintArchetype[] { archetype });
            Helpers.SetField(spontaneousHeaingResource, "m_MaxAmount", amount);

            var armorTraining = library.Get<BlueprintFeature>("3c380607706f209499d951b29d3c44f3");
            var advancedArmorTraining = library.TryGet<BlueprintFeature>("3e65a6725026458faabc9d0c2748974c") ?? armorTraining;
            var armorMastery = library.Get<BlueprintFeature>("ae177f17cfb45264291d4d7c2cb64671");

            archetype.RemoveFeatures = new LevelEntry[] {
                Helpers.LevelEntry(3, armorTraining),
                Helpers.LevelEntry(7, advancedArmorTraining),
                Helpers.LevelEntry(11, advancedArmorTraining),
                Helpers.LevelEntry(15, advancedArmorTraining),
                Helpers.LevelEntry(19, armorMastery),
            };


            archetype.AddFeatures = new LevelEntry[] {
                Helpers.LevelEntry(3, warriorMutagen),
                Helpers.LevelEntry(7, mutagenDiscovery),
                Helpers.LevelEntry(11, mutagenDiscovery),
                Helpers.LevelEntry(15, mutagenDiscovery),
                Helpers.LevelEntry(19, mutagenDiscovery),

                //Helpers.LevelEntry(1, warriorMutagen),
                //Helpers.LevelEntry(1, mutagenDiscovery),
            };
            fighter.Archetypes = fighter.Archetypes.AddToArray(archetype);
            fighter.Progression.UIGroups = fighter.Progression.UIGroups.AddToArray(Helpers.CreateUIGroup(warriorMutagen, mutagenDiscovery));
        }
    }
}
