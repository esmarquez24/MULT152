using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimSpeedDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerControllerCC_UD mover; // your Week 5 controller script


    [SerializeField] private Transform meshTransform;
    [SerializeField] private Vector3 meshOffset;

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

    void LateUpdate()
    {
        if (meshTransform)
        {
            meshTransform.localPosition = meshOffset;
        }
    }
}