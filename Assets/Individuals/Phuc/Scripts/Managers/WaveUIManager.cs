using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider waveProgressSlider;
    [SerializeField] private Slider levelProgressSlider;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text waveNumberText;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.25f;

    private WaveManager waveManager;

    private void Start()
    {
        waveManager = WaveManager.Instance;

        if (waveManager == null)
        {
            Debug.LogError("WaveManager not found in scene.");
            enabled = false;
            return;
        }

        waveManager.OnWaveComplete += OnWaveComplete;
        waveManager.OnLevelComplete += OnLevelComplete;

        StartCoroutine(UpdateUIRoutine());
    }

    private IEnumerator UpdateUIRoutine()
    {
        while (true)
        {
            UpdateUI();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private void UpdateUI()
    {
        if (waveManager == null) return;

        // Update progress sliders
        waveProgressSlider.value = waveManager.GetWaveProgress() / 100f;
        levelProgressSlider.value = waveManager.GetLevelProgress() / 100f;

        // Update countdown if wave is waiting
        int remainingTime = waveManager.GetTimeBetweenWave();
        countdownText.text = remainingTime > 0 ? $"Next Wave In: {remainingTime}s" : "";

        // Update wave number (displayed as 1-based index)
        waveNumberText.text = $"Wave: {GetCurrentWave()}";
    }

    private int GetCurrentWave()
    {
        // Since _curWave is private, we estimate it based on progress
        // Not 100% accurate, but aligns with how the system spawns waves
        float levelProgress = waveManager.GetLevelProgress();
        float waveProgress = waveManager.GetWaveProgress();
        int estimatedWave = Mathf.FloorToInt(levelProgress / 100f * waveManager._curData[0].Waves.Count) + 1;
        return Mathf.Clamp(estimatedWave, 1, waveManager._curData[0].Waves.Count);
    }

    private void OnWaveComplete()
    {
        waveProgressSlider.value = 1f;
    }

    private void OnLevelComplete()
    {
        countdownText.text = "Level Complete!";
    }

    private void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveComplete -= OnWaveComplete;
            waveManager.OnLevelComplete -= OnLevelComplete;
        }
    }
}
