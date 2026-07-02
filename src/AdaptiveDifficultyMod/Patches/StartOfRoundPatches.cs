using HarmonyLib;
using UnityEngine;
using AdaptiveDifficultyMod.Core;
using AdaptiveDifficultyMod.Difficulty;
using AdaptiveDifficultyMod.Metrics;

namespace AdaptiveDifficultyMod.Patches
{
    [HarmonyPatch(typeof(StartOfRound), "EndOfGameClientRpc")]
    internal static class StartOfRoundPatches
    {
        private static int lastProcessedFrame = -1;

        [HarmonyPostfix]
        private static void LogRoundEnd(int __2)
        {
            if (Time.frameCount == lastProcessedFrame)
            {
                return;
            }
            lastProcessedFrame = Time.frameCount;

            ModState.EnsureLoadedForSave(GameNetworkManager.Instance.currentSaveFileName); // NEW

            int playerCount = __2 + 1;
            RoundMetrics metrics = PerformanceTracker.BuildRoundMetrics(playerCount);

            float previousScore = ModState.Difficulty.Score;
            float deathRatio = DifficultyCalculator.CalculateDeathRatio(metrics);
            ModState.Difficulty.ApplyRoundResult(metrics);
            ModState.PersistCurrentScore(); // NEW

            Plugin.Logger.LogInfo(
                $"[Telemetry] Round End Deaths: {metrics.Deaths} Player Count: {metrics.PlayerCount} " +
                $"Death Ratio: {deathRatio:F2} Previous Score: {previousScore:F3} Current Score: {ModState.Difficulty.Score:F3}");

            PerformanceTracker.ResetRoundCounters();
        }
    }
}