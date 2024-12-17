using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Game Objects")]
    public GameObject coinCounterParent;
    public GameObject waveProgressParent;
    public GameObject pauseAndWaveParent;
    public GameObject mainUI;

    [Header("Buttons")]
    public Button startWaveButton;
    public Button pauseButton;
    public Button toggleTowerSelectButton;
    public Button resumeButton;
    public Button quitButton;
    public Button mainMenuButton;

    public Button Quit_Yes;
    public Button Quit_No;
    public Button MainMenu_No;
    public Button MainMenu_Yes;

    public Image circleImage;
    [SerializeField] private Button m_StartWaveButton;

    [Header("Other References")]
    private Animator anim;
    private Camera cam;
    [SerializeField] private GameObject m_TestEnemy;
    public TextMeshProUGUI coinCounterText; // Reference to the TextMeshProUGUI component

    public bool selectionPanelIsOpened;

    [Header("Scenes")]
    [SerializeField]
    private SceneField mainMenuScene;

    [Header("Menus")]
    public GameObject pauseMenu;
    public GameObject towerSelectMenu;
    public GameObject confirmationMenu;
    public GameObject confirmationMenu_MainMenu;

    public static UIManager Instance;

    public int startingCoinAmount;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 0f;
        
        StartCoroutine(ShowUIAfterTransition());
        pauseAndWaveParent.SetActive(false);
        // Initialize the coin counter
        UpdateCoinCounterUI();
        
       
        
        // Subscribe to currency updates
        CurrencyManager.Instance.InitializeCurrency(startingCoinAmount); // Example starting value
        UpdateCoinCounterUI();

        cam = Camera.main;
        anim = GetComponent<Animator>();
        anim.SetBool("isTowerSelectPanelOpened", false);
        // BUTTON EVENTS
        pauseButton.onClick.AddListener(OnPauseButtonClicked);
        toggleTowerSelectButton.onClick.AddListener(ToggleTowerSelectPanel);
        resumeButton.onClick.AddListener(OnResumeButton);
        quitButton.onClick.AddListener(OnQuitButton);
        mainMenuButton.onClick.AddListener(OnMainMenu);

        Quit_Yes.onClick.AddListener(OnConfirmQuit);
        Quit_No.onClick.AddListener(OnConfirmBack);
        MainMenu_No.onClick.AddListener(OnConfirmBackMainMenu);
        MainMenu_Yes.onClick.AddListener(OnConfirmMainMenu);
    }

    private void Update()
    {
        UpdateCoinCounterUI(); // Ensure the UI is in sync with the currency manager
    }

    private void UpdateCoinCounterUI()
    {
        if (CurrencyManager.Instance == null || CurrencyManager.Instance.GetCurrency() < 0) return;
        coinCounterText.text = $"{CurrencyManager.Instance.GetCurrency()}"; // Fetch currency from CurrencyManager
    }

    public void OnPauseButtonClicked()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        mainUI.SetActive(false);
        pauseMenu.SetActive(true);
        StartCoroutine(PauseGameAfterAnimation());
    }

    public void OnMainMenu()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        confirmationMenu_MainMenu.SetActive(true);
        anim.SetBool("isConfirmMainMenu", true);
    }

    public void OnResumeButton()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        mainUI.SetActive(true);
        pauseMenu.SetActive(false);
        anim.SetBool("isPause", false);
        StartCoroutine(ResumeGameAfterAnimation());
    }

    public void OnQuitButton()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        mainUI.SetActive(false);
        pauseMenu.SetActive(false);
        confirmationMenu.SetActive(true);
        anim.SetBool("isConfirmationMenu", true);
    }

    #region Confirmation Quit
    private void OnConfirmQuit()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        Application.Quit();
    }

    private void OnConfirmBack()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        anim.SetBool("isConfirmationMenu", false);
        pauseMenu.SetActive(true);
    }
    #endregion

    #region Confirmation Main Menu
    private void OnConfirmMainMenu()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        SceneManager.LoadScene(mainMenuScene);
    }

    private void OnConfirmBackMainMenu()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        anim.SetBool("isConfirmMainMenu", false);
        pauseMenu.SetActive(true);
    }
    #endregion

    private void ToggleTowerSelectPanel()
    {
        AudioManager.Instance.PlaySoundEffect("ButtonClick_SFX");
        selectionPanelIsOpened = !selectionPanelIsOpened;
        anim.SetBool("isTowerSelectPanelOpened", selectionPanelIsOpened);
        
        Debug.Log(selectionPanelIsOpened);
    }

    private IEnumerator PauseGameAfterAnimation()
    {
        Time.timeScale = 0f; // Pause the game
        anim.SetBool("isPause", true);
        cam.GetComponent<FreeFlyCamera>()._enableRotation = false;
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
    }

    private IEnumerator ResumeGameAfterAnimation()
    {
        Time.timeScale = 1f; // Resume the game
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        cam.GetComponent<FreeFlyCamera>()._enableRotation = true;
    }

    IEnumerator ShowUIAfterTransition()
    {
        Time.timeScale = 1f;
        yield return new WaitForSeconds(2f);
        
        pauseMenu.SetActive(false);
        mainUI.SetActive(true);
        confirmationMenu.SetActive(false);
        confirmationMenu_MainMenu.SetActive(false);
        pauseAndWaveParent.SetActive(true);
    }

    public void PlaySoundEffect()
    {
        AudioManager.Instance.PlaySoundEffect("Swoosh_SFX");
    }
}
