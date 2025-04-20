using System.Collections.Generic;
using UnityEngine;

public static class LevelUnlockManager
{
    private const string LEVEL_UNLOCK_KEY = "LevelUnlocked_";

    public static void InitializeLevels(List<SlideshowLevelData> levels)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            string key = LEVEL_UNLOCK_KEY + i;
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetInt(key, levels[i].isLockedInitially ? 0 : 1);
            }
        }
        PlayerPrefs.Save();
    }

    public static bool IsLevelLocked(int levelIndex)
    {
        string key = LEVEL_UNLOCK_KEY + levelIndex;
        return PlayerPrefs.GetInt(key, 1) == 0;
    }

    public static void UnlockLevel(int levelIndex)
    {
        string key = LEVEL_UNLOCK_KEY + levelIndex;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Debug.Log($"Level {levelIndex} unlocked.");
    }
}
