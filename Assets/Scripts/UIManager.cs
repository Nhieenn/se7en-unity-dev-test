using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls UI using Unity's modern UI Toolkit:
/// - Kick Button: Square with slight rounded corners, white bg, black text, visible only near a ball.
/// - Auto-Kick Button: Kicks the farthest ball.
/// - Reset Button: Reloads current scene.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class UIManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button kickButton;
    private Button autoKickButton;
    private Button resetButton;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        if (root == null) return;

        kickButton = root.Q<Button>("KickButton");
        autoKickButton = root.Q<Button>("AutoKickButton");
        resetButton = root.Q<Button>("ResetButton");

        if (kickButton != null)
        {
            kickButton.clicked += OnKickButtonClicked;
            // Initially hidden until near a ball
            kickButton.style.display = DisplayStyle.None;
        }

        if (autoKickButton != null)
        {
            autoKickButton.clicked += OnAutoKickButtonClicked;
        }

        if (resetButton != null)
        {
            resetButton.clicked += OnResetButtonClicked;
        }
    }

    private void OnDisable()
    {
        if (kickButton != null)
        {
            kickButton.clicked -= OnKickButtonClicked;
        }

        if (autoKickButton != null)
        {
            autoKickButton.clicked -= OnAutoKickButtonClicked;
        }

        if (resetButton != null)
        {
            resetButton.clicked -= OnResetButtonClicked;
        }
    }

    private void Update()
    {
        if (BallManager.Instance == null) return;

        // Toggle Kick Button visibility in UI Toolkit based on proximity to any ball
        if (kickButton != null)
        {
            bool shouldShowKick = BallManager.Instance.IsPlayerNearAnyBall;
            DisplayStyle targetDisplay = shouldShowKick ? DisplayStyle.Flex : DisplayStyle.None;

            if (kickButton.style.display.value != targetDisplay)
            {
                kickButton.style.display = targetDisplay;
            }
        }

        // Disable Auto-Kick button while a kick is already in progress
        if (autoKickButton != null)
        {
            autoKickButton.SetEnabled(!BallManager.Instance.IsKickingInProgress);
        }
    }

    private void OnKickButtonClicked()
    {
        if (BallManager.Instance != null)
        {
            BallManager.Instance.KickNearestBall();
        }
    }

    private void OnAutoKickButtonClicked()
    {
        if (BallManager.Instance != null)
        {
            BallManager.Instance.KickFarthestBall();
        }
    }

    private void OnResetButtonClicked()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
