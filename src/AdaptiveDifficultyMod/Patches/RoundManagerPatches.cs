using HarmonyLib;
using UnityEngine;
using AdaptiveDifficultyMod.Core;

namespace AdaptiveDifficultyMod.Patches
{
    [HarmonyPatch(typeof(RoundManager), "FinishGeneratingNewLevelClientRpc")]
    internal static class RoundManagerPatches
    {
        [HarmonyPostfix]
        private static void LogRoundStart()
        {
            Plugin.Logger.LogInfo("[Telemetry] Round started (FinishGeneratingNewLevelClientRpc fired).");
        }
    }

    [HarmonyPatch(typeof(RoundManager), "RefreshEnemiesList")]
    internal static class EnemySpawnScalingPatch
    {
        [HarmonyPostfix]
        private static void ScaleEnemySpawnPower(RoundManager __instance)
        {
            ModState.EnsureLoadedForSave(GameNetworkManager.Instance.currentSaveFileName);

            float multiplier = Mathf.Lerp(
                PluginConfig.EnemyMinMultiplier.Value,
                PluginConfig.EnemyMaxMultiplier.Value,
                ModState.Difficulty.Score);

            __instance.currentMaxOutsidePower *= multiplier;
            __instance.currentMaxInsidePower *= multiplier;

            Plugin.Logger.LogInfo(
                $"[Telemetry] Enemy spawn power scaled by {multiplier:F2}x (DifficultyScore={ModState.Difficulty.Score:F3}). " +
                $"MaxOutsidePower={__instance.currentMaxOutsidePower:F1}, MaxInsidePower={__instance.currentMaxInsidePower:F1}");
        }
    }

    [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
    internal static class ScrapValueScalingPatch
    {
        [HarmonyPrefix]
        private static void ScaleScrapValue(RoundManager __instance)
        {
            ModState.EnsureLoadedForSave(GameNetworkManager.Instance.currentSaveFileName);

            float multiplier = Mathf.Lerp(
                PluginConfig.ScrapMinMultiplier.Value,
                PluginConfig.ScrapMaxMultiplier.Value,
                ModState.Difficulty.Score);
            __instance.scrapValueMultiplier = multiplier;

            Plugin.Logger.LogInfo(
                $"[Telemetry] Scrap value multiplier set to {multiplier:F2}x (DifficultyScore={ModState.Difficulty.Score:F3}).");
        }
    }
}