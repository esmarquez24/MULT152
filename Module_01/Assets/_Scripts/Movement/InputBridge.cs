using UnityEngine;
using UnityEngine.InputSystem;

public class InputBridge : MonoBehaviour
{
    // Cached values your controller will read in Update()
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool sprintHeld { get; private set; }
    //public bool CrouchPressed { get; private set; } // edge-triggered this frame
    public bool crouchHeld { get; private set; }

    // Optional sensitivity (for Look scaling)
    [Header("Look Tuning")]
    public float lookSensitivity = 1.0f; // multiply mouse/right-stick

    // These are called by PlayerInput (Invoke Unity Events)
    public void OnMove(InputAction.CallbackContext ctx)
    {
        Move = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        // Read raw and scale-mouse delta is already frame-scaled by the system
        Look = ctx.ReadValue<Vector2>() * lookSensitivity;
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        // Buttons: started/performed/canceled-treat performed as held
        if (ctx.performed) sprintHeld = true;
        if (ctx.canceled) sprintHeld = false;
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        // Edge trigger: fire only on performed
        if (ctx.performed) { crouchHeld = true; } // pressed this frame
    if (ctx.canceled)  { crouchHeld = false; }
    }

    /*void LateUpdate()
    {
        // Reset edge-trigger flags after consumers had a chance to read them
        CrouchPressed = false;
    }*/
}