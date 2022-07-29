using BepInEx;
using HarmonyLib;

namespace UnbreakableTools
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "nightlight.dinkum.unbreakabletools";
        public const string pluginName = "Unbreakable Tools";
        public const string pluginVersion = "0.0.3";

        public void Awake()
        {
            Logger.LogInfo("Nightlight's Unbreakable Tools mod loaded!");

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(ToolPatch));
        }
    }

    class ToolPatch
    {
        [HarmonyPatch(typeof(Inventory), "useItemWithFuel")]
        [HarmonyPrefix]
        static bool DontUseFuel(Inventory __instance)
        {
            // Make sure the item has fuel, is a tool, and is repairable before running our code.
            // This shouldn't run for vehicles or tools that aren't repairable
            if (__instance.invSlots[__instance.selectedSlot].itemNo >= 0 &&
                __instance.allItems[__instance.invSlots[__instance.selectedSlot].itemNo].hasFuel &&
                __instance.allItems[__instance.invSlots[__instance.selectedSlot].itemNo].isATool &&
                __instance.allItems[__instance.invSlots[__instance.selectedSlot].itemNo].isRepairable)
            {
                // Refill fuel to max ONLY if it is less than max. This is more for aesthetics than function
                if (__instance.invSlots[__instance.selectedSlot].stack < __instance.allItems[__instance.invSlots[__instance.selectedSlot].itemNo].fuelMax)
                {
                    __instance.invSlots[__instance.selectedSlot].updateSlotContentsAndRefresh(__instance.invSlots[__instance.selectedSlot].itemNo, __instance.allItems[__instance.invSlots[__instance.selectedSlot].itemNo].fuelMax);
                }

                return false;
            }

            return true;
        }
    }
}
