using UnityEngine;
using UnityEngine.InputSystem;

public class HealOnKey : MonoBehaviour
{
    [SerializeField] private HealthComponent target;
    [SerializeField] private int healAmount = 5;
    [SerializeField] private Key key = Key.H;

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[key].wasPressedThisFrame)
        {
            target?.Heal(healAmount);
        }
    }
}