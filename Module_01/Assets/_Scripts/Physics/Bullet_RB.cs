using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BulletPhysics : MonoBehaviour
{
    public float speed = 500f;  // Force strength
    public float lifeTime = 3f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rb.AddForce(transform.forward * speed);
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        var enemy = collision.collider.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(10f);
        }

        Destroy(gameObject);
    }
}