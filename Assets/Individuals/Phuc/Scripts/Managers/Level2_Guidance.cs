using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Level2Guidance : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    public GameObject dialogueUI;

    [Header("Level 2 Dialogues")]
    public List<Dialogue> level2Dialogues;

    [Header("Scene Configuration")]
    [SerializeField] private SceneField level2Scene; // Assumed: "Level2"
    [SerializeField] private SceneField level3Scene; // Assumed: "Level3"

    private List<Dialogue.DialogueLine> currentDialogueLines;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private FreeFlyCamera _freeFlyCamera;

    public static Level2Guidance Instance;

    public enum SceneType
    {
        Level1,
        Level2,
        Level3
    }

    public SceneType currentSceneType = SceneType.Level2;
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
                Debug.LogWarning($"[Level2Guidance] Scene {currentSceneType} is not unlocked. Loading highest unlocked scene.");
                LoadHighestUnlockedScene();
            }
        }
        else
        {
            Debug.Log($"[Level2Guidance] Running in Editor: Allowing access to {currentSceneType}.");
        }
    }

    private void Start()
    {
        dialogueUI.SetActive(false);

        nextButton.onClick.AddListener(OnNextButtonClicked);
        _freeFlyCamera = FindObjectOfType<FreeFlyCamera>();

        SetupLevel2Scene();
    }

    private SceneType DetermineSceneType()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;

        if (level2Scene != null && currentSceneName == level2Scene.SceneName)
            return SceneType.Level2;

        Debug.LogWarning($"[Level2Guidance] Current scene '{currentSceneName}' not mapped. Defaulting to Level2.");
        return SceneType.Level2;
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
            Debug.LogError($"[Level2Guidance] Failed to load scene for {nextSceneType}. Scene name not found.");
        }
    }

    private string GetSceneName(SceneType sceneType)
    {
        switch (sceneType)
        {
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
            Debug.Log($"[Level2Guidance] Marked {sceneType} as completed. HighestCompletedLevel: {currentIndex}");
        }
    }

    private void SetupLevel2Scene()
    {
        Debug.Log("[Level2Guidance] Setting up Level 2 scene.");
        DisableMovements();
        SetDialogueSection("Level 2 Intro", () =>
        {
            Debug.Log("[Level2Guidance] Level 2 Intro dialogue completed.");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.startWaveButton.interactable = true;
                Debug.Log("[Level2Guidance] Enabled startWaveButton for Level 2.");
            }
            EnableMovements();
        });
    }

    public void SetDialogueSection(string sectionName, UnityAction onComplete)
    {
        Debug.Log($"[Level2Guidance] Setting dialogue section: {sectionName}");
        Dialogue dialogue = level2Dialogues?.Find(d => d.sectionName == sectionName);

        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            Debug.LogWarning($"[Level2Guidance] No dialogue found for section '{sectionName}' in Level 2 dialogues.");
            if (UIManager.Instance != null && UIManager.Instance.anim != null)
            {
                UIManager.Instance.anim.Play("hideUI");
                Debug.Log("[Level2Guidance] Played hideUI animation due to empty dialogue.");
            }
            else
            {
                dialogueUI.SetActive(false);
                Debug.LogWarning("[Level2Guidance] UIManager or anim is null. Hid dialogueUI directly.");
            }
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
            Debug.Log("[Level2Guidance] Reset Time.timeScale to 1 for dialogue.");
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
            if (UIManager.Instance != null && UIManager.Instance.anim != null)
            {
                UIManager.Instance.anim.Play("hideUI");
                Debug.Log("[Level2Guidance] Played hideUI animation at end of dialogue.");
            }
            else
            {
                dialogueUI.SetActive(false);
                Debug.LogWarning("[Level2Guidance] UIManager or anim is null. Hid dialogueUI directly.");
            }
            isDialogueActive = false;
            onComplete?.Invoke();
            EnableMovements();
            Debug.Log("[Level2Guidance] Dialogue section completed.");
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
            if (isDialogueActive && currentLineIndex >= currentDialogueLines.Count)
            {
                if (UIManager.Instance != null && UIManager.Instance.anim != null)
                {
                    UIManager.Instance.anim.Play("hideUI");
                    Debug.Log("[Level2Guidance] Played hideUI animation on extra Next press.");
                }
                else
                {
                    dialogueUI.SetActive(false);
                    Debug.LogWarning("[Level2Guidance] UIManager or anim is null. Hid dialogueUI directly on extra Next press.");
                }
                isDialogueActive = false;
                EnableMovements();
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.startWaveButton.interactable = true;
                    Debug.Log("[Level2Guidance] Enabled startWaveButton on extra Next press.");
                }
            }
            else if (isDialogueActive)
            {
                ShowCurrentLine(null);
            }
        }
    }

    private void EnableMovements()
    {
        if (_freeFlyCamera != null)
        {
            _freeFlyCamera._enableRotation = true;
            _freeFlyCamera._enableMovement = true;
            Debug.Log("[Level2Guidance] Enabled camera movements.");
        }
    }

    private void DisableMovements()
    {
        if (_freeFlyCamera != null)
        {
            _freeFlyCamera._enableRotation = false;
            _freeFlyCamera._enableMovement = false;
            Debug.Log("[Level2Guidance] Disabled camera movements.");
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

        if (Instance == this)
        {
            Instance = null;
            Destroy(gameObject);
        }
    }
}