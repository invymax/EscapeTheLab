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

    [Header("Player damage while chasing")]
    public float chaseDamagePerSecond = 5f;   // Сколько HP в секунду снимается при преследовании
    private PlayerHealth playerHealth;        // Кэш ссылки на здоровье игрока

    // NEW: учёт невидимости и маска видимости
    [Header("Vision settings")]
    [Tooltip("Какие слои учитывает луч зрения робота")]
    public LayerMask visionMask = ~0;
    [Tooltip("Слой невидимого игрока (будет исключён из visionMask автоматически, если найден)")]
    public string invisibleLayerName = "PlayerInvisible";
    private int invisibleLayer = -1;
    private PlayerEffects playerEffects;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = walkSpeed;

        if (patrolPoints.Length > 0)
            agent.destination = patrolPoints[currentPatrolIndex].position;

        // Логируем все объекты с тегом Player (на случай, если их несколько)
        GameObject[] playersWithTag = GameObject.FindGameObjectsWithTag("Player");
        if (playersWithTag != null && playersWithTag.Length > 0)
        {
            string names = "";
            foreach (var go in playersWithTag)
                names += go.name + ", ";
        }
        else
        {
        }

        // Попробуем найти игрока и компонент PlayerHealth надёжно:
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {

            // Сначала пробуем компонент на том же объекте
            playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                player = playerObj.transform;
            }
            else
            {
                // Затем пробуем в дочерних объектах
                playerHealth = playerObj.GetComponentInChildren<PlayerHealth>();
                if (playerHealth != null)
                {
                    player = playerHealth.transform;
                }
                else
                {
                    // Фоллбек: найти любой PlayerHealth в сцене
                    PlayerHealth phFallback = FindObjectOfType<PlayerHealth>();
                    if (phFallback != null)
                    {
                        playerHealth = phFallback;
                        player = phFallback.transform;
                    }
                    else
                    {
                    }
                }
            }
        }
        else
        {
            // Если ничего не найдено по тегу — пытаемся найти PlayerHealth прямо
            PlayerHealth ph = FindObjectOfType<PlayerHealth>();
            if (ph != null)
            {
                playerHealth = ph;
                player = ph.transform;
            }
            else
            {
            }
        }

        // NEW: кэш PlayerEffects для флага невидимости
        if (player != null)
            playerEffects = player.GetComponentInParent<PlayerEffects>();

        // NEW: авто-исключение слоя невидимости из маски зрения
        if (!string.IsNullOrEmpty(invisibleLayerName))
        {
            invisibleLayer = LayerMask.NameToLayer(invisibleLayerName);
            if (invisibleLayer >= 0)
            {
                visionMask &= ~(1 << invisibleLayer);
            }
        }

        if (playerHealth == null)
        {
        }
    }

    void Update()
    {
        if (isJumping) return;

        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (currentState == EnemyState.Attack)
        {
            // Наносим урон игроку при преследовании (если есть ссылка)
            if (playerHealth != null && !playerHealth.IsDead && chaseDamagePerSecond > 0f)
            {
                float dmg = chaseDamagePerSecond * Time.deltaTime;
                playerHealth.TakeDamage(dmg);
                // Debug.Log($"RobotAI: dealing {dmg:F3} damage to player. PlayerHP now {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
            }
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolBehavior();
                break;
            case EnemyState.Investigate:
                InvestigateBehavior();
                break;
            case EnemyState.Attack:
                AttackBehavior();
                break;
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
            if (patrolPoints.Length > 0)
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
        if (player == null)
            return;

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
            if (patrolPoints.Length > 0)
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
        if (player == null) return false;

        // NEW: если игрок невидим — не видим его
        if (playerEffects != null && playerEffects.IsInvisible)
            return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        float halfAngle = fovAngle / 2f;
        float step = fovAngle / 30f;

        for (float angle = -halfAngle; angle <= halfAngle; angle += step)
        {
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Vector3 dir = rot * transform.forward;

            // NEW: Raycast с маской, которая исключает слой невидимости
            if (Physics.Raycast(origin, dir, out RaycastHit hit, fovRadius, visionMask, QueryTriggerInteraction.Ignore))
            {
                // Safety: если вдруг маска не исключила — не видим объект на слое невидимости
                if (invisibleLayer >= 0 && hit.transform.gameObject.layer == invisibleLayer)
                    continue;

                if (hit.transform.CompareTag("Player") || hit.transform.root.CompareTag("Player"))
                {
                    // Дополнительная проверка флага (если другой объект игрока попал в луч)
                    var pe = hit.transform.GetComponentInParent<PlayerEffects>();
                    if (pe != null && pe.IsInvisible) return false;
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