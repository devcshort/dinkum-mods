using BepInEx;
using HarmonyLib;

namespace ExpensiveItem
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "nightlight.dinkum.expensiveitem";
        public const string pluginName = "Expensive Item";
        public const string pluginVersion = "0.0.1";

        public void Awake()
        {
            Logger.LogInfo("Nightlight's Expensive Item mod loaded!");

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(ChatPatch));
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
                    if (command.ToLower() == "setprice")
                    {
                        if (inv.invSlots[inv.selectedSlot].itemNo >= 0)
                        {
                            int tryPrice;
                            var isValid = int.TryParse(newPrice, out tryPrice);

                            if (isValid)
                            {
                                inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].value = tryPrice;
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
