using CallOfTheWild;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace TweakOrTreat
{
    //patching cotw so that we create dieties before lots of other stuff that changes them
    [HarmonyLib.HarmonyPatch(typeof(CallOfTheWild.Deities), "create")]
    class Deities
    {
        static void Postfix()
        {
            var library = CallOfTheWild.Main.library;
            CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = UnityModManager.modsPath + @"/TweakOrTreat/Icons/";

            var deities = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4");
            var paladinDeities = library.Get<BlueprintFeatureSelection>("a7c8b73528d34c2479b4bd638503da1d");

            {
                var ragathiel = library.CopyAndAdd<BlueprintFeature>("afc775188deb7a44aa4cbde03512c671", "RagathielFeature", "0a111217f49248eabdf8e9abe2537973"); //erastil

                ragathiel.SetNameDescriptionIcon("Ragathiel",
                                                "Ragathiel (pronounced rah-GATH-ee-el) is an empyreal lord—a good servant of the gods who through transcendence has achieved some small measure of divine power—known as the General of Vengeance. His portfolio includes chivalry, duty, and vengeance, and his holy symbol is a bastard sword crossed with a crimson wing.\n"
                                                + "Domains: Destruction, Good, Law, Nobility.\nFavored Weapon: Bastard sword.",
                                                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"ragathiel.png"));

                var bastardSwordProficiency = library.Get<BlueprintFeature>("57299a78b2256604dadf1ab9a42e2873");
                ragathiel.ReplaceComponent<AddFeatureOnClassLevel>(a => a.Feature = bastardSwordProficiency);
                ragathiel.ReplaceComponent<AddStartingEquipment>(a => a.CategoryItems = new Kingmaker.Enums.WeaponCategory[] { Kingmaker.Enums.WeaponCategory.BastardSword });
                ragathiel.ReplaceComponent<AddFacts>(a => a.Facts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[]
                {
                library.Get<BlueprintFeature>("6832681c9a91bf946a1d9da28c5be4b4"), //destruction
                library.Get<BlueprintFeature>("882521af8012fc749930b03dc18a69de"), //good
                library.Get<BlueprintFeature>("092714336606cfc45a37d2ab39fabfa8"), //law
                library.Get<BlueprintFeature>("e0471d01e73254a4ca23278705b75e57"), //nobility
                library.Get<BlueprintFeature>("8c769102f3996684fb6e09a2c4e7e5b9"), //channel positive
                });
                
                deities.AllFeatures = deities.AllFeatures.AddToArray(ragathiel);
                paladinDeities.AllFeatures = paladinDeities.AllFeatures.AddToArray(ragathiel);
            }

            {
                var tsukiyo = library.CopyAndAdd<BlueprintFeature>("afc775188deb7a44aa4cbde03512c671", "TsukiyoFeature", "a0308d0a3fca459eacc7fe83a3b03232"); //erastil

                tsukiyo.SetNameDescriptionIcon("Tsukiyo",
                                                "Tsukiyo is the Tian-Min deity of the moon, jade, and spirits. He is the brother of the evil Fumeiyoshi and is the paramour of Shizuru.\n"
                                                + "Domains: Darkness, Good, Law, Madness, Repose.\nFavored Weapon: Longspear.",
                                                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"tsukiyo.png"));

                var longspearProficiency = library.CopyAndAdd<BlueprintFeature>("70ab8880eaf6c0640887ae586556a652", "LongspearProficiency", "965c9ff0521a426aa9ff1323307f8816");
                longspearProficiency.SetNameDescription("Weapon Proficiency (Longspear)",
                                                    "You become proficient with longspear and can use them as a weapon.");
                longspearProficiency.ReplaceComponent<AddProficiencies>(a => a.WeaponProficiencies = new Kingmaker.Enums.WeaponCategory[] { Kingmaker.Enums.WeaponCategory.Longspear });
                tsukiyo.ReplaceComponent<AddFeatureOnClassLevel>(a => a.Feature = longspearProficiency);
                tsukiyo.ReplaceComponent<AddStartingEquipment>(a => a.CategoryItems = new Kingmaker.Enums.WeaponCategory[] { Kingmaker.Enums.WeaponCategory.Longspear });
                tsukiyo.ReplaceComponent<AddFacts>(a => a.Facts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[]
                {
                library.Get<BlueprintFeature>("6d8e7accdd882e949a63021af5cde4b8"), //darkness
                library.Get<BlueprintFeature>("882521af8012fc749930b03dc18a69de"), //good
                library.Get<BlueprintFeature>("092714336606cfc45a37d2ab39fabfa8"), //law
                library.Get<BlueprintFeature>("c346bcc77a6613040b3aa915b1ceddec"), //madness
                library.Get<BlueprintFeature>("076ba1e3a05fac146acfc956a9f41e95"), //repose
                library.Get<BlueprintFeature>("8c769102f3996684fb6e09a2c4e7e5b9"), //channel positive
                });

                deities.AllFeatures = deities.AllFeatures.AddToArray(tsukiyo);
                paladinDeities.AllFeatures = paladinDeities.AllFeatures.AddToArray(tsukiyo);
            }

            {
                var gruhastha = library.CopyAndAdd<BlueprintFeature>("afc775188deb7a44aa4cbde03512c671", "GruhasthaFeature", "757f4081d82749338cfcf5aff2909918"); //erastil

                gruhastha.SetNameDescriptionIcon("Gruhastha",
                                                "Gruhastha is a male Vudran god. He was a mortal nephew of Irori who, according to legend, ascended to divinity by authoring a book so perfectly profound that he merged with it and became a god—the Azvadeva Pujila.\n"
                                                + "Domains: Animals, Good, Knowledge, Law, Travel.\nFavored Weapon: Shortbow.",
                                                CallOfTheWild.LoadIcons.Image2Sprite.Create(@"gruhastha.png"));

                var shortbowProficiency = library.CopyAndAdd<BlueprintFeature>("70ab8880eaf6c0640887ae586556a652", "ShortbowProficiency", "48954e45db1e4a669ba9a9a9be34c1a2");
                shortbowProficiency.SetNameDescription("Weapon Proficiency (Shortbow)",
                                                    "You become proficient with shortbow and can use them as a weapon.");
                shortbowProficiency.ReplaceComponent<AddProficiencies>(a => a.WeaponProficiencies = new Kingmaker.Enums.WeaponCategory[] { Kingmaker.Enums.WeaponCategory.Shortbow });
                gruhastha.ReplaceComponent<AddFeatureOnClassLevel>(a => a.Feature = shortbowProficiency);
                gruhastha.ReplaceComponent<AddStartingEquipment>(a => a.CategoryItems = new Kingmaker.Enums.WeaponCategory[] { Kingmaker.Enums.WeaponCategory.Shortbow });
                gruhastha.ReplaceComponent<AddFacts>(a => a.Facts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[]
                {
                    library.Get<BlueprintFeature>("9f05f9da2ea5ae44eac47d407a0000e5"), //animal
                    library.Get<BlueprintFeature>("882521af8012fc749930b03dc18a69de"), //good
                    library.Get<BlueprintFeature>("443d44b3e0ea84046a9bf304c82a0425"), //knowledge
                    library.Get<BlueprintFeature>("092714336606cfc45a37d2ab39fabfa8"), //law
                    library.Get<BlueprintFeature>("c008853fe044bd442ae8bd22260592b7"), //travel
                    library.Get<BlueprintFeature>("8c769102f3996684fb6e09a2c4e7e5b9"), //channel positive
                });

                deities.AllFeatures = deities.AllFeatures.AddToArray(gruhastha);
                paladinDeities.AllFeatures = paladinDeities.AllFeatures.AddToArray(gruhastha);
            }

            CallOfTheWild.LoadIcons.Image2Sprite.icons_folder = UnityModManager.modsPath + @"/CallOfTheWild/Icons/";
        }
    }
}
