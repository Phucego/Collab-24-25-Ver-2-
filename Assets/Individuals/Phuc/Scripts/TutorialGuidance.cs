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
    public List<Dialogue> allDialogues; // List of all dialogues across the tutorial

    [Header("Animation Controller")]
    public Animator anim;

    // Unity Event for calling actions
    public UnityEvent OnIntroCompleted;
    public UnityEvent OnResourceTutorialStarted;
    public UnityEvent OnCameraMovementStarted;

    private List<Dialogue.DialogueLine> currentDialogueLines;
    private int currentLineIndex = 0;
    private bool isTutorialActive = false;
    
    [SerializeField]
    private bool movementDetected = false; // Tracks if movement has been detected during the intro

    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private FreeFlyCamera _freeFlyCamera;

    // Define the destination (e.g., a GameObject that represents the destination)
    public GameObject targetDestination;

    void Start()
    {
        dialogueUI.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClicked);

        _freeFlyCamera = FindObjectOfType<FreeFlyCamera>();

        StartIntro(); // Start the tutorial with the introduction
    }

    private void Update()
    {
        if (isTutorialActive && currentDialogueLines != null && currentDialogueLines.Count > 0)
        {
            if (DetectMovementInput() && currentDialogueLines[currentLineIndex].characterName == "Introduction")
            {
                movementDetected = true;
                CompleteMovementStep();
            }
        }
    }

    private void StartIntro()
    {
        SetDialogueSection("Introduction", OnIntroCompleted.Invoke);
    }

    public void EndIntro()
    {
        Debug.Log("Introduction completed.");
        StartCameraMovementTutorial(); // Move to the next tutorial phase
    }

    public void StartCameraMovementTutorial()
    {
        SetDialogueSection("Camera Movement", OnCameraMovementStarted.Invoke);
        anim.SetTrigger("hideUI");

        CompleteMovementStep();
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
            // Trigger the animation when the entire dialogue has finished
            anim.SetTrigger("isStart"); // Trigger the "isStart" animation at the end of the dialogue
            dialogueUI.SetActive(false);
            isTutorialActive = false;
            onComplete?.Invoke(); // Call the completion callback
            _freeFlyCamera._enableRotation = true;
            _freeFlyCamera._enableMovement = true;
        }
    }

    private void DisplayLine(Dialogue.DialogueLine line)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            _freeFlyCamera._enableRotation = false;
            _freeFlyCamera._enableMovement = false;
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
                ShowCurrentLine(null); // Continue showing the dialogue
                
            }
        }
    }

    private bool DetectMovementInput()
    {
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || 
               Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    }

    private void CompleteMovementStep()
    {
        // Mark movement detected and proceed to the next dialogue line if not already detected
        if (!movementDetected)
        {
            movementDetected = true;

            if (currentLineIndex < currentDialogueLines.Count - 1)
            {
                currentLineIndex++;
                ShowCurrentLine(null);
            }
            else
            {
                EndIntro();
            }
        }
    }

    // Check if the camera collides with the destination object
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == targetDestination)
        {
            Debug.Log("a");
            // The camera has collided with the destination object, move to the next tutorial phase
            StartCameraMovementTutorial();
        }
    }
}
