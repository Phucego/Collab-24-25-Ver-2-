using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText; // UI element for displaying the character's name
    public TextMeshProUGUI dialogueText;      // UI element for displaying the dialogue text
    public Button nextButton;                 // Button to progress through the dialogue

    [Header("Dialogue Data")]
    private Dialogue currentDialogue;         // The current Dialogue ScriptableObject being displayed
    private DialogueTrigger currentTrigger;   // The current DialogueTrigger providing dialogues
    private int currentLineIndex = 0;         // Index of the current line in the dialogue
    public bool isDialogueActive = false;     // Flag to indicate if a dialogue is active

    [Header("Player Interaction")]
    public GameObject dialogueUI;             // The dialogue UI container GameObject
    public LayerMask interactableLayer;       // Layer mask for detecting interactable objects
    public float interactionRange = 2f;       // Range within which the player can trigger dialogue

    private bool isTyping = false;            // Flag to track if text is currently being typed
    private Coroutine typingCoroutine;        // Reference to the active typing coroutine

    public static DialogueDisplay instance;   // Singleton instance for easy access

    #region Initialization
    private void Awake()
    {
        // Set up singleton pattern
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
        // Hide the dialogue UI initially
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        // Assign the next button's click event
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
    }
    #endregion

    #region Update and Interaction
    void Update()
    {
        // Check for player input to start dialogue (press 'E')
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryStartDialogue();
        }
    }

    void TryStartDialogue()
    {
        // Detect interactable objects within range
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
        foreach (var hit in hits)
        {
            DialogueTrigger trigger = hit.GetComponent<DialogueTrigger>();
            if (trigger != null && !isDialogueActive)
            {
                currentTrigger = trigger;
                StartDialogueFromTrigger();
                break;
            }
        }
    }
    #endregion

    #region Dialogue Management
    void StartDialogueFromTrigger()
    {
        // Check if there are dialogues to display
        if (currentTrigger == null || currentTrigger.dialogues.Count == 0)
        {
            return;
        }

        // Start the first dialogue and remove it from the list
        currentDialogue = currentTrigger.dialogues[0];
        currentTrigger.dialogues.RemoveAt(0);
        StartDialogue(currentDialogue);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        // Validate dialogue data
        if (dialogue == null || dialogue.dialogueLines.Count == 0)
        {
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;

        // Show the dialogue UI
        if (dialogueUI != null)
            dialogueUI.SetActive(true);

        DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
    }

    void DisplayLine(Dialogue.DialogueLine line)
    {
        // Update the character name
        if (characterNameText != null)
            characterNameText.text = line.characterName;

        // Clear the dialogue text
        if (dialogueText != null)
            dialogueText.text = "";

        // Stop any existing typing coroutine
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        // Start typing the new line
        isTyping = true;
        typingCoroutine = StartCoroutine(TypeText(line.dialogueText));
    }

    private IEnumerator TypeText(string fullText)
    {
        dialogueText.text = "";
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.05f); // Typing speed (adjustable)
        }
        isTyping = false;
        typingCoroutine = null;
    }

    public void OnNextButtonClicked()
    {
        if (currentDialogue == null)
            return;

        // Notify UIManager of dialogue progress
        if (UIManager.Instance != null)
            UIManager.Instance.OnDialogueNextPressed(currentLineIndex, currentDialogue.dialogueLines.Count);

        if (isTyping)
        {
            // Instantly complete the current line
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            dialogueText.text = currentDialogue.dialogueLines[currentLineIndex].dialogueText;
            isTyping = false;
        }
        else
        {
            // Progress to the next line or end the dialogue
            if (currentLineIndex < currentDialogue.dialogueLines.Count - 1)
            {
                currentLineIndex++;
                DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
            }
            else
            {
                EndDialogue();

                // Check for additional dialogues in the trigger
                if (currentTrigger != null && currentTrigger.dialogues.Count > 0)
                {
                    StartDialogueFromTrigger();
                }
            }
        }
    }

    void EndDialogue()
    {
        // Stop any ongoing typing
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;

        // Clear UI elements
        if (characterNameText != null)
            characterNameText.text = "";
        if (dialogueText != null)
            dialogueText.text = "";
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        isDialogueActive = false;
    }
    #endregion

    #region Editor Visualization
    void OnDrawGizmosSelected()
    {
        // Draw a wire sphere to visualize the interaction range in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    #endregion
}