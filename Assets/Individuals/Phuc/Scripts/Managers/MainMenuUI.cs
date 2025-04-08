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

    public void OnStartLevel()
    {
        mainMenuGroup.DOFade(0f, 0.3f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            mainMenuGroup.interactable = false;
            mainMenuGroup.blocksRaycasts = false;
            mainMenuCanvas.SetActive(false);

            //Setup slideshow BEFORE enabling it
            SetupLevelSlideshow();

            levelSelectionCanvas.SetActive(true);
            levelSelectionPanel.anchoredPosition = offScreenPos;
            levelSelectionPanel.DOAnchorPos(onScreenPos, slideDuration).SetEase(Ease.OutExpo);
        });

        gameTitle.DOFade(0f, 0.3f);
        indicator.DOScale(Vector3.one * 1.2f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => indicator.DOScale(Vector3.one, 0.2f));
    }



    public void OnBackToMainMenu()
    {
        levelSelectionPanel.DOAnchorPos(offScreenPos, slideDuration).SetEase(Ease.InExpo).OnComplete(() =>
        {
            levelSelectionCanvas.SetActive(false);
            mainMenuCanvas.SetActive(true);

            // Reset main menu UI
            RectTransform menuRect = mainMenuCanvas.GetComponent<RectTransform>();
            menuRect.anchoredPosition = new Vector2(-500, 0);
            mainMenuCanvas.transform.localScale = Vector3.one * 0.8f;

            menuRect.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutCubic);
            mainMenuCanvas.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            mainMenuGroup.alpha = 0f;
            mainMenuGroup.DOFade(1f, 0.4f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                mainMenuGroup.interactable = true;
                mainMenuGroup.blocksRaycasts = true;

                //Re-enable Start Button
                startButton.interactable = true;
            });

            gameTitle.DOFade(1f, 0.4f);
            indicator.DOScale(Vector3.one * 1.2f, 0.2f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => indicator.DOScale(Vector3.one, 0.2f));
        });
    }



    private void SetupLevelSlideshow()
    {
        // Avoid duplicate listeners
        leftArrowButton.onClick.RemoveAllListeners();
        rightArrowButton.onClick.RemoveAllListeners();
        playSelectedButton.onClick.RemoveAllListeners();
        backToMainButton.onClick.RemoveAllListeners();

        leftArrowButton.onClick.AddListener(() => ChangeSlide(-1));
        rightArrowButton.onClick.AddListener(() => ChangeSlide(1));
        playSelectedButton.onClick.AddListener(PlaySelectedLevel);
        backToMainButton.onClick.AddListener(OnBackToMainMenu);

        currentLevelIndex = 0;

        // Reset alpha
        levelPreviewImage.color = Color.white;
        levelNameText.color = Color.white;

        UpdateSlideshow();

        //Just in case it gets stuck as not interactable
        playSelectedButton.interactable = true;
    }



    public void ShowSlideshow()
    {
        startButton.interactable = false;

        mainMenuGroup.DOFade(0f, 0.3f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            mainMenuGroup.interactable = false;
            mainMenuGroup.blocksRaycasts = false;
            mainMenuCanvas.SetActive(false);

            levelSelectionCanvas.SetActive(true);
            levelSelectionPanel.anchoredPosition = offScreenPos;

            //Reset UI first
            ResetSlideshowUI();

            //THEN slide in
            levelSelectionPanel.DOAnchorPos(onScreenPos, slideDuration).SetEase(Ease.OutExpo);

            //Setup slideshow now that canvas is active
            SetupLevelSlideshow();
        });

        gameTitle.DOFade(0f, 0.3f);
        indicator.DOScale(Vector3.one * 1.2f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => indicator.DOScale(Vector3.one, 0.2f));
    }
    
    private void ResetSlideshowUI()
    {
        // Reset level name text
        levelNameText.text = "";
        levelNameText.rectTransform.anchoredPosition = levelTitleHiddenPos;
        levelNameText.color = new Color(1f, 1f, 1f, 1f); // Ensure full visible

        // Reset preview image
        levelPreviewImage.color = new Color(1f, 1f, 1f, 1f);

        // Reset level selection title
        if (levelSelectionTitleText != null)
        {
            levelSelectionTitleText.text = "LEVEL SELECTION";
            
            // Force correct alpha AND re-apply to avoid being overwritten
            levelSelectionTitleText.color = new Color(1f, 1f, 1f, 1f);
            levelSelectionTitleText.DOFade(1f, 0.3f);

            // Animate title position
            levelSelectionTitleText.rectTransform.anchoredPosition = levelSelectionTitleHiddenPos;
            levelSelectionTitleText.rectTransform.DOAnchorPos(levelSelectionTitleVisiblePos, 0.4f).SetEase(Ease.OutBack);

        }
    }

    private void ChangeSlide(int direction)
    {
        int total = slideshowLevels.Count;
        currentLevelIndex = (currentLevelIndex + direction + total) % total;

        levelPreviewImage.DOFade(0f, 0.2f).OnComplete(() =>
        {
            levelPreviewImage.sprite = slideshowLevels[currentLevelIndex].levelPreview;
            levelPreviewImage.DOFade(1f, 0.3f);
        });

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
