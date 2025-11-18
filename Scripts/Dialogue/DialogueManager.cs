using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject panel; // панель диалога

    [Header("Typing")]
    public float typingSpeed = 0.03f;
    private Coroutine typingRoutine;

    private DialogueSequence currentSequence;
    private int currentIndex;
    private bool isActive;

    public PlayerCamera playerCamera;
    public PlayerMovement playerMovement;


    void Update()
    {
        if (isActive && Input.GetMouseButtonDown(0))
        {
            NextOrSkip();
        }
    }


    public void StartDialogue(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentIndex = 0;
        typingSpeed = sequence != null ? sequence.defaultTypingSpeed : typingSpeed;
        isActive = true;
        panel.SetActive(true);

        if (playerCamera != null)
        {
            playerCamera.EnableInput(false, keepCursorLocked: false);
        }
        if (playerMovement != null)
        {
            playerMovement.inputEnabled = false;
        }

        // включаем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowCurrentLine();
    }


    public void ShowCurrentLine()
    {
        if (currentSequence == null || currentIndex >= currentSequence.lines.Length)
        {
            EndDialogue();
            return;
        }

        var line = currentSequence.lines[currentIndex];
        nameText.text = line.speakerName;
        avatarImage.sprite = line.avatar;

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(Typewriter(line.text));
    }

    IEnumerator Typewriter(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingRoutine = null;
    }

    public void NextOrSkip()
    {
        // если печатает Ч мгновенно показать весь текст, иначе Ч перейти к следующей реплике
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
            dialogueText.text = currentSequence.lines[currentIndex].text;
        }
        else
        {
            currentIndex++;
            ShowCurrentLine();
        }
    }

    public void EndDialogue()
    {
        isActive = false;
        panel.SetActive(false);

        // возвращаем управление
        if (playerCamera != null)
        {
            playerCamera.EnableInput(true, keepCursorLocked: true);
        }
        if (playerMovement != null)
        {
            playerMovement.inputEnabled = true;
        }

        // пр€чем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentSequence = null;
    }

    public bool IsActive() => isActive;
}