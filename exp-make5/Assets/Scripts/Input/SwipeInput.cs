using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetector : MonoBehaviour
{
    // Global events that any UI script can subscribe to
    public static event Action OnSwipeLeft;
    public static event Action OnSwipeRight;

    [Header("Settings")]
    [Tooltip("Minimum distance the finger must travel to count as a swipe.")]
    public float swipeThreshold = 50f;

    [Tooltip("Require swipe to start in the right X% of screen. Set to 1.0 to swipe from anywhere.")]
    [Range(0f, 1f)] public float startingZoneRightPercent = 0.2f;

    private Vector2 touchStartPos;
    private bool isValidTouchZone = false;

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            CheckTouchStart(Pointer.current.position.ReadValue());
        }

        if (Pointer.current.press.wasReleasedThisFrame)
        {
            CheckSwipeEnd(Pointer.current.position.ReadValue());
        }
    }

    private void CheckTouchStart(Vector2 position)
    {
        float cutoff = Screen.width * (1f - startingZoneRightPercent);

        if (position.x >= cutoff)
        {
            isValidTouchZone = true;
            touchStartPos = position;
        }
        else
        {
            isValidTouchZone = false;
        }
    }

    private void CheckSwipeEnd(Vector2 position)
    {
        if (!isValidTouchZone) return;

        float swipeDeltaX = position.x - touchStartPos.x;

        if (Mathf.Abs(swipeDeltaX) > swipeThreshold)
        {
            if (swipeDeltaX < 0)
            {
                OnSwipeLeft?.Invoke();
            }
            else if (swipeDeltaX > 0)
            {
                OnSwipeRight?.Invoke();
            }
        }

        isValidTouchZone = false;
    }
}