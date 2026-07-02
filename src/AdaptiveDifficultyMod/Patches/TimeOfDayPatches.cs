using HarmonyLib;
using AdaptiveDifficultyMod.Core;

namespace AdaptiveDifficultyMod.Patches
{
    [HarmonyPatch(typeof(TimeOfDay), "SetNewProfitQuota")]
    internal static class TimeOfDayPatches
    {
        [HarmonyPostfix]
        private static void LogQuotaCleared()
        {
            ModState.EnsureLoadedForSave(GameNetworkManager.Instance.currentSaveFileName); // NEW

            ModState.Difficulty.ApplyQuotaCleared();
            ModState.PersistCurrentScore(); // NEW

            Plugin.Logger.LogInfo($"[Telemetry] Quota cleared -> DifficultyScore={ModState.Difficulty.Score:F3}");
        }
    }
}