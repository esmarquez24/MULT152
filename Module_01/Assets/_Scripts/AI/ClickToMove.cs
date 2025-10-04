using UnityEngine;
using UnityEngine.AI;

public class ClickToMove : MonoBehaviour
{
    [SerializeField] Camera cam;                 // if not assigned, falls back to Camera.main
    [SerializeField] NavMeshAgent agent;
    [SerializeField] float maxProjectDist = 5f;  // how far from the hit point we’ll search the navmesh
    [SerializeField] LayerMask rayMask = ~0;     // optional: restrict which colliders you can click

    void Awake()
    {
        if (!cam) cam = Camera.main;
        // Optional safety: snap agent onto the mesh at start
        if (agent && NavMesh.SamplePosition(agent.transform.position, out var navHit, 2f, NavMesh.AllAreas))
            agent.Warp(navHit.position);
    }

    void Update()
    {
        if (!agent || !cam) return;

        // Right mouse button = move
        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, 2000f, rayMask))
            {
                // Project the click onto the nearest navmesh point to avoid off-mesh destinations
                if (NavMesh.SamplePosition(hit.point, out var navHit, maxProjectDist, NavMesh.AllAreas) && agent.isOnNavMesh)
                {
                    // Optional: verify path is reachable before committing (avoids “stuck” orders)
                    var path = new NavMeshPath();
                    if (agent.CalculatePath(navHit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                        agent.SetDestination(navHit.position);
                }
            }
        }
    }
}