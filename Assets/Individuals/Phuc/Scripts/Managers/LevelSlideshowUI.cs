using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class LevelSlideshowUI : MonoBehaviour
{
    [Header("UI Components")]
    public RectTransform slideshowPanel;
    public Image levelDisplayImage;
    public TextMeshProUGUI levelNameText;
    public Button leftButton, rightButton, playButton, backButton;
    [SerializeField] private TextMeshProUGUI levelSelectionTitle;
    private Vector2 levelIndicatorOriginalPos;

    [Header("Slideshow Data")]
    public List<SlideshowLevelData> levels = new List<SlideshowLevelData>();

    public Vector2 titleHiddenPos = new Vector2(-78f, -100f);
    public Vector2 titleVisiblePos = new Vector2(-78f, -12f);

    [Header("Animation Settings")]
    public Vector2 slideInPosition = new Vector2(-186f, -74f);
    public Vector2 slideOutPosition = new Vector2(1920f, -74f);
    public float slideDuration = 0.6f;

    private int currentIndex = 0;

    [Header("Background Panel")]
    public Image backgroundFadePanel;
    public float backgroundFadeDuration = 0.5f;

    [Header("Other UI Panels")]
    public GameObject mainMenuPanel;
    public SceneField mainMenuScene;
    [SerializeField] private GameObject levelIndicatorHint;

    [Header("Transition Effects")]
    [SerializeField] private Image fadeToBlackPanel;
    [SerializeField] private float fadeToBlackDuration = 1f;

    [Header("Locked Level Feedback")]
    [SerializeField] private Image lockIcon; // Lock icon image for locked levels
    [SerializeField] private TextMeshProUGUI lockedMessage; // Message for locked level feedback
    [SerializeField] private Sprite lockedLevelSprite; // Optional: Sprite for locked level preview
    [SerializeField] private bool preventLockedLevelLoading = true; // Prevent loading locked levels
    [SerializeField] private float lockedLevelOpacity = 0.5f; // Opacity for locked level images
    [SerializeField] private Vector2 lockIconPosition = new Vector2(-31f, 89f); // Lock icon position
    [SerializeField] private float lockedMessageDuration = 3f; // Duration to show locked message

    private Coroutine lockedMessageCoroutine;

    private void Start()
    {
        // Initialize unlock states via LevelUnlockManager
        LevelUnlockManager.InitializeLevels(levels);

        AnimateLevelSelectionTitle();

        if (fadeToBlackPanel != null)
            fadeToBlackPanel.gameObject.SetActive(false); // Start disabled

        slideshowPanel.anchoredPosition = slideOutPosition;

        if (backgroundFadePanel != null)
        {
            var bgColor = backgroundFadePanel.color;
            bgColor.a = 0f;
            backgroundFadePanel.color = bgColor;

            backgroundFadePanel.DOFade(0.6f, backgroundFadeDuration).OnComplete(() =>
            {
                slideshowPanel.DOAnchorPos(slideInPosition, slideDuration).SetEase(Ease.OutExpo);
            });
        }

        if (levelIndicatorHint != null)
        {
            RectTransform hintRect = levelIndicatorHint.GetComponent<RectTransform>();
            if (hintRect != null)
            {
                levelIndicatorOriginalPos = hintRect.anchoredPosition;
            }
        }
        else
        {
            slideshowPanel.DOAnchorPos(slideInPosition, slideDuration).SetEase(Ease.OutExpo);
        }

        leftButton.onClick.AddListener(PreviousLevel);
        rightButton.onClick.AddListener(NextLevel);
        playButton.onClick.AddListener(PlayCurrentLevel);
        backButton.onClick.AddListener(BackToMainMenu);

        // Initialize lock feedback UI
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(false);
            // Position the lock icon at x: -31, y: 89
            RectTransform lockRect = lockIcon.GetComponent<RectTransform>();
            if (lockRect != null)
            {
                lockRect.anchorMin = new Vector2(0.5f, 0.5f);
                lockRect.anchorMax = new Vector2(0.5f, 0.5f);
                lockRect.pivot = new Vector2(0.5f, 0.5f);
                lockRect.anchoredPosition = lockIconPosition; // Set to x: -31, y: 89
            }
        }
        if (lockedMessage != null)
        {
            lockedMessage.gameObject.SetActive(false);
            lockedMessage.text = "Level Locked! Complete previous levels to unlock.";
            // Ensure CanvasGroup for animation
            CanvasGroup cg = lockedMessage.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = lockedMessage.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        // Initialize the first level image
        InitializeLevelImage();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            PreviousLevel();

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            NextLevel();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            PlayCurrentLevel();
    }

    private void InitializeLevelImage()
    {
        if (levels == null || levels.Count == 0)
            return;

        bool isLocked = LevelUnlockManager.IsLevelLocked(currentIndex);

        levelDisplayImage.sprite = isLocked && lockedLevelSprite != null ? lockedLevelSprite : levels[currentIndex].levelPreview;
        levelDisplayImage.color = new Color(1, 1, 1, isLocked ? lockedLevelOpacity : 1f);

        // Show lock icon for locked levels
        if (lockIcon != null)
            lockIcon.gameObject.SetActive(isLocked);

        // Locked message is only shown when trying to play
        if (lockedMessage != null)
            lockedMessage.gameObject.SetActive(false);

        levelNameText.text = levels[currentIndex].displayName;
        playButton.interactable = !isLocked;
    }

    private void UpdateSlideshow()
    {
        if (levels == null || levels.Count == 0)
            return;

        bool isLocked = LevelUnlockManager.IsLevelLocked(currentIndex);

        levelDisplayImage.DOFade(0f, 0.2f).OnComplete(() =>
        {
            levelDisplayImage.sprite = isLocked && lockedLevelSprite != null ? lockedLevelSprite : levels[currentIndex].levelPreview;
            levelDisplayImage.color = new Color(1, 1, 1, 0);
            levelDisplayImage.DOFade(isLocked ? lockedLevelOpacity : 1f, 0.4f).SetEase(Ease.InOutSine);
        });

        levelNameText.rectTransform.DOKill();
        levelNameText.text = levels[currentIndex].displayName;
        levelNameText.rectTransform.anchoredPosition = titleHiddenPos;
        levelNameText.rectTransform.DOAnchorPos(titleVisiblePos, 0.4f).SetEase(Ease.OutBack);

        playButton.interactable = !isLocked;

        // Show lock icon for locked levels
        if (lockIcon != null)
            lockIcon.gameObject.SetActive(isLocked);

        // Locked message is only shown when trying to play
        if (lockedMessage != null)
            lockedMessage.gameObject.SetActive(false);
    }

    private void PreviousLevel()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        currentIndex = (currentIndex - 1 + levels.Count) % levels.Count;
        UpdateSlideshow();
    }

    private void NextLevel()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        currentIndex = (currentIndex + 1) % levels.Count;
        UpdateSlideshow();
    }

    private void PlayCurrentLevel()
    {
        if (levels == null || levels.Count == 0) return;

        if (preventLockedLevelLoading && LevelUnlockManager.IsLevelLocked(currentIndex))
        {
            Debug.LogWarning("Level is locked! Scene loading prevented.");
            if (lockedMessage != null)
            {
                // Stop any existing message coroutine
                if (lockedMessageCoroutine != null)
                    StopCoroutine(lockedMessageCoroutine);
                // Start new coroutine to show message
                lockedMessageCoroutine = StartCoroutine(ShowLockedMessage());
            }
            return;
        }

        var selectedLevel = levels[currentIndex];
        PlayerPrefs.SetInt("IsTutorial", selectedLevel.isTutorial ? 1 : 0);

        // Unlock the next level only if the current level is unlocked and playable
        if (currentIndex < levels.Count - 1 && !LevelUnlockManager.IsLevelLocked(currentIndex))
        {
            LevelUnlockManager.UnlockLevel(currentIndex + 1);
        }

        StartCoroutine(FadeAndLoadScene(selectedLevel.scene.SceneName));
    }

    private IEnumerator ShowLockedMessage()
    {
        if (lockedMessage == null) yield break;

        lockedMessage.gameObject.SetActive(true);
        CanvasGroup cg = lockedMessage.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.DOFade(1f, 0.3f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(lockedMessageDuration);
            cg.DOFade(0f, 0.3f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return new WaitForSeconds(lockedMessageDuration);
        }
        lockedMessage.gameObject.SetActive(false);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (fadeToBlackPanel != null)
        {
            fadeToBlackPanel.gameObject.SetActive(true);
            fadeToBlackPanel.color = new Color(0, 0, 0, 0);
            yield return fadeToBlackPanel.DOFade(1f, fadeToBlackDuration).SetEase(Ease.InOutSine).WaitForCompletion();
        }

        yield return LoadSceneAsync(sceneName);
    }

    private void BackToMainMenu()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");

        slideshowPanel.DOAnchorPos(slideOutPosition, slideDuration).SetEase(Ease.InExpo).OnComplete(() =>
        {
            backgroundFadePanel.DOFade(0f, backgroundFadeDuration).OnComplete(() =>
            {
                if (mainMenuPanel != null)
                    mainMenuPanel.SetActive(true);
                else
                    Debug.LogWarning("MainMenuPanel is not assigned!");

                if (levelIndicatorHint != null)
                    AnimateLevelIndicator();

                if (levelSelectionTitle != null)
                {
                    levelSelectionTitle.DOFade(0f, 0.3f).SetEase(Ease.InOutSine);
                    levelSelectionTitle.rectTransform.DOAnchorPos(titleHiddenPos, 0.3f).SetEase(Ease.InBack);
                }

                if (MainMenuUI.instance != null && MainMenuUI.instance.startButton != null)
                    MainMenuUI.instance.startButton.interactable = true;

                gameObject.SetActive(false);
            });
        });
    }

    private void AnimateLevelIndicator()
    {
        if (levelIndicatorHint == null) return;

        levelIndicatorHint.SetActive(true);
        levelIndicatorHint.transform.SetAsLastSibling(); // Bring it above other UI elements

        RectTransform hintRect = levelIndicatorHint.GetComponent<RectTransform>();
        if (hintRect != null)
        {
            // Anchor and pivot setup
            hintRect.anchorMin = new Vector2(0.5f, 0.5f);
            hintRect.anchorMax = new Vector2(0.5f, 0.5f);
            hintRect.pivot = new Vector2(0.5f, 0.5f);

            // Restore original scale
            hintRect.localScale = new Vector3(182.03f, 94.78234f, 1f);
            hintRect.localRotation = Quaternion.identity;

            // Animate from slightly below
            Vector2 finalPosition = new Vector2(-558f, 203f);
            hintRect.anchoredPosition = finalPosition + new Vector2(0, -40f);
            hintRect.DOAnchorPos(finalPosition, 0.4f).SetEase(Ease.OutBack);
        }

        // Optional fade-in
        CanvasGroup cg = levelIndicatorHint.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.DOFade(1f, 0.3f);
        }
    }

    private void AnimateLevelSelectionTitle()
    {
        if (levelSelectionTitle == null) return;

        levelSelectionTitle.enabled = true;
        levelSelectionTitle.color = new Color32(255, 255, 255, 0);
        levelSelectionTitle.rectTransform.anchoredPosition = new Vector2(-163f, -130f);
        levelSelectionTitle.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
        levelSelectionTitle.rectTransform.DOAnchorPos(new Vector2(-163f, -96f), 0.4f).SetEase(Ease.OutBack);
    }
}