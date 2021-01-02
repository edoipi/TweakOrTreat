using CallOfTheWild;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TweakOrTreat
{
    static class Utils
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

        public static BlueprintFeature CreateFeature(string name, string displayName, string description, string guid, Sprite icon, FeatureGroup group, params BlueprintComponent[][] components)
        {
            List<BlueprintComponent> list = new List<BlueprintComponent>();
            foreach(var componentArray in components)
            {
                list.AddRange(componentArray);
            }

            return Helpers.CreateFeature(name, displayName, description, guid, icon, group, list.ToArray());
        }

        public static ActionList ReplaceAction<T>(this ActionList oldActions, Action<T> lambda) where T : GameAction
        {
            var newActions = new List<GameAction>();

            foreach(var action in oldActions.Actions)
            {
                if(action is T)
                {
                    //System.Reflection.MethodInfo inst = action.GetType().GetMethod("MemberwiseClone",
                    //    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    //T newAction = (T)inst.Invoke(action as T, null);
                    T newAction = UnityEngine.Object.Instantiate(action as T);
                    lambda(newAction);

                    newActions.Add(newAction);

                    //lambda(action as T);
                    //newActions.Add(action);
                } else
                {
                    newActions.Add(action);
                }

                
            }

            return Helpers.CreateActionList(newActions.ToArray());
        }

        //credits Holic, it's only slightly altered
        static public void addContextActionApplyBuffOnCasterFactsToActivatedAbilityBuffNoRemove(BlueprintBuff target_buff, BlueprintBuff buff_to_add, params BlueprintUnitFact[] facts)
        {
            Kingmaker.ElementsSystem.GameAction[] pre_actions = new GameAction[] { };
            var condition = new Kingmaker.UnitLogic.Mechanics.Conditions.ContextConditionCasterHasFact[facts.Length];
            for (int i = 0; i < facts.Length; i++)
            {
                condition[i] = Helpers.CreateConditionCasterHasFact(facts[i]);
            }
            var action = Helpers.CreateConditional(condition, pre_actions.AddToArray(Common.createContextActionApplyBuff(buff_to_add, Helpers.CreateContextDuration(),
                                                                                     dispellable: false, is_child: true, is_permanent: true)));
            Common.addContextActionApplyBuffOnConditionToActivatedAbilityBuff(target_buff, action);
        }
    }
}
