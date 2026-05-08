using System.Collections;
using UnityEngine;
using TMPro;

public class TextHandler : MonoBehaviour
{
    private TMP_Text viewText;
    private Coroutine rollCoroutine;
    private string currentFullText;

    // Variables for external scripts to read/modify
    public bool isTyping { get; private set; }
    public float defaultTypeSpeed = 0.03f;
    public float currentTypeSpeed = 0.03f;

    void Awake()
    {
        viewText = GetComponent<TMP_Text>();
    }

    // Call this from VNManager to start new text
    public void PlayText(string text)
    {
        currentFullText = text;
        viewText.text = "";
        isTyping = true;

        // Stop any currently running text animations before starting a new one
        if (rollCoroutine != null) StopCoroutine(rollCoroutine);
        rollCoroutine = StartCoroutine(Rolltext());
    }

    // Call this to instantly show all text (skip animation)
    public void ForceFinish()
    {
        if (rollCoroutine != null) StopCoroutine(rollCoroutine);
        viewText.text = currentFullText;
        isTyping = false;
    }

    IEnumerator Rolltext()
    {
        foreach (char c in currentFullText)
        {
            viewText.text += c;

            // Pauses the coroutine based on our current speed
            yield return new WaitForSeconds(currentTypeSpeed);
        }
        isTyping = false; // Typing finished natively
    }
}