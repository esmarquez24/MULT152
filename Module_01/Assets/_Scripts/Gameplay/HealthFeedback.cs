using UnityEngine;

public class HealthFeedback : MonoBehaviour
{
    [SerializeField] private HealthComponent target;
    [SerializeField] public AudioSource hitSfx;
    [SerializeField] private GameObject deathFx;

    private void OnEnable()
    {
        if (target == null) return;
        target.OnDamaged += HandleDamaged;
        target.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (target == null) return;
        target.OnDamaged -= HandleDamaged;
        target.OnDied -= HandleDied;
    }


    private void HandleDamaged(int amt)
    {
        if (hitSfx != null) hitSfx.Play();
        // e.g., flash material, screenshake hook, etc.
    }

    private void HandleDied()
    {
        if (deathFx != null) Instantiate(deathFx, target.transform.position, Quaternion.identity);
        // Disable controls/renderer, etc.

                 Debug.Log(gameObject.name + "has DIED!");
                 Destroy(gameObject);
    }
}