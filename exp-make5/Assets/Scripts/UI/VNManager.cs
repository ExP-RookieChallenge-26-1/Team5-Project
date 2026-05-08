using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem; // 1. Added the New Input System namespace

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 5)]
    public string text;

    [Header("Visuals")]
    public GameObject characterObject;
    public bool hideCharacter;
    public bool bounceCharacter;

    [Header("Timing")]
    public float pauseAfterLine;
}

public class VNManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainTextObject;
    public TextHandler textHandler;
    public TMP_Text nameText;

    [Header("Dialogue Content")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    private int currentLineIndex = 0;

    [Header("Controls")]
    // 2. Changed KeyCode to Key
    public Key nextKey = Key.Space;

    private bool isPaused = false;
    private bool isFastForwarding = false;
    private bool isDialogueRunning = false;
    private bool isWaitingOnPause = false;

    void Start()
    {
        if (textHandler == null) textHandler = mainTextObject.GetComponent<TextHandler>();
        StartCoroutine(StartDialogueSequence());
    }

    IEnumerator StartDialogueSequence()
    {
        yield return new WaitForSeconds(1);
        mainTextObject.SetActive(true);
        isDialogueRunning = true;
        PlayNextLine();
    }

    void Update()
    {
        if (!isDialogueRunning || isPaused) return;

        if (isFastForwarding && !textHandler.isTyping && !isWaitingOnPause)
        {
            PlayNextLine();
            return;
        }

        // 3. Updated Input polling to use Keyboard.current from the New Input System
        if (Keyboard.current != null && Keyboard.current[nextKey].wasPressedThisFrame && !isWaitingOnPause)
        {
            Debug.Log("Space");
            AdvanceDialogue();
        }
    }

    void PlayNextLine()
    {
        if (currentLineIndex < dialogueLines.Count)
        {
            DialogueLine currentLine = dialogueLines[currentLineIndex];

            if (nameText != null) nameText.text = currentLine.speakerName;

            if (currentLine.characterObject != null)
            {
                if (currentLine.hideCharacter)
                {
                    currentLine.characterObject.SetActive(false);
                }
                else
                {
                    currentLine.characterObject.SetActive(true);
                    if (currentLine.bounceCharacter)
                    {
                        StartCoroutine(BounceCharacter(currentLine.characterObject));
                    }
                }
            }

            textHandler.PlayText(currentLine.text);

            if (currentLine.pauseAfterLine > 0)
            {
                StartCoroutine(WaitBetweenLines(currentLine.pauseAfterLine));
            }

            currentLineIndex++;
        }
        else
        {
            isDialogueRunning = false;
            mainTextObject.SetActive(false);
            if (nameText != null) nameText.transform.parent.gameObject.SetActive(false);
            Debug.Log("Dialogue Sequence Completed.");
        }
    }

    IEnumerator WaitBetweenLines(float waitTime)
    {
        isWaitingOnPause = true;
        yield return new WaitUntil(() => !textHandler.isTyping);
        yield return new WaitForSeconds(waitTime);
        isWaitingOnPause = false;

        if (isFastForwarding) PlayNextLine();
    }

    IEnumerator BounceCharacter(GameObject charObj)
    {
        Vector3 startPos = charObj.transform.localPosition;
        Vector3 upPos = startPos + new Vector3(0, 20f, 0);

        float time = 0.1f;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            charObj.transform.localPosition = Vector3.Lerp(startPos, upPos, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;

        while (elapsedTime < time)
        {
            charObj.transform.localPosition = Vector3.Lerp(upPos, startPos, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        charObj.transform.localPosition = startPos;
    }

    public void AdvanceDialogue()
    {
        if (isPaused) return;

        if (textHandler.isTyping)
        {
            textHandler.ForceFinish();
        }
        else if (!isWaitingOnPause)
        {
            PlayNextLine();
        }
    }

    public void UI_TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
    }

    public void UI_ToggleFullSkip()
    {
        isFastForwarding = !isFastForwarding;
        textHandler.currentTypeSpeed = isFastForwarding ? 0.005f : textHandler.defaultTypeSpeed;
        Debug.Log(isFastForwarding ? "Fast Forward ON" : "Fast Forward OFF");
    }
}