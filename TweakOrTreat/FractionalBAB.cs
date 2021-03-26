using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Class.LevelUp.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TweakOrTreat
{
    [HarmonyLib.HarmonyPatch(typeof(ApplyClassMechanics), "ApplyBaseStats")]
    class ApplyClassMechanics_ApplyBaseStats_Patch
    {
        enum SaveType : int
        {
            Low = 2,
            High = 3
        }

        static Dictionary<string, int> babMap = new Dictionary<string, int>
        {
            { "0538081888b2d8c41893d25d098dee99", 2 }, //low
            { "4c936de4249b61e419a3fb775b9f2581", 3 }, //med
            { "b3057560ffff3514299e8b93e7648a9d", 4 }, //high
        };

        static Dictionary<string, SaveType> saveMap = new Dictionary<string, SaveType>
        {
            //one for normal, one for prestige
            { "dc0c7c1aba755c54f96c089cdf7d14a3", SaveType.Low },
            { "dc5257e1100ad0d48b8f3b9798421c72", SaveType.Low },
            { "1f309006cd2855e4e91a6c3707f3f700", SaveType.High },
            { "ff4662bde9e75f145853417313842751", SaveType.High },
        };

        static int calculateExtraBab(ClassData classData, UnitDescriptor unit)
        {
            var babSum = 0;
            foreach (ClassData classData2 in unit.Progression.Classes)
            {
                var perLevel = babMap[classData2.BaseAttackBonus.AssetGuid];
                babSum += perLevel * classData2.Level;
            }

            var oldBabSum = babSum - babMap[classData.BaseAttackBonus.AssetGuid];

            return (babSum / 4) - (oldBabSum / 4);
        }

        static int calculateExtraSave(ClassData classData, UnitDescriptor unit, int nextClassLevel,
            Func<ClassData, BlueprintStatProgression> getSaveProg)
        {
            int isGood = (int)SaveType.Low;
            int saveSum = 0;
            foreach (ClassData classData2 in unit.Progression.Classes)
            {
                var perLevel = (int)saveMap[getSaveProg(classData2).AssetGuid];
                saveSum += perLevel * classData2.Level;

                if(classData.CharacterClass != classData2.CharacterClass || nextClassLevel > 1)
                {
                    isGood = Math.Max(isGood, perLevel);
                }
            }

            var classSaveBonus = (int)saveMap[getSaveProg(classData).AssetGuid];
            var oldsaveSum = saveSum - classSaveBonus;

            var saveDiff = (saveSum / 6) - (oldsaveSum / 6);

            if(isGood < classSaveBonus)
            {
                saveDiff += 2;
            }

            return saveDiff;
        }

        static bool Prefix(LevelUpState state, ClassData classData, UnitDescriptor unit)
        {
            unit.Stats.GetStat(StatType.BaseAttackBonus).BaseValue += calculateExtraBab(classData, unit);
            unit.Stats.GetStat(StatType.SaveFortitude).BaseValue += calculateExtraSave(classData, unit, state.NextClassLevel,
                c => c.FortitudeSave);
            unit.Stats.GetStat(StatType.SaveReflex).BaseValue += calculateExtraSave(classData, unit, state.NextClassLevel,
                c => c.ReflexSave);
            unit.Stats.GetStat(StatType.SaveWill).BaseValue += calculateExtraSave(classData, unit, state.NextClassLevel,
                c => c.WillSave);

            return false;
        }

        static bool Prepare()
        {
            return false;
        }
    }
    class FractionalBAB
    {
    }
}
