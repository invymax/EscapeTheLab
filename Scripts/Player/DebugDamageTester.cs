using UnityEngine;

public class DebugDamageTester : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public float damagePerPress = 10f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damagePerPress);
                Debug.Log($"DebugDamageTester: applied {damagePerPress} damage");
            }
        }
    }
}