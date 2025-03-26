using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText; // UI for the character's name
    public TextMeshProUGUI dialogueText; // UI for the dialogue text
    public Button nextButton; // Button to progress the dialogue

    [Header("Dialogue Data")]
    private Dialogue currentDialogue; // The currently active Dialogue ScriptableObject
    private DialogueTrigger currentTrigger; // The currently active DialogueTrigger
    private int currentLineIndex = 0;
    public bool isDialogueActive = false;

    [Header("Player Interaction")]
    public GameObject dialogueUI; // Reference to the dialogue UI container
    public LayerMask interactableLayer; // Layer mask for interactable characters
    public float interactionRange = 2f; // Range within which the player can interact


    public static DialogueDisplay instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        dialogueUI.SetActive(false); // Initially hide the dialogue UI
        nextButton.onClick.AddListener(OnNextButtonClicked); // Add button listener
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Press 'E' to interact
        {
            TryStartDialogue();
        }
    }

    void TryStartDialogue()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
        foreach (var hit in hits)
        {
            DialogueTrigger trigger = hit.GetComponent<DialogueTrigger>();
            if (trigger != null && !isDialogueActive)
            {
                currentTrigger = trigger; // Store the active trigger
                StartDialogueFromTrigger();
                break;
            }
        }
    }

    void StartDialogueFromTrigger()
    {
        if (currentTrigger == null || currentTrigger.dialogues.Count == 0)
        {
            Debug.LogWarning("DialogueTrigger is missing or has no dialogues.");
            return;
        }

        currentDialogue = currentTrigger.dialogues[0]; // Start with the first dialogue
        currentTrigger.dialogues.RemoveAt(0); // Remove the played dialogue from the list
        StartDialogue(currentDialogue);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            Debug.LogWarning("Dialogue data is missing or empty.");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        dialogueUI.SetActive(true); // Show dialogue UI
       
        DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
    }

    void DisplayLine(Dialogue.DialogueLine line)
    {
        characterNameText.text = line.characterName; // Set character name
        dialogueText.text = line.dialogueText; // Set dialogue text
    }

    public void OnNextButtonClicked()
    {
        if (currentLineIndex < currentDialogue.dialogueLines.Count - 1)
        {
            currentLineIndex++;
            DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();

            // Check if more dialogues are left in the trigger
            if (currentTrigger != null && currentTrigger.dialogues.Count > 0)
            {
                StartDialogueFromTrigger(); // Start the next dialogue
            }
        }
    }

    void EndDialogue()
    {
        Debug.Log("Dialogue finished.");
        characterNameText.text = "";
        dialogueText.text = "";
        dialogueUI.SetActive(false); // Hide the dialogue UI
        isDialogueActive = false;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize interaction range in the scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
