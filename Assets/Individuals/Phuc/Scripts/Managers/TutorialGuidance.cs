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

    public UnityEvent OnIntroCompleted;
    public UnityEvent OnBuildingCompleted;

    private List<Dialogue.DialogueLine> currentDialogueLines;
    private int currentLineIndex = 0;
    private bool isTutorialActive = false;

    [SerializeField]
    private bool movementDetected = false;

    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private FreeFlyCamera _freeFlyCamera;

    public GameObject targetDestination;

    private bool hasReachedDestination = false;

    
    [SerializeField] private BuildingManager buildingManager;
    private bool isTowerPlacementChecked = false;
    void Start()
    {
        dialogueUI.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClicked);

        _freeFlyCamera = FindObjectOfType<FreeFlyCamera>();
        buildingManager = FindObjectOfType<BuildingManager>();

        StartIntro();
    }

    private void Update()
    {
        if (isTutorialActive && currentDialogueLines != null && currentDialogueLines.Count > 0)
        {
            // Check if we're in the tower placement section and haven't placed a tower yet
            if (currentDialogueLines[currentLineIndex].characterName == "Introduction" && !isTowerPlacementChecked)
            {
                if (buildingManager != null && buildingManager.HasPlacedFirstTower())
                {
                    isTowerPlacementChecked = true;
                    currentLineIndex++;
                    if (isTutorialActive)
                    {
                        ShowCurrentLine(null);
                    }
                }
            }
            if (currentDialogueLines[currentLineIndex].characterName == "Introduction")
            {
                movementDetected = true;
            }
        }
    }

    public void StartIntro()
    {
        isTowerPlacementChecked = false;
        DisableMovements();
        SetDialogueSection("Introduction", OnIntroCompleted.Invoke);
    }

    private void SetDialogueSection(string sectionName, UnityAction onComplete)
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
        {
            StopCoroutine(typingCoroutine);
        }

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
        //Make sure that the player cannot presses next while the game is pausing
        if (isTyping && !UIManager.Instance.isPausing)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            dialogueText.text = currentDialogueLines[currentLineIndex].dialogueText;
            isTyping = false;
        }
        else
        {
            currentLineIndex++;
            if (isTutorialActive)
            {
                ShowCurrentLine(null);
            }
        }
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetDestination && !hasReachedDestination)
        {
            hasReachedDestination = true;
            Debug.Log("Camera reached destination.");

            DisableMovements();
            StartBuildingTutorial();
        
            Destroy(targetDestination);
        }
    }

    #region Start Tutorial Sections
    public void StartBuildingTutorial()
    {
        SetDialogueSection("Building", OnBuildingCompleted.Invoke);
        anim.SetTrigger("hideUI");
        CurrencyManager.Instance.currentCurrency = 50;

    }

    #endregion

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
}