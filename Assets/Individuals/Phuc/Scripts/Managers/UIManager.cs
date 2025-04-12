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
    public Button restartButton;
    
    public GameObject optionsContainer;

    [Header("Other References")]
    public TextMeshProUGUI coinCounterText;
    public TextMeshProUGUI lightningNotificationText;
    public TutorialGuidance dialogueManager;
    public SceneField mainMenuScene;
    public GameObject vineEntangleNotificationPanel;

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

    [Header("Wave Flag Tracker")]
    public GameObject waveFlagPrefab;
    public Transform waveFlagContainer;
    private List<GameObject> waveFlags = new List<GameObject>();

    private Animator anim;
    private Camera cam;
    private bool isRotated = false;
    private bool isSpeedUp = false;
    private bool isMuteButtonPressed = false;
    private GameStates previousState;

    public static UIManager Instance;

    private int totalWaves;

    private void Awake() => Instance = this;

    private void Start()
    {
        Time.timeScale = 0f;
        StartCoroutine(ShowUIAfterTransition());

        optionsContainer.SetActive(false);
        cam = Camera.main;
        anim = GetComponent<Animator>();
        anim.SetBool("isTowerSelectPanelOpened", false);

        CurrencyManager.Instance.InitializeCurrency(0);
        UpdateCoinCounterUI();

        lightningNotificationText.alpha = 0f;

        toggleTowerSelectButton.onClick.AddListener(ToggleTowerSelectPanel);
        resumeButton.onClick.AddListener(() => SetPauseState(false));
        quitButton.onClick.AddListener(() => ToggleConfirmationMenu(true));
        mainMenuButton.onClick.AddListener(() => ToggleConfirmationMenu(true, true));

        Quit_Yes.onClick.AddListener(Application.Quit);
        Quit_No.onClick.AddListener(() => ToggleConfirmationMenu(false));
        MainMenu_Yes.onClick.AddListener(() => SceneManager.LoadScene(mainMenuScene));
        MainMenu_No.onClick.AddListener(() => ToggleConfirmationMenu(false, true));

        chooseOptions.onClick.AddListener(ToggleChooseOptions);
        speedUpButton.onClick.AddListener(ToggleSpeed);
        pauseButton.onClick.AddListener(() => SetPauseState(true));
        muteButton.onClick.AddListener(ToggleMute);
        restartButton.onClick.AddListener(RestartCurrentScene);

        startWaveButton.onClick.AddListener(OnStartWaveClicked);
        startWaveButton.gameObject.SetActive(true);
        nextWaveCountdownText.gameObject.SetActive(false);

        LightningStrikeEvent lightningEvent = FindObjectOfType<LightningStrikeEvent>();
        if (lightningEvent != null)
        {
            lightningEvent.OnLightningStrike.AddListener(UpdateLightningNotification);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveComplete += StartNextWaveCountdown;
            WaveManager.Instance.OnLevelComplete += ShowVictoryPanel;

            totalWaves = WaveManager.Instance._curData[0].Waves.Count;
            InitializeWaveFlags(totalWaves);
        }

        Color c = nextWaveCountdownText.color;
        c.a = 0f;
        nextWaveCountdownText.color = c;

        if (waveProgressSlider != null)
        {
            waveProgressSlider.minValue = 0f;
            waveProgressSlider.maxValue = 1f;
            waveProgressSlider.value = 0f;
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
        
        if (waveProgressText != null)
        {
            waveProgressText.alpha = 0f;
            waveProgressText.rectTransform.anchoredPosition = new Vector2(241, -900); // hide lower than target
            waveProgressText.gameObject.SetActive(false);
        }
    }

    private void InitializeWaveFlags(int totalWaves)
    {
        foreach (Transform child in waveFlagContainer)
        {
            Destroy(child.gameObject);
        }
        waveFlags.Clear();

        if (waveProgressSlider == null || waveFlagPrefab == null || waveFlagContainer == null)
            return;

        RectTransform sliderRect = waveProgressSlider.GetComponent<RectTransform>();
        float sliderWidth = sliderRect.rect.width;

        for (int i = 0; i < totalWaves; i++)
        {
            GameObject flag = Instantiate(waveFlagPrefab, waveFlagContainer);
            waveFlags.Add(flag);

            RectTransform flagRect = flag.GetComponent<RectTransform>();

            float normalizedPos = totalWaves == 1 ? 1f : (float)(i + 1) / totalWaves;
            float xPos = Mathf.Lerp(0, sliderWidth, normalizedPos);

            flagRect.anchorMin = new Vector2(0f, 0.5f);
            flagRect.anchorMax = new Vector2(0f, 0.5f);
            flagRect.pivot = new Vector2(0.5f, 0.5f);
            flagRect.anchoredPosition = new Vector2(xPos, 0f);
            flagRect.localScale = Vector3.one;

            Image img = flag.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 0.3f;

                if (miniBossWaves.Contains(i + 1))
                    img.color = new Color(1f, 0.5f, 0.2f, c.a); // orange tint for boss waves
                else
                    img.color = c;
            }
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

        int totalEnemies = WaveManager.Instance._allEnemies;
        int enemiesKilled = WaveManager.Instance._despawned;

        if (totalEnemies <= 0) return;

        float progress = Mathf.Clamp01((float)enemiesKilled / totalEnemies);
        waveProgressSlider.DOValue(progress, 0.5f).SetEase(Ease.OutQuad);

        waveProgressText.text = $"{currentLevelName} - Wave {Mathf.Min(currentWave + 1, totalWaves)}/{totalWaves}";

        for (int i = 0; i < waveFlags.Count; i++)
        {
            Image img = waveFlags[i].GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = (i < currentWave) ? 1f : 0.3f;
                img.color = c;
            }
        }
    }

    private void ShowVictoryPanel()
    {
        if (victoryPanel == null) return;

        victoryPanel.SetActive(true);
        victoryPanel.transform.localScale = Vector3.zero;
        victoryPanel.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack);
    }

    private void SetPauseState(bool isPaused)
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");

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
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        confirmationMenu_MainMenu.SetActive(isMainMenu && isActive);
        confirmationMenu.SetActive(!isMainMenu && isActive);
        anim.SetBool(isMainMenu ? "isConfirmMainMenu" : "isConfirmationMenu", isActive);
    }

    private void RestartCurrentScene()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ToggleTowerSelectPanel()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
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
        AudioManager.Instance.SetAudioPaused(isMuteButtonPressed);
    }

    private void ToggleChooseOptions()
    {
        isRotated = !isRotated;
        anim.SetBool("isChooseOptionsOpened", isRotated);
        StartCoroutine(RotateChooseOptions(isRotated ? -93 : 0));
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
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

        WaveManager.Instance.StartWave();
        currentWave = 0;
        UpdateWaveProgress();
        hasStartedFirstWave = true;
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");

        AnimateWaveTextReveal(); //Call animation

        Sequence waveButtonSeq = DOTween.Sequence();
        waveButtonSeq.Append(startWaveRect.DOAnchorPosY(200f, 0.5f).SetEase(Ease.InBack));
        waveButtonSeq.Join(startWaveButton.image.DOFade(0f, 0.5f));
        waveButtonSeq.OnComplete(() => startWaveButton.gameObject.SetActive(false));
    }
    private void AnimateWaveTextReveal()
    {
        if (waveProgressText == null) return;

        waveProgressText.gameObject.SetActive(true);

        // Move from below to target (241, -778)
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
            Debug.Log("All waves completed.");
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

        AudioManager.Instance.PlaySoundEffect(isSpeedUp ? "SpeedUp_SFX" : "SlowDown_SFX");
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
