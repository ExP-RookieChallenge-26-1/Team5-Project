using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // <-- Added this namespace for the New Input System

public class MenuTransitionController : MonoBehaviour
{
    [Header("Standby Screen (Press to Start)")]
    [Tooltip("The CanvasGroup attached to the 'Press to Start' text or band.")]
    public CanvasGroup standbyGroup;
    public float pulseSpeed = 1.5f;
    public float minAlpha = 0.3f;

    [Header("Menu Elements (Sliding In)")]
    [Tooltip("Drag the Title and Buttons here IN ORDER (top to bottom).")]
    public RectTransform[] menuItems;
    [Tooltip("How far off-screen to the right they should start.")]
    public float slideOffset = 500f;
    [Tooltip("How long the slide takes for a single item.")]
    public float slideDuration = 0.6f;
    [Tooltip("The delay between each item starting its slide (the '차르륵' effect).")]
    public float cascadeDelay = 0.1f;

    [Header("Left Gradient (Optional)")]
    public CanvasGroup leftGradient;
    public float gradientFadeDuration = 0.5f;

    private Vector2[] originalPositions;
    private bool isMenuOpen = false;

    void Start()
    {
        originalPositions = new Vector2[menuItems.Length];
        for (int i = 0; i < menuItems.Length; i++)
        {
            originalPositions[i] = menuItems[i].anchoredPosition;
            menuItems[i].anchoredPosition += new Vector2(slideOffset, 0);
            menuItems[i].gameObject.SetActive(false);
        }

        if (leftGradient != null) leftGradient.alpha = 0f;

        StartCoroutine(PulseStandbyText());

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayBGM("플레이_BGM.mp3"); 
        }
    }

    void Update()
    {
        // <-- NEW INPUT SYSTEM CHECK -->
        // Pointer.current handles both Mouse clicks and Touchscreen taps
        if (!isMenuOpen && Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            StartMenuTransition();
        }
    }

    private void StartMenuTransition()
    {
        isMenuOpen = true;
        StopAllCoroutines();

        StartCoroutine(FadeCanvasGroup(standbyGroup, 0f, 0.3f));

        if (leftGradient != null)
        {
            StartCoroutine(FadeCanvasGroup(leftGradient, 1f, gradientFadeDuration));
        }

        StartCoroutine(CascadeSlideIn());
    }

    private IEnumerator PulseStandbyText()
    {
        while (true)
        {
            float alpha = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            standbyGroup.alpha = Mathf.Lerp(minAlpha, 1f, alpha);
            yield return null;
        }
    }

    private IEnumerator CascadeSlideIn()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].gameObject.SetActive(true);
            StartCoroutine(SlideItem(menuItems[i], originalPositions[i], slideDuration));
            yield return new WaitForSeconds(cascadeDelay);
        }
    }

    private IEnumerator SlideItem(RectTransform target, Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = target.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f);

            target.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        target.anchoredPosition = targetPosition;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
    {
        float startAlpha = cg.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }
}