using UnityEngine;
using UnityEngine.AI;

public class PatrolChaseAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] points;
    public Transform player;

    [Header("Distances")]
    public float chaseDistance = 8f;
    public float loseDistance = 12f;
    public float eyeHeight = 0.5f;     // Y offset for LOS

    [Header("Pathing")]
    [Range(0.05f, 1f)] public float repathRate = 0.2f;
    public float sampleRange = 2f;         // sampling radius for player/agent positions

    [Header("LOS")]
    public LayerMask losMask = ~0;         // which layers can block sight

    [Header("Collision")]
    public string playerTag = "Player"; // tag to check for destroy on hit

    enum State { Patrol, Chase }
    State state = State.Patrol;
    int index; float wait; float timer;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (points != null && points.Length > 0)
            SafeSetDestination(points[0].position);
    }

    void Update()
    {
        if (!agent) return;
        timer += Time.deltaTime;

        float dist = player ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;
        bool hasLOS = player && HasLineOfSight();

        switch (state)
        {
            case State.Patrol:
                PatrolTick();
                if (player && dist <= chaseDistance && hasLOS)
                {
                    state = State.Chase;
                    Debug.Log("State changed to CHASE");
                    agent.ResetPath();
                }
                break;

            case State.Chase:
                if (timer >= repathRate && player)
                {
                    SafeSetDestination(player.position);
                    timer = 0f;
                }
                if (dist >= loseDistance || !hasLOS)
                {
                    state = State.Patrol;
                    Debug.Log("State changed to PATROL");
                    agent.ResetPath();
                    if (points != null && points.Length > 0)
                        SafeSetDestination(points[index].position);
                }
                break;
        }
    }

    void PatrolTick()
    {
        if (agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            wait += Time.deltaTime;
            if (wait >= 0.5f && points != null && points.Length > 0)
            {
                index = (index + 1) % points.Length;
                SafeSetDestination(points[index].position);
                wait = 0f;
            }
        }
    }

    bool HasLineOfSight()
    {
        var from = transform.position + Vector3.up * eyeHeight;
        var to = player.position + Vector3.up * eyeHeight;
        // LOS is "clear" if the linecast hits nothing before the player
        return !Physics.Linecast(from, to, losMask);
    }

    void SafeSetDestination(Vector3 targetPos)
    {
        if (!agent) return;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var selfHit, sampleRange, NavMesh.AllAreas))
            {
                agent.Warp(selfHit.position);
            }
            else return; // can't recover
        }

        if (NavMesh.SamplePosition(targetPos, out var navHit, sampleRange, NavMesh.AllAreas))
        {
            var path = new NavMeshPath();
            if (agent.CalculatePath(navHit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(navHit.position);
            }
        }
    }

    //----Collision Handler-----
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[{name}] collided with Player - you got caught");
        if (collision.gameObject.CompareTag(playerTag))
        {
            Debug.Log($"[{name}] collided with Player - you got caught");
            Destroy(collision.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"[{name}] Triggered by Player - destroying self");
            Destroy(other.gameObject);
        }
    }
}