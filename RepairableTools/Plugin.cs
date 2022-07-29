using BepInEx;
using HarmonyLib;
using System.Collections.Generic;

namespace RepairableTools
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "nightlight.dinkum.repairabletools";
        public const string pluginName = "Repairable Tools";
        public const string pluginVersion = "0.0.1";

        public void Awake()
        {
            Logger.LogInfo("Nightlight's Repairable Tools mod loaded!");

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(ToolsPatch));
            harmony.PatchAll(typeof(AttackPatch));
        }
    }

    class ToolsPatch
    {
        [HarmonyPatch(typeof(Inventory), "useItemWithFuel")]
        [HarmonyPrefix]
        static bool DontLetToolReachZero(Inventory __instance)
        {
            InventoryItem currentItem = __instance.allItems[__instance.invSlots[__instance.selectedSlot].itemNo];
            
            //if (__instance.invSlots[__instance.selectedSlot].stack > 5)
            //{
            //    __instance.invSlots[__instance.selectedSlot].updateSlotContentsAndRefresh(__instance.invSlots[__instance.selectedSlot].itemNo, 5);
            //}

            // Make sure the item has fuel, is a tool, and is repairable before running our code.
            // This shouldn't run for vehicles or tools that aren't repairable
            if (__instance.invSlots[__instance.selectedSlot].itemNo >= 0 &&
                currentItem.hasFuel &&
                currentItem.isATool &&
                currentItem.isRepairable)
            {
                if (__instance.invSlots[__instance.selectedSlot].stack == 1)
                {
                    //currentItem.damagePerAttack = 0;
                    //currentItem.weaponDamage = 0;
                    //currentItem.value = 0;
                    return false;
                }
            }

            return true;
        }
    }

    class AttackPatch
    {
        [HarmonyPatch(typeof(CharInteract), "checkIfCanDamage")]
        [HarmonyPrefix]
        static bool dontDamage()
        {
            InventoryItem currentItem = Inventory.inv.allItems[Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo];

            // Make sure the item has fuel, is a tool, and is repairable before running our code.
            // This shouldn't run for vehicles or tools that aren't repairable
            if (Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo >= 0 &&
                currentItem.hasFuel &&
                currentItem.isATool &&
                currentItem.isRepairable)
            {
                if (Inventory.inv.invSlots[Inventory.inv.selectedSlot].stack == 1)
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(MeleeAttacks), "attackAndDealDamage")]
        [HarmonyPrefix]
        static bool dontAttack()
        {
            InventoryItem currentItem = Inventory.inv.allItems[Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo];

            // Make sure the item has fuel, is a tool, and is repairable before running our code.
            // This shouldn't run for vehicles or tools that aren't repairable
            if (Inventory.inv.invSlots[Inventory.inv.selectedSlot].itemNo >= 0 &&
                currentItem.hasFuel &&
                currentItem.isATool &&
                currentItem.isRepairable)
            {
                if (Inventory.inv.invSlots[Inventory.inv.selectedSlot].stack == 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
