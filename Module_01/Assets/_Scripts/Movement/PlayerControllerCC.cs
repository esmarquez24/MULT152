using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerCC : MonoBehaviour
{
    [Header("InputBridge")]
    [SerializeField] private InputBridge input;
    
    [Header("Move Speeds")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.0f;
    public float crouchSpeed = 1.8f;

    [Header("Input Filtering")]
    public float moveDeadzone = 0.15f;

    [Header("Smoothing")]
    public float accelTime = 0.12f;
    public float decelTime = 0.2f;

    [Header("Gravity")]
    public float gravity = -20f; // stronger than -9.81 for snappy grounding
    public float groundedStick = -2f; // small downward to keep grounded
    public LayerMask groundMask = ~0;

    [Header("Crouch")]
    public float standingHeight = 1.8f;
    public float crouchHeight = 1.2f;

    [Header("Sprint Stamina")]
    public float maxStamina = 5f;
    public float staminaDrainPerSec = 1.5f;
    public float staminaRegenPerSec = 1.0f;
    public float sprintCooldown = 0.25f;

    [Header("Jump")]
    public float jumpForce = 8f;

    CharacterController cc;
    Vector3 velocity;            // world-space velocity we apply via cc.Move
    Vector3 planarVelCurrent;    // current horizontal velocity (x,z)
    Vector3 planarVelRef;        // SmoothDamp ref
    float yVel;                  // vertical (gravity)
    float stamina;
    bool isCrouching;
    float lastSprintReleaseTime;

    // --- HUD accessors (add these) ---
    public float CurrentStamina => stamina;      // your existing stamina field
    public float MaxStamina => maxStamina;   // your existing maxStamina field
    public bool IsCrouching => isCrouching;  // your existing crouch flag
    public bool IsGrounded => cc ? cc.isGrounded : false;  // CharacterController grounded


    void Awake()
    {
        cc = GetComponent<CharacterController>();
        stamina = maxStamina;
        SetHeight(standingHeight);
    }

    void Update()
    {
        // ---- INPUT (OLD system) ----
        /*float x = Input.GetAxisRaw("Horizontal"); // A/D or left/right
        float z = Input.GetAxisRaw("Vertical");   // W/S or up/down
        bool sprintHeld = Input.GetKey(KeyCode.LeftShift);
        bool crouchHeld = Input.GetKey(KeyCode.LeftControl);*/

        Vector2 move = input ? input.Move : Vector2.zero;
        bool sprintHeld = input && input.sprintHeld;
        bool crouchHeld = input && input.crouchHeld;

        //Jump Input
        bool jumpPressed = input && input.jumpPressed;

        // --- Deadzone + no forced normalization ---
        // Filter tiny stick drift so it doesn't become full-speed.
        float mag = move.magnitude;
        Vector2 filtered = (mag >= moveDeadzone) ? move : Vector2.zero;

        // If someone mashes both axes > 1 (rare with keyboard), clamp to 1 without normalizing drift to 1.
        if (filtered.sqrMagnitude > 1f) filtered = filtered.normalized;

        //Replace x/z
        /*float x = move.x;
        float z = move.y;*/
        float x = filtered.x;
        float z = filtered.y;

        // desired speed
        bool wantsCrouch = crouchHeld;
        if (wantsCrouch && !isCrouching) StartCrouch();
        else if (!wantsCrouch && isCrouching) TryStand();

        float targetSpeed = isCrouching ? crouchSpeed : walkSpeed;
        bool canSprint = !isCrouching && stamina > 0.15f && Time.time - lastSprintReleaseTime > sprintCooldown;
        
        if (sprintHeld && canSprint && (Mathf.Abs(x) + Mathf.Abs(z) > 0.1f))
        {
            targetSpeed = runSpeed;
            stamina = Mathf.Max(0f, stamina - staminaDrainPerSec * Time.deltaTime);
            if (stamina <= 0f) lastSprintReleaseTime = Time.time;
        }
        else
        {
            stamina = Mathf.Min(maxStamina, stamina + staminaRegenPerSec * Time.deltaTime);
            if (!sprintHeld) lastSprintReleaseTime = Time.time;
        }

        // world-space desired planar velocity
        Vector3 inputDir = new Vector3(x, 0, z);
        Vector3 desiredPlanar = transform.TransformDirection(inputDir) * targetSpeed;

        // smooth accel/decel
        float smoothTime = (desiredPlanar.magnitude > planarVelCurrent.magnitude) ? accelTime : decelTime;
        planarVelCurrent = Vector3.SmoothDamp(planarVelCurrent, desiredPlanar, ref planarVelRef, smoothTime);

        // gravity + grounding
        bool grounded = cc.isGrounded;

        if (grounded)
        {
            // Snap to ground with a small downward velocity
            if (yVel < 0f) yVel = groundedStick;

            // Jump only on the press frame
            if (jumpPressed)
            {
                // Use set, not +=, to avoid stacking
                yVel = jumpForce;
                Debug.Log("Player Jumped");
            }
        }
        else
        {
            // Apply gravity while airborne
            yVel += gravity * Time.deltaTime;
        }

        // compose final velocity
        velocity = new Vector3(planarVelCurrent.x, yVel, planarVelCurrent.z);

        // move (handles step offset & sliding)
        cc.Move(velocity * Time.deltaTime);
    }

    void StartCrouch()
    {
        isCrouching = true;
        SetHeight(crouchHeight);
    }

    void TryStand()
    {
        // safety: don't stand up into a ceiling
        float check = standingHeight - cc.height;
        if (check >= 0.01f) { isCrouching = false; SetHeight(standingHeight); return; }

        Vector3 top = transform.position + Vector3.up * (cc.height * 0.5f);
        float radius = cc.radius * 0.95f;
        if (!Physics.CheckCapsule(top, top + Vector3.up * check, radius, groundMask))
        {
            isCrouching = false;
            SetHeight(standingHeight);
        }
    }

    void SetHeight(float h)
    {
        float centerAdjust = (h - cc.height) * 0.5f;
        cc.height = h;
        cc.center = new Vector3(cc.center.x, cc.center.y - centerAdjust, cc.center.z);
    }

    // Push lightweight rigidbodies without losing CC precision
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var rb = hit.collider.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
        {
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            rb.AddForce(pushDir * 3f, ForceMode.Impulse);
        }
    }
}