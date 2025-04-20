using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerPrefsResetter : MonoBehaviour
{
    [SerializeField] private MainMenuUI mainMenuUI; // Reference to MainMenuUI to access slideshowLevels

    private void Awake()
    {
        // Ensure this GameObject persists in the Editor to handle play mode changes
        DontDestroyOnLoad(gameObject);
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        // Subscribe to play mode state changes
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Reset PlayerPrefs when exiting play mode
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ResetLevelUnlockKeys();
        }
    }
#endif

    private void ResetLevelUnlockKeys()
    {
        if (mainMenuUI == null || mainMenuUI.slideshowLevels == null)
        {
           
            // Clear all LevelUnlocked keys if slideshowLevels is unavailable
            int index = 0;
            while (PlayerPrefs.HasKey(LevelUnlockManager.LEVEL_UNLOCK_KEY + index))
            {
                PlayerPrefs.DeleteKey(LevelUnlockManager.LEVEL_UNLOCK_KEY + index);
                index++;
            }
        }
        else
        {
            // Reset keys based on slideshowLevels
            for (int i = 0; i < mainMenuUI.slideshowLevels.Count; i++)
            {
                string key = LevelUnlockManager.LEVEL_UNLOCK_KEY + i;
                PlayerPrefs.DeleteKey(key); // Remove existing key
                // Reinitialize based on isLockedInitially
                PlayerPrefs.SetInt(key, mainMenuUI.slideshowLevels[i].isLockedInitially ? 0 : 1);
            }
        }

        // Clear IsTutorial key if used
        PlayerPrefs.DeleteKey("IsTutorial");

        PlayerPrefs.Save();
      
    }

    // Expose LevelUnlockManager's LEVEL_UNLOCK_KEY for compatibility
    private static class LevelUnlockManager
    {
        public const string LEVEL_UNLOCK_KEY = "LevelUnlocked_";
    }
}