using System.IO;
using BepInEx;
using AdaptiveDifficultyMod.Difficulty;

namespace AdaptiveDifficultyMod.Core
{
    internal static class DifficultyPersistence
    {
        private static string GetFilePath(string saveFileName)
        {
            string folder = Path.Combine(Paths.PluginPath, "AdaptiveDifficultyMod");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, $"difficulty_{saveFileName}.txt");
        }

        internal static float Load(string saveFileName)
        {
            string path = GetFilePath(saveFileName);
            if (File.Exists(path) && float.TryParse(File.ReadAllText(path), out float score))
            {
                return score;
            }
            return DifficultyCalculator.DefaultScore;
        }

        internal static void Save(string saveFileName, float score)
        {
            File.WriteAllText(GetFilePath(saveFileName), score.ToString());
        }
    }
}