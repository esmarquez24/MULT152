using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MovingWall : MonoBehaviour
{
    [Header("Motion")]
    public Vector3 moveDirection = Vector3.right; // local-space normalized
    public float travelDistance = 1.5f;           // meters
    public float speed = 1.0f;                    // m/s
    public bool startEnabled = false;

    [Header("Damage")]
    public int contactDamage = 9999;              // lethal by default

    Vector3 _startLocalPos;
    float _moved;
    bool _active;

    void Awake()
    {
        _startLocalPos = transform.localPosition;

        var col = GetComponent<Collider>();
        col.isTrigger = true; // detect player overlap to kill

        moveDirection = moveDirection.sqrMagnitude > 0f ? moveDirection.normalized : Vector3.right;
        _active = startEnabled;
    }

    public void Arm()
    {
        _active = true;
        _moved = 0f;
        transform.localPosition = _startLocalPos;
    }

    public void DisarmAndReset()
    {
        _active = false;
        _moved = 0f;
        transform.localPosition = _startLocalPos;
    }

    void Update()
    {
        if (!_active) return;

        float step = speed * Time.deltaTime; // paused by Time.timeScale (wanted)
        float remaining = Mathf.Max(0f, travelDistance - _moved);
        float move = Mathf.Min(step, remaining);

        transform.localPosition += moveDirection * move;
        _moved += move;

        if (_moved >= travelDistance) _active = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var hp = other.GetComponentInParent<HealthComponent>() ?? other.GetComponent<HealthComponent>();
        if (hp != null && !hp.IsDead)
            hp.Damage(Mathf.Max(1, contactDamage));
    }
}