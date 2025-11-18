using UnityEngine;
using UnityEngine.AI;
using static RobotAI;

public class DroneAI : MonoBehaviour
{
    public Transform[] patrolPoints;             // Taskai, kuriais drone patruliuoja
    public float detectionRange = 10f;           // Zaidejo aptikimo atstumas
    public float chaseSpeedMultiplier = 1.5f;    // Greicio padidejimas, kai vejasi
    public RobotAI groundRobot;                  // Nuoroda i zemes robota (jei yra)

    private int currentPointIndex = 0;           // Dabartinis patruliavimo taskas
    private NavMeshAgent agent;                  // Navigacijos komponentas
    private Transform player;                    // Nuoroda i zaidejo transform
    private float normalSpeed;                   // Pradinis greitis
    private bool chasingPlayer = false;          // Ar vejasi zaideja

    // NEW: маска видимости и невидимость игрока
    [Header("Vision settings")]
    public LayerMask visionMask = ~0;
    public string invisibleLayerName = "PlayerInvisible";
    private int invisibleLayer = -1;
    private PlayerEffects playerEffects;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
        normalSpeed = agent.speed;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
            playerEffects = player.GetComponentInParent<PlayerEffects>();

        if (!string.IsNullOrEmpty(invisibleLayerName))
        {
            invisibleLayer = LayerMask.NameToLayer(invisibleLayerName);
            if (invisibleLayer >= 0)
                visionMask &= ~(1 << invisibleLayer);
        }

        MoveToNextPoint();
    }

    void Update()
    {
        if (chasingPlayer)
        {
            if (player != null)
                agent.destination = player.position;

            // Informuojame zemes robota apie zaidejo buvima
            if (groundRobot != null && player != null)
            {
                groundRobot.SetState(EnemyState.Attack);
                groundRobot.UpdateDestination(player.position);
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MoveToNextPoint();
        }

        DetectPlayer();
    }

    void MoveToNextPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        agent.destination = patrolPoints[currentPointIndex].position;
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
    }

    void DetectPlayer()
    {
        if (player == null)
            return;

        bool playerVisible = HasLineOfSight();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange && playerVisible)
        {
            StartChasing();
        }
        else if (chasingPlayer && (!playerVisible || distanceToPlayer > detectionRange * 1.2f))
        {
            StopChasing();
        }
    }

    bool HasLineOfSight()
    {
        if (player == null)
            return false;

        // NEW: невидимого не видим
        if (playerEffects != null && playerEffects.IsInvisible)
            return false;

        RaycastHit hit;
        Vector3 direction = (player.position - transform.position).normalized;

        // NEW: Raycast с маской (исключён слой невидимости)
        if (Physics.Raycast(transform.position, direction, out hit, detectionRange, visionMask, QueryTriggerInteraction.Ignore))
        {
            // доп. защита на случай, если маска всё же включает слой
            if (invisibleLayer >= 0 && hit.collider.gameObject.layer == invisibleLayer)
                return false;

            Debug.DrawRay(transform.position, direction * detectionRange, Color.green);
            return hit.collider.gameObject.CompareTag("Player") ||
                   hit.collider.transform.root.CompareTag("Player");
        }
        return false;
    }

    void StartChasing()
    {
        if (!chasingPlayer)
        {
            chasingPlayer = true;
            agent.speed = normalSpeed * chaseSpeedMultiplier;

            if (groundRobot != null && player != null)
            {
                groundRobot.SetState(EnemyState.Attack);
                groundRobot.UpdateDestination(player.position);
            }
        }
    }

    void StopChasing()
    {
        chasingPlayer = false;
        agent.speed = normalSpeed;
        MoveToNextPoint();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (player != null)
        {
            Gizmos.color = HasLineOfSight() ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}