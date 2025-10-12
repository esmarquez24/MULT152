using UnityEngine;
using UnityEngine.InputSystem;

public class AmmoSim : MonoBehaviour
{
    [SerializeField] private AmmoUI ui;
    [SerializeField] private InputBridge input;

    [SerializeField] private UITK_HUD hud; 

    private int current = 30;
    private int magSize = 30;
    private int reserves = 90;

    void Start()
    {
        if (ui != null)
        {
            ui.Set(current, magSize, reserves);
        }
        else
        {
            Debug.LogWarning("[AmmoSim] AmmoUI reference not set.");
        }
    }

    
void Update()
    {
        if (input == null) return;

        // Fire input
        if (input.firePressed && current > 0)
        {
            current--;
            ui.Set(current, magSize, reserves);
            hud.SetAmmo(current, magSize, reserves);
            Debug.Log($"[AmmoSim] Fired! Ammo: {current}/{magSize} ({reserves})");
        }

        // Reload input
        if (input.reloadPressed && current < magSize && reserves > 0)
        {
            int need = magSize - current;
            int take = Mathf.Min(need, reserves);
            current += take;
            reserves -= take;
            ui.Set(current, magSize, reserves);
            hud.SetAmmo(current, magSize, reserves);
            Debug.Log($"[AmmoSim] Reloaded! Ammo: {current}/{magSize} ({reserves})");
        }
    }
}