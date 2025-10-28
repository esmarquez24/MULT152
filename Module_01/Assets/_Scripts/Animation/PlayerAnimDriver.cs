using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerAnimDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller; // optional but nice for speed calc
    [SerializeField] private Animator animator;
    [SerializeField] private InputActionAsset actions; // assign your .inputactions
    [SerializeField] private string mapName = "Gameplay";   // your action map
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string sprintActionName = "Sprint";

    [Header("Speeds (m/s)")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.5f;
    public bool useControllerVelocity = false; // if true, derive Speed from CC velocity.magnitude

    InputAction _move;
    InputAction _sprint;

    void Reset()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (actions)
        {
            var map = actions.FindActionMap(mapName, throwIfNotFound: false);
            if (map != null)
            {
                _move = map.FindAction(moveActionName, false);
                _sprint = map.FindAction(sprintActionName, false);
                _move?.Enable();
                _sprint?.Enable();
            }
        }
    }

    void OnDisable()
    {
        _move?.Disable();
        _sprint?.Disable();
    }

    void Update()
    {
        float speed = 0f;

        if (useControllerVelocity && controller != null)
        {
            // derive speed from actual movement
            var v = controller.velocity; v.y = 0f;
            speed = v.magnitude;
        }
        else if (_move != null)
        {
            Vector2 move = _move.ReadValue<Vector2>();
            bool sprinting = _sprint != null && _sprint.IsPressed();
            float target = sprinting ? runSpeed : walkSpeed;
            speed = move.sqrMagnitude > 0.0001f ? target : 0f;
        }

        animator.SetFloat("Speed", speed);
        animator.SetBool("IsMoving", speed > 0.1f);
    }
}