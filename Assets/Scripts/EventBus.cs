using System;
using UnityEngine;

/// <summary>
/// Central Event Bus for decoupling systems without direct component references.
/// </summary>
public static class EventBus
{
    // Triggered when a ball is kicked
    public static event Action<SoccerBall> OnBallKicked;

    // Triggered when a ball reaches the goal net
    public static event Action<SoccerBall, Vector3> OnBallScored;

    // Triggered when the kick/celebration sequence completes and player can kick again
    public static event Action OnKickSequenceCompleted;

    public static void TriggerBallKicked(SoccerBall ball)
    {
        OnBallKicked?.Invoke(ball);
    }

    public static void TriggerBallScored(SoccerBall ball, Vector3 goalPosition)
    {
        OnBallScored?.Invoke(ball, goalPosition);
    }

    public static void TriggerKickSequenceCompleted()
    {
        OnKickSequenceCompleted?.Invoke();
    }
}
