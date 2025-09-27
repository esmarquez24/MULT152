using UnityEngine;

public class CollisionDemo : MonoBehaviour
{
    // Non-trigger collision
    private void OnCollisionEnter(Collision c)
    {
        Debug.Log($"Collision with {c.collider.name}. RelativeVel={c.relativeVelocity}");
    }

    // Trigger volume
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Entered trigger: {other.name}");
        if (other.CompareTag("Pickup"))
        {
            // Example: disable and "collect"
            other.gameObject.SetActive(false);
        }
    }
}