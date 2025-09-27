using UnityEngine;
using UnityEngine.InputSystem;

public class DamageOnKey : MonoBehaviour
{
    [SerializeField] private HealthComponent target;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private Key key = Key.K;

    void Update()
    {
        if (Keyboard.current[key].wasPressedThisFrame)

        {
            target?.Damage(damageAmount);
        }
    }
}