using BepInEx;
using HarmonyLib;

namespace UnlimitedStamina
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "nightlight.dinkum.unlimitedstamina";
        public const string pluginName = "Unlimited Stamina";
        public const string pluginVersion = "0.0.2";

        public void Awake()
        {
            Logger.LogInfo("Nightlight's Unlimited Stamina mod loaded!");

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(StaminaPatch));
        }
    }

    class StaminaPatch
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
}
