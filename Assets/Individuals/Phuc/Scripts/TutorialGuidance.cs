using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TowerDefenseTutorial : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText; // UI for the character's name
    public TextMeshProUGUI dialogueText; // UI for the dialogue text
    public Button nextButton; // Button to progress the dialogue
    public GameObject dialogueUI; // Reference to the dialogue UI container
   // public GameObject mainUI; // Reference to the main game UI container

    [Header("Tutorial Data")]
    public List<Dialogue> tutorialSteps; // List of tutorial steps (Dialogue ScriptableObjects)

    [Header("Animation Controller")]
    public Animator anim; // Animator for triggering animations

    private int currentStepIndex = 0; // Tracks the current tutorial step
    private bool isTutorialActive = false; // Tracks if the tutorial is active

    private Coroutine typingCoroutine; // For word-by-word display

    void Start()
    {
        // Initialize UI
        dialogueUI.SetActive(false);
        nextButton.onClick.AddListener(OnNextButtonClicked);

        // Start the tutorial automatically
        StartTutorial();
    }

    void StartTutorial()
    {
        if (tutorialSteps == null || tutorialSteps.Count == 0)
        {
            Debug.LogWarning("No tutorial steps assigned.");
            return;
        }

        currentStepIndex = 0;
        isTutorialActive = true;
        dialogueUI.SetActive(true);
        //if (mainUI != null) mainUI.SetActive(false); // Disable main UI

        ShowTutorialStep();
    }

    void ShowTutorialStep()
    {
        if (currentStepIndex < tutorialSteps.Count)
        {
            Dialogue currentDialogue = tutorialSteps[currentStepIndex];
            DisplayLine(currentDialogue.dialogueLines[0]);

            // Add mechanic-specific highlights or actions here
            ExecuteStepActions(currentStepIndex);
        }
        else
        {
            EndTutorial();
        }
    }

    void DisplayLine(Dialogue.DialogueLine line)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        characterNameText.text = line.characterName; // Display the character's name
        typingCoroutine = StartCoroutine(TypeSentence(line.dialogueText));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // Adjust typing speed here
        }
    }

    public void OnNextButtonClicked()
    {
        if (currentStepIndex == 0 && anim != null)
        {
            anim.SetTrigger("isStart"); // Trigger animation on first dialogue
        }

        currentStepIndex++;
        ShowTutorialStep();
    }

    void EndTutorial()
    {
        Debug.Log("Tutorial completed.");
        characterNameText.text = "";
        dialogueText.text = "";
        dialogueUI.SetActive(false);
     //   if (mainUI != null) mainUI.SetActive(true); // Re-enable main UI
        isTutorialActive = false;

        // Resume game if paused
        Time.timeScale = 1;
    }

    void ExecuteStepActions(int stepIndex)
    {
        // Define specific actions for each step
        switch (stepIndex)
        {
            case 0:
                Debug.Log("Highlighting build menu.");
                HighlightBuildMenu();
                break;

            case 1:
                Debug.Log("Highlighting tiles.");
                HighlightTiles();
                break;

            case 2:
                Debug.Log("Pausing game for resource explanation.");
                PauseGame();
                break;

            default:
                Debug.Log("No specific action for this step.");
                break;
        }
    }

    void HighlightBuildMenu()
    {
        // Example of highlighting the build menu
        Debug.Log("Build menu highlighted (Add your specific logic here).");
    }

    void HighlightTiles()
    {
        // Example of highlighting specific tiles
        Debug.Log("Tiles highlighted (Add your specific logic here).");
    }

    void PauseGame()
    {
        Time.timeScale = 0; // Pause the game
    }
    
}