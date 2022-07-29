using BepInEx;
using HarmonyLib;

namespace AnimalsDontBreakFree
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGuid = "nightlight.dinkum.animalsdontbreakfree";
        public const string pluginName = "Animals Don't Break Free";
        public const string pluginVersion = "0.0.1";

        public void Awake()
        {
            Logger.LogInfo("Nightlight's Animals Don't Break Free mod loaded!");

            Harmony harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(TrapPatch));
        }
    }

    class TrapPatch
    {
        [HarmonyPatch(typeof(TrappedAnimal), "startFreeSelfRoutine")]
        [HarmonyPrefix]
        static bool DontAllowAnimalToEscape()
        {
            return false;
        }
    }
}
