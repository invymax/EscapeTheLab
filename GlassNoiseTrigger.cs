using UnityEngine;

public class GlassNoiseTrigger : MonoBehaviour
{
    public float soundRadius = 10f;           // Garso plitimo spindulys
    public AudioSource audioSource;           // Garsinis saltinis (stiklo garsas)

    private void Start()
    {
        // Jei audioSource nepriskirtas - bandome gauti is objekto
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // Kai i objekta ieina zaidejas
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Paleidziam garso klipa
            if (audioSource != null)
                audioSource.Play();

            // Ispėjame netoliese esancius robotus
            AlertEnemies();
        }
    }

    // Funkcija, kuri patikrina, ar robotai yra garso spindulyje
    void AlertEnemies()
    {
        // Randame visus objektus aplink per nurodyta spinduli
        Collider[] hits = Physics.OverlapSphere(transform.position, soundRadius);

        foreach (var hit in hits)
        {
            // Patikriname ar yra RobotAI komponentas
            RobotAI robot = hit.GetComponent<RobotAI>();
            if (robot != null)
            {
                robot.GoToSound(transform.position); // Siunciame robota i garso taska
            }
        }
    }

    // Scenoje piesiame raudona apskritima aplink garso saltini (debug tikslams)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, soundRadius);
    }
}
