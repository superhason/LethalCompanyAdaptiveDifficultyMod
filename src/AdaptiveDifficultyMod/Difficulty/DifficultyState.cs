using AdaptiveDifficultyMod.Core;
using AdaptiveDifficultyMod.Metrics;

namespace AdaptiveDifficultyMod.Difficulty
{
    public class DifficultyState
    {
        public float Score { get; private set; } = DifficultyCalculator.DefaultScore;

        public void ApplyRoundResult(RoundMetrics metrics)
        {
            Score = DifficultyCalculator.UpdateScore(
                Score,
                metrics,
                PluginConfig.DeathRatioPenaltyWeight.Value,
                PluginConfig.AdjustmentRate.Value);
        }

        public void ApplyQuotaCleared()
        {
            Score = DifficultyCalculator.ApplyQuotaCleared(
                Score,
                PluginConfig.QuotaMetBonus.Value,
                PluginConfig.AdjustmentRate.Value);
        }

        public void LoadScore(float score)
        {
            Score = score;
        }
    }
}