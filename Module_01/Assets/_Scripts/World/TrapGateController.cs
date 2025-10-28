using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrapGateController : MonoBehaviour
{
    [Header("References")]
    public DoorAutoOpen door;
    public MovingWall wallLeft;
    public MovingWall wallRight;
    public HealthComponent playerHealth; // optional (auto-found by tag if null)

    [Header("Timing")]
    [Tooltip("Seconds until the trap kills the player if walls haven't already.")]
    public float killTimerSeconds = 8f;

    [Header("Rules")]
    public bool oneShot = true;
    public bool lockDoorOnTrigger = true;

    bool _armed;
    Collider _col;

    void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;

        if (!playerHealth)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO) playerHealth = playerGO.GetComponentInChildren<HealthComponent>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_armed && oneShot) return;

        _armed = true;

        if (lockDoorOnTrigger && door) door.SetLocked(true);

        if (wallLeft) wallLeft.Arm();
        if (wallRight) wallRight.Arm();

        if (killTimerSeconds > 0f) StartCoroutine(KillTimer());
    }

    IEnumerator KillTimer()
    {
        float t = killTimerSeconds;
        while (t > 0f)
        {
            if (playerHealth && playerHealth.IsDead) yield break; // already dead by wall
            t -= Time.deltaTime;
            yield return null;
        }
        if (playerHealth && !playerHealth.IsDead)
        {
            // Lethal damage = current HP (or large)
            playerHealth.Damage(playerHealth.Current > 0 ? playerHealth.Current : 9999);
        }
    }
}