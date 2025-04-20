using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WaveEnemyPreview : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button previewButton;
    [SerializeField] private GameObject enemyPreviewPanel;
    [SerializeField] private TextMeshProUGUI enemyListText;

    private bool isPanelVisible = false;
    private WaveManager waveManager;

    private void Awake()
    {
        // Initialize panel as hidden
        enemyPreviewPanel.SetActive(false);
        if (enemyListText != null)
        {
            enemyListText.alpha = 0f;
        }
    }

    private void Start()
    {
        waveManager = WaveManager.Instance;
        if (waveManager == null)
        {
            Debug.LogError("WaveManager instance not found!");
            return;
        }

        // Set panel position to x: -19, y: 192
        if (enemyPreviewPanel != null)
        {
            RectTransform panelRect = enemyPreviewPanel.GetComponent<RectTransform>();
            panelRect.anchoredPosition = new Vector2(-19f, 192f);
        }

        // Set up EventTrigger for hover
        EventTrigger trigger = previewButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = previewButton.gameObject.AddComponent<EventTrigger>();
        }

        // Add PointerEnter event
        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => { OnPointerEnter(); });
        trigger.triggers.Add(enterEntry);

        // Add PointerExit event
        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => { OnPointerExit(); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnPointerEnter()
    {
        if (isPanelVisible || waveManager == null || waveManager._curData.Count == 0) return;

        // Get current wave data
        int currentWaveIndex = waveManager._curWave;
        if (currentWaveIndex >= waveManager._curData[0].Waves.Count)
        {
            enemyListText.text = "No more waves.";
            ShowPanel();
            return;
        }

        // Collect enemy data for the current wave
        List<string> enemyEntries = new List<string>();
        var wave = waveManager._curData[0].Waves[currentWaveIndex];
        foreach (var group in wave.Groups)
        {
            foreach (var enemy in group.Enemies)
            {
                int totalAmount = enemy.Amount * (group.IsLoop ? group.LoopAmount : 1);
                enemyEntries.Add($"{enemy.Name} x{totalAmount}");
            }
        }

        // Display enemies in the panel
        enemyListText.text = string.Join("\n", enemyEntries);
        ShowPanel();
    }

    private void OnPointerExit()
    {
        if (!isPanelVisible) return;
        HidePanel();
    }

    private void ShowPanel()
    {
        isPanelVisible = true;
        enemyPreviewPanel.SetActive(true);

        // Reset scale and alpha
        enemyPreviewPanel.transform.localScale = Vector3.zero;
        enemyListText.alpha = 0f;

        // DOTween animation: scale up and fade in
        Sequence showSeq = DOTween.Sequence();
        showSeq.Append(enemyPreviewPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
        showSeq.Join(enemyListText.DOFade(1f, 0.3f));
    }

    private void HidePanel()
    {
        isPanelVisible = false;

        // DOTween animation: scale down and fade out
        Sequence hideSeq = DOTween.Sequence();
        hideSeq.Append(enemyPreviewPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        hideSeq.Join(enemyListText.DOFade(0f, 0.3f));
        hideSeq.OnComplete(() => enemyPreviewPanel.SetActive(false));
    }
}