using BepInEx;
using HarmonyLib;

namespace SetStackSize
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        Harmony harmony;
        public const string pluginGuid = "nightlight.dinkum.setstacksize";
        public const string pluginName = "Set Stack Size";
        public const string pluginVersion = "0.0.1";

        public void Awake()
        {
            Logger.LogInfo("Nightlight's Set Stack Size mod loaded!");

            harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(ChatPatch));
        }

        public void Destroy()
        {
            harmony.UnpatchSelf();
        }

        class ChatPatch
        {
            [HarmonyPatch(typeof(CharMovement), "CmdSendChatMessage")]
            [HarmonyPrefix]
            static bool OverrideServerCmd(string newMessage)
            {
                
                Inventory inv = Inventory.inv;

                if (newMessage.StartsWith("/"))
                {
                    string commandWithoutSlash = newMessage.Replace("/", string.Empty);

                    string command = commandWithoutSlash.Split(' ')[0];
                    string newPrice = commandWithoutSlash.Split(' ')[1];
                    if (command.ToLower() == "setstacksize")
                    {
                        if (inv.invSlots[inv.selectedSlot].itemNo >= 0)
                        {
                            int tryPrice;
                            var isValid = int.TryParse(newPrice, out tryPrice);

                            if (isValid && inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].isStackable)
                            {
                                inv.invSlots[inv.selectedSlot].stack = tryPrice;
                                inv.invSlots[inv.selectedSlot].refreshSlot();
                            }

                            return false;
                        }
                    }

                    return false;
                }

                return true;
            }
        }
    }
}
