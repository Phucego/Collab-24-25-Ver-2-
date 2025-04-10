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

    [Header("Slideshow Data")]
    public List<SlideshowLevelData> slideshowLevels = new List<SlideshowLevelData>();
    public TextMeshProUGUI levelSelectionTitleText;
    public Vector2 levelTitleVisiblePos = new Vector2(0, 100);
    public Vector2 levelTitleHiddenPos = new Vector2(0, -300); // animate up from below

    public Vector2 levelSelectionTitleHiddenPos;
    public Vector2 levelSelectionTitleVisiblePos;


    private int selectedIndex = 0;
    private List<Button> menuButtons = new List<Button>();
    private int currentLevelIndex = 0;

    [Header("Fade Controls")]
    public CanvasGroup mainMenuGroup;

    public static MainMenuUI instance;
    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();

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

        instance = this;
        
       
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

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            menuButtons[selectedIndex].onClick.Invoke();
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
        // startButton.onClick.AddListener(OnStartLevel);
        startButton.onClick.AddListener(ShowSlideshow);

        settingsButton.onClick.AddListener(() => Debug.Log("Open Settings"));
        quitButton.onClick.AddListener(() => ToggleConfirmationMenu(true));
        backButton.onClick.AddListener(OnBackToMainMenu);
        
        Quit_Yes.onClick.AddListener(Application.Quit);
        Quit_No.onClick.AddListener(() => ToggleConfirmationMenu(false));

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

        selectedIndex = 0; //Fix: Ensure selectedIndex is always reset
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
            // FIRST: Reactivate main menu
            mainMenuCanvas.SetActive(true);
            mainMenuGroup.alpha = 0f;

            // Reset visual positions before animating
            RectTransform menuRect = mainMenuCanvas.GetComponent<RectTransform>();
            menuRect.anchoredPosition = new Vector2(-500, 0);
            mainMenuCanvas.transform.localScale = Vector3.one * 0.8f;

            // Fade in canvas group AFTER it’s active
            mainMenuGroup.DOFade(1f, 0.4f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                mainMenuGroup.interactable = true;
                mainMenuGroup.blocksRaycasts = true;

                // Re-enable Start Button
                startButton.interactable = true;
            });

            // Animate title + scaling
            gameTitle.DOFade(1f, 0.4f);
            indicator.DOScale(Vector3.one * 1.2f, 0.2f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => indicator.DOScale(Vector3.one, 0.2f));

            // Animate position/scale of menu
            menuRect.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutCubic);
            mainMenuCanvas.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            // Hide the level selection canvas AFTER main menu is ready
            levelSelectionCanvas.SetActive(false);

            // Fade and animate title
            gameTitle.gameObject.SetActive(true);
            gameTitle.color = new Color(gameTitle.color.r, gameTitle.color.g, gameTitle.color.b, 0f);
            gameTitle.DOFade(1f, 0.4f);
            gameTitle.rectTransform.anchoredPosition = new Vector2(0, 300);
            gameTitle.rectTransform.DOAnchorPosY(0, 0.7f).SetEase(Ease.OutBack);

            // Play audio
            AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
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
            // Make sure the GameObject is active
            levelSelectionTitleText.gameObject.SetActive(true);

            // Reset alpha and position
            Color color = levelSelectionTitleText.color;
            color.a = 0f;
            levelSelectionTitleText.color = color;

            levelSelectionTitleText.text = "LEVEL SELECTION";
            levelSelectionTitleText.rectTransform.anchoredPosition = levelSelectionTitleHiddenPos;

            // Animate position and fade
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
        playSelectedButton.interactable = true;

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
                FadeInLevelSelectionTitle(); //Fade in title after setup
            });
        });

        gameTitle.DOFade(0f, 0.3f);
        indicator.DOScale(Vector3.one * 1.2f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => indicator.DOScale(Vector3.one, 0.2f));
    }

    private void ResetSlideshowUI()
    {
        levelPreviewImage.color = new Color(1f, 1f, 1f, 1f); // full opacity
        levelNameText.text = "";
        levelNameText.rectTransform.anchoredPosition = levelTitleHiddenPos;

        // Title fade in
        if (levelSelectionTitleText != null)
        {
            levelSelectionTitleText.gameObject.SetActive(true); // ← this line is essential
            levelSelectionTitleText.text = "LEVEL SELECTION";
            levelSelectionTitleText.color = new Color(1f, 1f, 1f, 0f); // reset alpha
            levelSelectionTitleText.rectTransform.anchoredPosition = levelSelectionTitleHiddenPos;
            levelSelectionTitleText.rectTransform.DOAnchorPos(levelSelectionTitleVisiblePos, 0.4f).SetEase(Ease.OutBack);
            levelSelectionTitleText.DOFade(1f, 0.4f).SetEase(Ease.InOutSine);
        }
    }


    private void ChangeSlide(int direction)
    {
        if (slideshowLevels == null || slideshowLevels.Count == 0) return;

        int total = slideshowLevels.Count;
        currentLevelIndex = (currentLevelIndex + direction + total) % total;

        if (slideshowLevels[currentLevelIndex].levelPreview == null)
        {
            Debug.LogWarning("Missing level preview sprite.");
            return;
        }

        levelPreviewImage.DOFade(0f, 0.2f).OnComplete(() =>
        {
            levelPreviewImage.sprite = slideshowLevels[currentLevelIndex].levelPreview;
            levelPreviewImage.DOFade(1f, 0.3f);
        });

        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        levelNameText.text = "";
        levelNameText.rectTransform.anchoredPosition = levelTitleHiddenPos;
        levelNameText.text = slideshowLevels[currentLevelIndex].displayName;
        levelNameText.rectTransform.DOAnchorPos(levelTitleVisiblePos, 0.4f).SetEase(Ease.OutBack);

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 0);
        playSelectedButton.interactable = currentLevelIndex <= unlockedLevel;
    }


    private void UpdateSlideshow()
    {
        levelPreviewImage.color = new Color(1, 1, 1, 0);
        levelPreviewImage.sprite = slideshowLevels[currentLevelIndex].levelPreview;
        levelPreviewImage.DOFade(1f, 0.3f);

        levelNameText.text = slideshowLevels[currentLevelIndex].displayName;
        levelNameText.rectTransform.anchoredPosition = levelTitleVisiblePos;

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 0);
        playSelectedButton.interactable = currentLevelIndex <= unlockedLevel;
    }

    private void PlaySelectedLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        StartCoroutine(LoadLevelCoroutine(levelIndex));
    }

    private IEnumerator LoadLevelCoroutine(int levelIndex)
    {
        anim.SetTrigger("isStart");
        yield return new WaitForSeconds(1f);

        int currentUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 0);
        if (levelIndex >= currentUnlocked && levelIndex < slideshowLevels.Count - 1)
        {
            PlayerPrefs.SetInt("UnlockedLevel", Mathf.Max(currentUnlocked, levelIndex + 1));
            PlayerPrefs.Save();
        }


        SceneManager.LoadScene(slideshowLevels[levelIndex].scene);
    }

    #endregion
}
