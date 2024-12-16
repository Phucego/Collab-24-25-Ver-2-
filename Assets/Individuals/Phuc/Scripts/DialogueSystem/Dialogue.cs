using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue", order = 0)]
public class Dialogue : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        public string characterName; // The name of the character speaking
        [TextArea(3, 5)]
        public string dialogueText; // The dialogue line
    }

    public string sectionName; // Name of the tutorial section (e.g., "Introduction", "CameraMovement")
    public List<DialogueLine> dialogueLines = new List<DialogueLine>(); // List of dialogue lines
}