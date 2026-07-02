using BepInEx.Configuration;

namespace AdaptiveDifficultyMod.Core
{
    internal static class PluginConfig
    {
        internal static ConfigEntry<float> QuotaMetBonus;
        internal static ConfigEntry<float> DeathRatioPenaltyWeight;
        internal static ConfigEntry<float> AdjustmentRate;
        internal static ConfigEntry<float> EnemyMinMultiplier;
        internal static ConfigEntry<float> EnemyMaxMultiplier;
        internal static ConfigEntry<float> ScrapMinMultiplier;
        internal static ConfigEntry<float> ScrapMaxMultiplier;

        internal static void Bind(ConfigFile config)
        {
            QuotaMetBonus = config.Bind(
                "Difficulty", "QuotaMetBonus", 0.3f,
                "Score bonus applied when a quota is cleared.");

            DeathRatioPenaltyWeight = config.Bind(
                "Difficulty", "DeathRatioPenaltyWeight", -0.6f,
                "Score penalty weight for a full-crew wipe (scaled down by actual death ratio).");

            AdjustmentRate = config.Bind(
                "Difficulty", "AdjustmentRate", 0.35f,
                "How much each event's performance signal nudges the running difficulty score.");

            EnemyMinMultiplier = config.Bind(
                "EnemySpawning", "MinMultiplier", 0.75f,
                "Enemy spawn power multiplier at minimum difficulty.");

            EnemyMaxMultiplier = config.Bind(
                "EnemySpawning", "MaxMultiplier", 1.5f,
                "Enemy spawn power multiplier at maximum difficulty.");

            ScrapMinMultiplier = config.Bind(
                "Economy", "MinMultiplier", 1.0f,
                "Scrap value multiplier at minimum difficulty.");

            ScrapMaxMultiplier = config.Bind(
                "Economy", "MaxMultiplier", 1.5f,
                "Scrap value multiplier at maximum difficulty.");
        }
    }
}