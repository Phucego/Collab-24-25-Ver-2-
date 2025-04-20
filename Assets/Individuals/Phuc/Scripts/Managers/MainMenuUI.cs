using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu Elements")]
    public Animator anim;
    public GameObject confirmationMenu;
    public GameObject levelSelectionCanvas;
    public GameObject mainMenuCanvas;
    public TextMeshProUGUI gameTitle;

    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public Button backButton;
    public Button Quit_Yes;
    public Button Quit_No;

    [Header("Navigation Indicators")]
    public RectTransform indicator;
    public RectTransform confirmationIndicator;
    public float indicatorOffset = -3.4f;

    [Header("Level Selection UI")]
    public RectTransform levelSelectionPanel;
    public float slideDuration = 0.5f;
    public Vector2 offScreenPos = new Vector2(1920, 0);
    public Vector2 onScreenPos = Vector2.zero;

    public Image levelPreviewImage;
    public TextMeshProUGUI levelNameText;
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Button playSelectedButton;
    public Button backToMainButton;

    [Header("Locked Level Feedback")]
    [SerializeField] private Image lockIcon; // Lock icon for locked levels
    [SerializeField] private TextMeshProUGUI lockedMessage; // Message for locked levels
    [SerializeField] private Sprite lockedLevelSprite; // Optional: Sprite for locked level preview
    [SerializeField] private float lockedLevelOpacity = 0.5f; // Opacity for locked levels
    [SerializeField] private Vector2 lockIconPosition = new Vector2(-31f, 89f); // Lock icon position
    [SerializeField] private float lockedMessageDuration = 3f; // Duration to show locked message

    [Header("Slideshow Data")]
    public List<SlideshowLevelData> slideshowLevels = new List<SlideshowLevelData>();
    public TextMeshProUGUI levelSelectionTitleText;
    public Vector2 levelTitleVisiblePos = new Vector2(0, 100);
    public Vector2 levelTitleHiddenPos = new Vector2(0, -300);
    public Vector2 levelSelectionTitleHiddenPos;
    public Vector2 levelSelectionTitleVisiblePos;

    private int selectedIndex = 0;
    private List<Button> menuButtons = new List<Button>();
    private int currentLevelIndex = 0;
    private Coroutine lockedMessageCoroutine;

    [Header("Fade Controls")]
    public CanvasGroup mainMenuGroup;

    public static MainMenuUI instance;

    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();
        instance = this;

        confirmationMenu.SetActive(false);
        levelSelectionCanvas.SetActive(false);
        levelSelectionPanel.anchoredPosition = offScreenPos;

        menuButtons.Add(startButton);
        menuButtons.Add(settingsButton);
        menuButtons.Add(quitButton);

        AssignButtonListeners();
        AssignHoverListeners();
        MoveIndicator(menuButtons[selectedIndex]);

        gameTitle.rectTransform.DOAnchorPosY(0, 0.7f).From(new Vector2(0, 300)).SetEase(Ease.OutBack);

        // Initialize lock feedback UI
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(false);
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
            CanvasGroup cg = lockedMessage.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = lockedMessage.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }
    }

    private void Start()
    {
        // Initialize level unlock states
        LevelUnlockManager.InitializeLevels(slideshowLevels);
    }

    private void Update()
    {
        HandleMenuNavigation();

        if (levelSelectionCanvas.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeSlide(-1);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                ChangeSlide(1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                PlaySelectedLevel();
        }
    }

    #region Menu Navigation

    private void HandleMenuNavigation()
    {
        int previousIndex = selectedIndex;

        if (confirmationMenu.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                selectedIndex = Mathf.Max(0, selectedIndex - 1);
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                selectedIndex = Mathf.Min(menuButtons.Count - 1, selectedIndex + 1);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                selectedIndex = (selectedIndex - 1 + menuButtons.Count) % menuButtons.Count;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                selectedIndex = (selectedIndex + 1) % menuButtons.Count;
        }

        if (previousIndex != selectedIndex)
            StartCoroutine(SmoothMoveIndicator(menuButtons[selectedIndex]));
    }

    private IEnumerator SmoothMoveIndicator(Button targetButton)
    {
        float duration = 0.2f;
        float elapsed = 0f;

        RectTransform targetIndicator = confirmationMenu.activeSelf ? confirmationIndicator : indicator;

        Vector3 startPos = targetIndicator.position;
        Vector3 targetPos = new Vector3(
            confirmationMenu.activeSelf ? targetButton.transform.position.x : targetIndicator.position.x,
            targetButton.transform.position.y - indicatorOffset,
            targetIndicator.position.z
        );

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            targetIndicator.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        targetIndicator.position = targetPos;
    }

    private void MoveIndicator(Button button)
    {
        RectTransform targetIndicator = confirmationMenu.activeSelf ? confirmationIndicator : indicator;

        targetIndicator.position = new Vector3(
            confirmationMenu.activeSelf ? button.transform.position.x : targetIndicator.position.x,
            button.transform.position.y - indicatorOffset,
            targetIndicator.position.z
        );
    }

    #endregion

    #region Button Setup

    private void AssignButtonListeners()
    {
        startButton.onClick.AddListener(ShowSlideshow);
        settingsButton.onClick.AddListener(() => Debug.Log("Open Settings"));
        quitButton.onClick.AddListener(() => ToggleConfirmationMenu(true));
        backButton.onClick.AddListener(OnBackToMainMenu);
        Quit_Yes.onClick.AddListener(Application.Quit);
        Quit_No.onClick.AddListener(() => ToggleConfirmationMenu(false));

        // Level selection buttons
        leftArrowButton.onClick.AddListener(() => ChangeSlide(-1));
        rightArrowButton.onClick.AddListener(() => ChangeSlide(1));
        playSelectedButton.onClick.AddListener(PlaySelectedLevel);
        backToMainButton.onClick.AddListener(OnBackToMainMenu);
    }

    private void AssignHoverListeners()
    {
        foreach (Button button in menuButtons)
        {
            if (button.GetComponent<EventTrigger>() == null)
                AddHoverListener(button);
        }
    }

    private void ToggleConfirmationMenu(bool isActive, bool isMainMenu = false)
    {
        confirmationMenu.SetActive(isActive);
        anim.SetBool(isMainMenu ? "isConfirmMainMenu" : "isConfirmationMenu", isActive);

        menuButtons.Clear();

        if (isActive)
        {
            menuButtons.Add(Quit_Yes);
            menuButtons.Add(Quit_No);
        }
        else
        {
            menuButtons.Add(startButton);
            menuButtons.Add(settingsButton);
            menuButtons.Add(quitButton);
        }

        selectedIndex = 0;
        MoveIndicator(menuButtons[selectedIndex]);

        foreach (Button button in menuButtons)
        {
            if (button.GetComponent<EventTrigger>() == null)
                AddHoverListener(button);
        }
    }

    private void AddHoverListener(Button button)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener((eventData) => MoveIndicator(button));
        trigger.triggers.Add(entry);
    }

    #endregion

    #region Level Slideshow

    public void OnBackToMainMenu()
    {
        levelSelectionPanel.DOAnchorPos(offScreenPos, slideDuration).SetEase(Ease.InExpo).OnComplete(() =>
        {
            mainMenuCanvas.SetActive(true);
            mainMenuGroup.alpha = 0f;

            RectTransform menuRect = mainMenuCanvas.GetComponent<RectTransform>();
            menuRect.anchoredPosition = new Vector2(-500, 0);
            mainMenuCanvas.transform.localScale = Vector3.one * 0.8f;

            mainMenuGroup.DOFade(1f, 0.4f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                mainMenuGroup.interactable = true;
                mainMenuGroup.blocksRaycasts = true;
                startButton.interactable = true;
            });

            gameTitle.DOFade(1f, 0.4f);
            indicator.DOScale(Vector3.one * 1.2f, 0.2f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => indicator.DOScale(Vector3.one, 0.2f));

            menuRect.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutCubic);
            mainMenuCanvas.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            levelSelectionCanvas.SetActive(false);

            gameTitle.gameObject.SetActive(true);
            gameTitle.color = new Color(gameTitle.color.r, gameTitle.color.g, gameTitle.color.b, 0f);
            gameTitle.DOFade(1f, 0.4f);
            gameTitle.rectTransform.anchoredPosition = new Vector2(0, 300);
            gameTitle.rectTransform.DOAnchorPosY(0, 0.7f).SetEase(Ease.OutBack);
        });

        if (levelSelectionTitleText != null)
        {
            levelSelectionTitleText.DOFade(0f, 0.3f).SetEase(Ease.InOutSine);
            levelSelectionTitleText.rectTransform.DOAnchorPos(levelSelectionTitleHiddenPos, 0.3f).SetEase(Ease.InBack);
        }
    }

    private void FadeInLevelSelectionTitle()
    {
        if (levelSelectionTitleText != null)
        {
            levelSelectionTitleText.gameObject.SetActive(true);
            Color color = levelSelectionTitleText.color;
            color.a = 0f;
            levelSelectionTitleText.color = color;

            levelSelectionTitleText.text = "LEVEL SELECTION";
            levelSelectionTitleText.rectTransform.anchoredPosition = levelSelectionTitleHiddenPos;

            levelSelectionTitleText.rectTransform.DOAnchorPos(levelSelectionTitleVisiblePos, 0.4f).SetEase(Ease.OutBack);
            levelSelectionTitleText.DOFade(1f, 0.4f).SetEase(Ease.InOutSine);
        }
    }

    private void SetupLevelSlideshow()
    {
        if (slideshowLevels == null || slideshowLevels.Count == 0)
        {
            Debug.LogError("No slideshow levels assigned.");
            return;
        }

        currentLevelIndex = 0;
        UpdateSlideshow();
    }

    public void ShowSlideshow()
    {
        startButton.interactable = false;

        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        mainMenuGroup.DOFade(0f, 0.3f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            mainMenuGroup.interactable = false;
            mainMenuGroup.blocksRaycasts = false;
            mainMenuCanvas.SetActive(false);

            levelSelectionCanvas.SetActive(true);
            levelSelectionPanel.anchoredPosition = offScreenPos;

            ResetSlideshowUI();

            levelSelectionPanel.DOAnchorPos(onScreenPos, slideDuration).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                SetupLevelSlideshow();
                FadeInLevelSelectionTitle();
            });
        });

        gameTitle.DOFade(0f, 0.3f);
        indicator.DOScale(Vector3.one * 1.2f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => indicator.DOScale(Vector3.one, 0.2f));
    }

    private void ResetSlideshowUI()
    {
        levelPreviewImage.color = new Color(1f, 1f, 1f, 0f);
        levelNameText.text = "";
        levelNameText.rectTransform.anchoredPosition = levelTitleHiddenPos;

        if (lockIcon != null)
            lockIcon.gameObject.SetActive(false);

        if (lockedMessage != null)
            lockedMessage.gameObject.SetActive(false);

        if (levelSelectionTitleText != null)
        {
            levelSelectionTitleText.gameObject.SetActive(true);
            levelSelectionTitleText.text = "LEVEL SELECTION";
            levelSelectionTitleText.color = new Color(1f, 1f, 1f, 0f);
            levelSelectionTitleText.rectTransform.anchoredPosition = levelSelectionTitleHiddenPos;
        }
    }

    private void ChangeSlide(int direction)
    {
        if (slideshowLevels == null || slideshowLevels.Count == 0) return;

        currentLevelIndex = (currentLevelIndex + direction + slideshowLevels.Count) % slideshowLevels.Count;

        if (slideshowLevels[currentLevelIndex].levelPreview == null)
        {
            Debug.LogWarning("Missing level preview sprite.");
            return;
        }

        UpdateSlideshow();
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
    }

    private void UpdateSlideshow()
    {
        bool isLocked = LevelUnlockManager.IsLevelLocked(currentLevelIndex);

        levelPreviewImage.DOFade(0f, 0.2f).OnComplete(() =>
        {
            levelPreviewImage.sprite = isLocked && lockedLevelSprite != null ? lockedLevelSprite : slideshowLevels[currentLevelIndex].levelPreview;
            levelPreviewImage.color = new Color(1, 1, 1, 0);
            levelPreviewImage.DOFade(isLocked ? lockedLevelOpacity : 1f, 0.3f);
        });

        levelNameText.rectTransform.DOKill();
        levelNameText.text = slideshowLevels[currentLevelIndex].displayName;
        levelNameText.rectTransform.anchoredPosition = levelTitleHiddenPos;
        levelNameText.rectTransform.DOAnchorPos(levelTitleVisiblePos, 0.4f).SetEase(Ease.OutBack);

        if (lockIcon != null)
            lockIcon.gameObject.SetActive(isLocked);

        if (lockedMessage != null)
            lockedMessage.gameObject.SetActive(false);

        playSelectedButton.interactable = !isLocked;
    }

    private void PlaySelectedLevel()
    {
        if (LevelUnlockManager.IsLevelLocked(currentLevelIndex))
        {
            Debug.LogWarning("Level is locked! Scene loading prevented.");
            if (lockedMessage != null)
            {
                if (lockedMessageCoroutine != null)
                    StopCoroutine(lockedMessageCoroutine);
                lockedMessageCoroutine = StartCoroutine(ShowLockedMessage());
            }
            return;
        }

        LoadLevel(currentLevelIndex);
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

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= slideshowLevels.Count) return;

        StartCoroutine(LoadLevelCoroutine(levelIndex));
    }

    private IEnumerator LoadLevelCoroutine(int levelIndex)
    {
        anim.SetTrigger("isStart");
        yield return new WaitForSeconds(1f);

        PlayerPrefs.SetInt("IsTutorial", slideshowLevels[levelIndex].isTutorial ? 1 : 0);
        SceneManager.LoadScene(slideshowLevels[levelIndex].scene);
    }

    public void OnLevelCompleted(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < slideshowLevels.Count - 1)
        {
            LevelUnlockManager.UnlockLevel(levelIndex + 1);
            Debug.Log($"Level {levelIndex} completed. Unlocked level {levelIndex + 1}.");
        }
    }

    #endregion
}