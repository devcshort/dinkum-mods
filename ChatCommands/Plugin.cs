using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChatCommands
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        Harmony harmony;
        public const string pluginGuid = "nightlight.dinkum.chatcommands";
        public const string pluginName = "Chat Commands";
        public const string pluginVersion = "0.0.2";

        public void Awake()
        {
            Logger.LogInfo("Nightlight's Chat Commands mod loaded!");

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
                if (newMessage.StartsWith("/"))
                {
                    string commandWithoutSlash = newMessage.Replace("/", string.Empty);
                    string command = commandWithoutSlash.Split(' ')[0].ToLower();
                    string arg = commandWithoutSlash.Split(' ')[1].ToLower();

                    if (command == "setstacksize")
                    {
                        SetStackSize(arg);
                    }
                    else if (command == "unstuck")
                    {
                        UnstuckNpc(arg);
                    }
                    else if (command == "setprice")
                    {
                        SetItemPrice(arg);
                    }
                    else if (command == "unlimitedstamina")
                    {
                        TurnUnlimitedStaminaOnOff(arg);
                    }
                    else if (command == "indestructibletools")
                    {
                        TurnIndestructibleToolsOnOff(arg);
                    }
                    else if (command == "unbreakabletools")
                    {
                        TurnUnbreakableToolsOnOff(arg);
                    }
                    else if (command == "indestructibletraps")
                    {
                        TurnIndestructibleTrapsOnOff(arg);
                    }
                    else if (command == "infinitehealth")
                    {
                        TurnInfiniteHealthOnOff(arg);
                    }

                    return false;
                }

                return true;
            }

            // Set stack size of stackable items to any number (within range)
            static void SetStackSize(string newStackSize)
            {
                Inventory inv = Inventory.inv;

                if (inv.invSlots[inv.selectedSlot].itemNo >= 0)
                {
                    int parsedStackSize;
                    var isValid = int.TryParse(newStackSize, out parsedStackSize);

                    if (isValid && inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].isStackable)
                    {
                        inv.invSlots[inv.selectedSlot].stack = parsedStackSize;
                        inv.invSlots[inv.selectedSlot].refreshSlot();
                        NotificationManager.manage.createChatNotification("Set stack size of " + inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].itemName + " to " + string.Format("{0:n0}", parsedStackSize));
                    }
                }
            }

            // Unstuck specific NPCs (setting their position to your own)
            static void UnstuckNpc(string npcName)
            {
                Dictionary<string, int> npcId = new Dictionary<string, int>()
                {
                    { "john", 1 },
                    { "fletch", 6 }
                };

                foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
                {
                    if (gameObj.name == "NPC(Clone)")
                    {
                        NPCIdentity npcIdentity = gameObj.GetComponentInChildren<NPCIdentity>();

                        int npcNo;
                        var isValid = npcId.TryGetValue(npcName, out npcNo);

                        if (isValid && npcIdentity.NPCNo == npcNo)
                        {
                            NPCAI npcAi = gameObj.GetComponentInChildren<NPCAI>();
                            GameObject player = GameObject.Find("Char [connId=0]");

                            var lastPosVariable = AccessTools.Field(typeof(NPCAI), "lastPos");
                            lastPosVariable.SetValue(npcAi, player.transform.position);

                            var lastPosAnimVariable = AccessTools.Field(typeof(NPCAI), "lastPosAnim");
                            lastPosAnimVariable.SetValue(npcAi, player.transform.position);

                            var lastNavPosAnimVariable = AccessTools.Field(typeof(NPCAI), "lastNavPos");
                            lastNavPosAnimVariable.SetValue(npcAi, player.transform.position);

                            gameObj.transform.position = player.transform.position;
                            npcAi.myAgent.transform.position = player.transform.position;

                            NotificationManager.manage.createChatNotification("Attempted to unstuck " + npcName);
                        }
                    }
                }
            }

            // Set price of item to be sold at John's shop
            static void SetItemPrice(string newPrice)
            {
                Inventory inv = Inventory.inv;

                if (inv.invSlots[inv.selectedSlot].itemNo >= 0)
                {
                    int parsedPrice;
                    var isValid = int.TryParse(newPrice, out parsedPrice);

                    if (isValid)
                    {
                        inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].value = parsedPrice;
                        NotificationManager.manage.createChatNotification("Set price of " + inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].itemName + " to " + string.Format("{0:n0}", parsedPrice) + " Dinks");
                    }
                }
            }

            static Harmony staminaPatch;
            static void TurnUnlimitedStaminaOnOff(string enableDisable)
            {
                if (staminaPatch == null)
                {
                    staminaPatch = new Harmony("nightlight.dinkum.unlimitedstamina");
                }

                if (enableDisable == "enable")
                {
                    staminaPatch.PatchAll(typeof(UnlimitedStaminaPatch));
                    NotificationManager.manage.createChatNotification("Unlimited stamina enabled.");
                }
                else if (enableDisable == "disable")
                {
                    staminaPatch.UnpatchSelf();
                    NotificationManager.manage.createChatNotification("Unlimited stamina disabled.");
                }
            }

            static Harmony indestructibleToolsPatch;
            static void TurnIndestructibleToolsOnOff(string enableDisable)
            {
                if (indestructibleToolsPatch == null)
                {
                    indestructibleToolsPatch = new Harmony("nightlight.dinkum.indestructibletools");
                }

                if (enableDisable == "enable")
                {
                    indestructibleToolsPatch.PatchAll(typeof(IndestructibleToolsPatch));
                    NotificationManager.manage.createChatNotification("Indestructible tools enabled.");
                }
                else if (enableDisable == "disable")
                {
                    indestructibleToolsPatch.UnpatchSelf();
                    NotificationManager.manage.createChatNotification("Indestructible tools disabled.");
                }
            }

            static Harmony unbreakbleToolsPatch;
            static void TurnUnbreakableToolsOnOff(string enableDisable)
            {
                if (unbreakbleToolsPatch == null)
                {
                    unbreakbleToolsPatch = new Harmony("nightlight.dinkum.unbreakabletools");
                }

                if (enableDisable == "enable")
                {
                    unbreakbleToolsPatch.PatchAll(typeof(UnbreakableToolsPatch));
                    NotificationManager.manage.createChatNotification("Unbreakable tools enabled.");
                }
                else if (enableDisable == "disable")
                {
                    unbreakbleToolsPatch.UnpatchSelf();
                    NotificationManager.manage.createChatNotification("Unbreakable tools disabled.");
                }
            }

            static Harmony indestructibleTrapsPatch;
            static void TurnIndestructibleTrapsOnOff(string enableDisable)
            {
                if (indestructibleTrapsPatch == null)
                {
                    indestructibleTrapsPatch = new Harmony("nightlight.dinkum.indestructibletraps");
                }

                if (enableDisable == "enable")
                {
                    indestructibleTrapsPatch.PatchAll(typeof(IndestructibleTrapsPatch));
                    NotificationManager.manage.createChatNotification("Indestructible traps enabled.");
                }
                else if (enableDisable == "disable")
                {
                    indestructibleTrapsPatch.UnpatchSelf();
                    NotificationManager.manage.createChatNotification("Indestructible traps disabled.");
                }
            }

            static Harmony infiniteHealthPatch;
            static void TurnInfiniteHealthOnOff(string enableDisable)
            {
                if (infiniteHealthPatch == null)
                {
                    infiniteHealthPatch = new Harmony("nightlight.dinkum.infinitehealth");
                }

                if (enableDisable == "enable")
                {
                    infiniteHealthPatch.PatchAll(typeof(UnlimitedHealthPatch));
                    NotificationManager.manage.createChatNotification("Infinite health enabled.");
                }
                else if (enableDisable == "disable")
                {
                    infiniteHealthPatch.UnpatchSelf();
                    NotificationManager.manage.createChatNotification("Infinite health disabled.");
                }
            }
        }
    }

    class UnlimitedStaminaPatch
    {
        [HarmonyPatch(typeof(StatusManager), "changeStamina")]
        [HarmonyPrefix]
        static bool DontUseStamina(ref float ___stamina, float ___staminaMax)
        {
            if (___stamina < ___staminaMax)
            {
                ___stamina = ___staminaMax;
            }

            return true;
        }
    }

    class IndestructibleToolsPatch
    {
        [HarmonyPatch(typeof(Inventory), "useItemWithFuel")]
        [HarmonyPrefix]
        static bool DontUseFuel()
        {
            Inventory inv = Inventory.inv;

            // Make sure the item has fuel, is a tool, and is repairable before running our code.
            // This shouldn't run for vehicles or tools that aren't repairable
            if (inv.invSlots[inv.selectedSlot].itemNo >= 0 &&
                inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].hasFuel &&
                inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].isATool &&
                inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].isRepairable)
            {
                // Refill fuel to max ONLY if it is less than max. This is more for aesthetics than function
                if (inv.invSlots[inv.selectedSlot].stack < inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].fuelMax)
                {
                    inv.invSlots[inv.selectedSlot].updateSlotContentsAndRefresh(inv.invSlots[inv.selectedSlot].itemNo, inv.allItems[inv.invSlots[inv.selectedSlot].itemNo].fuelMax);
                }

                return false;
            }

            return true;
        }
    }

    class UnbreakableToolsPatch
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

    class IndestructibleTrapsPatch
    {
        [HarmonyPatch(typeof(TrappedAnimal), "startFreeSelfRoutine")]
        [HarmonyPrefix]
        static bool DontAllowAnimalToEscape()
        {
            return false;
        }
    }

    class UnlimitedHealthPatch
    {
        [HarmonyPatch(typeof(Damageable), "OnTakeDamage")]
        [HarmonyPrefix]
        static bool DontTakeDamage(Damageable __instance, ref int newHealth)
        {
            if (__instance.myChar)
            {
                newHealth = 50;
            }

            return true;
        }
    }
}
