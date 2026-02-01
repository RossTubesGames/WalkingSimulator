using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class OrcPatrol : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Player Detection (Tag)")]
    [SerializeField] private string playerTag = "Player";

    [Header("Patrol")]
    [SerializeField] private float walkPointRange = 8f;
    [SerializeField] private float patrolRepathTimeout = 20f;
    [SerializeField] private int maxPointAttempts = 20;
    [SerializeField] private float navMeshSampleDistance = 2f;
    [SerializeField] private float reachedDistance = 1f;

    [Header("Detection (Distance Based)")]
    [SerializeField] private float sightRange = 10f;
    [SerializeField] private float attackRange = 2f;

    [Header("Speeds")]
    [SerializeField] private float patrolSpeed = 0.5f;
    [SerializeField] private float chaseSpeed = 2.0f;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private float caughtDelay = 5f;
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private Vector3 patrolOrigin;
    private Vector3 walkPoint;
    private bool walkPointSet;
    private float timeSincePointSet;

    private bool isCatchingPlayer;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        FindPlayerIfNeeded();

        patrolOrigin = transform.position;

        if (agent != null)
            agent.autoRepath = true;

        if (gameoverScreen != null)
            gameoverScreen.SetActive(false);
    }

    private void Update()
    {
        if (agent == null) return;
        if (isCatchingPlayer) return;

        FindPlayerIfNeeded();
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        bool inSight = distToPlayer <= sightRange;
        bool inAttack = distToPlayer <= attackRange;

        if (!inAttack && !inSight)
            Patrol();
        else if (!inAttack && inSight)
            ChasePlayer();
        else
            AttackPlayer();
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null) return;

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
            player = p.transform;
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!walkPointSet)
        {
            TrySetNewPatrolPoint();
            return;
        }

        timeSincePointSet += Time.deltaTime;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid ||
            agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            walkPointSet = false;
            return;
        }

        if (timeSincePointSet >= patrolRepathTimeout)
        {
            walkPointSet = false;
            return;
        }

        if (!agent.pathPending && agent.hasPath && agent.remainingDistance <= reachedDistance)
        {
            walkPointSet = false;
        }
    }

    private void TrySetNewPatrolPoint()
    {
        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < maxPointAttempts; i++)
        {
            Vector2 rand2D = Random.insideUnitCircle * walkPointRange;
            Vector3 candidate = patrolOrigin + new Vector3(rand2D.x, 0f, rand2D.y);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                continue;

            if (!agent.CalculatePath(hit.position, path))
                continue;

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            walkPoint = hit.position;
            walkPointSet = true;
            timeSincePointSet = 0f;

            agent.SetDestination(walkPoint);
            return;
        }

        walkPointSet = false;
    }

    private void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);

        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
    }

    private void AttackPlayer()
    {
        if (isCatchingPlayer) return;

        isCatchingPlayer = true;
        agent.ResetPath();

        StartCoroutine(CaughtRoutine());
    }

    private IEnumerator CaughtRoutine()
    {
        if (gameoverScreen != null)
            gameoverScreen.SetActive(true);

        yield return new WaitForSeconds(caughtDelay);

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? patrolOrigin : transform.position, walkPointRange);
    }
}
