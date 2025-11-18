using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZone : MonoBehaviour
{
    // Kai zaidejas pateks i DeathZone, ivyksta trigger'is
    private void OnTriggerEnter(Collider other)
    {
        // Patikriname, ar "Player" tag'as
        if (other.CompareTag("Player"))
        {
            Debug.Log("Restart");
            RestartScene(); // Perkrauname scena
        }
    }

    // Metodas, kuris uzkrauna dabartine scena is naujo
    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
