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

    private void Start()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        mainUI.SetActive(true);
        confirmationMenu.SetActive(false);
        confirmationMenu_MainMenu.SetActive(false);

        cam = Camera.main;
        anim = GetComponent<Animator>();
        anim.SetBool("isTowerSelectPanelOpened", false);

        // Initialize the coin counter
        UpdateCoinCounterUI();

        // Subscribe to currency updates
        CurrencyManager.Instance.InitializeCurrency(25); // Example starting value
        UpdateCoinCounterUI();

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
        mainUI.SetActive(false);
        pauseMenu.SetActive(true);
        StartCoroutine(PauseGameAfterAnimation());
    }

    public void OnMainMenu()
    {
        confirmationMenu_MainMenu.SetActive(true);
        anim.SetBool("isConfirmMainMenu", true);
    }

    public void OnResumeButton()
    {
        mainUI.SetActive(true);
        pauseMenu.SetActive(false);
        anim.SetBool("isPause", false);
        StartCoroutine(ResumeGameAfterAnimation());
    }

    public void OnQuitButton()
    {
        mainUI.SetActive(false);
        pauseMenu.SetActive(false);
        confirmationMenu.SetActive(true);
        anim.SetBool("isConfirmationMenu", true);
    }

    #region Confirmation Quit
    private void OnConfirmQuit()
    {
        Application.Quit();
    }

    private void OnConfirmBack()
    {
        anim.SetBool("isConfirmationMenu", false);
        pauseMenu.SetActive(true);
    }
    #endregion

    #region Confirmation Main Menu
    private void OnConfirmMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    private void OnConfirmBackMainMenu()
    {
        anim.SetBool("isConfirmMainMenu", false);
        pauseMenu.SetActive(true);
    }
    #endregion

    private void ToggleTowerSelectPanel()
    {
        selectionPanelIsOpened = !selectionPanelIsOpened;
        anim.SetBool("isTowerSelectPanelOpened", selectionPanelIsOpened);
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
}
