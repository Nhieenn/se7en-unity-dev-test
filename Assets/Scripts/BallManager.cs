using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all soccer balls in the scene.
/// Calculates closest ball and farthest ball relative to Player for Kick and Auto-Kick actions.
/// </summary>
public class BallManager : MonoBehaviour
{
    public static BallManager Instance { get; private set; }

    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;

    [Header("Detection Settings")]
    [Tooltip("Maximum distance from player to ball to enable the Kick button.")]
    [SerializeField] private float kickInteractionDistance = 3.0f;

    private static readonly List<SoccerBall> allBalls = new List<SoccerBall>();

    public SoccerBall ClosestBallToPlayer { get; private set; }
    public bool IsPlayerNearAnyBall => ClosestBallToPlayer != null;

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

    private void Start()
    {
        if (playerTransform == null)
        {
            PlayerMovement player = FindObjectOfType<PlayerMovement>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        UpdateClosestBall();
    }

    public static void RegisterBall(SoccerBall ball)
    {
        if (!allBalls.Contains(ball))
        {
            allBalls.Add(ball);
        }
    }

    public static void UnregisterBall(SoccerBall ball)
    {
        allBalls.Remove(ball);
    }

    private void UpdateClosestBall()
    {
        if (playerTransform == null) return;

        SoccerBall nearest = null;
        float minSqrDistance = kickInteractionDistance * kickInteractionDistance;
        Vector3 playerPos = playerTransform.position;

        for (int i = 0; i < allBalls.Count; i++)
        {
            SoccerBall ball = allBalls[i];
            if (ball == null || ball.IsKicked) continue;

            float sqrDist = (ball.transform.position - playerPos).sqrMagnitude;
            if (sqrDist <= minSqrDistance)
            {
                minSqrDistance = sqrDist;
                nearest = ball;
            }
        }

        ClosestBallToPlayer = nearest;
    }

    /// <summary>
    /// Kicks the closest ball near player (used by the Kick Button).
    /// </summary>
    public void KickNearestBall()
    {
        if (ClosestBallToPlayer != null && !ClosestBallToPlayer.IsKicked)
        {
            ClosestBallToPlayer.KickToGoal();
            ClosestBallToPlayer = null;
        }
    }

    /// <summary>
    /// Kicks the ball that is farthest from the player (used by the Auto-Kick Button).
    /// </summary>
    public void KickFarthestBall()
    {
        if (playerTransform == null || allBalls.Count == 0) return;

        SoccerBall farthest = null;
        float maxSqrDistance = -1f;
        Vector3 playerPos = playerTransform.position;

        for (int i = 0; i < allBalls.Count; i++)
        {
            SoccerBall ball = allBalls[i];
            if (ball == null || ball.IsKicked) continue;

            float sqrDist = (ball.transform.position - playerPos).sqrMagnitude;
            if (sqrDist > maxSqrDistance)
            {
                maxSqrDistance = sqrDist;
                farthest = ball;
            }
        }

        if (farthest != null)
        {
            farthest.KickToGoal();
        }
        else
        {
            Debug.Log("[BallManager] No unkicked balls remaining to Auto-Kick!");
        }
    }
}
