using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 5)]
    public string text;

    [Header("Visuals")]
    public string characterID;
    public bool hideCharacter;
    public bool bounceCharacter;

    [Header("Timing")]
    public float pauseAfterLine;
}

[System.Serializable]
public struct CharacterReference
{
    public string characterID;
    public GameObject characterObject;
}

public class VNManager : MonoBehaviour
{
    // GLOBAL ACCESSOR: Allows any external script or button to find the active manager instantly
    public static VNManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject mainTextObject;
    public TextHandler textHandler;
    public TMP_Text nameText;

    [Header("Dialogue Content")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    public List<CharacterReference> characterRoster = new List<CharacterReference>();
    private int currentLineIndex = 0;

    [Header("Controls")]
    public Key nextKey = Key.Space;

    [Header("Game Integration")]
    public UnityEvent OnDialogueStarted;
    public UnityEvent OnDialogueEnded;

    // --- INTERNAL STATE FLAGS ---
    private bool isPaused = false;
    private bool isFastForwarding = false;
    private bool isDialogueRunning = false;
    private bool isWaitingOnPause = false;
    private bool isSkipping = false;
    private float inputCooldown = 0f;

    // --- SKIP ENGINE VARIABLES ---
    private Coroutine skipCoroutine;
    public float skipDelay = 0.05f;

    private void Awake()
    {
        // Enforce a strict Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartConversationWithFile(string newCsvFileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, newCsvFileName);

        if (File.Exists(filePath))
        {
            dialogueLines.Clear();
            string[] lines = File.ReadAllLines(filePath);
            string splitPattern = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] columns = Regex.Split(lines[i], splitPattern);

                if (columns.Length >= 6)
                {
                    for (int j = 0; j < columns.Length; j++)
                    {
                        columns[j] = columns[j].TrimStart('"').TrimEnd('"').Replace("\"\"", "\"");
                    }

                    DialogueLine newLine = new DialogueLine();
                    newLine.speakerName = columns[0];
                    newLine.text = columns[1];
                    newLine.characterID = columns[2];

                    bool.TryParse(columns[3], out newLine.hideCharacter);
                    bool.TryParse(columns[4], out newLine.bounceCharacter);
                    float.TryParse(columns[5], out newLine.pauseAfterLine);

                    dialogueLines.Add(newLine);
                }
            }
        }
        else
        {
            Debug.LogError($"Could not find dialogue file at {filePath}. Make sure it is in a StreamingAssets folder!");
            return;
        }

        gameObject.SetActive(true);
        isDialogueRunning = true;
        isFastForwarding = false;
        isSkipping = false;
        isWaitingOnPause = false;
        inputCooldown = 0f;

        if (textHandler == null) textHandler = mainTextObject.GetComponent<TextHandler>();
        mainTextObject.SetActive(true);

