using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3f;
    public TextMeshProUGUI promptText;

    private Interactable currentTarget;

    // ссылка на менеджер диалога
    public DialogueManager dialogueManager;

    void Update()
    {
        // если диалог активен — полностью скрываем подсказку и не ищем объекты
        if (dialogueManager != null && dialogueManager.IsActive())
        {
            promptText.gameObject.SetActive(false);
            currentTarget = null;
            return;
        }

        CheckForInteractable();

        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
        {
            currentTarget.Interact();
            // сразу скрываем подсказку при старте взаимодействия
            promptText.gameObject.SetActive(false);
        }
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                currentTarget = interactable;
                promptText.text = interactable.promptMessage;
                promptText.gameObject.SetActive(true);
                return;
            }
        }

        // если ничего не нашли
        currentTarget = null;
        promptText.gameObject.SetActive(false);
    }
}