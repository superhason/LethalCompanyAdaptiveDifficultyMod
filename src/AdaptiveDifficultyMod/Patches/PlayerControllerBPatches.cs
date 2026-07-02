using HarmonyLib;
using GameNetcodeStuff;
using AdaptiveDifficultyMod.Metrics;

namespace AdaptiveDifficultyMod.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
    internal static class PlayerControllerBPatches
    {
        [HarmonyPostfix]
        private static void LogPlayerDeath(PlayerControllerB __instance)
        {
            PerformanceTracker.RecordDeath();
            Plugin.Logger.LogInfo($"[Telemetry] Player died: {__instance.playerUsername}");
        }
    }
}