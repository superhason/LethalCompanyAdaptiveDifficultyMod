using AdaptiveDifficultyMod.Difficulty;

namespace AdaptiveDifficultyMod.Core
{
    internal static class ModState
    {
        internal static readonly DifficultyState Difficulty = new DifficultyState();
        private static string loadedSaveFileName;

        internal static void EnsureLoadedForSave(string saveFileName)
        {
            if (loadedSaveFileName == saveFileName)
            {
                return;
            }
            loadedSaveFileName = saveFileName;
            Difficulty.LoadScore(DifficultyPersistence.Load(saveFileName));
        }

        internal static void PersistCurrentScore()
        {
            if (loadedSaveFileName != null)
            {
                DifficultyPersistence.Save(loadedSaveFileName, Difficulty.Score);
            }
        }
    }
}