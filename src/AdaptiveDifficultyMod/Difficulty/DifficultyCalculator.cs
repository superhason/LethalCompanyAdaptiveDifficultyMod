using AdaptiveDifficultyMod.Metrics;

namespace AdaptiveDifficultyMod.Difficulty
{
    public static class DifficultyCalculator
    {
        public const float MinScore = 0.0f;
        public const float MaxScore = 1.0f;
        public const float DefaultScore = 0.5f;

        public static float CalculateDeathRatio(RoundMetrics metrics)
        {
            return metrics.PlayerCount > 0 ? (float)metrics.Deaths / metrics.PlayerCount : 0f;
        }

        public static float UpdateScore(float currentScore, RoundMetrics metrics, float deathRatioPenaltyWeight, float adjustmentRate)
        {
            float deathRatio = CalculateDeathRatio(metrics);
            float performance = Clamp(deathRatio * deathRatioPenaltyWeight, -1f, 1f);

            float newScore = currentScore + performance * adjustmentRate;
            return Clamp(newScore, MinScore, MaxScore);
        }

        public static float ApplyQuotaCleared(float currentScore, float quotaMetBonus, float adjustmentRate)
        {
            float newScore = currentScore + quotaMetBonus * adjustmentRate;
            return Clamp(newScore, MinScore, MaxScore);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}