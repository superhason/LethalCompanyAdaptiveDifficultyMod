using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdaptiveDifficultyMod.Difficulty;
using AdaptiveDifficultyMod.Metrics;

namespace AdaptiveDifficultyMod.Tests
{
    [TestClass]
    public class DifficultyCalculatorTests
    {
        private const float Delta = 0.0001f;
        private const float DeathRatioPenaltyWeight = -0.6f;
        private const float AdjustmentRate = 0.35f;
        private const float QuotaMetBonus = 0.3f;

        [TestMethod]
        public void NoDeaths_ScoreUnchanged()
        {
            var metrics = new RoundMetrics { Deaths = 0, PlayerCount = 4 };
            float result = DifficultyCalculator.UpdateScore(0.5f, metrics, DeathRatioPenaltyWeight, AdjustmentRate);
            Assert.AreEqual(0.5f, result, Delta);
        }

        [TestMethod]
        public void SomeDeaths_DecreasesScore()
        {
            var metrics = new RoundMetrics { Deaths = 2, PlayerCount = 4 };
            float result = DifficultyCalculator.UpdateScore(0.5f, metrics, DeathRatioPenaltyWeight, AdjustmentRate);
            Assert.AreEqual(0.395f, result, Delta);
        }

        [TestMethod]
        public void SameDeaths_LargerCrew_LessPenalized()
        {
            var smallCrew = new RoundMetrics { Deaths = 2, PlayerCount = 4 };
            var largeCrew = new RoundMetrics { Deaths = 2, PlayerCount = 8 };

            float smallCrewResult = DifficultyCalculator.UpdateScore(0.5f, smallCrew, DeathRatioPenaltyWeight, AdjustmentRate);
            float largeCrewResult = DifficultyCalculator.UpdateScore(0.5f, largeCrew, DeathRatioPenaltyWeight, AdjustmentRate);

            Assert.AreEqual(0.395f, smallCrewResult, Delta);
            Assert.AreEqual(0.4475f, largeCrewResult, Delta);
            Assert.IsTrue(largeCrewResult > smallCrewResult);
        }

        [TestMethod]
        public void ManyDeaths_ClampsPerformanceSignal()
        {
            var metrics = new RoundMetrics { Deaths = 10, PlayerCount = 4 };
            float result = DifficultyCalculator.UpdateScore(0.5f, metrics, DeathRatioPenaltyWeight, AdjustmentRate);
            Assert.AreEqual(0.15f, result, Delta);
        }

        [TestMethod]
        public void Score_NeverGoesBelowMin()
        {
            var metrics = new RoundMetrics { Deaths = 10, PlayerCount = 4 };
            float result = DifficultyCalculator.UpdateScore(0.05f, metrics, DeathRatioPenaltyWeight, AdjustmentRate);
            Assert.AreEqual(DifficultyCalculator.MinScore, result, Delta);
        }

        [TestMethod]
        public void ZeroPlayerCount_DoesNotThrow_TreatsDeathRatioAsZero()
        {
            var metrics = new RoundMetrics { Deaths = 0, PlayerCount = 0 };
            float result = DifficultyCalculator.UpdateScore(0.5f, metrics, DeathRatioPenaltyWeight, AdjustmentRate);
            Assert.AreEqual(0.5f, result, Delta);
        }

        [TestMethod]
        public void QuotaCleared_IncreasesScore()
        {
            float result = DifficultyCalculator.ApplyQuotaCleared(0.5f, QuotaMetBonus, AdjustmentRate);
            Assert.AreEqual(0.605f, result, Delta);
        }

        [TestMethod]
        public void QuotaCleared_NeverExceedsMax()
        {
            float result = DifficultyCalculator.ApplyQuotaCleared(0.95f, QuotaMetBonus, AdjustmentRate);
            Assert.AreEqual(DifficultyCalculator.MaxScore, result, Delta);
        }
    }
}