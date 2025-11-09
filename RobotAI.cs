using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class RobotAI : MonoBehaviour
{
    public enum EnemyState { Idle, Patrol, Investigate, Attack }
    private EnemyState currentState = EnemyState.Patrol;

    [Header("Patruliavimas")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    [Header("Judejimas ir komponentai")]
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Matymo laukas")]
    public float fovRadius = 10f;
    public float fovAngle = 60f;
    private Transform player;

    [Header("Greicio nustatymai")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;

    [Header("Sokimo nustatymai")]
    public float jumpDuration = 1.0f;
    public float jumpHeight = 2.0f;
    private bool isJumping = false;

    [Header("Stovimo nustatymai")] 
    private float idleTimer = 0f;
    private float idleDuration = 3.3f;

    [Header("Zaidejo atmintis")]
    public float memoryDuration = 3f;
    private float memoryTimer = 0f;
    private bool isInvestigateRunning = false;

    [Header("Sugavimo principas")]
    public float catchTimeRequired = 40f;       
    public float catchDecayRate = 2f;           
    private float currentCatchTime = 0f;        

    public UnityEngine.UI.Image catchCircleUI; 


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = walkSpeed;

        if (patrolPoints.Length > 0)
            agent.destination = patrolPoints[currentPatrolIndex].position;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (isJumping) return;

        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (currentState == EnemyState.Attack)
        {
            if (IsPlayerInFOV())
            {
                currentCatchTime += Time.deltaTime;

                if (currentCatchTime >= catchTimeRequired)
                {
                    TriggerEndgame();
                }
            }
            else
            {
                currentCatchTime -= catchDecayRate * Time.deltaTime;
                if (currentCatchTime < 0f)
                    currentCatchTime = 0f;
            }
        }
        else
        {
            currentCatchTime -= catchDecayRate * Time.deltaTime;
            if (currentCatchTime < 0f)
                currentCatchTime = 0f;
        }

        UpdateCatchCircleUI(currentCatchTime / catchTimeRequired);

        switch (currentState)
        {
            //NavMesh
            case EnemyState.Patrol:
                PatrolBehavior();
                break;
            //Audio Sensor, NavMesh
            case EnemyState.Investigate:
                InvestigateBehavior();
                break;
            //VisualSensor, Navmesh
            case EnemyState.Attack:
                AttackBehavior();
                break;
            //NavMesh
            case EnemyState.Idle:
                IdleBehavior();
                break;
        }

        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(PerformJump());
        }
    }


    public void SetState(EnemyState newState)
    {
        currentState = newState;

        if (newState == EnemyState.Attack)
        {
            agent.speed = runSpeed;
            agent.isStopped = false;

            animator.ResetTrigger("Idle");
            animator.ResetTrigger("Idle3");

            animator.SetBool("PlayerDetected", true);
        }
        else if (newState == EnemyState.Patrol)
        {
            agent.speed = walkSpeed;
            agent.isStopped = false;
            agent.destination = patrolPoints[currentPatrolIndex].position;

            animator.SetBool("PlayerDetected", false);
        }
        else if (newState == EnemyState.Idle)
        {
            idleDuration = 3.3f;
            idleTimer = 0f;
            agent.isStopped = true;

            if (Random.value < 0.5f)
            {
                animator.SetTrigger("Idle");
            }
            else
            {
                animator.SetTrigger("Idle3");
            }
        }

        if (newState != EnemyState.Investigate)
        {
            isInvestigateRunning = false;
        }

    }

    private void PatrolBehavior()
    {
        if (IsPlayerInFOV())
        {
            memoryTimer = memoryDuration;
            SetState(EnemyState.Attack);
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (currentState == EnemyState.Patrol)
            {
                if (Random.value < 0.5f)
                {
                    SetState(EnemyState.Idle);
                }
                else
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                    agent.destination = patrolPoints[currentPatrolIndex].position;
                }
            }
        }
    }

    private void InvestigateBehavior()
    {
        if (IsPlayerInFOV())
        {
            memoryTimer = memoryDuration;
            SetState(EnemyState.Attack);
        }
        else if (!agent.pathPending && agent.remainingDistance < 1.0f)
        {
            if (!isInvestigateRunning)
            {
                StartCoroutine(InvestigateRoutine());
            }
        }
    }

    private void AttackBehavior()
    {

            agent.destination = player.position;

            if (!IsPlayerInFOV())
            {
                memoryTimer -= Time.deltaTime;
                if (memoryTimer <= 0)
                {
                    SetState(EnemyState.Patrol);
                }
            }
            else
            {
                memoryTimer = memoryDuration; 
            }
    }

    private void IdleBehavior()
    {
        agent.isStopped = true;
        idleTimer += Time.deltaTime;

        if (IsPlayerInFOV())
        {
            memoryTimer = memoryDuration;
            agent.isStopped = false;
            SetState(EnemyState.Attack);
            return;
        }

        if (idleTimer >= idleDuration)
        {
            idleTimer = 0f;
            agent.isStopped = false;

            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.destination = patrolPoints[currentPatrolIndex].position;

            SetState(EnemyState.Patrol);
        }

    }

    IEnumerator InvestigateRoutine()
    {
        isInvestigateRunning = true;

        agent.isStopped = true;
        animator.SetTrigger("Investigate");

        float timer = 0f;
        while (timer < 5f)
        {
            if (IsPlayerInFOV())
            {
                agent.isStopped = false;
                SetState(EnemyState.Attack);
                isInvestigateRunning = false;
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
        SetState(EnemyState.Patrol);
        isInvestigateRunning = false;
    }

    private bool IsPlayerInFOV()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        float halfAngle = fovAngle / 2f;
        float step = fovAngle / 30f;

        for (float angle = -halfAngle; angle <= halfAngle; angle += step)
        {
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Vector3 dir = rot * transform.forward;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, fovRadius))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    IEnumerator PerformJump()
    {
        isJumping = true;

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 startPos = transform.position;
        Vector3 endPos = data.endPos;

        animator.SetTrigger("JumpStart");

        float elapsedTime = 0f;
        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y += jumpHeight * 4 * t * (1 - t);
            transform.position = currentPos;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        agent.CompleteOffMeshLink();
        animator.SetTrigger("JumpEnd");

        isJumping = false;
    }

    public void GoToSound(Vector3 soundPos)
    {
        agent.destination = soundPos;
        SetState(EnemyState.Investigate);
    }

    private void UpdateCatchCircleUI(float fillAmount)
    {
        if (catchCircleUI != null)
        {
            fillAmount = Mathf.Clamp01(fillAmount);
            catchCircleUI.fillAmount = fillAmount;

            if (fillAmount < 0.3f)
            {
                catchCircleUI.color = Color.green;
            }
            else if (fillAmount < 0.75f)
            {
                catchCircleUI.color = Color.yellow;
            }
            else
            {
                catchCircleUI.color = Color.red;
            }
        }
    }

    private void TriggerEndgame()
    {
        Debug.Log("game over");
        agent.isStopped = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        float step = fovAngle / 30f;
        float halfAngle = fovAngle / 2f;

        for (float angle = -halfAngle; angle <= halfAngle; angle += step)
        {
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Vector3 dir = rot * transform.forward;
            Gizmos.DrawRay(origin, dir.normalized * fovRadius);
        }
    }

    public void UpdateDestination(Vector3 destination)
    {
        agent.destination = destination;
    }

}
