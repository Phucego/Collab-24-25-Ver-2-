using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening; 

public class UIManager : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject coinCounterParent, waveProgressParent, mainUI;
    public GameObject pauseMenu, towerSelectMenu, confirmationMenu, confirmationMenu_MainMenu;

    [Header("Main UI Elements")]
    public Button toggleTowerSelectButton;
    public Button resumeButton;
    public Button quitButton; 
    public Button mainMenuButton;
        

    [Header("Confirmation UI Elements")] 
    public Button Quit_Yes, Quit_No, MainMenu_No, MainMenu_Yes;

    [Header("Choose Options UI Elements")] 
    public Button chooseOptions, speedUpButton, pauseButton, muteButton, restartButton;
    public GameObject optionsContainer;

    [Header("Other References")]
    public TextMeshProUGUI coinCounterText;
    public TextMeshProUGUI lightningNotificationText; // Lightning Strike UI Notification
    public TutorialGuidance dialogueManager;
    public SceneField mainMenuScene;
    
    [Header("Wave Start UI")]
    public Button startWaveButton;
    public RectTransform startWaveRect;
    public TextMeshProUGUI nextWaveCountdownText;
    private bool hasStartedFirstWave = false;
    private Coroutine countdownCoroutine;
    public int currentWave = 0; // Stores the current wave number
    
    
    private Animator anim;
    private Camera cam;
    private bool isRotated = false;
    private bool isSpeedUp = false;
    private bool isMuteButtonPressed = false;    
    private GameStates previousState;   

    public static UIManager Instance;

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

        // Initialize lightning notification
        lightningNotificationText.alpha = 0f;

        // Button Listeners
        
        // MAIN UI
        toggleTowerSelectButton.onClick.AddListener(ToggleTowerSelectPanel);
        resumeButton.onClick.AddListener(() => SetPauseState(false));
        quitButton.onClick.AddListener(() => ToggleConfirmationMenu(true));
        mainMenuButton.onClick.AddListener(() => ToggleConfirmationMenu(true, true));

        // CONFIRMATION UI
        Quit_Yes.onClick.AddListener(Application.Quit);
        Quit_No.onClick.AddListener(() => ToggleConfirmationMenu(false));
        MainMenu_Yes.onClick.AddListener(() => SceneManager.LoadScene(mainMenuScene));
        MainMenu_No.onClick.AddListener(() => ToggleConfirmationMenu(false, true));

        // CHOOSE OPTIONS UI
        chooseOptions.onClick.AddListener(ToggleChooseOptions);
        speedUpButton.onClick.AddListener(ToggleSpeed);
        pauseButton.onClick.AddListener(() => SetPauseState(true));
        muteButton.onClick.AddListener(ToggleMute);
        restartButton.onClick.AddListener(RestartCurrentScene);
        
        // Initialize UI and button listeners
        startWaveButton.onClick.AddListener(OnStartWaveClicked);
        startWaveButton.gameObject.SetActive(true);
        nextWaveCountdownText.gameObject.SetActive(false);

        // Ensure Lightning event triggers countdown from the start
        LightningStrikeEvent lightningEvent = FindObjectOfType<LightningStrikeEvent>();
        if (lightningEvent != null)
        {
            lightningEvent.OnLightningStrike.AddListener(UpdateLightningNotification);
        }

        // Subscribe to Wave complete event for countdown between waves
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveComplete += StartNextWaveCountdown;
        }
        Color c = nextWaveCountdownText.color;
        c.a = 0f;
        nextWaveCountdownText.color = c;
        
    }

    private void Update() 
    {
        UpdateCoinCounterUI();
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) 
            SetPauseState(true);

        if (BuildingManager.Instance.pendingObj != null)
        {
            anim.SetBool("onNotifyCancel", true);
        }
        else
        {
            anim.SetBool("onNotifyCancel", false);
        }
    }

    private void UpdateCoinCounterUI()
    {
        if (CurrencyManager.Instance == null) return;
        coinCounterText.text = $"{CurrencyManager.Instance.GetCurrency()}";
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

        // Reset timescale in case it was changed (e.g., paused or sped up)
        Time.timeScale = 1f;

        // Reload the current active scene
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

        hasStartedFirstWave = true;

        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");

        // Animate the start wave button and hide it after the animation
        Sequence waveButtonSeq = DOTween.Sequence();
        waveButtonSeq.Append(startWaveRect.DOAnchorPosY(200f, 0.5f).SetEase(Ease.InBack));
        waveButtonSeq.Join(startWaveButton.image.DOFade(0f, 0.5f));
        waveButtonSeq.OnComplete(() =>
        {
            startWaveButton.gameObject.SetActive(false);
        });

        // Start Wave 1
        WaveManager.Instance.StartWave();
        currentWave = 1; // Update current wave to 1
    }

    public void StartNextWaveCountdown()
    {
        // Skip countdown on the first wave (currentWave == 1)
        if (currentWave == 1)
        {
            currentWave++;
            return;
        }

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(NextWaveCountdownRoutine());
    }

    private IEnumerator NextWaveCountdownRoutine()
    {
        float duration = 30f;
        float timer = duration;

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

        //Trigger Lightning Strikes based on wave
        LightningStrikeEvent lightningEvent = FindObjectOfType<LightningStrikeEvent>();
        if (lightningEvent != null)
        {
            lightningEvent.StrikeRandomSpots(currentWave);
        }

        // Start the next wave
        WaveManager.Instance.StartWave();
        currentWave++;
    }

    
    private void ToggleSpeed()
    {
        isSpeedUp = !isSpeedUp;
        anim.SetTrigger("isSpeedChange");
        Time.timeScale = isSpeedUp ? 4f : 1f;
        if (isSpeedUp)
        {
            AudioManager.Instance.PlaySoundEffect("SpeedUp_SFX");
        }
        else
        {
            AudioManager.Instance.PlaySoundEffect("SpeedDown_SFX");
        }
    }

    private void UpdateLightningNotification(string placeholderName, int pathID)
    {
        // Only display the path number
        string message = pathID >= 0 ? $" Lightning struck at: Path {pathID}!" : " Lightning struck!";
   

        lightningNotificationText.text = message;
    
        // Fade in instantly
        lightningNotificationText.DOFade(1f, 0.3f).OnComplete(() =>
        {
            // Hold for 2 seconds, then fade out
            StartCoroutine(FadeOutLightningNotification());
        });
    }
    
    
    public void ShowLightningStrikeUI()
    {
        lightningNotificationText.text = "⚡ Lightning Strike Active!";
        lightningNotificationText.DOFade(1f, 0.3f);
    }

    public void HideLightningStrikeUI()
    {
        StartCoroutine(FadeOutLightningNotification());
    }

    private IEnumerator FadeOutLightningNotification()
    {
        yield return new WaitForSeconds(2f);
        lightningNotificationText.DOFade(0f, 0.5f); // Smooth fade out
    }

}