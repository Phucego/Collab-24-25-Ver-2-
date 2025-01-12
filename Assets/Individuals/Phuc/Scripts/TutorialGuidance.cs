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
    public List<Dialogue> allDialogues;

    [Header("Animation Controller")]
    public Animator anim;

    public UnityEvent OnIntroCompleted;
    public UnityEvent OnCameraMovementCompleted;

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

    void Start()
    {
        dialogueUI.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClicked);

        _freeFlyCamera = FindObjectOfType<FreeFlyCamera>();

        StartIntro();
    }

    private void Update()
    {
        if (isTutorialActive && currentDialogueLines != null && currentDialogueLines.Count > 0)
        {
            if (DetectMovementInput() && currentDialogueLines[currentLineIndex].characterName == "Introduction")
            {
                movementDetected = true;
            }
        }
    }

    public void StartIntro()
    {
        DisableMovements();
        SetDialogueSection("Introduction", OnIntroCompleted.Invoke);
    }

    private void SetDialogueSection(string sectionName, UnityAction onComplete)
    {
        Dialogue dialogue = allDialogues.Find(d => d.sectionName == sectionName);
        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
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
            EnableMovements(); // Enable movements after dialogue finishes
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
        if (isTyping)
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

    private bool DetectMovementInput()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetDestination && !hasReachedDestination)
        {
            hasReachedDestination = true;
            Debug.Log("Camera reached destination.");

            DisableMovements(); // Disable movements during dialogue
            StartCameraMovementTutorial();

            // Delete the destination point after collision
            Destroy(targetDestination);
        }
    }

    public void StartCameraMovementTutorial()
    {
        SetDialogueSection("Camera Movement", OnCameraMovementCompleted.Invoke);
        anim.SetTrigger("hideUI");
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
}
