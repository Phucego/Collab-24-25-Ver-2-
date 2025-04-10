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

    private void Start()
    {
        if (!PlayerPrefs.HasKey("UnlockedLevel"))
        {
            PlayerPrefs.SetInt("UnlockedLevel", 0);
            PlayerPrefs.Save();
        }

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
        else
        {
            slideshowPanel.DOAnchorPos(slideInPosition, slideDuration).SetEase(Ease.OutExpo);
        }

        leftButton.onClick.AddListener(PreviousLevel);
        rightButton.onClick.AddListener(NextLevel);
        playButton.onClick.AddListener(PlayCurrentLevel);
        backButton.onClick.AddListener(BackToMainMenu);

        UpdateSlideshow();
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

    private void UpdateSlideshow()
    {
        if (levels == null || levels.Count == 0)
            return;

        levelDisplayImage.DOFade(0f, 0.2f).OnComplete(() =>
        {
            levelDisplayImage.sprite = levels[currentIndex].levelPreview;
            levelDisplayImage.color = new Color(1, 1, 1, 0);
            levelDisplayImage.DOFade(1f, 0.4f).SetEase(Ease.InOutSine);
        });

        levelNameText.rectTransform.DOKill();
        levelNameText.text = levels[currentIndex].displayName;
        levelNameText.rectTransform.anchoredPosition = titleHiddenPos;
        levelNameText.rectTransform.DOAnchorPos(titleVisiblePos, 0.4f).SetEase(Ease.OutBack);

        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 0);
        playButton.interactable = currentIndex <= unlocked;
    }

    private void PreviousLevel()
    {
        currentIndex = (currentIndex - 1 + levels.Count) % levels.Count;
        UpdateSlideshow();
    }

    private void NextLevel()
    {
        currentIndex = (currentIndex + 1) % levels.Count;
        UpdateSlideshow();
    }

    private void PlayCurrentLevel()
    {
        if (levels == null || levels.Count == 0) return;

        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 0);
        if (currentIndex > unlocked)
        {
            Debug.LogWarning("Level is locked!");
            return;
        }

        var selectedLevel = levels[currentIndex];
        PlayerPrefs.SetInt("IsTutorial", selectedLevel.isTutorial ? 1 : 0);

        if (currentIndex >= unlocked && currentIndex < levels.Count - 1)
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentIndex + 1);
            PlayerPrefs.Save();
        }

        StartCoroutine(FadeAndLoadScene(selectedLevel.scene.SceneName));
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
                    levelIndicatorHint.SetActive(true);

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

    //Reusable animation method for the level selection title
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
