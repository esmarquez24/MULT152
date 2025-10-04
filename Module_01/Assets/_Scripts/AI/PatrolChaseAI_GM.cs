using UnityEngine;
using UnityEngine.AI;

public class PatrolChaseAI_GM : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;      // assign in Inspector, or it will auto-find in children
    public Transform[] points;
    public Transform player;        // assign scene instance (or tagged "Player")

    [Header("Distances")]
    public float chaseDistance = 8f;
    public float loseDistance = 12f;
    public float eyeHeight = 1.3f;   // Y offset for LOS (raised a bit to avoid floors)

    [Header("Pathing")]
    [Range(0.05f, 1f)] public float repathRate = 0.2f;
    public float sampleRange = 2f;       // sampling radius for player/agent positions

    [Header("LOS")]
    public LayerMask losMask = ~0;       // which layers can block sight (e.g., Walls/Ground)

    enum State { Patrol, Chase }
    State state = State.Patrol;
    int index;
    float wait;
    float timer;

    // scratch for non-alloc LOS
    static readonly RaycastHit[] _losHits = new RaycastHit[8];

    void Start()
    {
        // Allow the script to live on a parent/manager object
        if (!agent)
        {
            agent = GetComponentInChildren<NavMeshAgent>();
            if (!agent)
            {
                Debug.LogError($"[{name}] No NavMeshAgent assigned or found in children. Disabling AI.");
                enabled = false;
                return;
            }
        }

        // Auto-fix common mistake: player assigned to prefab instead of scene instance
        if (!player || !player.gameObject.scene.IsValid())
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (points != null && points.Length > 0)
            SafeSetDestination(points[0].position);
    }

    void Update()
    {
        if (!agent) return;
        timer += Time.deltaTime;

        // Use the agent's *real* position source (Transform if updatePosition, otherwise nextPosition)
        Vector3 agentPos = (!agent.updatePosition) ? agent.nextPosition : agent.transform.position;
        Vector3 playerPos = player ? player.position : Vector3.negativeInfinity;

        float dist = player ? Vector3.Distance(agentPos, playerPos) : Mathf.Infinity;
        bool hasLOS = player && HasLineOfSight(agentPos, playerPos);

        switch (state)
        {
            case State.Patrol:
                PatrolTick();
                if (player && dist <= chaseDistance && hasLOS)
                {
                    state = State.Chase;
                    agent.ResetPath();
#if UNITY_EDITOR
                    Debug.Log($"[{name}] → CHASE (dist={dist:F2}, LOS={hasLOS})");
#endif
                }
                break;

            case State.Chase:
                if (timer >= repathRate && player)
                {
                    SafeSetDestination(playerPos);
                    timer = 0f;
                }
                if (dist >= loseDistance || !hasLOS)
                {
                    state = State.Patrol;
                    agent.ResetPath();
                    if (points != null && points.Length > 0)
                        SafeSetDestination(points[index].position);
#if UNITY_EDITOR
                    Debug.Log($"[{name}] → PATROL (dist={dist:F2}, LOS={hasLOS})");
#endif
                }
                break;
        }

#if UNITY_EDITOR
        // Debug line to visualize the LOS each frame
        if (player)
        {
            var from = agentPos + Vector3.up * eyeHeight;
            var to = playerPos + Vector3.up * eyeHeight;
            Debug.DrawLine(from, to, hasLOS ? Color.green : Color.red);

            // Console readout
            Debug.Log(
                $"[AI DEBUG] State={state} | Dist={dist:F2} | InChaseRange={dist <= chaseDistance} | " +
                $"InLoseRange={dist >= loseDistance} | HasLOS={hasLOS}"
            );
        }
#endif
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

    bool HasLineOfSight(Vector3 fromPos, Vector3 toPos)
    {
        // Raise both ends so we don't skim floors/steps
        Vector3 from = fromPos + Vector3.up * eyeHeight;
        Vector3 to = toPos + Vector3.up * eyeHeight;
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;
        dir /= dist;

        // Raycast only up to the player distance, against occluders
        int count = Physics.RaycastNonAlloc(new Ray(from, dir), _losHits, dist, losMask, QueryTriggerInteraction.Ignore);

        // Find nearest valid blocker (ignore self/children and the player/children)
        float nearest = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            var h = _losHits[i];
            if (h.distance <= 0f) continue;
            var t = h.transform;

            if (t == agent.transform || t.IsChildOf(agent.transform)) continue; // ignore self
            if (player && (t == player || t.IsChildOf(player))) continue;       // ignore player

            if (h.distance < nearest) nearest = h.distance;
        }

        return !(nearest < float.PositiveInfinity);
    }

    void SafeSetDestination(Vector3 targetPos)
    {
        if (!agent) return;

        // Ensure the agent itself is on a NavMesh
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(agent.transform.position, out var selfHit, sampleRange, NavMesh.AllAreas))
                agent.Warp(selfHit.position);
            else
                return; // can't recover
        }

        // Sample the target onto the NavMesh and path to it
        if (NavMesh.SamplePosition(targetPos, out var navHit, sampleRange, NavMesh.AllAreas))
        {
            var path = new NavMeshPath();
            if (agent.CalculatePath(navHit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                agent.SetDestination(navHit.position);
        }
    }
}