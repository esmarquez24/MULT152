using UnityEngine;
using UnityEngine.AI;

public class AgentChaseTarget : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target;
    [Range(0.05f, 1f)] public float repathRate = 0.2f; // seconds
    float t;

    void Update()
    {
        if (!agent || !target) return;
        t += Time.deltaTime;
        if (t >= repathRate && agent.isOnNavMesh)
        {
            // Sample target in case it’s slightly off the baked surface
            if (NavMesh.SamplePosition(target.position, out var navHit, 2f, NavMesh.AllAreas))
                agent.SetDestination(navHit.position);
            t = 0f;
        }
    }
}