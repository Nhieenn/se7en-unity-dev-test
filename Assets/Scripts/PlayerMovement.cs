using UnityEngine;

/// <summary>
/// Handles player movement using WASD / Arrow keys, smooth rotation towards movement direction,
/// and updates Animator blend parameter for Idle/Run transitions.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravity = -20f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string blendParameter = "Blend";
    [SerializeField] private float animationDampTime = 0.1f;

    private CharacterController characterController;
    private Vector3 verticalVelocity;
    private int blendHash;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        blendHash = Animator.StringToHash(blendParameter);
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. Read input from WASD or Arrow Keys
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 2. Movement logic
        Vector3 moveVelocity = Vector3.zero;
        if (inputDirection.sqrMagnitude > 0.001f)
        {
            // Move in horizontal plane
            moveVelocity = inputDirection * moveSpeed;

            // Smooth rotation towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 3. Apply gravity to keep character grounded
        if (characterController.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f; // Slight downward force to stay grounded
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }

        Vector3 finalMove = (moveVelocity + verticalVelocity) * Time.deltaTime;
        characterController.Move(finalMove);

        // 4. Update Animator parameter (Blend: 0 = Idle, >0 = Walk/Run)
        // In AnimatorController_Jamo: 0 is Idle, 0.25 is Walk, 0.6 is Run
        if (animator != null)
        {
            float targetBlend = inputDirection.sqrMagnitude > 0.001f ? 0.6f : 0f;
            animator.SetFloat(blendHash, targetBlend, animationDampTime, Time.deltaTime);
        }
    }
}
