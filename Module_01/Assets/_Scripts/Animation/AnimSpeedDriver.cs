using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimSpeedDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerControllerCC_UD mover; // your Week 5 controller script

    void Reset()
    {
        animator = GetComponent<Animator>();
        if (!mover) mover = GetComponent<PlayerControllerCC_UD>();
    }

    void Update()
    {
        if (!animator || !mover) return;
        float speed = Mathf.Max(0f, mover.CurrentSpeed); // m/s
        animator.SetFloat("Speed", speed);
        animator.SetBool("IsMoving", speed > 0.1f);
    }
}