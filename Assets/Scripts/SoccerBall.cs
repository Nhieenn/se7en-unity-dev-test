using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Represents an individual soccer ball with flight physics/animation towards a target goal,
/// particle triggering on arrival, and triggers events via EventBus.
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

        // Trigger EventBus event
        EventBus.TriggerBallKicked(this);

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

        // Spawn Confetti Particle Explosion via ObjectPool
        PlayGoalEffects(targetGoalPos);

        // Notify systems (Player celebrates, Camera resets, etc.)
        EventBus.TriggerBallScored(this, targetGoalPos);
    }

    private void PlayGoalEffects(Vector3 effectPosition)
    {
        if (ObjectPool.Instance != null)
        {
            ObjectPool.Instance.SpawnConfetti(effectPosition, 4f);
            return;
        }

        // Fallback if ObjectPool is not present in scene
        GameObject effectObj = null;
        if (confettiPrefab != null)
        {
            effectObj = Instantiate(confettiPrefab, effectPosition, Quaternion.identity);
        }
        else
        {
            GameObject loadedPrefab = Resources.Load<GameObject>("Confetti Explosion - Stars");
            if (loadedPrefab != null)
            {
                effectObj = Instantiate(loadedPrefab, effectPosition, Quaternion.identity);
            }
        }

        if (effectObj != null)
        {
            ParticleSystem[] particles = effectObj.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Play(true);
            }

            Destroy(effectObj, 4f);
        }
    }
}
