using UnityEngine;
using TMPro;
using DG.Tweening;

public class QuickTipManager : MonoBehaviour
{
    public static QuickTipManager Instance;

    [Header("Quick Tip UI")]
    public GameObject quickTipPanel;
    public TextMeshProUGUI quickTipText;
    public float slideDuration = 0.3f;
    public float displayDuration = 4f;

    private RectTransform rectTransform;
    private Tween currentTween;

    // Your custom position values
    private readonly Vector2 visiblePos = new Vector2(-8.6f, -162f);    // on-screen
    private readonly Vector2 hiddenPos = new Vector2(-400f, -162f);     // off-screen to the left

    private void Awake()
    {
        Instance = this;
        rectTransform = quickTipPanel.GetComponent<RectTransform>();
        quickTipPanel.SetActive(false);
    }

    public void ShowTip(string message)
    {
        quickTipText.text = message;
        quickTipPanel.SetActive(true);

        if (currentTween != null && currentTween.IsActive()) currentTween.Kill();

        rectTransform.anchoredPosition = hiddenPos;

        // Slide in
        AudioManager.Instance.PlaySoundEffect("QuickTip_SFX");
        currentTween = rectTransform.DOAnchorPos(visiblePos, slideDuration)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                // Delay before sliding out
                DOVirtual.DelayedCall(displayDuration, () =>
                {
                    AudioManager.Instance.PlaySoundEffect("QuickTip_SFX");
                    rectTransform.DOAnchorPos(hiddenPos, slideDuration)
                        .SetEase(Ease.InCubic)
                        .OnComplete(() => quickTipPanel.SetActive(false));
                });
            });
    }
}