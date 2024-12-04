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


    [Header("Scenes To Load")] 
    [SerializeField]
    private SceneField _tutorialScene;

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        Time.timeScale = 1f;
        anim = GetComponent<Animator>();

        //loadingBarOBJ.SetActive(false);
        levelSelectionCanvas.SetActive(false);

        startButton.onClick.AddListener(MainMenuOut);
        backButton.onClick.AddListener(MainMenuIn);
        quitButton.onClick.AddListener(QuitGame);

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
       
        mainMenuCanvas.SetActive(false);
    }

    public void MainMenuIn()
    {
        anim.SetBool("fromMenu", false);
      
        mainMenuCanvas.SetActive(true);
      
        levelSelectionCanvas.SetActive(false);
    
    }



    public void QuitGame()
    {
        Application.Quit();
    }
}
