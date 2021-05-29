//using Kingmaker;
//using Kingmaker.Blueprints.Root;
//using Kingmaker.Blueprints.Root.Strings;
//using Kingmaker.Items;
//using Kingmaker.PubSubSystem;
//using Kingmaker.UI._ConsoleUI.ContextMenu;
//using Kingmaker.UI.Common;
//using Kingmaker.UI.Vendor;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TweakOrTreat
//{
//    [HarmonyLib.HarmonyPatch(typeof(ItemsFilterStrings), "GetText", new Type[] { typeof(ItemsFilter.SorterType) })]
//    static class ItemsFilterStrings_GetText_Patch
//    {
//        internal static bool Prefix(ItemsFilter.SorterType type, ref string __result)
//        {
//            if(type == (ItemsFilter.SorterType)InventorySort.valueWeightSortUp)
//            {
//                __result = "Price/Weight (in ascending order)";
//                return false;
//            }
//            if (type == (ItemsFilter.SorterType)InventorySort.valueWeightSortDown)
//            {
//                __result = "Price/Weight (in descending order)";
//                return false;
//            }
//            return true;
//        }
//    }

//    [HarmonyLib.HarmonyPatch(typeof(ItemsFilter), "ShowSortingContextMenu")]
//    static class ItemsFilter_ShowSortingContextMenu_Patch
//    {
//        internal static bool Prefix(IContextMenuOwner owner, Action<ItemsFilter.SorterType> action)
//        {
//            List<ContextMenuCollectionEntity> list = new List<ContextMenuCollectionEntity>();
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.TypeUp), delegate ()
//            {
//                action(ItemsFilter.SorterType.TypeUp);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.TypeDown), delegate ()
//            {
//                action(ItemsFilter.SorterType.TypeDown);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.PriceUp), delegate ()
//            {
//                action(ItemsFilter.SorterType.PriceUp);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.PriceDown), delegate ()
//            {
//                action(ItemsFilter.SorterType.PriceDown);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.NameUp), delegate ()
//            {
//                action(ItemsFilter.SorterType.NameUp);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.NameDown), delegate ()
//            {
//                action(ItemsFilter.SorterType.NameDown);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.DateUp), delegate ()
//            {
//                action(ItemsFilter.SorterType.DateUp);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.DateDown), delegate ()
//            {
//                action(ItemsFilter.SorterType.DateDown);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.WeightUp), delegate ()
//            {
//                action(ItemsFilter.SorterType.WeightUp);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText(ItemsFilter.SorterType.WeightDown), delegate ()
//            {
//                action(ItemsFilter.SorterType.WeightDown);
//            }, () => true));

//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText((ItemsFilter.SorterType)InventorySort.valueWeightSortUp), delegate ()
//            {
//                action((ItemsFilter.SorterType)InventorySort.valueWeightSortUp);
//            }, () => true));
//            list.Add(new ContextMenuCollectionEntity(LocalizedTexts.Instance.ItemsFilter.GetText((ItemsFilter.SorterType)InventorySort.valueWeightSortDown), delegate ()
//            {
//                action((ItemsFilter.SorterType)InventorySort.valueWeightSortDown);
//            }, () => true));

//            List<ContextMenuCollectionEntity> entities = list;
//            ContextMenuCollection collection = new ContextMenuCollection(entities)
//            {
//                Owner = owner.GetOwnerRectTransform(),
//                ContextType = ContextMenuType.FullScreenContextMenu
//            };
//            EventBus.RaiseEvent<IContextMenuHandler>(delegate (IContextMenuHandler z)
//            {
//                z.OnContextMenuRequest(collection);
//            });


//            return false;
//        }
//    }

//    [HarmonyLib.HarmonyPatch(typeof(FilterController), "Initialize")]
//    static class FilterController_Initialize_Patch
//    {
//        public static void Initialize(FilterController __instance)
//        {
//            if (__instance.m_SlotsGroup == null)
//            {
//                return;
//            }
//            if (__instance.Save)
//            {
//                __instance.CurrentFilter = Game.Instance.Player.UISettings.InventoryFilter;
//                __instance.CurrentSorter = Game.Instance.Player.UISettings.InventorySorter;
//            }
//            else
//            {
//                __instance.CurrentFilter = ItemsFilter.FilterType.NoFilter;
//                __instance.CurrentSorter = ItemsFilter.SorterType.NotSorted;
//            }
//            if (__instance.DropdownMenu == null)
//            {
//                return;
//            }
//            __instance.DropdownMenu.ClearOptions();
//            List<string> list = new List<string>();
//            foreach (object obj in Enum.GetValues(typeof(ItemsFilter.SorterType)))
//            {
//                ItemsFilter.SorterType type = (ItemsFilter.SorterType)obj;
//                list.Add(LocalizedTexts.Instance.ItemsFilter.GetText(type));
//            }

//            list.Add(LocalizedTexts.Instance.ItemsFilter.GetText((ItemsFilter.SorterType)InventorySort.valueWeightSortUp));
//            list.Add(LocalizedTexts.Instance.ItemsFilter.GetText((ItemsFilter.SorterType)InventorySort.valueWeightSortDown));

//            __instance.DropdownMenu.AddOptions(list);
//            __instance.InitilizeButtons();
//        }

//        internal static bool Prefix(FilterController __instance)
//        {
//            Initialize(__instance);

//            return false;
//        }
//    }

//    [HarmonyLib.HarmonyPatch(typeof(ItemsFilter), "ItemSorter")]
//    static class ItemsFilter_ItemSorter_Patch
//    {
//        private static int CompareByValueWeightRatio(ItemEntity a, ItemEntity b, ItemsFilter.FilterType filter)
//        {
//            int num = ItemsFilter.GetItemType(a, filter).CompareTo(ItemsFilter.ItemType.Other);
//            int num2 = ItemsFilter.GetItemType(b, filter).CompareTo(ItemsFilter.ItemType.Other);

//            var aWeight = a.Blueprint.Weight > 0.0? a.Blueprint.Weight : 0.0001;
//            var bWeight = b.Blueprint.Weight > 0.0 ? b.Blueprint.Weight : 0.0001;

//            int num3 = (a.Cost / aWeight).CompareTo(b.Cost / bWeight);
//            if (num != -1 || num2 != -1)
//            {
//                return ItemsFilter.GetItemType(a, filter).CompareTo(ItemsFilter.GetItemType(b, filter));
//            }
//            if (num3 != 0)
//            {
//                return num3;
//            }
//            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
//        }
//        internal static bool Prefix(ref List<ItemEntity> __result, ItemsFilter.SorterType type, List<ItemEntity> items, ItemsFilter.FilterType filter)
//        {
//            if (type == (ItemsFilter.SorterType)InventorySort.valueWeightSortUp)
//            {
//                items.Sort((ItemEntity a, ItemEntity b) => CompareByValueWeightRatio(a, b, filter));
//                __result = items;
//                return false;
//            }
//            if (type == (ItemsFilter.SorterType)InventorySort.valueWeightSortDown)
//            {
//                items.Sort((ItemEntity a, ItemEntity b) => -1*CompareByValueWeightRatio(a, b, filter));
//                __result = items;
//                return false;
//            }
//            return true;
//        }
//    }

//    class InventorySort
//    {
//        public static int valueWeightSortUp = 11;
//        public static int valueWeightSortDown = 12;
//    }
//}
