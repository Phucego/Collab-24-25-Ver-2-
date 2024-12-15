using UnityEngine;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    public List<Dialogue> dialogues; // A list of Dialogue ScriptableObjects
    
    [SerializeField]
    private int currentDialogueIndex = 0; // To track the current dialogue

    // Method to trigger the next dialogue
    public void TriggerNextDialogue()
    {
        if (currentDialogueIndex < dialogues.Count)
        {
            Dialogue currentDialogue = dialogues[currentDialogueIndex];
            PlayDialogue(currentDialogue); // Replace this with your actual dialogue playing logic
            currentDialogueIndex++;
        }
        else
        {
            Debug.Log("All dialogues have been played.");
        }
    }

    // Method to reset the dialogue sequence
    public void ResetDialogues()
    {
        currentDialogueIndex = 0;
    }

    // Example placeholder for dialogue playing logic
    private void PlayDialogue(Dialogue dialogue)
    {
        foreach (var line in dialogue.dialogueLines)
        {
            Debug.Log($"{line.characterName}: {line.dialogueText}");
        }
    }
}