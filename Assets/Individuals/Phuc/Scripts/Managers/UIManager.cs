using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject coinCounterParent, waveProgressParent, mainUI;
    public GameObject pauseMenu, towerSelectMenu, confirmationMenu, confirmationMenu_MainMenu;
    
    [Header("Main UI Elements")] 
    public Button toggleTowerSelectButton, resumeButton, quitButton, mainMenuButton;
    
    [Header("Confirmation UI Elements")] 
    public Button Quit_Yes, Quit_No, MainMenu_No, MainMenu_Yes;

    [Header("Choose Options UI Elements")] 
    public Button chooseOptions, speedUpButton, pauseButton, muteButton;
    public GameObject optionsContainer;
    
    [Header("Other References")]
    public TextMeshProUGUI coinCounterText;
    public TutorialGuidance dialogueManager;
    public SceneField mainMenuScene;

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

        // Button Listeners
        
        //MAIN UI
        toggleTowerSelectButton.onClick.AddListener(ToggleTowerSelectPanel);
        resumeButton.onClick.AddListener(() => SetPauseState(false));
        quitButton.onClick.AddListener(() => ToggleConfirmationMenu(true));
        mainMenuButton.onClick.AddListener(() => ToggleConfirmationMenu(true, true));

        //CONFIRMATION UI
        Quit_Yes.onClick.AddListener(Application.Quit);
        Quit_No.onClick.AddListener(() => ToggleConfirmationMenu(false));
        MainMenu_Yes.onClick.AddListener(() => SceneManager.LoadScene(mainMenuScene));
        MainMenu_No.onClick.AddListener(() => ToggleConfirmationMenu(false, true));

        //CHOOSE OPTIONS UI
        chooseOptions.onClick.AddListener(ToggleChooseOptions);
        speedUpButton.onClick.AddListener(ToggleSpeed);
        pauseButton.onClick.AddListener(() => SetPauseState(true));
        muteButton.onClick.AddListener(ToggleMute);
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
        
        else if (BuildingManager.Instance.pendingObj == null)
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

        if (isRotated)
        {
            optionsContainer.SetActive(true);
        }
        else
        {
            optionsContainer.SetActive(false);   
        }
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

    private void ToggleSpeed()
    {
        isSpeedUp = !isSpeedUp;
        AudioManager.Instance.PlaySoundEffect("SpeedUp_SFX");
        anim.SetTrigger("isSpeedChange");
        Time.timeScale = isSpeedUp ? 4f : 1f;
    }
}
