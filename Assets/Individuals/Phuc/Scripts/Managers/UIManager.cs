using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("Game Objects")] 
    public GameObject coinCounterParent;
    public GameObject waveProgressParent;
    public GameObject mainUI;
    public GameObject pauseMenu;
    public GameObject towerSelectMenu;
    public GameObject confirmationMenu;
    public GameObject confirmationMenu_MainMenu;

    [Header("Victory UI")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryText;
    public Button victoryNextLevelButton;
    public Button victoryMainMenuButton;
    public GameObject fireworkPrefab;
    public GameObject castle;

    [Header("Main UI Elements")]
    public Button toggleTowerSelectButton;
    public Button resumeButton;
    public Button quitButton;
    public Button mainMenuButton;

    [Header("Confirmation UI Elements")] 
    public Button Quit_Yes; 
    public Button Quit_No;
    public Button MainMenu_No;
    public Button MainMenu_Yes;

    [Header("Choose Options UI Elements")]
    public Button chooseOptions;
    public Button speedUpButton;
    public Button pauseButton;    
    public Button muteButton;
    [SerializeField] private Sprite muteSprite; // Sprite when audio is muted
    [SerializeField] private Sprite unmuteSprite; // Sprite when audio is unmuted
    public Button restartButton;
    
    public GameObject optionsContainer;

    [Header("Other References")]
    public TextMeshProUGUI coinCounterText;
    public TextMeshProUGUI lightningNotificationText;
    public TutorialGuidance dialogueManager;
    public SceneField mainMenuScene;
    public GameObject vineEntangleNotificationPanel;
    public Image fadePanel;
    public SceneField nextLevel;

    [Header("Wave Start UI")]
    public Button startWaveButton;
    public RectTransform startWaveRect;
    public TextMeshProUGUI nextWaveCountdownText;
    private bool hasStartedFirstWave = false;
    private Coroutine countdownCoroutine;
    public int currentWave = 0;

    [Header("Wave Progress Tracker")]
    public Slider waveProgressSlider;
    public TextMeshProUGUI waveProgressText;
    public TextMeshProUGUI minibossNotificationText;
    public string currentLevelName = "Level 1";
    public List<int> miniBossWaves = new List<int> { 3, 6, 9 };

    private Animator anim;
    private Camera cam;
    private bool isRotated = false;
    private bool isSpeedUp = false;
    private bool isMuteButtonPressed = false;
    private GameStates previousState;

    public static UIManager Instance;

    private int totalWaves;

    [SerializeField] private SceneField tutorialLevel;
    [SerializeField] private SceneField nextScene;

    // Track active tween for wave progress slider
    private Tween waveSliderTween;

    private void Awake() => Instance = this;

    private void Start()
    {
        // Set DOTween capacity to prevent max tweens warning
        DOTween.SetTweensCapacity(500, 50);

        // Load mute state from PlayerPrefs
        isMuteButtonPressed = PlayerPrefs.GetInt("MuteState", 0) == 1;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetAudioPaused(isMuteButtonPressed);
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null in UIManager.Start. Mute state not applied.");
        }

        Time.timeScale = 0f;
        StartCoroutine(ShowUIAfterTransition());

        optionsContainer.SetActive(false);
        cam = Camera.main;
        anim = GetComponent<Animator>();
        anim.SetBool("isTowerSelectPanelOpened", false);

        if (SceneManager.GetActiveScene().name == tutorialLevel.SceneName)
        {
            CurrencyManager.Instance.InitializeCurrency(0);
            UpdateCoinCounterUI();
        }
        else if (SceneManager.GetActiveScene().name == nextScene.SceneName)
        {
            CurrencyManager.Instance.InitializeCurrency(50);
            UpdateCoinCounterUI();
        }
        
        lightningNotificationText.alpha = 0f;

        toggleTowerSelectButton.onClick.AddListener(ToggleTowerSelectPanel);
        resumeButton.onClick.AddListener(() => SetPauseState(false));
        quitButton.onClick.AddListener(() => ToggleConfirmationMenu(true));
        mainMenuButton.onClick.AddListener(() => ToggleConfirmationMenu(true, true));

        Quit_Yes.onClick.AddListener(Application.Quit);
        Quit_No.onClick.AddListener(() => ToggleConfirmationMenu(false));
        MainMenu_Yes.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(mainMenuScene)));
        MainMenu_No.onClick.AddListener(() => ToggleConfirmationMenu(false, true));

        chooseOptions.onClick.AddListener(ToggleChooseOptions);
        speedUpButton.onClick.AddListener(ToggleSpeed);
        pauseButton.onClick.AddListener(() => SetPauseState(true));
        muteButton.onClick.AddListener(ToggleMute);
        restartButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(SceneManager.GetActiveScene().name)));

        startWaveButton.onClick.AddListener(OnStartWaveClicked);
        startWaveButton.gameObject.SetActive(true);
        nextWaveCountdownText.gameObject.SetActive(false);

        if (victoryNextLevelButton != null)
            victoryNextLevelButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(nextLevel)));
        if (victoryMainMenuButton != null)
            victoryMainMenuButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(mainMenuScene)));

        if (fadePanel != null)
        {
            Color fadeColor = fadePanel.color;
            fadeColor.a = 0f;
            fadePanel.color = fadeColor;
            fadePanel.gameObject.SetActive(false);
        }

        LightningStrikeEvent lightningEvent = FindObjectOfType<LightningStrikeEvent>();
        if (lightningEvent != null)
        {
            lightningEvent.OnLightningStrike.AddListener(UpdateLightningNotification);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveComplete += StartNextWaveCountdown;
            WaveManager.Instance.OnLevelComplete += StartVictorySequence;
            try
            {
                if (WaveManager.Instance._curData != null && WaveManager.Instance._curData.Count > 0)
                {
                    totalWaves = WaveManager.Instance._curData[0].Waves.Count;
                }
                else
                {
                    Debug.LogWarning("WaveManager._curData is empty or null. Setting totalWaves to 0.");
                    totalWaves = 0;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to access WaveManager._curData: {e.Message}");
                totalWaves = 0;
            }
        }
        else
        {
            Debug.LogWarning("WaveManager.Instance is null. Victory sequence may not trigger.");
        }

        Color c = nextWaveCountdownText.color;
        c.a = 0f;
        nextWaveCountdownText.color = c;

        if (waveProgressSlider != null)
        {
            try
            {
                waveProgressSlider.minValue = 0f;
                waveProgressSlider.maxValue = 1f;
                waveProgressSlider.value = 0f;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize waveProgressSlider: {e.Message}");
                waveProgressSlider.gameObject.SetActive(false);
            }
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        
        if (waveProgressText != null)
        {
            waveProgressText.alpha = 0f;
            waveProgressText.rectTransform.anchoredPosition = new Vector2(241, -900);
            waveProgressText.gameObject.SetActive(false);
        }

        // Initialize mute button sprite
        if (muteButton != null && muteSprite != null && unmuteSprite != null)
        {
            muteButton.image.sprite = isMuteButtonPressed ? muteSprite : unmuteSprite;
        }
    }

    private void Update()
    {
        UpdateCoinCounterUI();
        UpdateWaveProgress();

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            SetPauseState(true);

        anim.SetBool("onNotifyCancel", BuildingManager.Instance.pendingObj != null);
    }

    private void UpdateCoinCounterUI()
    {
        if (CurrencyManager.Instance == null) return;
        coinCounterText.text = $"{CurrencyManager.Instance.GetCurrency()}";
    }

    private void UpdateWaveProgress()
    {
        if (waveProgressSlider == null || WaveManager.Instance == null) return;

        try
        {
            int totalEnemies = WaveManager.Instance._allEnemies;
            int enemiesKilled = WaveManager.Instance._despawned;

            if (totalEnemies <= 0) return;

            float progress = Mathf.Clamp01((float)enemiesKilled / totalEnemies);

            if (waveSliderTween != null && waveSliderTween.IsActive())
            {
                waveSliderTween.Kill();
            }

            waveSliderTween = waveProgressSlider.DOValue(progress, 0.5f).SetEase(Ease.OutQuad);

            waveProgressText.text = $"{currentLevelName} - Wave {Mathf.Min(currentWave + 1, totalWaves)}/{totalWaves}";
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to update wave progress: {e.Message}");
            waveProgressSlider.gameObject.SetActive(false);
        }
    }

    private void StartVictorySequence()
    {
        if (TutorialGuidance._instance != null)
        {
            TutorialGuidance._instance.CompleteScene(TutorialGuidance._instance.currentSceneType);
        }
        else
        {
            Debug.LogWarning("TutorialGuidance._instance is null. Cannot mark scene as completed.");
        }

        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {
        mainUI.SetActive(false);
        waveProgressParent.SetActive(false);
        coinCounterParent.SetActive(false);
        optionsContainer.SetActive(false);
        pauseMenu.SetActive(false);
        towerSelectMenu.SetActive(false);
        confirmationMenu.SetActive(false);
        confirmationMenu_MainMenu.SetActive(false);
        if (vineEntangleNotificationPanel != null)
            vineEntangleNotificationPanel.SetActive(false);
        startWaveButton.gameObject.SetActive(false);
        nextWaveCountdownText.gameObject.SetActive(false);
        if (minibossNotificationText != null)
            minibossNotificationText.gameObject.SetActive(false);

        if (fireworkPrefab != null && castle != null)
        {
            StartCoroutine(SpawnFireworks());
        }
        else
        {
            Debug.LogWarning("Firework Prefab or Castle not assigned in UIManager.");
        }

        yield return StartCoroutine(ShowVictoryPanel());
    }

    private IEnumerator SpawnFireworks()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 spawnPos = castle.transform.position + new Vector3(
                UnityEngine.Random.Range(-5f, 5f),
                UnityEngine.Random.Range(5f, 10f),
                UnityEngine.Random.Range(-5f, 5f)
            );
            GameObject firework = Instantiate(fireworkPrefab, spawnPos, Quaternion.identity);
            Destroy(firework, 3f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator ShowVictoryPanel()
    {
        if (victoryPanel == null) yield break;

        RectTransform panelRect = victoryPanel.GetComponent<RectTransform>();
        Vector2 originalPanelPos = panelRect.anchoredPosition;
        panelRect.anchoredPosition = originalPanelPos + new Vector2(0, -500f);
        victoryPanel.SetActive(true);

        if (victoryText != null)
            victoryText.text = $"Victory! {currentLevelName} Completed!";

        Vector3 nextLevelOriginalScale = Vector3.one;
        Vector2 nextLevelOriginalPos = Vector2.zero;
        Vector3 mainMenuOriginalScale = Vector3.one;
        Vector2 mainMenuOriginalPos = Vector2.zero;

        if (victoryNextLevelButton != null)
        {
            RectTransform nextLevelRect = victoryNextLevelButton.GetComponent<RectTransform>();
            nextLevelOriginalScale = nextLevelRect.localScale;
            nextLevelOriginalPos = nextLevelRect.anchoredPosition;
            nextLevelRect.localScale = nextLevelOriginalScale;
            nextLevelRect.anchoredPosition = nextLevelOriginalPos;
            victoryNextLevelButton.gameObject.SetActive(false);
        }

        if (victoryMainMenuButton != null)
        {
            RectTransform mainMenuRect = victoryMainMenuButton.GetComponent<RectTransform>();
            mainMenuOriginalScale = mainMenuRect.localScale;
            mainMenuOriginalPos = mainMenuRect.anchoredPosition;
            mainMenuRect.localScale = mainMenuOriginalScale;
            mainMenuRect.anchoredPosition = mainMenuOriginalPos;
            victoryMainMenuButton.gameObject.SetActive(false);
        }

        yield return panelRect.DOAnchorPos(originalPanelPos, 0.8f).SetEase(Ease.OutQuad).WaitForCompletion();

        if (victoryNextLevelButton != null)
        {
            RectTransform nextLevelRect = victoryNextLevelButton.GetComponent<RectTransform>();
            nextLevelRect.localScale = nextLevelOriginalScale;
            nextLevelRect.anchoredPosition = nextLevelOriginalPos;
            victoryNextLevelButton.gameObject.SetActive(true);
        }

        if (victoryMainMenuButton != null)
        {
            RectTransform mainMenuRect = victoryMainMenuButton.GetComponent<RectTransform>();
            mainMenuRect.localScale = mainMenuOriginalScale;
            mainMenuRect.anchoredPosition = mainMenuOriginalPos;
            victoryMainMenuButton.gameObject.SetActive(true);
        }

        Time.timeScale = 0f;
        AudioManager.Instance?.PlaySoundEffect("Victory_SFX");
    }

    private void SetPauseState(bool isPaused)
    {
        AudioManager.Instance?.PlaySoundEffect("ButtonClick_SFX");

        if (isPaused)
        {
            previousState = GameStatesManager.Instance.GetCurrentState();
            GameStatesManager.Instance.ChangeState(GameStates.Pause);
        }
        else
        {
            GameStatesManager.Instance.ChangeState(previousState);
        }

        mainUI.SetActive(!isPaused);
        pauseMenu.SetActive(isPaused);
        anim.SetBool("isPause", isPaused);

        StartCoroutine(ToggleGameTime(isPaused));
    }

    private void ToggleConfirmationMenu(bool isActive, bool isMainMenu = false)
    {
        AudioManager.Instance?.PlaySoundEffect("ButtonClick_SFX");
        confirmationMenu_MainMenu.SetActive(isMainMenu && isActive);
        confirmationMenu.SetActive(!isMainMenu && isActive);
        anim.SetBool(isMainMenu ? "isConfirmMainMenu" : "isConfirmationMenu", isActive);
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        if (fadePanel == null)
        {
            Debug.LogWarning("Fade Panel not assigned. Loading scene without fade.");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        fadePanel.gameObject.SetActive(true);
        yield return fadePanel.DOFade(1f, 0.5f).WaitForCompletion();
        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1f;
    }

    private IEnumerator LoadSceneWithFade(SceneField sceneField)
    {
        if (string.IsNullOrEmpty(sceneField.SceneName))
        {
            Debug.LogError("SceneField is empty. Cannot load scene.");
            yield break;
        }
        yield return StartCoroutine(LoadSceneWithFade(sceneField.SceneName));
    }

    private void ToggleTowerSelectPanel()
    {
        AudioManager.Instance?.PlaySoundEffect("ButtonClick_SFX");
        bool isOpened = !anim.GetBool("isTowerSelectPanelOpened");
        anim.SetBool("isTowerSelectPanelOpened", isOpened);
    }

    private IEnumerator ToggleGameTime(bool isPaused)
    {
        yield return new WaitForSecondsRealtime(anim.GetCurrentAnimatorStateInfo(0).length);
        Time.timeScale = isPaused ? 0f : 1f;
        cam.GetComponent<FreeFlyCamera>()._enableRotation = !isPaused;
    }

    private IEnumerator ShowUIAfterTransition()
    {
        Time.timeScale = 1f;
        yield return new WaitForSeconds(2f);
        pauseMenu.SetActive(false);
        mainUI.SetActive(true);
        confirmationMenu.SetActive(false);
        confirmationMenu_MainMenu.SetActive(false);
    }

    private void ToggleMute()
    {
        isMuteButtonPressed = !isMuteButtonPressed;
        PlayerPrefs.SetInt("MuteState", isMuteButtonPressed ? 1 : 0);
        Debug.Log($"Mute state: {isMuteButtonPressed}");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetAudioPaused(isMuteButtonPressed);
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null in ToggleMute. Mute state not applied.");
        }
        
        if (muteButton != null && muteSprite != null && unmuteSprite != null)
        {
            muteButton.image.sprite = isMuteButtonPressed ? muteSprite : unmuteSprite;
        }
    }

    private void ToggleChooseOptions()
    {
        isRotated = !isRotated;
        anim.SetBool("isChooseOptionsOpened", isRotated);
        StartCoroutine(RotateChooseOptions(isRotated ? -93 : 0));
        AudioManager.Instance?.PlaySoundEffect("ButtonClick_SFX");
        optionsContainer.SetActive(isRotated);
    }

    private IEnumerator RotateChooseOptions(float targetZRotation)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Quaternion startRotation = chooseOptions.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            chooseOptions.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        chooseOptions.transform.rotation = targetRotation;
    }

    private void OnStartWaveClicked()
    {
        if (hasStartedFirstWave) return;

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartWave();
            currentWave = 0;
            UpdateWaveProgress();
            hasStartedFirstWave = true;
            AudioManager.Instance?.PlaySoundEffect("ButtonClick_SFX");

            AnimateWaveTextReveal();

            Sequence waveButtonSeq = DOTween.Sequence();
            waveButtonSeq.Append(startWaveRect.DOAnchorPosY(200f, 0.5f).SetEase(Ease.InBack));
            waveButtonSeq.Join(startWaveButton.image.DOFade(0f, 0.5f));
            waveButtonSeq.OnComplete(() => startWaveButton.gameObject.SetActive(false));
        }
        else
        {
            Debug.LogWarning("WaveManager.Instance is null. Cannot start wave.");
        }
    }

    private void AnimateWaveTextReveal()
    {
        if (waveProgressText == null) return;

        waveProgressText.gameObject.SetActive(true);
        waveProgressText.rectTransform.anchoredPosition = new Vector2(241, -900);

        Sequence textSeq = DOTween.Sequence();
        textSeq.Append(waveProgressText.rectTransform.DOAnchorPosY(-778f, 0.6f).SetEase(Ease.OutBack));
        textSeq.Join(waveProgressText.DOFade(1f, 0.4f));
    }

    public void StartNextWaveCountdown()
    {
        currentWave++;

        if (currentWave >= totalWaves)
        {
            Debug.Log("All waves completed. Skipping countdown.");
            return;
        }

        UpdateWaveProgress();
        StartCoroutine(NextWaveCountdownRoutine());
    }

    private IEnumerator NextWaveCountdownRoutine()
    {
        float timer = 30f;

        if (miniBossWaves.Contains(currentWave + 1))
        {
            ShowMiniBossNotification($"Mini Boss Incoming at Wave {currentWave + 1}!");
        }

        nextWaveCountdownText.gameObject.SetActive(true);
        nextWaveCountdownText.transform.localScale = Vector3.zero;
        Color startColor = nextWaveCountdownText.color;
        startColor.a = 0f;
        nextWaveCountdownText.color = startColor;

        nextWaveCountdownText.DOFade(1f, 0.3f);
        nextWaveCountdownText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        while (timer > 0)
        {
            nextWaveCountdownText.text = $"Next Wave in: {Mathf.CeilToInt(timer)}s";
            timer -= Time.deltaTime;
            yield return null;
        }

        nextWaveCountdownText.text = "";

        Sequence endSeq = DOTween.Sequence();
        endSeq.Append(nextWaveCountdownText.DOFade(0f, 0.3f));
        endSeq.Join(nextWaveCountdownText.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        endSeq.OnComplete(() => nextWaveCountdownText.gameObject.SetActive(false));

        if (WaveManager.Instance != null)
            WaveManager.Instance.StartWave();
    }

    private void ShowMiniBossNotification(string message)
    {
        if (minibossNotificationText == null) return;

        minibossNotificationText.text = message;
        minibossNotificationText.transform.localScale = Vector3.zero;

        Color startColor = minibossNotificationText.color;
        startColor.a = 0f;
        minibossNotificationText.color = startColor;

        minibossNotificationText.gameObject.SetActive(true);
        minibossNotificationText.DOFade(1f, 0.3f);
        minibossNotificationText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        StartCoroutine(FadeOutMiniBossNotification());
    }

    private IEnumerator FadeOutMiniBossNotification()
    {
        yield return new WaitForSeconds(2.5f);
        minibossNotificationText.DOFade(0f, 0.5f).OnComplete(() =>
        {
            minibossNotificationText.gameObject.SetActive(false);
        });
    }

    private void ToggleSpeed()
    {
        isSpeedUp = !isSpeedUp;
        anim.SetTrigger("isSpeedChange");
        Time.timeScale = isSpeedUp ? 4f : 1f;

        AudioManager.Instance?.PlaySoundEffect(isSpeedUp ? "SpeedUp_SFX" : "SlowDown_SFX");
    }

    private void UpdateLightningNotification(string placeholderName, int pathID)
    {
        string message = pathID >= 0 ? $" Lightning struck at: Path {pathID}!" : " Lightning struck!";
        lightningNotificationText.text = message;

        lightningNotificationText.DOFade(1f, 0.3f).OnComplete(() =>
        {
            StartCoroutine(FadeOutLightningNotification());
        });
    }

    public void ShowVineEntangleUI()
    {
        if (vineEntangleNotificationPanel == null) return;

        vineEntangleNotificationPanel.SetActive(true);
        vineEntangleNotificationPanel.transform.localScale = Vector3.zero;

        vineEntangleNotificationPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        StartCoroutine(HideVineEntangleAfterDelay());
    }

    public void HideVineEntangleUI()
    {
        if (vineEntangleNotificationPanel == null) return;

        vineEntangleNotificationPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() => vineEntangleNotificationPanel.SetActive(false));
    }

    private IEnumerator HideVineEntangleAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        HideVineEntangleUI();
    }

    public void HideLightningStrikeUI()
    {
        StartCoroutine(FadeOutLightningNotification());
    }

    private IEnumerator FadeOutLightningNotification()
    {
        yield return new WaitForSeconds(2f);
        lightningNotificationText.DOFade(0f, 0.5f);
    }
}