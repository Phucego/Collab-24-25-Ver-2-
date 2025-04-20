using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class TutorialGuidance : MonoBehaviour
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
    private TutorialGuidance Instance;
    // EVENTS
    public UnityEvent OnIntroCompleted;
    public UnityEvent OnMovementPracticeCompleted;
    public UnityEvent OnBuildingCompleted;
    public UnityEvent OnCorruptedIntroCompleted;
    public UnityEvent OnTowerDamaged;

    private List<Dialogue.DialogueLine> currentDialogueLines;
    private int currentLineIndex = 0;
    private bool isTutorialActive = false;

    [SerializeField] private bool movementDetected = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private FreeFlyCamera _freeFlyCamera;

    // Tutorial checkpoints
    public GameObject buildingDestination;
    public GameObject targetDestination;
    public GameObject corruptedIntroDestination;

    // Progress tracking
    private bool hasReachedTargetDestination = false;
    private bool hasReachedBuildingDestination = false;
    private bool hasReachedCorruptedZone = false;
    private bool firstTimeTowerDamaged = false;
    private bool isTowerPlacementChecked = false;

    private bool hasShownPostFirstWaveDialogue = false;
    private bool hasShownPostSecondWaveDialogue = false;
    private bool corruptedIntroCompleted = false;

    [SerializeField] private BuildingManager buildingManager;

    // Lock wave start until corruption tutorial is done
    private bool startWaveLocked = true;

    public static TutorialGuidance _instance;

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        dialogueUI.SetActive(false);

        buildingDestination.SetActive(false);
        corruptedIntroDestination.SetActive(false);

        nextButton.onClick.AddListener(OnNextButtonClicked);

        _freeFlyCamera = FindObjectOfType<FreeFlyCamera>();
        buildingManager = FindObjectOfType<BuildingManager>();

        WaveManager.Instance.OnWaveComplete += HandleWaveCompleted;

        // Disable start wave until corrupted tutorial is complete
        if (UIManager.Instance != null)
            UIManager.Instance.startWaveButton.interactable = false;

        StartIntro();
    }

    private void Update()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject)
        {
            case GameObject obj when obj == targetDestination && !hasReachedTargetDestination:
                hasReachedTargetDestination = true;
                DisableMovements();
                StartMovementPractice();
                Destroy(targetDestination);
                break;

            case GameObject obj when obj == buildingDestination && !hasReachedBuildingDestination:
                hasReachedBuildingDestination = true;
                DisableMovements();
                StartBuildingTutorial();
                Destroy(buildingDestination);
                break;

            case GameObject obj when obj == corruptedIntroDestination && !hasReachedCorruptedZone:
                hasReachedCorruptedZone = true;
                StartCorruptedIntro();
                DisableMovements();
                Destroy(corruptedIntroDestination);
                break;
        }
    }

    #region Tutorial Sections

    public void StartMovementPractice()
    {
        SetDialogueSection("Camera Practice", OnIntroCompleted.Invoke);
        anim.SetTrigger("hideUI");
        buildingDestination.SetActive(true);
    }

    public void StartBuildingTutorial()
    {
        SetDialogueSection("Building", OnBuildingCompleted.Invoke);
        anim.SetTrigger("hideUI");
        CurrencyManager.Instance.currentCurrency = 25;
    }

    public void StartCorruptedIntro()
    {
        EnableStartWave();
        SetDialogueSection("Corrupted Zone Intro", () =>
        {
            OnCorruptedIntroCompleted?.Invoke();
        
        });

        anim.SetTrigger("hideUI");
    }

    private void EnableStartWave()
    {
        startWaveLocked = false;
        corruptedIntroCompleted = true;

        if (UIManager.Instance != null && UIManager.Instance.startWaveButton != null)
            UIManager.Instance.startWaveButton.interactable = true;
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

    #endregion

    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= HandleWaveCompleted;
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
