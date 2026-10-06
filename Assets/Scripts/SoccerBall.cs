using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Represents an individual soccer ball with flight physics/animation towards a target goal,
/// particle triggering on arrival, and camera follow support.
/// </summary>
public class SoccerBall : MonoBehaviour
{
    [Header("Kick Settings")]
    [SerializeField] private float flightDuration = 1.2f;
    [SerializeField] private float arcHeight = 3.5f;
    [SerializeField] private float spinSpeed = 720f;

    [Header("Effects")]
    [Tooltip("Particle effect prefab to spawn when ball reaches goal.")]
    [SerializeField] private GameObject confettiPrefab;

    public bool IsKicked { get; private set; } = false;

    private Rigidbody rb;
    private Collider ballCollider;

    public event Action<SoccerBall> OnGoalReached;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ballCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        BallManager.RegisterBall(this);
    }

    private void OnDisable()
    {
        BallManager.UnregisterBall(this);
    }

    /// <summary>
    /// Kicks the ball towards the closest goal.
    /// </summary>
    public void KickToGoal(Goal targetGoal = null)
    {
        if (IsKicked) return;

        if (targetGoal == null)
        {
            targetGoal = Goal.GetClosestGoal(transform.position);
        }

        if (targetGoal == null)
        {
            Debug.LogWarning("[SoccerBall] No Goal found in scene to kick towards!");
            return;
        }

        IsKicked = true;

        // Disable physics during controlled flight trajectory
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        // Trigger camera to follow this ball, and return to player after flight + 2 seconds
        if (CameraController.Instance != null)
        {
            CameraController.Instance.SetTarget(transform, flightDuration + 2.0f);
        }

        StartCoroutine(FlyToGoalRoutine(targetGoal.TargetPosition));
    }

    private IEnumerator FlyToGoalRoutine(Vector3 targetGoalPos)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        // Random rotation axis for realistic spinning effect while flying
        Vector3 rotationAxis = Vector3.Cross((targetGoalPos - startPos).normalized, Vector3.up);
        if (rotationAxis.sqrMagnitude < 0.001f) rotationAxis = Vector3.right;

        while (elapsed < flightDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flightDuration);

            // Parabolic curve: 4 * h * t * (1 - t) gives arc peak at t = 0.5
            float heightOffset = 4f * arcHeight * t * (1f - t);
            Vector3 currentPos = Vector3.Lerp(startPos, targetGoalPos, t);
            currentPos.y += heightOffset;

            transform.position = currentPos;

            // Spin the ball
            transform.Rotate(rotationAxis, spinSpeed * Time.deltaTime, Space.World);

            yield return null;
        }

        transform.position = targetGoalPos;

        // Spawn Confetti Particle Explosion
        PlayGoalEffects(targetGoalPos);

        OnGoalReached?.Invoke(this);
    }

    private void PlayGoalEffects(Vector3 effectPosition)
    {
        if (confettiPrefab != null)
        {
            GameObject effect = Instantiate(confettiPrefab, effectPosition, Quaternion.identity);
            Destroy(effect, 4f); // Auto cleanup after particle plays
        }
        else
        {
            // Try loading from Resources if not assigned in Inspector
            GameObject loadedPrefab = Resources.Load<GameObject>("Confetti Explosion - Stars");
            if (loadedPrefab != null)
            {
                GameObject effect = Instantiate(loadedPrefab, effectPosition, Quaternion.identity);
                Destroy(effect, 4f);
            }
        }
    }
}
