using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    /* public GameObject loadingBarOBJ;

     public Image _loadingBar;*/
    Animator anim;
  
    public GameObject levelSelectionCanvas;
    public GameObject mainMenuCanvas;
   
    public TextMeshProUGUI gameName;


    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public Button backButton;
    public Button Quit_Yes;
    public Button Quit_No;

    public GameObject confirmMenuCanvas;


    [Header("Scenes To Load")] 
    [SerializeField]
    private SceneField _tutorialScene;

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();

        confirmMenuCanvas.SetActive(false); 

        //loadingBarOBJ.SetActive(false);
        levelSelectionCanvas.SetActive(false);

        startButton.onClick.AddListener(MainMenuOut);
        backButton.onClick.AddListener(MainMenuIn);
        quitButton.onClick.AddListener(QuitGame);
        Quit_Yes.onClick.AddListener(OnConfirmQuit);
        Quit_No.onClick.AddListener(OnConfirmBack);
    }
    public void LoadTutorialScene()
    {
        SceneManager.LoadSceneAsync(_tutorialScene);
    }
  

    private IEnumerator LoadingBarProgress()
    {
        float loadProgress = 0f;
        for (int i = 0; i < _scenesToLoad.Count;  i++)
        {
            while (!_scenesToLoad[i].isDone)
            {
                loadProgress = _scenesToLoad[i].progress;
                //_loadingBar.fillAmount = loadProgress / _scenesToLoad.Count;
                yield return null;
            }
        }
    }

    public void MainMenuOut()
    {
        anim.SetBool("fromMenu", true);
        levelSelectionCanvas.SetActive(true);
        //mainMenuCanvas.SetActive(false);
    }

    public void MainMenuIn()
    {
        anim.SetBool("fromMenu", false);
      
        mainMenuCanvas.SetActive(true);
      
        levelSelectionCanvas.SetActive(false);
    
    }

    #region Confirmation Menu
    private void OnConfirmQuit()
    {
        Debug.Log("Quit");
        Application.Quit();

    }
    private void OnConfirmBack()
    {
        anim.SetBool("isConfirmationMenu", false);
    }

    #endregion

    public void QuitGame()
    {
        anim.SetBool("isConfirmationMenu", true);
    }
}
