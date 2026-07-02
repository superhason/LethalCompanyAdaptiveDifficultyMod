using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using AdaptiveDifficultyMod.Core;

namespace AdaptiveDifficultyMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "com.hanson.lethalcompany.adaptivedifficulty";
        internal const string PluginName = "Adaptive Difficulty Mod";
        internal const string PluginVersion = "0.1.0";

        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"{PluginName} loaded successfully!");

            PluginConfig.Bind(Config);

            new Harmony(PluginGuid).PatchAll();
            Logger.LogInfo("Harmony patches applied.");
        }
    }
}