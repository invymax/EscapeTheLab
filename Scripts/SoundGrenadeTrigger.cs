using UnityEngine;

public class SoundGrenadeTrigger : MonoBehaviour
{
    [Header("Garso nustatymai")]
    public float soundRadius = 12f;      // Garso girdimumo spindulys
    public float soundIntensity = 1f;    
    public AudioSource audioSource;      // AudioSource komponentas (garso saltinis)

    private bool triggered = false;      // Ar jau buvo suveikta (kad nesikartotu)

    private void Start()
    {
        // Jei audioSource nepriskirtas - bandom paimti is paties objekto
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // Kai granata susiduria su kokiu nors objektu
    private void OnCollisionEnter(Collision collision)
    {
        if (triggered) return;  // Jei jau suveike - nieko nedarom
        triggered = true;

        // Paleidziame garsa
        if (audioSource != null)
            audioSource.Play();

        // Randame visus priesus su tagu "Robot"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Robot");

        foreach (GameObject enemy in enemies)
        {
            // Skaiciuojame atstuma tarp granatos ir prieso
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            // Jei priesas yra garso spindulyje - siunciam ji patikrinti garso saltini
            if (distance <= soundRadius)
            {
                RobotAI robot = enemy.GetComponent<RobotAI>();
                if (robot != null)
                {
                    robot.GoToSound(transform.position);
                }
            }
        }
    }

    // Scenoje nupiesiame garso spinduli (tik kai objektas pazymetas)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);  // Pusiau permatomas apvalkalas
        Gizmos.DrawSphere(transform.position, soundRadius);
    }
}
