using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
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
    
    
    [SerializeField] private Button m_StartWaveButton;


    [Header("Other References")]
    private Animator anim;
    private Camera cam;
    [SerializeField] private GameObject m_TestEnemy;
    public TextMeshProUGUI coinCounterText; // Reference to the TextMeshProUGUI component
    public int coinCount;

    public bool selectionPanelIsOpened;
    [Header("Scenes")]

    [SerializeField]
    private SceneField mainMenuScene;


    [Header("Menus")]
    public GameObject pauseMenu;
    public GameObject towerSelectMenu;
    public GameObject confirmationMenu;

    private void Start()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        mainUI.SetActive(true);
        confirmationMenu.SetActive(false);
        
        //selectionPanelIsOpened = false;
        cam = Camera.main;
        anim = GetComponent<Animator>();

        anim.SetBool("isTowerSelectPanelOpened", false);

        // Initialize the coin counter
        UpdateCoinCounterUI();


        //BUTTON EVENTS
      //  m_StartWaveButton.onClick.AddListener(StartGame);
        pauseButton.onClick.AddListener(OnPauseButtonClicked);
        toggleTowerSelectButton.onClick.AddListener(ToggleTowerSelectPanel);
        resumeButton.onClick.AddListener(OnResumeButton);
        quitButton.onClick.AddListener(OnQuitButton);
        mainMenuButton.onClick.AddListener(OnMainMenu);
        Quit_Yes.onClick.AddListener(OnConfirmQuit);
        Quit_No.onClick.AddListener(OnConfirmBack);


    }
   

    private void Update()
    {
        UpdateCoinCounterUI();
    }

    public void OnPauseButtonClicked()
    {
        mainUI.SetActive(false);
        pauseMenu.SetActive(true);

        // Wait for the animation to finish, then pause the game
        StartCoroutine(PauseGameAfterAnimation());
    }

    public void OnMainMenu()
    {
        
        //SceneManager.LoadScene(mainMenuScene);
    }
    public void OnResumeButton()
    {
        mainUI.SetActive(true);
        pauseMenu.SetActive(false);
        // Trigger resume animation
        anim.SetBool("isPause", false);

        // Wait for the animation to finish, then resume the game
        StartCoroutine(ResumeGameAfterAnimation());
    }

    public void OnQuitButton()
    {
        mainUI.SetActive(false);
        pauseMenu.SetActive(false);
        confirmationMenu.SetActive(true);
        anim.SetBool("isConfirmationMenu", true);
  
        
    }

    #region Confirmation Menu
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
    // Update the coin counter UI
    private void UpdateCoinCounterUI()
    {
        if (coinCount < 0) return;
        coinCounterText.text = $"{coinCount}";
    }

    // TODO: Call back the score when the enemy dies
    private IEnumerator AddScoreAfterEnemyDies()
    {   
        m_TestEnemy.GetComponent<EnemyDrops>().InitEnemy((coin) =>
        {
            coinCount += coin;
            UpdateCoinCounterUI(); // Update the UI whenever the score changes

        });
        yield return new WaitForSeconds(1f);
    }

    private void ToggleTowerSelectPanel()
    {
        // Toggle the boolean value
        selectionPanelIsOpened = !selectionPanelIsOpened;

        // Set the animation parameter directly
        anim.SetBool("isTowerSelectPanelOpened", selectionPanelIsOpened);
    }

    private IEnumerator PauseGameAfterAnimation()
    {
        Time.timeScale = 0f; // Pause the game
        anim.SetBool("isPause", true);
        cam.GetComponent<FreeFlyCamera>()._enableRotation = false;
        // Wait for the animation to complete
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

    }

    private IEnumerator ResumeGameAfterAnimation()
    {
        Time.timeScale = 1f; // Resume the game
        var animStateInfo = anim.GetCurrentAnimatorStateInfo(0).length;
        // Wait for the animation to complete
        yield return new WaitForSeconds(animStateInfo);
        cam.GetComponent<FreeFlyCamera>()._enableRotation = true;
       
    }
}
