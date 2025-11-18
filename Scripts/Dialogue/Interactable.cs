using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string promptMessage = "Paspauskite 'E' ";

    public virtual void Interact()
    {
        Debug.Log("Взаимодействие с " + gameObject.name);
        // Здесь запускаешь диалог, катсцену или что угодно
    }
}