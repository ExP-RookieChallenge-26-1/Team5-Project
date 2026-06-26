using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SlidingPanel : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("Animation Settings")]
    public float animationDuration = 0.3f;
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("How far the panel moves on the X axis when closing. Adjust this value in the inspector!")]
    public float slideDistance = 500f; // Set your default value here

    private Vector2 openPosition;
    private Vector2 closedPosition;
    private bool isPanelOpen = false;
    private Coroutine animationCoroutine;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Grab the starting position as the default "Open" state
        openPosition = rectTransform.anchoredPosition;

        // Use your custom slideDistance variable instead of the automatic width
        closedPosition = new Vector2(openPosition.x + slideDistance, openPosition.y);

        // Initialize closed
        rectTransform.anchoredPosition = closedPosition;
    }

    void OnEnable()
    {
        SwipeDetector.OnSwipeLeft += HandleSwipeLeft;
        SwipeDetector.OnSwipeRight += HandleSwipeRight;
    }

    void OnDisable()
    {
        SwipeDetector.OnSwipeLeft -= HandleSwipeLeft;
        SwipeDetector.OnSwipeRight -= HandleSwipeRight;
    }

    private void HandleSwipeLeft()
    {
        if (!isPanelOpen) TogglePanel(true);
    }

    private void HandleSwipeRight()
    {
        if (isPanelOpen) TogglePanel(false);
    }

    public void TogglePanel(bool open)
    {
        if (open && ((StageEventManager.Instance != null && StageEventManager.Instance.isGameOver) || VNManager.IsDialogueActive))
    {
        return;
    }

        isPanelOpen = open;
        Vector2 target = isPanelOpen ? openPosition : closedPosition;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(SlideRoutine(target));
    }

    private IEnumerator SlideRoutine(Vector2 targetPosition)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float curveT = animationCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveT);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }
}