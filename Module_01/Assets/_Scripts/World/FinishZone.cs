using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FinishZone : MonoBehaviour
{
    [Header("References")]
    public HealthComponent playerHealth; // optional, auto-found by Player tag if null
    public WinUI winUI;                  // optional, auto-found if null

    [Header("Rules")]
    public bool oneShot = true;

    bool _triggered;
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

        if (!winUI)
            winUI = WinUI.Instance ?? FindAnyObjectByType<WinUI>(FindObjectsInactive.Include);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_triggered && oneShot) return;
        _triggered = true;

        // Must be alive to win
        if (playerHealth && playerHealth.IsDead) return;

        if (!winUI)
            winUI = WinUI.Instance ?? FindAnyObjectByType<WinUI>(FindObjectsInactive.Include);

        if (winUI) winUI.Show();
        else Debug.LogWarning("[FinishZone] WinUI not found; cannot show win screen.");
    }
}