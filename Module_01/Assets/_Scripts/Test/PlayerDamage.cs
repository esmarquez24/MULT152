using UnityEngine;
using System.Collections.Generic;

public class PlayerDamage : MonoBehaviour
{
    [Header("Damage")]
    [Min(0f)] public float damageAmount = 20f;

    [Tooltip("Optional. Leave empty to ignore tags entirely.")]
    public string enemyTag = "Enemy";

    [Tooltip("Cooldown between hits against the SAME enemy (seconds).")]
    public float perTargetCooldown = 0.25f;

    // track last time we hit each enemy so CharacterController collisions don't spam
    private readonly Dictionary<EnemyHealth, float> lastHitAt = new();

    // For CharacterController-based player controllers
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        TryDealDamage(hit.collider);
    }

    // For Rigidbody-based players
    private void OnCollisionEnter(Collision collision)
    {
        TryDealDamage(collision.collider);
    }

    private void TryDealDamage(Collider other)
    {
        if (!other) return;

        // Optional tag gate. Leave enemyTag empty to skip this.
        if (!string.IsNullOrEmpty(enemyTag))
        {
            // Check collider or its attached rigidbody (common when the tag is on the root)
            if (!other.CompareTag(enemyTag) &&
                !(other.attachedRigidbody && other.attachedRigidbody.CompareTag(enemyTag)))
            {
                return;
            }
        }

        // Find EnemyHealth on this collider or any parent
        var enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null) return;

        // Per-target cooldown to avoid multiple hits per frame
        if (lastHitAt.TryGetValue(enemyHealth, out float lastTime))
        {
            if (Time.time - lastTime < perTargetCooldown) return;
        }

        enemyHealth.TakeDamage(damageAmount);
        lastHitAt[enemyHealth] = Time.time;
        // Debug.Log($"Dealt {damageAmount} to {enemyHealth.name}");
    }
}