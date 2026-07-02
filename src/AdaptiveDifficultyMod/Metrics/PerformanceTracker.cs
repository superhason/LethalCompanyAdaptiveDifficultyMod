namespace AdaptiveDifficultyMod.Metrics
{
    internal static class PerformanceTracker
    {
        private static int deathsThisRound;

        internal static void RecordDeath()
        {
            deathsThisRound++;
        }

        internal static RoundMetrics BuildRoundMetrics(int playerCount)
        {
            return new RoundMetrics
            {
                Deaths = deathsThisRound,
                PlayerCount = playerCount
            };
        }

        internal static void ResetRoundCounters()
        {
            deathsThisRound = 0;
        }
    }
}