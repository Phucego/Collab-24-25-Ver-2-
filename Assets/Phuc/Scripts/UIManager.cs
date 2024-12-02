using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Main Interface")]
    public GameObject coinCounterParent;
    public GameObject waveProgressParent;
    public GameObject pauseAndWaveParent;
    public GameObject crosshair;
    public GameObject mainUI;
    public TextMeshProUGUI coinCounterText; // Reference to the TextMeshProUGUI component
    public int coinCount;
    public Button startWaveButton;
    public Button pauseButton;
    [Header("Other References")]
    private Animator anim;
    private Camera cam;
    [SerializeField] private GameObject m_TestEnemy;

    [SerializeField]
    private SceneField mainMenuScene;
    [Header("Menus")]
    public GameObject pauseMenu;

  

    private void Start()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        mainUI.SetActive(true);

        pauseButton.onClick.AddListener(OnPauseButtonClicked);

        cam = Camera.main;
        anim = GetComponent<Animator>();

        // Initialize the coin counter
        UpdateCoinCounterUI();
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
        SceneManager.LoadScene(mainMenuScene);
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
        Application.Quit();
    }

    private IEnumerator PauseGameAfterAnimation()
    {
        anim.SetBool("isPause", true);
        cam.GetComponent<FreeFlyCamera>()._enableRotation = false;
        // Wait for the animation to complete
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        Time.timeScale = 0f; // Pause the game
    }

    private IEnumerator ResumeGameAfterAnimation()
    {
        // Wait for the animation to complete
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(1).length);
        cam.GetComponent<FreeFlyCamera>()._enableRotation = true;
        Time.timeScale = 1f; // Resume the game
    }

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
}
