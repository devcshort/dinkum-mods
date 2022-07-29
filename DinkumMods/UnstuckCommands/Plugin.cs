using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace UnstuckCommands
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "nightlight.dinkum.unstuckcommands";
        public const string pluginName = "Unstuck Commands";
        public const string pluginVersion = "0.0.1";

        public static Dictionary<string, int> npcs = new Dictionary<string, int>()
        {
            { "john", 1 },
            { "fletch", 6 }
        };

        public void Awake()
        {
            Logger.LogInfo("Nightlight's Unstuck Commands mod loaded!");

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(ChatPatch));
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

                    string command = commandWithoutSlash.Split(' ')[0];
                    string npcName = commandWithoutSlash.Split(' ')[1];
                    if (command.ToLower() == "unstuck")
                    {
                        if (npcName != null)
                        {
                            foreach (var gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
                            {
                                if (gameObj.name == "NPC(Clone)")
                                {
                                    NPCIdentity npcIdentity = gameObj.GetComponentInChildren<NPCIdentity>();

                                    int npcNo;
                                    var value = npcs.TryGetValue(npcName, out npcNo);

                                    if (value != false && npcIdentity.NPCNo == npcNo)
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
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                return true;
            }
        }
    }
}
