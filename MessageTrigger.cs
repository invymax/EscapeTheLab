using UnityEngine;
using TMPro;

public class MessageTrigger : MonoBehaviour
{
    public TextMeshProUGUI messageText; // Teksto laukas, kuriame rodomas pranesimas
    private bool messageShown = false;  // Ar jau buvo parodytas pranesimas

    private void Start()
    {
        // Isjungiam pradzioje pranesima, jei jis yra priskirtas
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    // Kai zaidejas ieina i zona
    private void OnTriggerEnter(Collider other)
    {
        // Jei pranesimas dar nebuvo parodytas ir objektas yra zaidejas
        if (!messageShown && other.CompareTag("Player"))
        {
            ShowMessage();
            messageShown = true; // Pažymime, kad jau parodyta
        }
    }

    // Funkcija, kuri rodo pranesima ekrane
    private void ShowMessage()
    {
        if (messageText != null)
        {
            messageText.text = "Find 2 red buttons in this room to open the door.";
            messageText.gameObject.SetActive(true);
            Invoke(nameof(HideMessage), 5f); // Paslepiam po 5 sekundziu
        }
    }

    // Paslepia teksta po kurio laiko
    private void HideMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
}
