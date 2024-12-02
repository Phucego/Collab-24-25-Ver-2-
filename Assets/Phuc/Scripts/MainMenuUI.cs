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
    public GameObject loadingBarOBJ;

    public Image _loadingBar;
    public GameObject[] objToHide;
    public GameObject levelSelectionCanvas;
    public TextMeshProUGUI gameName;
    
    [Header("Scenes To Load")] 
    [SerializeField]
    private string _tutorialScene = "TutorialScene";

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        loadingBarOBJ.SetActive(false);
        levelSelectionCanvas.SetActive(false);

    }

    public void ShowLevelSelection()
    {
        HideMenu();
        loadingBarOBJ.SetActive(true);
        levelSelectionCanvas.SetActive(true);
   
        _scenesToLoad.Add(SceneManager.LoadSceneAsync(_tutorialScene));

        StartCoroutine(LoadingBarProgress());
    }

    public void LoadTutorialScene()
    {
        SceneManager.LoadSceneAsync(_tutorialScene);
    }
    void HideMenu()
    {
        for (int i = 0; i < objToHide.Length;  i++)
        {
            objToHide[i].SetActive(false);
        }
    }

    private IEnumerator LoadingBarProgress()
    {
        float loadProgress = 0f;
        for (int i = 0; i < _scenesToLoad.Count;  i++)
        {
            while (!_scenesToLoad[i].isDone)
            {
                loadProgress = _scenesToLoad[i].progress;
                _loadingBar.fillAmount = loadProgress / _scenesToLoad.Count;
                yield return null;
            }
        }
    }
}
