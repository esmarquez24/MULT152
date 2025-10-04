using UnityEngine;
using UnityEngine.UI;  // UGUI
using TMPro;
// If you prefer TextMeshPro, see the TMP notes below.

public class PlayerHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerControllerCC controller;  // assign your player controller
    public InputBridge input;              // assign your input bridge

    [Header("UI - Stamina")]
    public Slider staminaSlider;           // UI Slider
    public Image staminaFill;             // optional: the Fill image (for tinting)

    [Header("UI - Labels (UGUI)")]
    public TextMeshProUGUI jumpLabel;                 // e.g., "Jump: ON/OFF"   Replace TextMeshProUGUI with Text for the old way
    public TextMeshProUGUI crouchLabel;               // e.g., "Crouch: ON/OFF"

    [Header("Colors")]
    public Color onColor = new Color(0.2f, 1f, 0.2f, 1f);
    public Color offColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Header("Jump Flash")]
    public float jumpFlashDuration = 0.2f; // seconds to show "Jump: ON" after press
    float _jumpFlashTimer;

    void Reset()
    {
        // Try to auto-wire in editor
        if (!controller) controller = Object.FindAnyObjectByType<PlayerControllerCC>();
        if (!input) input = Object.FindAnyObjectByType<InputBridge>();
    }

    void Start()
    {
        if (!controller)
        {
            controller = Object.FindAnyObjectByType<PlayerControllerCC>();
        }
        if (!input)
        {
            input = Object.FindAnyObjectByType<InputBridge>();
        }

        if (staminaSlider)
        {
            // Configure slider range based on controller values (fallbacks if zero)
            float max = controller ? Mathf.Max(1f, controller.MaxStamina) : 100f;
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = max;
        }
    }

    void Update()
    {
        if (!controller) return;

        // --- Stamina bar ---
        if (staminaSlider)
        {
            staminaSlider.value = controller.CurrentStamina;

            // Optional: tint when low or sprinting, etc.
            if (staminaFill)
            {
                // Example: fade to red under 25%
                float pct = controller.MaxStamina > 0f ? controller.CurrentStamina / controller.MaxStamina : 0f;
                staminaFill.color = Color.Lerp(offColor, onColor, pct);
            }
        }

        // --- Jump indicator ---
        // Flash ON when jump is pressed (edge-trigger from InputBridge)
        if (input && input.jumpPressed)
        {
            _jumpFlashTimer = jumpFlashDuration;
        }
        if (_jumpFlashTimer > 0f) _jumpFlashTimer -= Time.deltaTime;

        if (jumpLabel)
        {
            bool on = _jumpFlashTimer > 0f;
            jumpLabel.text = on ? "Jump: ON" : "Jump: OFF";
            jumpLabel.color = on ? onColor : offColor;
        }

        // --- Crouch indicator (live) ---
        if (crouchLabel)
        {
            bool crouching = controller.IsCrouching;
            crouchLabel.text = crouching ? "Crouch: ON" : "Crouch: OFF";
            crouchLabel.color = crouching ? onColor : offColor;
        }
    }
}