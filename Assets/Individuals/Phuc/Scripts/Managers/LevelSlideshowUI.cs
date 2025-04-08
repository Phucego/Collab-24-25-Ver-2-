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

    private void Start()
    {
        if (!PlayerPrefs.HasKey("UnlockedLevel"))
        {
            PlayerPrefs.SetInt("UnlockedLevel", 0);
            PlayerPrefs.Save();
        }

        if (levelSelectionTitle != null)
        {
            levelSelectionTitle.enabled = true;
            levelSelectionTitle.color = new Color32(255, 255, 255, 0); // Start transparent

            // Fade in alpha
            levelSelectionTitle.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);

            // Animate slight vertical slide for added polish
            levelSelectionTitle.rectTransform.anchoredPosition = new Vector2(-163f, -130f);
            levelSelectionTitle.rectTransform.DOAnchorPos(new Vector2(-163f, -96f), 0.4f).SetEase(Ease.OutBack);
        }


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

        StartCoroutine(LoadSceneAsync(selectedLevel.scene.SceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
                asyncLoad.allowSceneActivation = true;

            yield return null;
        }
    }

    private void BackToMainMenu()
    {
        slideshowPanel.DOAnchorPos(slideOutPosition, slideDuration).SetEase(Ease.InExpo).OnComplete(() =>
        {
            backgroundFadePanel.DOFade(0f, backgroundFadeDuration).OnComplete(() =>
            {
                gameObject.SetActive(false);

                if (mainMenuPanel != null)
                    mainMenuPanel.SetActive(true);

                if (levelIndicatorHint != null)
                    levelIndicatorHint.SetActive(true);

                if (levelSelectionTitle != null)
                {
                    levelSelectionTitle.enabled = true;
                    levelSelectionTitle.color = new Color32(255, 255, 255, 255);
                    levelSelectionTitle.canvasRenderer.SetAlpha(1f);
                    levelSelectionTitle.rectTransform.anchoredPosition = titleVisiblePos;
                }

                var button = MainMenuUI.instance?.startButton;
                if (button != null)
                    button.interactable = true;
            });
        });
    }
}
