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

    [Header("Level 1 Dialogues")]
    public List<Dialogue> level1Dialogues;

    [Header("Level 2 Dialogues")]
    public List<Dialogue> level2Dialogues;

    [Header("Level 3 Dialogues")]
    public List<Dialogue> level3Dialogues;

    [Header("Animation Controller")]
    public Animator anim;

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
    private bool corruptedIntroCompleted = false;

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

        if (!Application.isEditor)
        {
            if (!IsSceneUnlocked(currentSceneType))
            {
                Debug.LogWarning($"Scene {currentSceneType} is not unlocked. Loading highest unlocked scene.");
                LoadHighestUnlockedScene();
            }
        }
        else
        {
            Debug.Log($"Running in Editor: Allowing access to {currentSceneType} regardless of progression.");
        }
    }

    void Start()
    {
        dialogueUI.SetActive(false);

        // Only manage destinations in Tutorial scene
        if (currentSceneType == SceneType.Tutorial)
        {
            if (buildingDestination != null) buildingDestination.SetActive(false);
            if (corruptedIntroDestination != null) corruptedIntroDestination.SetActive(false);
            if (targetDestination != null) targetDestination.SetActive(true); // Enable first destination
        }
        else
        {
            // Deactivate destinations in non-Tutorial scenes
            if (buildingDestination != null) buildingDestination.SetActive(false);
            if (corruptedIntroDestination != null) corruptedIntroDestination.SetActive(false);
            if (targetDestination != null) targetDestination.SetActive(false);
        }

        nextButton.onClick.AddListener(OnNextButtonClicked);

        _freeFlyCamera = FindObjectOfType<FreeFlyCamera>();
        buildingManager = FindObjectOfType<BuildingManager>();

        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete += HandleWaveCompleted;

        if (sceneSetupActions.ContainsKey(currentSceneType))
        {
            sceneSetupActions[currentSceneType]?.Invoke();
        }
        else
        {
            Debug.LogWarning($"No setup defined for scene type: {currentSceneType}");
        }
    }

    private SceneType DetermineSceneType()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;

        if (tutorialScene != null && currentSceneName == tutorialScene.SceneName)
            return SceneType.Tutorial;
        else if (level1Scene != null && currentSceneName == level1Scene.SceneName)
            return SceneType.Level1;
        else if (level2Scene != null && currentSceneName == level2Scene.SceneName)
            return SceneType.Level2;
        else if (level3Scene != null && currentSceneName == level3Scene.SceneName)
            return SceneType.Level3;

        Debug.LogWarning($"Current scene '{currentSceneName}' not mapped to any SceneType. Defaulting to Tutorial.");
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
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Failed to load scene for {nextSceneType}. Scene name not found.");
        }
    }

    private string GetSceneName(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Tutorial:
                return tutorialScene?.SceneName;
            case SceneType.Level1:
                return level1Scene?.SceneName;
            case SceneType.Level2:
                return level2Scene?.SceneName;
            case SceneType.Level3:
                return level3Scene?.SceneName;
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
        if (UIManager.Instance != null)
            UIManager.Instance.startWaveButton.interactable = false;

        StartIntro();
    }

    private void SetupLevel1Scene()
    {
        SetDialogueSection("Level 1 Intro", () =>
        {
            startWaveLocked = false;
            if (UIManager.Instance != null)
                UIManager.Instance.startWaveButton.interactable = true;
            EnableMovements();
        });
    }

    private void SetupLevel2Scene()
    {
        SetDialogueSection("Level 2 Intro", () =>
        {
            startWaveLocked = false;
            if (UIManager.Instance != null)
                UIManager.Instance.startWaveButton.interactable = true;
            EnableMovements();
        });
    }

    private void SetupLevel3Scene()
    {
        startWaveLocked = false;
        if (UIManager.Instance != null)
            UIManager.Instance.startWaveButton.interactable = true;
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
        SetDialogueSection("Introduction", OnIntroCompleted.Invoke);
    }

    public void SetDialogueSection(string sectionName, UnityAction onComplete)
    {
        Dialogue dialogue = null;
        List<Dialogue> activeDialogues = GetDialogueListForCurrentScene();

        if (activeDialogues != null)
        {
            dialogue = activeDialogues.Find(d => d.sectionName == sectionName);
        }

        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            Debug.LogWarning($"No dialogue found for section '{sectionName}' in {currentSceneType} dialogues.");
            onComplete?.Invoke();
            if (currentSceneType == SceneType.Tutorial)
            {
                EnableMovements();
            }
            return;
        }

        currentDialogueLines = dialogue.dialogueLines;
        currentLineIndex = 0;
        isTutorialActive = true;
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
                Debug.LogWarning($"No dialogue list defined for scene type: {currentSceneType}");
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
            anim.SetTrigger("isStart");
            dialogueUI.SetActive(false);
            isTutorialActive = false;
            onComplete?.Invoke();
            if (currentSceneType == SceneType.Tutorial)
            {
                EnableMovements();
            }
        }
    }

    private void DisplayLine(Dialogue.DialogueLine line)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        isTyping = true;
        characterNameText.text = line.characterName;
        typingCoroutine = StartCoroutine(TypeSentence(line.dialogueText));
    }

    private IEnumerator TypeSentence(string sentence)
    {
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
        if (isTyping && !UIManager.Instance.pauseMenu.activeSelf)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = currentDialogueLines[currentLineIndex].dialogueText;
            isTyping = false;
        }
        else
        {
            currentLineIndex++;
            if (isTutorialActive)
                ShowCurrentLine(null);
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
                if (targetDestination != null) Destroy(targetDestination);
                break;

            case GameObject obj when obj == buildingDestination && !hasReachedBuildingDestination:
                hasReachedBuildingDestination = true;
                DisableMovements();
                StartBuildingTutorial();
                if (buildingDestination != null) Destroy(buildingDestination);
                break;

            case GameObject obj when obj == corruptedIntroDestination && !hasReachedCorruptedZone:
                hasReachedCorruptedZone = true;
                StartCorruptedIntro();
                DisableMovements();
                if (corruptedIntroDestination != null) Destroy(corruptedIntroDestination);
                break;
        }
    }

    public void StartMovementPractice()
    {
        if (currentSceneType != SceneType.Tutorial) return;
        SetDialogueSection("Camera Practice", OnMovementPracticeCompleted.Invoke);
        anim.SetTrigger("hideUI");
        if (buildingDestination != null) buildingDestination.SetActive(true);
    }

    public void StartBuildingTutorial()
    {
        if (currentSceneType != SceneType.Tutorial) return;
        SetDialogueSection("Building", OnBuildingCompleted.Invoke);
        anim.SetTrigger("hideUI");
        CurrencyManager.Instance.currentCurrency = 80;
    }

    public void StartCorruptedIntro()
    {
        if (currentSceneType != SceneType.Tutorial) return;
        EnableStartWave();
        SetDialogueSection("Corrupted Zone Intro", OnCorruptedIntroCompleted.Invoke);
        anim.SetTrigger("hideUI");
        corruptedIntroCompleted = true;
    }

    private void EnableStartWave()
    {
        startWaveLocked = false;
        if (UIManager.Instance != null && UIManager.Instance.startWaveButton != null)
            UIManager.Instance.startWaveButton.interactable = true;
    }

    private void HandleWaveCompleted()
    {
        if (WaveManager.Instance == null || UIManager.Instance == null)
            return;

        int waveIndex = UIManager.Instance.currentWave;

        if (currentSceneType == SceneType.Tutorial)
        {
            if (!hasShownPostFirstWaveDialogue && waveIndex == 0)
            {
                hasShownPostFirstWaveDialogue = true;
                DisableMovements();
                anim.SetTrigger("hideUI");
                SetDialogueSection("Post First Wave", null);
            }
            else if (!hasShownPostSecondWaveDialogue && waveIndex == 1)
            {
                hasShownPostSecondWaveDialogue = true;
                DisableMovements();
                anim.SetTrigger("hideUI");
                SetDialogueSection("Post Second Wave", null);
            }
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
        {
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
        }

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