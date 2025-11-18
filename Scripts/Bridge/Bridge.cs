using Unity.AI.Navigation;
using System.Collections;
using UnityEngine;

public class Bridge : MonoBehaviour { 

    public Animator animator;
    public NavMeshSurface navMeshSurface;
    public float updateDelay = 2.0f;

    public void DropBridge()
    {
            animator.SetBool("DropBridge", true);
            StartCoroutine(UpdateNavMeshAfterDelay(updateDelay));
    }

    private IEnumerator UpdateNavMeshAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

            navMeshSurface.BuildNavMesh();
    }
}