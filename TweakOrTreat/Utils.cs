using JetBrains.Annotations;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
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
    class Utils
    {
        public static int GetArchetypeLevel(UnitDescriptor unit, BlueprintCharacterClass clazz, BlueprintArchetype archetype)
        {
            int num = 0;
            foreach (ClassLevelsForPrerequisites classLevelsForPrerequisites in unit.Progression.Features.SelectFactComponents<ClassLevelsForPrerequisites>())
            {
                if (classLevelsForPrerequisites.FakeClass == clazz)
                {
                    num += (int)(classLevelsForPrerequisites.Modifier * (double)unit.Progression.GetClassLevel(classLevelsForPrerequisites.ActualClass) + (double)classLevelsForPrerequisites.Summand);
                }
            }
            ClassData classData = unit.Progression.GetClassData(clazz);

            if(classData == null || !classData.Archetypes.Contains(archetype))
            {
                return 0;
            }

            return unit.Progression.GetClassLevel(clazz) + num;
        }

        public static int GetClassLevel(UnitDescriptor unit, BlueprintCharacterClass clazz)
        {
            int num = 0;
            foreach (ClassLevelsForPrerequisites classLevelsForPrerequisites in unit.Progression.Features.SelectFactComponents<ClassLevelsForPrerequisites>())
            {
                if (classLevelsForPrerequisites.FakeClass == clazz)
                {
                    num += (int)(classLevelsForPrerequisites.Modifier * (double)unit.Progression.GetClassLevel(classLevelsForPrerequisites.ActualClass) + (double)classLevelsForPrerequisites.Summand);
                }
            }
            return unit.Progression.GetClassLevel(clazz) + num;
        }
    }
}
