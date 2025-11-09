using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Animator[] doorAnimators; // Duris valdantys animatoriai

    // Kai robotas ieina i trigger zona
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Robot"))
        {
            foreach (Animator animator in doorAnimators)
            {
                // Ijungiame atidarymo animacija
                animator.SetBool("Open", true);
            }
        }
    }

    // Kai robotas iseina is trigger zonos
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Robot"))
        {
            foreach (Animator animator in doorAnimators)
            {
                // Ijungiame uzdarymo animacija
                animator.SetBool("Open", false);
            }
        }
    }
}
