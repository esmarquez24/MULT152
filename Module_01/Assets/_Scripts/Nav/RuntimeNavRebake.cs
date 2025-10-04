using UnityEngine;
using Unity.AI.Navigation;   // <- required for NavMeshSurface

public class RuntimeNavRebake : MonoBehaviour
{
    [Tooltip("NavMeshSurface to rebuild at runtime")]
    public NavMeshSurface surface;

    [Tooltip("Seconds to wait after last request before rebuilding")]
    public float debounce = 0.25f;

    float timer;
    bool pending;

    public void RequestRebuild()
    {
        pending = true;
        timer = 0f;
    }

    void Update()
    {
        if (!pending || !surface) return;

        timer += Time.deltaTime;
        if (timer >= debounce)
        {
            // NOTE: synchronous; expect a frame spike proportional to surface size/complexity
            surface.BuildNavMesh();
            pending = false;
        }
    }
}