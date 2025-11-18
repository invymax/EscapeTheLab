using UnityEngine;

public class PhoneInteractable : Interactable
{
    public DialogueManager dialogueManager;
    public DialogueSequence dialogue;

    public override void Interact()
    {
        Debug.Log("Запускаем диалог!");
        if (dialogueManager != null && dialogue != null)
        {
            dialogueManager.StartDialogue(dialogue);
        }
        else
        {
            Debug.LogWarning("DialogueManager или DialogueSequence не назначены!");
        }
    }
}