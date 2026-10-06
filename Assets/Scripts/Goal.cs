using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a Soccer Goal in the scene.
/// Keeps a static registry of all goals for fast distance lookup.
/// </summary>
public class Goal : MonoBehaviour
{
    public static List<Goal> AllGoals { get; private set; } = new List<Goal>();

    [Header("Goal Target")]
    [Tooltip("Target point inside the net where the ball should fly into. If null, uses this transform position.")]
    [SerializeField] private Transform targetPoint;

    public Vector3 TargetPosition => targetPoint != null ? targetPoint.position : transform.position;

    private void OnEnable()
    {
        if (!AllGoals.Contains(this))
        {
            AllGoals.Add(this);
        }
    }

    private void OnDisable()
    {
        AllGoals.Remove(this);
    }

    /// <summary>
    /// Finds the closest goal from a given position.
    /// </summary>
    public static Goal GetClosestGoal(Vector3 fromPosition)
    {
        if (AllGoals == null || AllGoals.Count == 0) return null;

        Goal closest = null;
        float minSqrDistance = float.MaxValue;

        for (int i = 0; i < AllGoals.Count; i++)
        {
            Goal goal = AllGoals[i];
            if (goal == null) continue;

            float sqrDist = (goal.TargetPosition - fromPosition).sqrMagnitude;
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                closest = goal;
            }
        }

        return closest;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(TargetPosition, 0.5f);
    }
}
