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

    [Header("Lose UI")]
    public GameObject losePanel;
    public TextMeshProUGUI loseText;
    public Button loseRestartButton;
    public Button loseMainMenuButton;

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
    public IGuidance guidanceManager; // Reference to Level1Guidance, Level2Guidance, etc.
    public SceneField mainMenuScene; // Assumed: "MainMenu"
    public GameObject vineEntangleNotificationPanel;
    public Image fadePanel;
    public SceneField nextLevel; // Assumed: "Level2" if current is "Level1"

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

    public Animator anim;
    private Camera cam;
    private bool isRotated = false;
    public bool isSpeedUp = false;
    private bool isMuteButtonPressed = false;
    private GameStates previousState;

    public static UIManager Instance;

    private int totalWaves;

    [SerializeField] private SceneField tutorialLevel; // Assumed: "Tutorial"
    [SerializeField] private SceneField level1Scene; // Assumed: "Level1"

    // Track active tween for wave progress slider
    private Tween waveSliderTween;

    private void Awake() => Instance = this;

    private void Start()
    {
        // Set DOTween capacity to prevent max tweens warning
        DOTween.SetTweensCapacity(500, 50);

        // Set audio to unmuted at start
        isMuteButtonPressed = false;
        PlayerPrefs.SetInt("MuteState", 0); // Save unmuted state
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetAudioPaused(false); // Unmute audio
        }
        else
        {
            Debug.LogWarning("[UIManager] AudioManager.Instance is null in Start. Mute state not applied.");
        }

        // Update mute button sprite
        if (muteButton != null && muteSprite != null && unmuteSprite != null)
        {
            muteButton.image.sprite = unmuteSprite;
        }

        // Ensure time scale is set to 1 at start
        Time.timeScale = 1f;
        StartCoroutine(ShowUIAfterTransition());

        optionsContainer.SetActive(false);
        cam = Camera.main;
        anim = GetComponent<Animator>();
        anim.SetBool("isTowerSelectPanelOpened", false);

        // Initialize currency based on scene
        if (SceneManager.GetActiveScene().name == tutorialLevel.SceneName)
        {
            CurrencyManager.Instance.InitializeCurrency(0);
            UpdateCoinCounterUI();
        }
        else if (SceneManager.GetActiveScene().name == level1Scene.SceneName)
        {
            CurrencyManager.Instance.InitializeCurrency(50);
            UpdateCoinCounterUI();
        }
        
        lightningNotificationText.alpha = 0f;

        // Set up button listeners
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

        // Setup lose panel buttons
        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(SceneManager.GetActiveScene().name)));
        if (loseMainMenuButton != null)
            loseMainMenuButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(mainMenuScene)));

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

        // Initialize wave manager and total waves
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveComplete += StartNextWaveCountdown;
            WaveManager.Instance.OnLevelComplete += StartVictorySequence;
            try
            {
                if (WaveManager.Instance._curData != null && WaveManager.Instance._curData.Count > 0 && WaveManager.Instance._curData[0].Waves != null)
                {
                    totalWaves = WaveManager.Instance._curData[0].Waves.Count;
                    Debug.Log($"[UIManager] Initialized for {SceneManager.GetActiveScene().name}. Total waves: {totalWaves}, Current wave: {currentWave}");
                }
                else
                {
                    Debug.LogError("[UIManager] WaveManager._curData is empty, null, or Waves is null. Setting totalWaves to 1 as fallback.");
                    totalWaves = 1; // Fallback to prevent immediate victory
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIManager] Failed to access WaveManager._curData: {e.Message}. Setting totalWaves to 1 as fallback.");
                totalWaves = 1;
            }
        }
        else
        {
            Debug.LogError("[UIManager] WaveManager.Instance is null. Cannot subscribe to wave events. Setting totalWaves to 1 as fallback.");
            totalWaves = 1;
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
                Debug.LogError($"[UIManager] Failed to initialize waveProgressSlider: {e.Message}");
                waveProgressSlider.gameObject.SetActive(false);
            }
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);
        
        if (waveProgressText != null)
        {
            waveProgressText.alpha = 0f;
            waveProgressText.rectTransform.anchoredPosition = new Vector2(241, -900);
            waveProgressText.gameObject.SetActive(false);
        }

        // Find guidance manager
        guidanceManager = FindObjectOfType<MonoBehaviour>() as IGuidance;
        if (guidanceManager == null)
        {
            Debug.LogWarning("[UIManager] No IGuidance component found in scene. Dialogue UI animations will be skipped.");
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
            Debug.LogWarning($"[UIManager] Failed to update wave progress: {e.Message}");
            waveProgressSlider.gameObject.SetActive(false);
        }
    }

    public void StartVictorySequence()
    {
        Debug.Log($"[UIManager] Starting victory sequence for {currentLevelName}. Current wave: {currentWave}, Total waves: {totalWaves}");
        if (guidanceManager != null)
        {
            guidanceManager.GetAnimator()?.SetTrigger("hideUI");
            Debug.Log("[UIManager] Triggered hideUI animation for victory sequence.");
        }
        StartCoroutine(VictorySequence());
    }

    public void StartLoseSequence()
    {
        Debug.Log($"[UIManager] Starting lose sequence for {currentLevelName}. Current wave: {currentWave}, Total waves: {totalWaves}");
        if (guidanceManager != null)
        {
            guidanceManager.GetAnimator()?.SetTrigger("hideUI");
            Debug.Log("[UIManager] Triggered hideUI animation for lose sequence.");
        }
        StartCoroutine(LoseSequence());
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
            Debug.LogWarning("[UIManager] Firework Prefab or Castle not assigned in UIManager.");
        }

        yield return StartCoroutine(ShowVictoryPanel());
    }

    private IEnumerator LoseSequence()
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

        yield return StartCoroutine(ShowLosePanel());
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

        bool hasNextLevel = !string.IsNullOrEmpty(nextLevel.SceneName);

        if (victoryNextLevelButton != null)
        {
            RectTransform nextLevelRect = victoryNextLevelButton.GetComponent<RectTransform>();
            nextLevelOriginalScale = nextLevelRect.localScale;
            nextLevelOriginalPos = nextLevelRect.anchoredPosition;
            nextLevelRect.localScale = nextLevelOriginalScale;
            nextLevelRect.anchoredPosition = nextLevelOriginalPos;
            victoryNextLevelButton.gameObject.SetActive(hasNextLevel); // Only show if nextLevel exists
        }

        if (victoryMainMenuButton != null)
        {
            RectTransform mainMenuRect = victoryMainMenuButton.GetComponent<RectTransform>();
            mainMenuOriginalScale = mainMenuRect.localScale;
            mainMenuOriginalPos = mainMenuRect.anchoredPosition;
            mainMenuRect.localScale = mainMenuOriginalScale;
            mainMenuRect.anchoredPosition = mainMenuOriginalPos;
            victoryMainMenuButton.gameObject.SetActive(true);
        }

        yield return panelRect.DOAnchorPos(originalPanelPos, 0.8f).SetEase(Ease.OutQuad).WaitForCompletion();

        if (victoryNextLevelButton != null && hasNextLevel)
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

    private IEnumerator ShowLosePanel()
    {
        if (losePanel == null) yield break;

        RectTransform panelRect = losePanel.GetComponent<RectTransform>();
        Vector2 originalPanelPos = panelRect.anchoredPosition;
        panelRect.anchoredPosition = originalPanelPos + new Vector2(0, -500f);
        losePanel.SetActive(true);

        if (loseText != null)
            loseText.text = $"You have failed to defend the last bastion! Try again?";

        Vector3 restartOriginalScale = Vector3.one;
        Vector2 restartOriginalPos = Vector2.zero;
        Vector3 mainMenuOriginalScale = Vector3.one;
        Vector2 mainMenuOriginalPos = Vector2.zero;

        if (loseRestartButton != null)
        {
            RectTransform restartRect = loseRestartButton.GetComponent<RectTransform>();
            restartOriginalScale = restartRect.localScale;
            restartOriginalPos = restartRect.anchoredPosition;
            restartRect.localScale = restartOriginalScale;
            restartRect.anchoredPosition = restartOriginalPos;
            loseRestartButton.gameObject.SetActive(false);
        }

        if (loseMainMenuButton != null)
        {
            RectTransform mainMenuRect = loseMainMenuButton.GetComponent<RectTransform>();
            mainMenuOriginalScale = mainMenuRect.localScale;
            mainMenuOriginalPos = mainMenuRect.anchoredPosition;
            mainMenuRect.localScale = mainMenuOriginalScale;
            mainMenuRect.anchoredPosition = mainMenuOriginalPos;
            loseMainMenuButton.gameObject.SetActive(false);
        }

        yield return panelRect.DOAnchorPos(originalPanelPos, 0.8f).SetEase(Ease.OutQuad).WaitForCompletion();

        if (loseRestartButton != null)
        {
            RectTransform restartRect = loseRestartButton.GetComponent<RectTransform>();
            restartRect.localScale = restartOriginalScale;
            restartRect.anchoredPosition = restartOriginalPos;
            loseRestartButton.gameObject.SetActive(true);
        }

        if (loseMainMenuButton != null)
        {
            RectTransform mainMenuRect = loseMainMenuButton.GetComponent<RectTransform>();
            mainMenuRect.localScale = mainMenuOriginalScale;
            mainMenuRect.anchoredPosition = mainMenuOriginalPos;
            loseMainMenuButton.gameObject.SetActive(true);
        }

        Time.timeScale = 0f;
        AudioManager.Instance?.PlaySoundEffect("Defeat_SFX");
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
            Debug.LogWarning("[UIManager] Fade Panel not assigned. Loading scene without fade.");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        // Reset time scale and audio state before loading
        Time.timeScale = 1f;
        isSpeedUp = false;
        if (AudioManager.Instance != null)
        {
            isMuteButtonPressed = false;
            PlayerPrefs.SetInt("MuteState", 0);
            AudioManager.Instance.SetAudioPaused(false);
            if (muteButton != null && unmuteSprite != null)
            {
                muteButton.image.sprite = unmuteSprite;
            }
        }

        fadePanel.gameObject.SetActive(true);
        yield return fadePanel.DOFade(1f, 0.5f).WaitForCompletion();
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LoadSceneWithFade(SceneField sceneField)
    {
        if (string.IsNullOrEmpty(sceneField.SceneName))
        {
            Debug.LogError("[UIManager] SceneField is empty. Cannot load scene.");
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
        Debug.Log($"[UIManager] Mute state: {isMuteButtonPressed}");
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetAudioPaused(isMuteButtonPressed);
        }
        else
        {
            Debug.LogWarning("[UIManager] AudioManager.Instance is null in ToggleMute. Mute state not applied.");
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
            Debug.Log($"[UIManager] Starting first wave for {SceneManager.GetActiveScene().name}. Current wave: {currentWave}, Total waves: {totalWaves}");
            WaveManager.Instance.StartWave();
            currentWave = 0;
            UpdateWaveProgress();
            hasStartedFirstWave = true;
            AudioManager.Instance?.PlaySoundEffect("ButtonClick_SFX"); // Button click sound
            AudioManager.Instance?.PlaySoundEffect("StartWave_SFX");  // Wave start sound

            AnimateWaveTextReveal();

            Sequence waveButtonSeq = DOTween.Sequence();
            waveButtonSeq.Append(startWaveRect.DOAnchorPosY(200f, 0.5f).SetEase(Ease.InBack));
            waveButtonSeq.Join(startWaveButton.image.DOFade(0f, 0.5f));
            waveButtonSeq.OnComplete(() => startWaveButton.gameObject.SetActive(false));

            // Trigger hideUI animation for guidance UI
            if (guidanceManager != null)
            {
                guidanceManager.GetAnimator()?.SetTrigger("hideUI");
                Debug.Log("[UIManager] Triggered hideUI animation on wave start.");
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] WaveManager.Instance is null in OnStartWaveClicked. Cannot start wave.");
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
        Debug.Log($"[UIManager] StartNextWaveCountdown called. Current wave: {currentWave}, Total waves: {totalWaves}, Scene: {SceneManager.GetActiveScene().name}");
        if (totalWaves <= 0)
        {
            Debug.LogError("[UIManager] totalWaves is invalid (<= 0). Cannot proceed with wave countdown.");
            StartLoseSequence();
            return;
        }

        if (currentWave + 1 >= totalWaves)
        {
            Debug.Log("[UIManager] All waves completed. Triggering victory sequence.");
            StartVictorySequence();
            return;
        }

        currentWave++;
        Debug.Log($"[UIManager] Incremented currentWave to: {currentWave}");
        UpdateWaveProgress();
        StartCountdown();

        // Trigger hideUI animation for guidance UI
        if (guidanceManager != null)
        {
            guidanceManager.GetAnimator()?.SetTrigger("hideUI");
            Debug.Log("[UIManager] Triggered hideUI animation for next wave countdown.");
        }
    }

    private void StartCountdown()
    {
        Debug.Log($"[UIManager] Starting countdown for wave {currentWave + 1}");
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            Debug.Log("[UIManager] Stopped existing countdown coroutine.");
        }
        countdownCoroutine = StartCoroutine(NextWaveCountdownRoutine());
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
            timer -= Time.unscaledDeltaTime; // Use unscaled time to avoid pause issues
            yield return null;
        }

        nextWaveCountdownText.text = "";

        Sequence endSeq = DOTween.Sequence();
        endSeq.Append(nextWaveCountdownText.DOFade(0f, 0.3f));
        endSeq.Join(nextWaveCountdownText.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        endSeq.OnComplete(() => nextWaveCountdownText.gameObject.SetActive(false));

        if (WaveManager.Instance != null)
        {
            Debug.Log($"[UIManager] Countdown complete. Starting wave {currentWave + 1}");
            WaveManager.Instance.StartWave();
            AudioManager.Instance?.PlaySoundEffect("StartWave_SFX"); // Wave start sound
        }
        else
        {
            Debug.LogWarning("[UIManager] WaveManager.Instance is null in NextWaveCountdownRoutine. Cannot start wave.");
        }

        countdownCoroutine = null;
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