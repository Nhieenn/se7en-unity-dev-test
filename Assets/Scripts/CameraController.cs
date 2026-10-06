using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the Top-Down camera following the Player or a Soccer Ball smoothly.
/// Listens to EventBus for ball kicks and scored events to automatically track targets.
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Target Settings")]
    [SerializeField] private Transform defaultTarget; // The Player (Jammo)
    [SerializeField] private Transform currentTarget;

    [Header("Top-Down Offset Settings")]
    [Tooltip("Offset relative to the target (X: horizontal, Y: height, Z: depth).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -8f);

    [Header("Smooth Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool useSmoothDamp = true;
    [SerializeField] private float smoothTime = 0.2f;

    [Header("Rotation Settings")]
    [Tooltip("If true, camera keeps a fixed top-down pitch angle looking down.")]
    [SerializeField] private bool lockRotation = true;
    [SerializeField] private Vector3 fixedEulerRotation = new Vector3(50f, 0f, 0f);

    private Vector3 currentVelocity = Vector3.zero;
    private Coroutine returnRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        EventBus.OnBallKicked += HandleBallKicked;
        EventBus.OnBallScored += HandleBallScored;
    }

    private void OnDisable()
    {
        EventBus.OnBallKicked -= HandleBallKicked;
        EventBus.OnBallScored -= HandleBallScored;
    }

    private void Start()
    {
        if (defaultTarget == null)
        {
            PlayerMovement player = FindObjectOfType<PlayerMovement>();
            if (player != null)
            {
                defaultTarget = player.transform;
            }
        }

        currentTarget = defaultTarget;

        if (lockRotation)
        {
            transform.rotation = Quaternion.Euler(fixedEulerRotation);
        }
    }

    private void LateUpdate()
    {
        if (currentTarget == null) return;

        Vector3 desiredPosition = currentTarget.position + offset;

        if (useSmoothDamp)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }

        if (lockRotation)
        {
            transform.rotation = Quaternion.Euler(fixedEulerRotation);
        }
    }

    private void HandleBallKicked(SoccerBall ball)
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
        currentTarget = ball.transform;
    }

    private void HandleBallScored(SoccerBall ball, Vector3 goalPos)
    {
        // Wait 2 seconds at the goal, then return to player
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
        }
        returnRoutine = StartCoroutine(ReturnToDefaultTargetRoutine(2.0f));
    }

    public void SetTarget(Transform newTarget, float delayBeforeReturn = 0f)
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        currentTarget = newTarget != null ? newTarget : defaultTarget;

        if (delayBeforeReturn > 0f)
        {
            returnRoutine = StartCoroutine(ReturnToDefaultTargetRoutine(delayBeforeReturn));
        }
    }

    public void ResetToDefaultTarget()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
        currentTarget = defaultTarget;
    }

    private IEnumerator ReturnToDefaultTargetRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentTarget = defaultTarget;
        returnRoutine = null;
    }
}