        OnDialogueStarted?.Invoke();
        StartCoroutine(StartDialogueSequence());
    }

    IEnumerator StartDialogueSequence()
    {
        currentLineIndex = 0;
        mainTextObject.SetActive(true);
        if (nameText != null && nameText.transform.parent != null)
        {
            nameText.transform.parent.gameObject.SetActive(true);
        }

        PlayNextLine();
        yield break;
    }

    void Update()
    {
        if (!isDialogueRunning || isPaused) return;

        if (inputCooldown > 0f) inputCooldown -= Time.deltaTime;

        bool nextInputPressed = false;

        if (Keyboard.current != null && Keyboard.current[nextKey].wasPressedThisFrame)
            nextInputPressed = true;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            nextInputPressed = true;
        }

        if (isSkipping)
        {
            if (nextInputPressed && inputCooldown <= 0f)
            {
                isSkipping = false;
                if (skipCoroutine != null) StopCoroutine(skipCoroutine);
                inputCooldown = 0.2f;
            }
            return;
        }

        if (isFastForwarding && !textHandler.isTyping && !isWaitingOnPause)
        {
            PlayNextLine();
            return;
        }

        if (nextInputPressed && !isWaitingOnPause && inputCooldown <= 0f)
        {
            AdvanceDialogue();
        }
    }

    void PlayNextLine()
    {
        if (currentLineIndex < dialogueLines.Count)
        {
            DialogueLine currentLine = dialogueLines[currentLineIndex];

            if (nameText != null) nameText.text = currentLine.speakerName;

            GameObject activeCharacter = GetCharacterObject(currentLine.characterID);
            if (activeCharacter != null)
            {
                if (currentLine.hideCharacter)
                {
                    activeCharacter.SetActive(false);
                }
                else
                {
                    activeCharacter.SetActive(true);
                    if (currentLine.bounceCharacter) StartCoroutine(BounceCharacter(activeCharacter));
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
            EndDialogue();
        }
    }

    public void AdvanceDialogue()
    {
        Debug.Log("AdvanceDialogue");
        if (!isDialogueRunning || isPaused) return;

        if (isSkipping)
        {
            isSkipping = false;
            if (skipCoroutine != null) StopCoroutine(skipCoroutine);
            inputCooldown = 0.2f;
            return;
        }

        if (isFastForwarding)
        {
            UI_ToggleFullSkip();
            return;
        }

        if (textHandler.isTyping)
        {
            textHandler.ForceFinish();
        }
        else if (!isWaitingOnPause)
        {
            PlayNextLine();
        }
    }

    private void EndDialogue()
    {
        if (skipCoroutine != null) StopCoroutine(skipCoroutine);
        StopAllCoroutines();

        isDialogueRunning = false;
        isWaitingOnPause = false;
        isFastForwarding = false;
        isSkipping = false;
        inputCooldown = 0f;

        mainTextObject.SetActive(false);
        if (nameText != null && nameText.transform.parent != null)
            nameText.transform.parent.gameObject.SetActive(false);

        foreach (var character in characterRoster)
        {
            if (character.characterObject != null) character.characterObject.SetActive(false);
        }

        OnDialogueEnded?.Invoke();
        gameObject.SetActive(false);
    }

    GameObject GetCharacterObject(string id)
    {
        if (string.IsNullOrEmpty(id) || id.ToLower() == "system") return null;

        foreach (var character in characterRoster)
        {
            if (character.characterID == id) return character.characterObject;
        }

        GameObject sceneCharacter = GameObject.Find(id);
        if (sceneCharacter != null) return sceneCharacter;

        Debug.LogWarning($"VNManager looked everywhere but couldn't find a character named '{id}' to bounce!");
        return null;
    }

    IEnumerator WaitBetweenLines(float waitTime)
    {
        isWaitingOnPause = true;

        while (textHandler != null && textHandler.isTyping)
        {
            if (isSkipping) break;
            yield return null;
        }

        float timer = 0;
        while (timer < waitTime)
        {
            if (isSkipping || isFastForwarding) break;

            timer += Time.deltaTime;
            yield return null;
        }

        isWaitingOnPause = false;
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

    public void UI_SkipEntireSequence()
    {
        if (!isDialogueRunning || isSkipping) return;

        Debug.Log("Skip Mode Triggered! Fast-forwarding CSV lines...");

        isSkipping = true;
        isFastForwarding = false;
        inputCooldown = 0.2f;

        skipCoroutine = StartCoroutine(AutoSkipRoutine());
    }

    public void UI_ToggleFullSkip()
    {
        if (!isDialogueRunning || textHandler == null) return;

        isFastForwarding = !isFastForwarding;
        textHandler.currentTypeSpeed = isFastForwarding ? 0.005f : textHandler.defaultTypeSpeed;
        inputCooldown = 0.2f;
    }

    IEnumerator AutoSkipRoutine()
    {
        while (isSkipping && isDialogueRunning)
        {
            if (textHandler != null && textHandler.isTyping)
            {
                textHandler.ForceFinish();
            }

            yield return new WaitForSeconds(skipDelay);

            isWaitingOnPause = false;
            PlayNextLine();
        }
    }
}