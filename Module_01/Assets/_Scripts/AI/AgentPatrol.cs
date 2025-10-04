using UnityEngine;
using UnityEngine.AI;

public class AgentPatrol : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] points;
    public float waitAtPoint = 0.5f;
    public float sampleRange = 2f;     // how far we sample to snap to navmesh
    public bool teleportOnFail = false;// demo option: warp if point is unreachable

    int index;
    float wait;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        TryGo(points != null && points.Length > 0 ? points[0].position : transform.position);
    }

    void Update()
    {
        if (!agent || agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            wait += Time.deltaTime;
            if (wait >= waitAtPoint && points != null && points.Length > 0)
            {
                index = (index + 1) % points.Length;
                TryGo(points[index].position);
                wait = 0f;
            }
        }
    }

    void TryGo(Vector3 targetPos)
    {
        if (!agent) return;
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var selfHit, sampleRange, NavMesh.AllAreas))
                agent.Warp(selfHit.position);
            else return; // still not on mesh
        }

        if (NavMesh.SamplePosition(targetPos, out var navHit, sampleRange, NavMesh.AllAreas))
        {
            var path = new NavMeshPath();
            if (agent.CalculatePath(navHit.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetDestination(navHit.position);
            }
            else if (teleportOnFail)
            {
                agent.Warp(navHit.position);
            }
        }
    }
}