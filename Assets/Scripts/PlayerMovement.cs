using System.Collections;
using UnityEngine;

/// <summary>
/// State Machine for Player States (Idle, Moving, HappyCelebration).
/// Listens to EventBus for Goal Scored events to trigger celebration animation.
/// While in Celebration state, player input is locked and animation is forced to Happy.
/// </summary>
public enum PlayerStateType
{
    Idle,
    Moving,
    HappyCelebration
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravity = -20f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string blendParameter = "Blend";
    [SerializeField] private string happyTrigger = "happy";
    [SerializeField] private string normalTrigger = "normal";
    [SerializeField] private float animationDampTime = 0.1f;
    [SerializeField] private float celebrationDuration = 2.5f;

    public PlayerStateType CurrentState { get; private set; } = PlayerStateType.Idle;

    private CharacterController characterController;
    private float verticalSpeed = 0f;
    private int blendHash;
    private int happyHash;
    private int normalHash;
    private Coroutine celebrationRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        characterController = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        blendHash = Animator.StringToHash(blendParameter);
        happyHash = Animator.StringToHash(happyTrigger);
        normalHash = Animator.StringToHash(normalTrigger);
    }

    private void OnEnable()
    {
        EventBus.OnBallScored += HandleGoalScored;
    }

    private void OnDisable()
    {
        EventBus.OnBallScored -= HandleGoalScored;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case PlayerStateType.Idle:
            case PlayerStateType.Moving:
                HandleMovementState();
                break;

            case PlayerStateType.HappyCelebration:
                HandleCelebrationState();
                break;
        }
    }

    private void HandleMovementState()
    {
        // 1. Read input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // 2. State transition between Idle and Moving
        if (inputDirection.sqrMagnitude > 0.001f)
        {
            CurrentState = PlayerStateType.Moving;

            // Move & Rotate
            Vector3 moveVelocity = inputDirection * moveSpeed;
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            ApplyGravityAndMove(moveVelocity);
        }
        else
        {
            CurrentState = PlayerStateType.Idle;
            ApplyGravityAndMove(Vector3.zero);
        }

        // 3. Update Animator Blend
        if (animator != null)
        {
            float targetBlend = (CurrentState == PlayerStateType.Moving) ? 0.6f : 0f;
            animator.SetFloat(blendHash, targetBlend, animationDampTime, Time.deltaTime);
        }
    }

    private void HandleCelebrationState()
    {
        // Force Blend to 0 so the Happy Blend Tree plays Idle_Happy animation instead of running
        if (animator != null)
        {
            animator.SetFloat(blendHash, 0f);
        }

        // Apply only downward gravity to keep player grounded, ignoring any WASD input keys
        ApplyGravityAndMove(Vector3.zero);
    }

    private void ApplyGravityAndMove(Vector3 moveVelocity)
    {
        if (characterController.isGrounded)
        {
            if (verticalSpeed < 0f)
            {
                verticalSpeed = -5f;
            }
        }
        else
        {
            verticalSpeed += gravity * Time.deltaTime;
        }

        moveVelocity.y = verticalSpeed;
        characterController.Move(moveVelocity * Time.deltaTime);
    }

    private void HandleGoalScored(SoccerBall ball, Vector3 goalPos)
    {
        // Transition to Happy Celebration State
        if (celebrationRoutine != null)
        {
            StopCoroutine(celebrationRoutine);
        }
        celebrationRoutine = StartCoroutine(CelebrateRoutine());
    }

    private IEnumerator CelebrateRoutine()
    {
        CurrentState = PlayerStateType.HappyCelebration;

        if (animator != null)
        {
            // Reset any conflicting triggers and set happy
            animator.ResetTrigger(normalHash);
            animator.SetTrigger(happyHash);
            animator.SetFloat(blendHash, 0f); // Ensure it triggers Idle Happy (Blend=0)
        }

        yield return new WaitForSeconds(celebrationDuration);

        // Return to Normal State
        if (animator != null)
        {
            animator.ResetTrigger(happyHash);
            animator.SetTrigger(normalHash);
        }

        CurrentState = PlayerStateType.Idle;
        celebrationRoutine = null;

        // Unlock kick actions
        EventBus.TriggerKickSequenceCompleted();
    }
}
