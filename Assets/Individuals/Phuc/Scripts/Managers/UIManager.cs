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
    [SerializeField] private Sprite muteSprite;
    [SerializeField] private Sprite unmuteSprite;
    public Button restartButton;
    public Button skipWaveButton;

    public GameObject optionsContainer;

    [Header("Other References")]
    public TextMeshProUGUI coinCounterText;
    public TextMeshProUGUI lightningNotificationText;
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

    public Animator anim;
    private Camera cam;
    private bool isRotated = false;
    public bool isSpeedUp = false;
    private bool isMuteButtonPressed = false;
    public GameStates previousState;
    public string currentSceneName;
    public static UIManager Instance { get; private set; }

    private int totalWaves;
    private Tween waveSliderTween;

    [SerializeField] private SceneField tutorialLevel;
    [SerializeField] private SceneField level1Scene;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DOTween.SetTweensCapacity(500, 50);

        currentSceneName = SceneManager.GetActiveScene().name;
        
        isMuteButtonPressed = false;
        PlayerPrefs.SetInt("MuteState", 0);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetAudioPaused(false);

        if (muteButton != null && muteSprite != null && unmuteSprite != null)
            muteButton.image.sprite = unmuteSprite;

        Time.timeScale = 1f;
        isSpeedUp = false;
        StartCoroutine(ShowUIAfterTransition());

        if (optionsContainer != null)
            optionsContainer.SetActive(false);

        cam = Camera.main;
        anim = GetComponent<Animator>();
        if (anim != null && HasParameter("isTowerSelectPanelOpened"))
            anim.SetBool("isTowerSelectPanelOpened", false);

        if (currentSceneName == tutorialLevel?.SceneName)
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.InitializeCurrency(0);
            UpdateCoinCounterUI();
            if (startWaveButton != null)
                startWaveButton.interactable = false;
        }
        else if (currentSceneName == level1Scene?.SceneName)
        {
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.InitializeCurrency(240);
            UpdateCoinCounterUI();
        }

        if (lightningNotificationText != null)
            lightningNotificationText.alpha = 0f;

        SetupButtonListeners();

        if (startWaveButton != null)
            startWaveButton.gameObject.SetActive(true);

        if (nextWaveCountdownText != null)
            nextWaveCountdownText.gameObject.SetActive(false);

        if (fadePanel != null)
        {
            Color fadeColor = fadePanel.color;
            fadeColor.a = 0f;
            fadePanel.color = fadeColor;
            fadePanel.gameObject.SetActive(false);
        }

        if (skipWaveButton != null)
            skipWaveButton.gameObject.SetActive(false);

        LightningStrikeEvent lightningEvent = FindObjectOfType<LightningStrikeEvent>();
        if (lightningEvent != null)
            lightningEvent.OnLightningStrike.AddListener(UpdateLightningNotification);

        hasStartedFirstWave = false;
        currentWave = 0;

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveComplete += StartNextWaveCountdown;
            WaveManager.Instance.OnLevelComplete += StartVictorySequence;
            totalWaves = WaveManager.Instance._curData != null &&
                         WaveManager.Instance._curData.Count > 0 &&
                         WaveManager.Instance._curData[0].Waves != null
                ? WaveManager.Instance._curData[0].Waves.Count
                : 1;
        }
        else
            totalWaves = 1;

        if (nextWaveCountdownText != null)
        {
            Color c = nextWaveCountdownText.color;
            c.a = 0f;
            nextWaveCountdownText.color = c;
        }

        if (waveProgressSlider != null)
        {
            waveProgressSlider.minValue = 0f;
            waveProgressSlider.maxValue = 1f;
            waveProgressSlider.value = 0f;
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
    }

    private void SetupButtonListeners()
    {
        if (toggleTowerSelectButton != null)
            toggleTowerSelectButton.onClick.AddListener(ToggleTowerSelectPanel);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => SetPauseState(false));
        if (quitButton != null)
            quitButton.onClick.AddListener(() => ToggleConfirmationMenu(true));
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => ToggleConfirmationMenu(true, true));

        if (Quit_Yes != null)
            Quit_Yes.onClick.AddListener(Application.Quit);
        if (Quit_No != null)
            Quit_No.onClick.AddListener(() => ToggleConfirmationMenu(false));
        if (MainMenu_Yes != null)
            MainMenu_Yes.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(mainMenuScene)));
        if (MainMenu_No != null)
            MainMenu_No.onClick.AddListener(() => ToggleConfirmationMenu(false, true));

        if (chooseOptions != null)
            chooseOptions.onClick.AddListener(ToggleChooseOptions);
        if (speedUpButton != null)
            speedUpButton.onClick.AddListener(ToggleSpeed);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => SetPauseState(true));
        if (muteButton != null)
            muteButton.onClick.AddListener(ToggleMute);
        if (restartButton != null)
            restartButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(SceneManager.GetActiveScene().name)));
        if (skipWaveButton != null)
            skipWaveButton.onClick.AddListener(SkipWave);

        if (victoryNextLevelButton != null)
            victoryNextLevelButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(nextLevel)));
        if (victoryMainMenuButton != null)
            victoryMainMenuButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(mainMenuScene)));

        if (loseRestartButton != null)
            loseRestartButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(SceneManager.GetActiveScene().name)));
        if (loseMainMenuButton != null)
            loseMainMenuButton.onClick.AddListener(() => StartCoroutine(LoadSceneWithFade(mainMenuScene)));

        if (startWaveButton != null)
            startWaveButton.onClick.AddListener(OnStartWaveClicked);
    }

    private void Update()
    {
        UpdateCoinCounterUI();
        UpdateWaveProgress();

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            SetPauseState(true);
    }

    private void UpdateCoinCounterUI()
    {
        if (CurrencyManager.Instance != null && coinCounterText != null)
            coinCounterText.text = $"{CurrencyManager.Instance.GetCurrency()}";
    }

    private void UpdateWaveProgress()
    {
        if (waveProgressSlider == null || WaveManager.Instance == null || waveProgressText == null)
            return;

        int totalEnemies = WaveManager.Instance._allEnemies;
        int enemiesKilled = WaveManager.Instance._despawned;

        if (totalEnemies <= 0)
            return;

        float progress = Mathf.Clamp01((float)enemiesKilled / totalEnemies);

        if (waveSliderTween != null && waveSliderTween.IsActive())
            waveSliderTween.Kill();

        waveSliderTween = waveProgressSlider.DOValue(progress, 0.5f).SetEase(Ease.OutQuad);

        waveProgressText.text = $"{currentLevelName} - Wave {Mathf.Min(currentWave + 1, totalWaves)}/{totalWaves}";
    }

    public void StartVictorySequence()
    {
        PlayHideUIAnimation();
        if (TutorialGuidance._instance != null)
            TutorialGuidance._instance.CompleteScene(TutorialGuidance._instance.currentSceneType);
      
        StartCoroutine(VictorySequence());
    }

    public void StartLoseSequence()
    {
        PlayHideUIAnimation();
        StartCoroutine(LoseSequence());
    }

    private void PlayHideUIAnimation()
    {
        if (anim != null && mainUI != null)
        {
            // Attempt to play the "hideUI" animation; fallback to deactivating mainUI if Animator is not set up
            try
            {
                anim.Play("hideUI");
            }
            catch
            {
                mainUI.SetActive(false);
            }
        }
        else if (mainUI != null)
        {
            mainUI.SetActive(false);
        }
    }

    private IEnumerator VictorySequence()
    {
        DeactivateAllUI();
        if (fireworkPrefab != null && castle != null)
            StartCoroutine(SpawnFireworks());

        yield return StartCoroutine(ShowVictoryPanel());
    }

    private IEnumerator LoseSequence()
    {
        DeactivateAllUI();
        yield return StartCoroutine(ShowLosePanel());
    }

    private void DeactivateAllUI()
    {
        if (mainUI != null) mainUI.SetActive(false);
        if (waveProgressParent != null) waveProgressParent.SetActive(false);
        if (coinCounterParent != null) coinCounterParent.SetActive(false);
        if (optionsContainer != null) optionsContainer.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (towerSelectMenu != null) towerSelectMenu.SetActive(false);
        if (confirmationMenu != null) confirmationMenu.SetActive(false);
        if (confirmationMenu_MainMenu != null) confirmationMenu_MainMenu.SetActive(false);
        if (vineEntangleNotificationPanel != null) vineEntangleNotificationPanel.SetActive(false);
        if (startWaveButton != null) startWaveButton.gameObject.SetActive(false);
        if (nextWaveCountdownText != null) nextWaveCountdownText.gameObject.SetActive(false);
        if (minibossNotificationText != null) minibossNotificationText.gameObject.SetActive(false);
        if (skipWaveButton != null) skipWaveButton.gameObject.SetActive(false);
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
        if (victoryPanel == null)
            yield break;

        RectTransform panelRect = victoryPanel.GetComponent<RectTransform>();
        Vector2 originalPanelPos = panelRect.anchoredPosition;
        panelRect.anchoredPosition = originalPanelPos + new Vector2(0, -500f);
        victoryPanel.SetActive(true);

        if (victoryText != null)
            victoryText.text = $"Victory! {currentLevelName} Completed!";

        bool hasNextLevel = !string.IsNullOrEmpty(nextLevel?.SceneName);
        if (victoryNextLevelButton != null)
            victoryNextLevelButton.gameObject.SetActive(hasNextLevel);
        if (victoryMainMenuButton != null)
            victoryMainMenuButton.gameObject.SetActive(true);

        yield return panelRect.DOAnchorPos(originalPanelPos, 0.8f).SetEase(Ease.OutQuad).WaitForCompletion();

        Time.timeScale = 0f;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySoundEffect("Victory_SFX");
    }

    private IEnumerator ShowLosePanel()
    {
        if (losePanel == null)
            yield break;

        RectTransform panelRect = losePanel.GetComponent<RectTransform>();
        Vector2 originalPanelPos = panelRect.anchoredPosition;
        panelRect.anchoredPosition = originalPanelPos + new Vector2(0, -500f);
        losePanel.SetActive(true);

        if (loseText != null)
            loseText.text = $"You have failed to defend the last bastion! Try again?";

        if (loseRestartButton != null)
            loseRestartButton.gameObject.SetActive(false);
        if (loseMainMenuButton != null)
            loseMainMenuButton.gameObject.SetActive(false);

        yield return panelRect.DOAnchorPos(originalPanelPos, 0.8f).SetEase(Ease.OutQuad).WaitForCompletion();

        if (loseRestartButton != null)
            loseRestartButton.gameObject.SetActive(true);
        if (loseMainMenuButton != null)
            loseMainMenuButton.gameObject.SetActive(true);

        Time.timeScale = 0f;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySoundEffect("Defeat_SFX");
    }

    private void SetPauseState(bool isPaused)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");

        if (isPaused)
        {
            previousState = GameStatesManager.Instance != null ? GameStatesManager.Instance.GetCurrentState() : GameStates.WaveActive;
            if (GameStatesManager.Instance != null)
                GameStatesManager.Instance.ChangeState(GameStates.Pause);
        }
        else
        {
            if (GameStatesManager.Instance != null)
                GameStatesManager.Instance.ChangeState(previousState);
        }

        if (mainUI != null)
            mainUI.SetActive(!isPaused);
        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);
        if (anim != null && HasParameter("isPause"))
            anim.SetBool("isPause", isPaused);

        StartCoroutine(ToggleGameTime(isPaused));
    }

    private void ToggleConfirmationMenu(bool isActive, bool isMainMenu = false)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");

        if (confirmationMenu_MainMenu != null)
            confirmationMenu_MainMenu.SetActive(isMainMenu && isActive);
        if (confirmationMenu != null)
            confirmationMenu.SetActive(!isMainMenu && isActive);

        if (anim != null)
        {
            if (isMainMenu && HasParameter("isConfirmMainMenu"))
                anim.SetBool("isConfirmMainMenu", isActive);
            else if (!isMainMenu && HasParameter("isConfirmationMenu"))
                anim.SetBool("isConfirmationMenu", isActive);
        }
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        Time.timeScale = 1f;
        isSpeedUp = false;
        if (AudioManager.Instance != null)
        {
            isMuteButtonPressed = false;
            PlayerPrefs.SetInt("MuteState", 0);
            AudioManager.Instance.SetAudioPaused(false);
            if (muteButton != null && unmuteSprite != null)
                muteButton.image.sprite = unmuteSprite;
        }

        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            yield return fadePanel.DOFade(1f, 0.5f).WaitForCompletion();
        }

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator LoadSceneWithFade(SceneField sceneField)
    {
        if (string.IsNullOrEmpty(sceneField?.SceneName))
            yield break;
        yield return StartCoroutine(LoadSceneWithFade(sceneField.SceneName));
    }

    private void ToggleTowerSelectPanel()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");

        bool isOpened = anim != null && !anim.GetBool("isTowerSelectPanelOpened");
        if (anim != null && HasParameter("isTowerSelectPanelOpened"))
            anim.SetBool("isTowerSelectPanelOpened", isOpened);
    }

    private IEnumerator ToggleGameTime(bool isPaused)
    {
        if (anim != null)
            yield return new WaitForSecondsRealtime(anim.GetCurrentAnimatorStateInfo(0).length);
        Time.timeScale = isPaused ? 0f : 1f;
        if (cam != null)
        {
            FreeFlyCamera freeFlyCam = cam.GetComponent<FreeFlyCamera>();
            if (freeFlyCam != null)
                freeFlyCam._enableRotation = !isPaused;
        }
    }

    private IEnumerator ShowUIAfterTransition()
    {
        Time.timeScale = 1f;
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        if (mainUI != null)
            mainUI.SetActive(true);
        if (confirmationMenu != null)
            confirmationMenu.SetActive(false);
        if (confirmationMenu_MainMenu != null)
            confirmationMenu_MainMenu.SetActive(false);
        yield return new WaitForSeconds(0.5f);
    }

    public void OnDialogueNextPressed(int currentLineIndex, int totalLines)
    {
    }

    private void ToggleMute()
    {
        isMuteButtonPressed = !isMuteButtonPressed;
        PlayerPrefs.SetInt("MuteState", isMuteButtonPressed ? 1 : 0);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetAudioPaused(isMuteButtonPressed);
        if (muteButton != null && muteSprite != null && unmuteSprite != null)
            muteButton.image.sprite = isMuteButtonPressed ? muteSprite : unmuteSprite;
    }

    private void ToggleChooseOptions()
    {
        isRotated = !isRotated;
        if (anim != null && HasParameter("isChooseOptionsOpened"))
            anim.SetBool("isChooseOptionsOpened", isRotated);
        StartCoroutine(RotateChooseOptions(isRotated ? -93 : 0));
        if (optionsContainer != null)
            optionsContainer.SetActive(isRotated);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
    }

    private IEnumerator RotateChooseOptions(float targetZRotation)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Quaternion startRotation = chooseOptions != null ? chooseOptions.transform.rotation : Quaternion.identity;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (chooseOptions != null)
                chooseOptions.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        if (chooseOptions != null)
            chooseOptions.transform.rotation = targetRotation;
    }

    private void OnStartWaveClicked()
    {
        if (hasStartedFirstWave || WaveManager.Instance == null)
            return;

        WaveManager.Instance.StartWave();
        currentWave = 0;
        UpdateWaveProgress();
        hasStartedFirstWave = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
            AudioManager.Instance.PlaySoundEffect("StartWave_SFX");
        }

        AnimateWaveTextReveal();

        if (startWaveRect != null && startWaveButton != null)
        {
            Sequence waveButtonSeq = DOTween.Sequence();
            waveButtonSeq.Append(startWaveRect.DOAnchorPosY(200f, 0.5f).SetEase(Ease.InBack));
            waveButtonSeq.Join(startWaveButton.image.DOFade(0f, 0.5f));
            waveButtonSeq.OnComplete(() => { if (startWaveButton != null) startWaveButton.gameObject.SetActive(false); });
        }

        PlayHideUIAnimation();
        if (GameStatesManager.Instance != null)
            GameStatesManager.Instance.ChangeState(GameStates.WaveActive);
    }

    private void AnimateWaveTextReveal()
    {
        if (waveProgressText == null)
            return;

        waveProgressText.gameObject.SetActive(true);
        waveProgressText.rectTransform.anchoredPosition = new Vector2(241, -900);

        Sequence textSeq = DOTween.Sequence();
        textSeq.Append(waveProgressText.rectTransform.DOAnchorPosY(-778f, 0.6f).SetEase(Ease.OutBack));
        textSeq.Join(waveProgressText.DOFade(1f, 0.4f));
    }

    public void StartNextWaveCountdown()
    {
        if (totalWaves < 0)
        {
            StartLoseSequence();
            return;
        }

        if (currentWave + 1 > totalWaves)
        {
            StartVictorySequence();
            return;
        }
        
        Debug.Log($"Starting countdown for wave {currentWave + 1} in scene {currentSceneName}");
       // currentWave++; // Increment currentWave to align with wave completion
        UpdateWaveProgress();
        StartCountdown();
        PlayHideUIAnimation();
    }

    public void ResetCountdownState()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        if (nextWaveCountdownText != null)
        {
            nextWaveCountdownText.gameObject.SetActive(false);
            nextWaveCountdownText.text = "";
            Color c = nextWaveCountdownText.color;
            c.a = 0f;
            nextWaveCountdownText.color = c;
        }
        if (skipWaveButton != null)
        {
            skipWaveButton.gameObject.SetActive(false);
        }
    }

    public void StartCountdown()
    {
        ResetCountdownState();
        countdownCoroutine = StartCoroutine(NextWaveCountdownRoutine());
        if (GameStatesManager.Instance != null)
        {
            Debug.Log($"StartCountdown: Setting game state to WaveCountdown for wave {currentWave + 1}");
            GameStatesManager.Instance.ChangeState(GameStates.WaveCountdown);
        }
    }

    private IEnumerator NextWaveCountdownRoutine()
    {
        Debug.Log($"NextWaveCountdownRoutine: Starting countdown for wave {currentWave + 1}");
        float timer = 30f;

        if (startWaveButton != null)
            startWaveButton.interactable = false;

        if (miniBossWaves.Contains(currentWave + 1))
            ShowMiniBossNotification($"Mini Boss Incoming at Wave {currentWave + 1}!");

        if (nextWaveCountdownText != null)
        {
            nextWaveCountdownText.gameObject.SetActive(true);
            nextWaveCountdownText.transform.localScale = Vector3.zero;
            Color startColor = nextWaveCountdownText.color;
            startColor.a = 0f;
            nextWaveCountdownText.color = startColor;

            nextWaveCountdownText.DOFade(1f, 0.3f);
            nextWaveCountdownText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        if (skipWaveButton != null)
            skipWaveButton.gameObject.SetActive(true);

        float minCountdownDuration = 1f; // Ensure at least 1 second for WaveCountdown state
        float elapsed = 0f;

        while (timer > 0)
        {
            if (nextWaveCountdownText != null)
                nextWaveCountdownText.text = $"Next Wave in: {Mathf.CeilToInt(timer)}s";
            timer -= Time.unscaledDeltaTime * (isSpeedUp ? 4f : 1f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure minimum duration
        while (elapsed < minCountdownDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (nextWaveCountdownText != null)
        {
            nextWaveCountdownText.text = "";
            Sequence endSeq = DOTween.Sequence();
            endSeq.Append(nextWaveCountdownText.DOFade(0f, 0.3f));
            endSeq.Join(nextWaveCountdownText.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
            endSeq.OnComplete(() =>
            {
                if (nextWaveCountdownText != null)
                    nextWaveCountdownText.gameObject.SetActive(false);
                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.StartWave();
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlaySoundEffect("StartWave_SFX");
                    if (GameStatesManager.Instance != null)
                    {
                        Debug.Log($"NextWaveCountdownRoutine: Setting game state to WaveActive for wave {currentWave + 1}");
                        GameStatesManager.Instance.ChangeState(GameStates.WaveActive);
                    }
                }
            });
        }
        if (skipWaveButton != null)
            skipWaveButton.gameObject.SetActive(false);

        Debug.Log($"NextWaveCountdownRoutine: Countdown completed, starting wave {currentWave + 1}");
        countdownCoroutine = null;
    }

    private void ShowMiniBossNotification(string message)
    {
        if (minibossNotificationText == null)
            return;

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
        if (minibossNotificationText != null)
        {
            minibossNotificationText.DOFade(0f, 0.5f).OnComplete(() =>
            {
                if (minibossNotificationText != null)
                    minibossNotificationText.gameObject.SetActive(false);
            });
        }
    }

    private void ToggleSpeed()
    {
        isSpeedUp = !isSpeedUp;
        if (anim != null && HasParameter("isSpeedChange"))
            anim.SetTrigger("isSpeedChange");
        Time.timeScale = isSpeedUp ? 4f : 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySoundEffect(isSpeedUp ? "SpeedUp_SFX" : "SlowDown_SFX");
    }

    private void SkipWave()
    {
        if (WaveManager.Instance != null && WaveManager.Instance._waitingForNextWave)
        {
            WaveManager.Instance.SkipToNextWave();
            if (countdownCoroutine != null)
                StopCoroutine(countdownCoroutine);
            if (skipWaveButton != null)
                skipWaveButton.gameObject.SetActive(false);
            if (nextWaveCountdownText != null)
                nextWaveCountdownText.gameObject.SetActive(false);
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.StartWave();
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySoundEffect("StartWave_SFX");
                if (GameStatesManager.Instance != null)
                {
                    Debug.Log("Setting game state to WaveActive (skip wave)");
                    GameStatesManager.Instance.ChangeState(GameStates.WaveActive);
                }
            }
        }
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
    }

    private void UpdateLightningNotification(string placeholderName, int pathID)
    {
        if (lightningNotificationText == null)
            return;

        string message = pathID >= 0 ? $" Lightning struck at: Path {pathID}!" : " Lightning struck!";
        lightningNotificationText.text = message;

        lightningNotificationText.DOFade(1f, 0.3f).OnComplete(() =>
        {
            StartCoroutine(FadeOutLightningNotification());
        });
    }

    public void ShowVineEntangleUI()
    {
        if (vineEntangleNotificationPanel == null)
            return;

        vineEntangleNotificationPanel.SetActive(true);
        vineEntangleNotificationPanel.transform.localScale = Vector3.zero;
        vineEntangleNotificationPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        StartCoroutine(HideVineEntangleAfterDelay());
    }

    public void HideVineEntangleUI()
    {
        if (vineEntangleNotificationPanel == null)
            return;

        vineEntangleNotificationPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() => { if (vineEntangleNotificationPanel != null) vineEntangleNotificationPanel.SetActive(false); });
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
        if (lightningNotificationText != null)
            lightningNotificationText.DOFade(0f, 0.5f);
    }

    public void ResetWaveState()
    {
        currentWave = 0;
        hasStartedFirstWave = false;
        if (startWaveButton != null)
        {
            startWaveButton.gameObject.SetActive(true);
            startWaveButton.interactable = true;
        }
        if (nextWaveCountdownText != null)
            nextWaveCountdownText.gameObject.SetActive(false);
        if (skipWaveButton != null)
            skipWaveButton.gameObject.SetActive(false);
        UpdateWaveProgress();
        if (GameStatesManager.Instance != null)
            GameStatesManager.Instance.ChangeState(GameStates.WaveSetup);
    }

    private bool HasParameter(string parameterName)
    {
        if (anim == null || string.IsNullOrEmpty(parameterName))
            return false;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == parameterName)
                return true;
        }
        return false;
    }
}