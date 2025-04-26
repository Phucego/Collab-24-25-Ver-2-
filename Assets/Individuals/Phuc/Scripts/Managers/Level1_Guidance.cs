using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Level1Guidance : MonoBehaviour, IGuidance
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    public GameObject dialogueUI;

    [Header("Level 1 Dialogues")]
    public List<Dialogue> level1Dialogues;

    [Header("Animation Controller")]
    public Animator anim; // Assigned in Inspector, must have "hideUI" trigger

    [Header("Scene Configuration")]
    [SerializeField] private SceneField level1Scene; // Assumed: "Level1"
    [SerializeField] private SceneField level2Scene; // Assumed: "Level2"

    private List<Dialogue.DialogueLine> currentDialogueLines;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private FreeFlyCamera _freeFlyCamera;

    public static Level1Guidance Instance;

    public enum SceneType
    {
        Level1,
        Level2,
        Level3
    }

    public SceneType currentSceneType = SceneType.Level1;
    private const string PROGRESS_KEY = "HighestCompletedLevel";
    private static readonly SceneType[] SceneOrder = { SceneType.Level1, SceneType.Level2, SceneType.Level3 };

    private void Awake()
    {
        Instance = this;

        currentSceneType = DetermineSceneType();

        if (!Application.isEditor)
        {
            if (!IsSceneUnlocked(currentSceneType))
            {
                Debug.LogWarning($"[Level1Guidance] Scene {currentSceneType} is not unlocked. Loading highest unlocked scene.");
                LoadHighestUnlockedScene();
            }
        }
        else
        {
            Debug.Log($"[Level1Guidance] Running in Editor: Allowing access to {currentSceneType}.");
        }
    }

    private void Start()
    {
        dialogueUI.SetActive(false);

        nextButton.onClick.AddListener(OnNextButtonClicked);
        _freeFlyCamera = FindObjectOfType<FreeFlyCamera>();

        SetupLevel1Scene();
    }

    public Animator GetAnimator()
    {
        return anim;
    }

    private SceneType DetermineSceneType()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;

        if (level1Scene != null && currentSceneName == level1Scene.SceneName)
            return SceneType.Level1;

        Debug.LogWarning($"[Level1Guidance] Current scene '{currentSceneName}' not mapped. Defaulting to Level1.");
        return SceneType.Level1;
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
            Debug.LogError($"[Level1Guidance] Failed to load scene for {nextSceneType}. Scene name not found.");
        }
    }

    private string GetSceneName(SceneType sceneType)
    {
        switch (sceneType)
        {
            case SceneType.Level1:
                return level1Scene?.SceneName;
            case SceneType.Level2:
                return level2Scene?.SceneName;
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
            Debug.Log($"[Level1Guidance] Marked {sceneType} as completed. HighestCompletedLevel: {currentIndex}");
        }
    }

    private void SetupLevel1Scene()
    {
        Debug.Log("[Level1Guidance] Setting up Level 1 scene.");
        DisableMovements();
        SetDialogueSection("Level 1 Intro", () =>
        {
            Debug.Log("[Level1Guidance] Level 1 Intro dialogue completed.");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.startWaveButton.interactable = true;
                Debug.Log("[Level1Guidance] Enabled startWaveButton for Level 1.");
            }
            dialogueUI.SetActive(false);
            EnableMovements();
        });
    }

    public void SetDialogueSection(string sectionName, UnityAction onComplete)
    {
        Debug.Log($"[Level1Guidance] Setting dialogue section: {sectionName}");
        Dialogue dialogue = level1Dialogues?.Find(d => d.sectionName == sectionName);

        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            Debug.LogWarning($"[Level1Guidance] No dialogue found for section '{sectionName}' in Level 1 dialogues.");
            dialogueUI.SetActive(false);
            onComplete?.Invoke();
            EnableMovements();
            return;
        }

        // Reset timescale to 1 if sped up
        if (UIManager.Instance != null && UIManager.Instance.isSpeedUp)
        {
            UIManager.Instance.isSpeedUp = false;
            Time.timeScale = 1f;
            AudioManager.Instance?.PlaySoundEffect("SlowDown_SFX");
            UIManager.Instance.anim.SetTrigger("isSpeedChange");
            Debug.Log("[Level1Guidance] Reset Time.timeScale to 1 for dialogue.");
        }

        currentDialogueLines = dialogue.dialogueLines;
        currentLineIndex = 0;
        isDialogueActive = true;
        dialogueUI.SetActive(true);

        ShowCurrentLine(onComplete);
    }

    private void ShowCurrentLine(UnityAction onComplete)
    {
        if (currentLineIndex < currentDialogueLines.Count)
        {
            DisplayLine(currentDialogueLines[currentLineIndex]);
        }
        else
        {
            if (anim != null)
                anim.SetTrigger("hideUI"); // Trigger hideUI animation
            dialogueUI.SetActive(false);
            isDialogueActive = false;
            onComplete?.Invoke();
            EnableMovements();
            Debug.Log("[Level1Guidance] Dialogue section completed. Hiding dialogue UI.");
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
            if (isDialogueActive)
                ShowCurrentLine(null);
        }
    }

    private void EnableMovements()
    {
        if (_freeFlyCamera != null)
        {
            _freeFlyCamera._enableRotation = true;
            _freeFlyCamera._enableMovement = true;
            Debug.Log("[Level1Guidance] Enabled camera movements.");
        }
    }

    private void DisableMovements()
    {
        if (_freeFlyCamera != null)
        {
            _freeFlyCamera._enableRotation = false;
            _freeFlyCamera._enableMovement = false;
            Debug.Log("[Level1Guidance] Disabled camera movements.");
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
        anim = null;

        if (Instance == this)
        {
            Instance = null;
            Destroy(gameObject);
        }
    }
}