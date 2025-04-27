using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TutorialGuidance : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    public GameObject dialogueUI;

    [Header("Tutorial Dialogues")]
    public List<Dialogue> tutorialDialogues;

    [Header("Level 1 dialogues")]
    public List<Dialogue> level1Dialogues;

    [Header("Level 2 dialogues")]
    public List<Dialogue> level2Dialogues;

    [Header("Level 3 dialogues")]
    public List<Dialogue> level3Dialogues;

    public UnityEvent OnIntroCompleted;
    public UnityEvent OnMovementPracticeCompleted;
    public UnityEvent OnBuildingCompleted;
    public UnityEvent OnCorruptedIntroCompleted;
    public UnityEvent OnTowerDamaged;

    [Header("Scene Configuration")]
    [SerializeField] private SceneField tutorialScene;
    [SerializeField] private SceneField level1Scene;
    [SerializeField] private SceneField level2Scene;
    [SerializeField] private SceneField level3Scene;

    [Header("Tutorial Destinations")]
    public GameObject buildingDestination;
    public GameObject targetDestination;
    public GameObject corruptedIntroDestination;

    private List<Dialogue.DialogueLine> currentDialogueLines;
    private int currentLineIndex = 0;
    private bool isTutorialActive = false;

    [SerializeField] private bool movementDetected = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private FreeFlyCamera _freeFlyCamera;

    private bool hasReachedTargetDestination = false;
    private bool hasReachedBuildingDestination = false;
    private bool hasReachedCorruptedZone = false;
    private bool firstTimeTowerDamaged = false;
    private bool isTowerPlacementChecked = false;

    private bool hasShownPostFirstWaveDialogue = false;
    private bool hasShownPostSecondWaveDialogue = false;
    private bool hasShownPostThirdWaveDialogue = false;
    private bool corruptedIntroCompleted = false;

    public UIManager _uimManager;
    [SerializeField] private BuildingManager buildingManager;

    private bool startWaveLocked = true;

    public static TutorialGuidance _instance;

    public enum SceneType
    {
        Tutorial,
        Level1,
        Level2,
        Level3
    }

    public SceneType currentSceneType;
    private Dictionary<SceneType, Action> sceneSetupActions;

    private const string PROGRESS_KEY = "HighestCompletedLevel";
    private static readonly SceneType[] SceneOrder = { SceneType.Tutorial, SceneType.Level1, SceneType.Level2, SceneType.Level3 };

    private void Awake()
    {
        _instance = this;

        sceneSetupActions = new Dictionary<SceneType, Action>
        {
            { SceneType.Tutorial, SetupTutorialScene },
            { SceneType.Level1, SetupLevel1Scene },
            { SceneType.Level2, SetupLevel2Scene },
            { SceneType.Level3, SetupLevel3Scene }
        };

        currentSceneType = DetermineSceneType();

        if (!Application.isEditor && !IsSceneUnlocked(currentSceneType))
        {
            LoadHighestUnlockedScene();
        }
    }

    void Start()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
        
        if (currentSceneType == SceneType.Tutorial)
        {
            if (buildingDestination != null) buildingDestination.SetActive(false);
            if (corruptedIntroDestination != null) corruptedIntroDestination.SetActive(false);
            if (targetDestination != null) targetDestination.SetActive(true);
            // Disable Start Wave button initially for Tutorial
            if (UIManager.Instance != null && UIManager.Instance.startWaveButton != null)
            {
                UIManager.Instance.startWaveButton.interactable = false;
                UIManager.Instance.startWaveButton.gameObject.SetActive(true); // Ensure button is visible but not interactable
            }
            startWaveLocked = true;
        }
        else
        {
            if (buildingDestination != null) buildingDestination.SetActive(false);
            if (corruptedIntroDestination != null) corruptedIntroDestination.SetActive(false);
            if (targetDestination != null) targetDestination.SetActive(false);
            // Enable Start Wave button immediately for other levels
            if (UIManager.Instance != null && UIManager.Instance.startWaveButton != null)
                UIManager.Instance.startWaveButton.interactable = true;
            EnableMovements();
        }

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);

        _freeFlyCamera = FindObjectOfType<FreeFlyCamera>();
        buildingManager = FindObjectOfType<BuildingManager>();

        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete += HandleWaveCompleted;

        if (sceneSetupActions.ContainsKey(currentSceneType))
            sceneSetupActions[currentSceneType]?.Invoke();
    }

    private SceneType DetermineSceneType()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;

        if (tutorialScene != null && !string.IsNullOrEmpty(tutorialScene.SceneName) && currentSceneName == tutorialScene.SceneName)
            return SceneType.Tutorial;
        if (level1Scene != null && !string.IsNullOrEmpty(level1Scene.SceneName) && currentSceneName == level1Scene.SceneName)
            return SceneType.Level1;
        if (level2Scene != null && !string.IsNullOrEmpty(level2Scene.SceneName) && currentSceneName == level2Scene.SceneName)
            return SceneType.Level2;
        if (level3Scene != null && !string.IsNullOrEmpty(level3Scene.SceneName) && currentSceneName == level3Scene.SceneName)
            return SceneType.Level3;

        return SceneType.Tutorial;
    }

    private bool IsSceneUnlocked(SceneType sceneType)
    {
        int highestCompletedIndex = PlayerPrefs.GetInt(PROGRESS_KEY, -1);
        int currentSceneIndex = Array.IndexOf(SceneOrder, sceneType);
        return currentSceneIndex <= highestCompletedIndex + 1;
    }

    private void LoadHighestUnlockedScene()
    {
        int highestCompletedIndex = PlayerPrefs.GetInt(PROGRESS_KEY, -1);
        int nextSceneIndex = highestCompletedIndex + 1;
        if (nextSceneIndex >= SceneOrder.Length)
            nextSceneIndex = SceneOrder.Length - 1;

        SceneType nextSceneType = SceneOrder[nextSceneIndex];
        string sceneName = GetSceneName(nextSceneType);
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }

    private string GetSceneName(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Tutorial:
                return tutorialScene != null && !string.IsNullOrEmpty(tutorialScene.SceneName) ? tutorialScene.SceneName : null;
            case SceneType.Level1:
                return level1Scene != null && !string.IsNullOrEmpty(level1Scene.SceneName) ? level1Scene.SceneName : null;
            case SceneType.Level2:
                return level2Scene != null && !string.IsNullOrEmpty(level2Scene.SceneName) ? level2Scene.SceneName : null;
            case SceneType.Level3:
                return level3Scene != null && !string.IsNullOrEmpty(level3Scene.SceneName) ? level3Scene.SceneName : null;
            default:
                return null;
        }
    }

    public void CompleteScene(SceneType sceneType)
    {
        int currentIndex = Array.IndexOf(SceneOrder, sceneType);
        int highestCompletedIndex = PlayerPrefs.GetInt(PROGRESS_KEY, -1);

        if (currentIndex > highestCompletedIndex)
        {
            PlayerPrefs.SetInt(PROGRESS_KEY, currentIndex);
            PlayerPrefs.Save();
        }
    }

    private void SetupTutorialScene()
    {
        StartIntro();
    }

    private void SetupLevel1Scene()
    {
        SetDialogueSection("Level 1 Intro", () =>
        {
            EnableStartWave();
            EnableMovements();
            if (UIManager.Instance != null)
                UIManager.Instance.ResetWaveState();
        });
    }

    private void SetupLevel2Scene()
    {
        SetDialogueSection("Level 2 Intro", () =>
        {
            EnableStartWave();
            EnableMovements();
            if (UIManager.Instance != null)
                UIManager.Instance.ResetWaveState();
        });
    }

    private void SetupLevel3Scene()
    {
        EnableStartWave();
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
        EnableMovements();
    }

    private void Update()
    {
        if (currentSceneType != SceneType.Tutorial) return;

        if (isTutorialActive && currentDialogueLines != null && currentDialogueLines.Count > 0)
        {
            if (currentDialogueLines[currentLineIndex].characterName == "Introduction" && !isTowerPlacementChecked)
            {
                if (buildingManager != null && buildingManager.HasPlacedFirstTower())
                {
                    isTowerPlacementChecked = true;
                    currentLineIndex++;
                    if (isTutorialActive)
                        ShowCurrentLine(null);
                }
            }

            if (currentDialogueLines[currentLineIndex].characterName == "Introduction")
            {
                movementDetected = true;
            }
        }
    }

    private void StartIntro()
    {
        isTowerPlacementChecked = false;
        DisableMovements();
        SetDialogueSection("Introduction", () =>
        {
            Debug.Log("Introduction dialogue completed");
            OnIntroCompleted.Invoke();
        });
    }

    public void SetDialogueSection(string sectionName, UnityAction onComplete)
    {
        Dialogue dialogue = null;
        List<Dialogue> activeDialogues = GetDialogueListForCurrentScene();
        
        if (activeDialogues != null)
            dialogue = activeDialogues.Find(d => d.sectionName == sectionName);
       
        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            if (dialogueUI != null)
                dialogueUI.SetActive(false);
            onComplete?.Invoke();
            if (currentSceneType == SceneType.Tutorial || currentSceneType == SceneType.Level1)
            {
                if (!startWaveLocked)
                    EnableStartWave();
                EnableMovements();
            }
            return;
        }

        if (UIManager.Instance != null && UIManager.Instance.isSpeedUp)
        {
            UIManager.Instance.isSpeedUp = false;
            Time.timeScale = 1f;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySoundEffect("SlowDown_SFX");
            if (UIManager.Instance != null && UIManager.Instance.anim != null)
                UIManager.Instance.anim.SetTrigger("isSpeedChange");
            UIManager.Instance.anim.SetTrigger("hideUI");
        }

        currentDialogueLines = dialogue.dialogueLines;
        currentLineIndex = 0;
        isTutorialActive = true;
        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        ShowCurrentLine(onComplete);
    }

    private List<Dialogue> GetDialogueListForCurrentScene()
    {
        switch (currentSceneType)
        {
            case SceneType.Tutorial:
                return tutorialDialogues;
            case SceneType.Level1:
                return level1Dialogues;
            case SceneType.Level2:
                return level2Dialogues;
            case SceneType.Level3:
                return level3Dialogues;
            default:
                return null;
        }
    }

    private void ShowCurrentLine(UnityAction onComplete)
    {
        if (currentLineIndex < currentDialogueLines.Count)
        {
            DisplayLine(currentDialogueLines[currentLineIndex]);
        }
        else
        {
            if (dialogueUI != null)
                dialogueUI.SetActive(false);
            isTutorialActive = false;
            Debug.Log($"Dialogue section completed. startWaveLocked: {startWaveLocked}, Scene: {currentSceneType}");
            onComplete?.Invoke();
            if (currentSceneType == SceneType.Tutorial)
            {
                // Only enable start wave if explicitly allowed by callback
                // startWaveLocked is managed by the callback
            }
            else
            {
                EnableStartWave();
                EnableMovements();
            }
        }
    }

    private void DisplayLine(Dialogue.DialogueLine line)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = true;
        if (characterNameText != null)
            characterNameText.text = line.characterName;
        if (dialogueText != null)
            dialogueText.text = "";
        typingCoroutine = StartCoroutine(TypeSentence(line.dialogueText));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText == null)
        {
            isTyping = false;
            yield break;
        }

        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.02f);
        }

        isTyping = false;
    }

    private void OnNextButtonClicked()
    {
        if (isTyping && UIManager.Instance != null && !UIManager.Instance.pauseMenu.activeSelf)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (dialogueText != null && currentDialogueLines != null && currentLineIndex < currentDialogueLines.Count)
                dialogueText.text = currentDialogueLines[currentLineIndex].dialogueText;
            isTyping = false;
        }
        else
        {
            currentLineIndex++;
            if (isTutorialActive && currentLineIndex >= currentDialogueLines.Count)
            {
                if (dialogueUI != null)
                    dialogueUI.SetActive(false);
                isTutorialActive = false;
                
                UIManager.Instance.anim.SetTrigger("isStart");

                EnableMovements();
                Debug.Log($"Last dialogue line completed. startWaveLocked: {startWaveLocked}, Scene: {currentSceneType}");
                
            }
            else if (isTutorialActive)
            {
                ShowCurrentLine(null);
            }
        }
    }

    public void StartMovementPractice()
    {
        if (currentSceneType != SceneType.Tutorial) return;
        SetDialogueSection("Camera Practice", () =>
        {
            OnMovementPracticeCompleted.Invoke();
            startWaveLocked = false;
            EnableStartWave();
            Debug.Log("Movement practice completed, enabling start wave");
        });
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
        if (buildingDestination != null)
            buildingDestination.SetActive(true);
    }

    public void StartBuildingTutorial()
    {
        if (currentSceneType != SceneType.Tutorial) return;
        SetDialogueSection("Building", () =>
        {
            OnBuildingCompleted.Invoke();
            startWaveLocked = false;
            EnableStartWave();
            Debug.Log("Building tutorial completed, enabling start wave");
        });
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.currentCurrency = 80;
    }

    public void StartCorruptedIntro()
    {
        if (currentSceneType != SceneType.Tutorial) return;
        SetDialogueSection("Corrupted Zone Intro", () =>
        {
            OnCorruptedIntroCompleted.Invoke();
            startWaveLocked = false;
 
            Debug.Log("Corrupted intro completed, enabling start wave");
        });
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
        EnableStartWave();
        corruptedIntroCompleted = true;
    }

    private void EnableStartWave()
    {
        startWaveLocked = false;
        if (UIManager.Instance != null && UIManager.Instance.startWaveButton != null)
            UIManager.Instance.startWaveButton.interactable = true;
    }

    private void DisableStartWave()
    {
        startWaveLocked = true;
        if (UIManager.Instance != null && UIManager.Instance.startWaveButton != null)
            UIManager.Instance.startWaveButton.interactable = false;
    }

    private void HandleWaveCompleted()
    {
        if (WaveManager.Instance == null || UIManager.Instance == null)
            return;

        int waveIndex = UIManager.Instance.currentWave;

        if (currentSceneType == SceneType.Tutorial)
        {
            DisableStartWave(); // Disable start wave button before showing dialogues
            EnableMovements();

            if (!hasShownPostFirstWaveDialogue && waveIndex == 0)
            {
                hasShownPostFirstWaveDialogue = true;
                DisableMovements();
                SetDialogueSection("Post First Wave", () =>
                {
                   
                    EnableStartWave();
                    EnableMovements();
                   
                    Debug.Log("Post First Wave dialogue completed, enabling start wave");
                });
            }
            else if (!hasShownPostSecondWaveDialogue && waveIndex == 1)
            {
                hasShownPostSecondWaveDialogue = true;
                DisableMovements();
                SetDialogueSection("Post Second Wave", () =>
                {
                    EnableMovements();
                    EnableStartWave();
                    Debug.Log("Post Second Wave dialogue completed, enabling start wave");
                });
            }
            else if (!hasShownPostThirdWaveDialogue && waveIndex == 2)
            {
                hasShownPostThirdWaveDialogue = true;
                DisableMovements();
                SetDialogueSection("Post Third Wave", () =>
                {
                    EnableStartWave();
                    EnableMovements();
                    Debug.Log("Post Third Wave dialogue completed, enabling start wave");
                });
               
            }
        }
        else if (currentSceneType == SceneType.Level1)
        {
            EnableMovements();

            if (!hasShownPostFirstWaveDialogue && waveIndex == 0)
            {
                hasShownPostFirstWaveDialogue = true;
                DisableMovements();
                SetDialogueSection("Post Wave 1", () =>
                {
                    EnableStartWave();
                    EnableMovements();
                });
            }
            else if (!hasShownPostSecondWaveDialogue && waveIndex == 1)
            {
                hasShownPostSecondWaveDialogue = true;
                DisableMovements();
                SetDialogueSection("Post Wave 2", () =>
                {
                    EnableStartWave();
                    EnableMovements();
                });
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentSceneType != SceneType.Tutorial) return;

        switch (other.gameObject)
        {
            case GameObject obj when obj == targetDestination && !hasReachedTargetDestination:
                hasReachedTargetDestination = true;
                DisableMovements();
                StartMovementPractice();
                if (UIManager.Instance != null && UIManager.Instance.anim != null)
                    UIManager.Instance.anim.SetTrigger("hideUI");
                if (targetDestination != null) Destroy(targetDestination);
                break;

            case GameObject obj when obj == buildingDestination && !hasReachedBuildingDestination:
                hasReachedBuildingDestination = true;
                DisableMovements();
                StartBuildingTutorial();
                if (UIManager.Instance != null && UIManager.Instance.anim != null)
                    UIManager.Instance.anim.SetTrigger("hideUI");
                if (buildingDestination != null) Destroy(buildingDestination);
                break;

            case GameObject obj when obj == corruptedIntroDestination && !hasReachedCorruptedZone:
                hasReachedCorruptedZone = true;
                StartCorruptedIntro();
                DisableMovements();
                if (UIManager.Instance != null && UIManager.Instance.anim != null)
                    UIManager.Instance.anim.SetTrigger("hideUI");
                if (corruptedIntroDestination != null) Destroy(corruptedIntroDestination);
                break;
        }
    }

    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= HandleWaveCompleted;
    }

    private void EnableMovements()
    {
        if (_freeFlyCamera != null)
        {
            _freeFlyCamera._enableRotation = true;
            _freeFlyCamera._enableMovement = true;
        }
    }

    private void DisableMovements()
    {
        if (_freeFlyCamera != null)
        {
            _freeFlyCamera._enableRotation = false;
            _freeFlyCamera._enableMovement = false;
        }
    }

    public void Cleanup()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentDialogueLines = null;
        characterNameText = null;
        dialogueText = null;
        nextButton = null;
        dialogueUI = null;

        if (_instance == this)
        {
            _instance = null;
            Destroy(gameObject);
        }
    }
}