using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class Level1_Guidance : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;
    public GameObject dialogueUI;

    [Header("Tutorial Data")]
    public List<Dialogue> dialogueScriptables;

    [Header("Animation Controller")]
    public Animator anim;

    private Level1_Guidance Instance;
    // EVENTS
  
    private List<Dialogue.DialogueLine> currentDialogueLines;
    private int currentLineIndex = 0;
    private bool isTutorialActive = false;

    [SerializeField] private bool movementDetected = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    
    //WAVE DIALOGUES
    private bool hasShownPostFirstWaveDialogue;
    private bool hasShownPostSecondWaveDialogue;

    private FreeFlyCamera _freeFlyCamera;
    
    [SerializeField] private BuildingManager buildingManager;
    
    
    void Start()
    {
        dialogueUI.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClicked);

        _freeFlyCamera = FindObjectOfType<FreeFlyCamera>();
        buildingManager = FindObjectOfType<BuildingManager>();

        WaveManager.Instance.OnWaveComplete += HandleWaveCompleted;
        

        StartIntro();
    }
    private void HandleWaveCompleted()
    {
        if (WaveManager.Instance == null || UIManager.Instance == null)
            return;

        int waveIndex = UIManager.Instance.currentWave;

        // First wave completed (index 0)
        if (!hasShownPostFirstWaveDialogue && waveIndex == 0)
        {
            hasShownPostFirstWaveDialogue = true;
            DisableMovements();
            anim.SetTrigger("hideUI");
            SetDialogueSection("Post First Wave", null);
        }

        // Second wave completed (index 1)
        else if (!hasShownPostSecondWaveDialogue && waveIndex == 1)
        {
            hasShownPostSecondWaveDialogue = true;
            DisableMovements();
            anim.SetTrigger("hideUI");
            SetDialogueSection("Post Second Wave", null);
        }
    }
    

    private void StartIntro()
    {
        DisableMovements();
        SetDialogueSection("Level 1 Start", null);
    }

    public void SetDialogueSection(string sectionName, UnityAction onComplete)
    {
        Dialogue dialogue = dialogueScriptables.Find(d => d.sectionName == sectionName);
        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            Debug.LogWarning($"No dialogue found for section: {sectionName}");
            onComplete?.Invoke();
            return;
        }

        currentDialogueLines = dialogue.dialogueLines;
        currentLineIndex = 0;
        isTutorialActive = true;
        dialogueUI.SetActive(true);

        ShowCurrentLine(onComplete);
    }
    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= HandleWaveCompleted;
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
            EnableMovements();
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
    
    private void EnableMovements()
    {
        _freeFlyCamera._enableRotation = true;
        _freeFlyCamera._enableMovement = true;
    }

    private void DisableMovements()
    {
        _freeFlyCamera._enableRotation = false;
        _freeFlyCamera._enableMovement = false;
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

        // Clear references
        currentDialogueLines = null;
        characterNameText = null;
        dialogueText = null;
        nextButton = null;
        dialogueUI = null;

        // Destroy the GameObject
        if (Instance == this)
        {
            Instance = null;
            Destroy(gameObject);
        }
    }
}
