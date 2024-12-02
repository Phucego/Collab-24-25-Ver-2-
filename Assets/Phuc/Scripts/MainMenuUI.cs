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
    public GameObject[] objToHide;
    public GameObject levelSelectionCanvas;
    public GameObject mainMenuCanvas;
    public TextMeshProUGUI gameName;


    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
  

    [Header("Scenes To Load")] 
    [SerializeField]
    private string _tutorialScene = "TutorialScene";

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        anim = GetComponent<Animator>();

        //loadingBarOBJ.SetActive(false);
        levelSelectionCanvas.SetActive(false);
    }

    public void ShowLevelSelection()
    {
       
        //loadingBarOBJ.SetActive(true);
        StartCoroutine(MainMenuOut());
    
       // _scenesToLoad.Add(SceneManager.LoadSceneAsync(_tutorialScene));
        StartCoroutine(LoadingBarProgress());
       
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

    IEnumerator MainMenuOut()
    {
        anim.SetBool("fromMenu", true);
        levelSelectionCanvas.SetActive(true);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        mainMenuCanvas.SetActive(false);
    }

    IEnumerator MainMenuIn()
    {
        anim.SetBool("fromMenu", false);
      
        mainMenuCanvas.SetActive(true);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        levelSelectionCanvas.SetActive(false);
    
    }


    public void GetBackToMainMenu()
    {
        StartCoroutine(MainMenuIn());
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
