using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public enum EnemyState { Patrol, Investigate, Chase, Search }

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform eyes;
    public Transform[] waypoints;
    public bool randomPatrol = true;

    [Header("Speeds")]
    public float patrolSpeed = 2f;
    public float investigateSpeed = 3f;
    public float chaseSpeed = 4.5f;

    [Header("Senses")]
    public float viewDistance = 12f;
    [Range(10f, 180f)] public float viewAngle = 100f;
    public float closeRange = 2.5f;

    [Header("Behaviour Timers")]
    public float waypointWait = 2f;
    public float loseSightTime = 3f;
    public float searchDuration = 6f;
    public float searchTurnSpeed = 60f;
    public float catchDistance = 1.5f;
    public float arriveDistance = 0.5f;

    [Header("Waypoint Audio")]
    public AudioClip waypointSound;
    [Range(0f, 1f)] public float waypointVolume = 1f;
    AudioSource sfx;
    bool arrivalSoundPlayed;

    public EnemyState State { get; private set; } = EnemyState.Patrol;

    int waypointIndex;
    float waitTimer, loseTimer, searchTimer;
    Vector3 lastKnownPos;
    PlayerStealth stealth;
    NavMeshAgent agent;

    private float pathUpdateCooldown = 0.3f;
    private float pathUpdateTimer;

    void Start()
    {
        sfx = GetComponent<AudioSource>();
        if (sfx == null) sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.spatialBlend = 1f;
        sfx.minDistance = 2f;
        sfx.maxDistance = 25f;
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) 
        {
            agent.stoppingDistance = arriveDistance;
            agent.updateRotation = true; 
        }

        if (player != null) stealth = player.GetComponentInParent<PlayerStealth>();
        
        if (waypoints != null && waypoints.Length > 0)
        {
            waypointIndex = randomPatrol ? Random.Range(0, waypoints.Length) : 0;
        }
        
        EnterState(EnemyState.Patrol);
    }

    void Update()
    {
        bool canSee = CanSeePlayer();

        switch (State)
        {
            case EnemyState.Patrol:      UpdatePatrol(canSee);      break;
            case EnemyState.Investigate: UpdateInvestigate(canSee); break;
            case EnemyState.Chase:       UpdateChase(canSee);       break;
            case EnemyState.Search:      UpdateSearch(canSee);      break;
        }
        if (Input.GetKeyDown(KeyCode.H) && player != null) HearNoise(player.position);
    }

    void EnterState(EnemyState next)
    {
        State = next;
        waitTimer = 0f;
        loseTimer = 0f;
        searchTimer = 0f;
        pathUpdateTimer = 0f;

        if (next == EnemyState.Search)
        {
            if (agent != null && agent.isOnNavMesh) agent.ResetPath();
        }
        
        if (next == EnemyState.Patrol)
        {
            SetPatrolTarget();
        }

        if (next == EnemyState.Investigate)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.speed = investigateSpeed;
                agent.SetDestination(lastKnownPos);
            }
        }
    }

    void UpdatePatrol(bool canSee)
    {
        if (canSee) { EnterState(EnemyState.Chase); return; }
        if (waypoints == null || waypoints.Length == 0 || waypoints[waypointIndex] == null || agent == null) return;

        agent.speed = patrolSpeed;

        float distanceToTarget = Vector3.Distance(transform.position, waypoints[waypointIndex].position);
        
        if (distanceToTarget <= arriveDistance || (agent.hasPath && agent.remainingDistance <= agent.stoppingDistance))
        {
            if (!arrivalSoundPlayed)
            {
                arrivalSoundPlayed = true;
                if (waypointSound != null)
                {
                    sfx.pitch = Random.Range(0.9f, 1.1f);
                    sfx.PlayOneShot(waypointSound, waypointVolume);
                }
            }
            waitTimer += Time.deltaTime;
            if (waitTimer >= waypointWait)
            {
                waitTimer = 0f;
                NextWaypoint();
            }
        }
    }

    void SetPatrolTarget()
    {
        if (agent != null && agent.isOnNavMesh && waypoints != null && waypoints.Length > 0 && waypoints[waypointIndex] != null)
        {
            agent.SetDestination(waypoints[waypointIndex].position);
        }
    }

    void UpdateInvestigate(bool canSee)
    {
        if (canSee) { EnterState(EnemyState.Chase); return; }
        if (agent == null || !agent.isOnNavMesh) return;

        agent.speed = investigateSpeed;

        float distanceToSound = Vector3.Distance(transform.position, lastKnownPos);
        
        if (distanceToSound <= arriveDistance || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
        {
            EnterState(EnemyState.Search);
        }
    }

    void UpdateChase(bool canSee)
    {
        if (player == null || agent == null || !agent.isOnNavMesh) return;
        agent.speed = chaseSpeed;

        pathUpdateTimer += Time.deltaTime;

        if (canSee)
        {
            loseTimer = 0f;
            lastKnownPos = player.position;

            if (pathUpdateTimer >= pathUpdateCooldown)
            {
                pathUpdateTimer = 0f;
                agent.SetDestination(player.position);
            }
        }
        else
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= loseSightTime) { EnterState(EnemyState.Investigate); return; }
            
            if (pathUpdateTimer >= pathUpdateCooldown)
            {
                pathUpdateTimer = 0f;
                agent.SetDestination(lastKnownPos);
            }
        }

        Vector3 flat = player.position - transform.position; flat.y = 0f;
        if (canSee && flat.magnitude <= catchDistance) CatchPlayer();
    }

    void UpdateSearch(bool canSee)
    {
        if (canSee) { EnterState(EnemyState.Chase); return; }
        transform.Rotate(0f, searchTurnSpeed * Time.deltaTime, 0f);
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchDuration) EnterState(EnemyState.Patrol);
    }

    void NextWaypoint()
    {
        arrivalSoundPlayed = false;
        if (waypoints == null || waypoints.Length <= 1) return;
        
        if (randomPatrol)
        {
            int next;
            do { next = Random.Range(0, waypoints.Length); } while (next == waypointIndex);
            waypointIndex = next;
        }
        else
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
        }

        SetPatrolTarget();
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;
        if (stealth != null && stealth.IsHidden) return false;

        Vector3 eyePos = eyes != null ? eyes.position : transform.position + Vector3.up * 1.6f;
        Vector3 toPlayer = player.position - eyePos;
        float dist = toPlayer.magnitude;
        if (dist > viewDistance) return false;

        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        bool inCone = Vector3.Angle(transform.forward, flat) <= viewAngle * 0.5f;
        bool veryClose = flat.magnitude <= closeRange;
        if (!inCone && !veryClose) return false;

        RaycastHit[] hits = Physics.RaycastAll(eyePos, toPlayer.normalized, dist + 0.5f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        foreach (RaycastHit h in hits)
        {
            Collider c = h.collider;
            if (c.transform.IsChildOf(transform)) continue;
            if (c.isTrigger) continue; 
            if (c.attachedRigidbody != null && !c.attachedRigidbody.isKinematic && !IsPlayer(c)) continue;
            
            return IsPlayer(c);
        }
        return false;
    }

    bool IsPlayer(Collider c) => c.transform == player || c.transform.IsChildOf(player);

    public void HearNoise(Vector3 position)
    {
        Debug.Log("Heard noise at " + position);
        if (State == EnemyState.Chase) return;
        if (agent == null || !agent.isOnNavMesh) return;

        lastKnownPos = position;
        EnterState(EnemyState.Investigate);
    }

    void CatchPlayer()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 e = eyes != null ? eyes.position : transform.position + Vector3.up * 1.6f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, closeRange);
        Vector3 l = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward * viewDistance;
        Vector3 r = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward * viewDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(e, l);
        Gizmos.DrawRay(e, r);
    }
}

