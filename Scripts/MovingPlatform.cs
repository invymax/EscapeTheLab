using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public NavMeshSurface navMeshSurface;
    public float speed = 3f;
    public float waitTime = 5f; 

    private Vector3 target;
    private bool isWaiting = false;

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("“очки A и B должны быть назначены в инспекторе!");
            return;
        }

        transform.position = pointA.position;
        target = pointB.position;
    }

    void Update()
    {
        if (!isWaiting)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                StartCoroutine(WaitAndSwitchTarget());
            }
        }
    }

    private IEnumerator WaitAndSwitchTarget()
    {
        isWaiting = true;
        navMeshSurface.BuildNavMesh();
        yield return new WaitForSeconds(waitTime);
        target = (target == pointB.position) ? pointA.position : pointB.position;
        navMeshSurface.BuildNavMesh();
        isWaiting = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}
