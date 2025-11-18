using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator animator;
    public void OpenDoor()
    {
        if (animator != null)
        {
            animator.SetBool("OpenDoor", true);
        }
        else
        {
            Debug.LogWarning("Animator не найден на объекте двери!");
        }
    }
}