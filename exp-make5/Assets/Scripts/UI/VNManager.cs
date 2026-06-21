using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem; // New Input System
using System.IO; // Required for reading files
using System.Text.RegularExpressions; // Required for CSV safe-splitting
using UnityEngine.EventSystems;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 5)]
    public string text;

    [Header("Visuals")]
    public string characterID; // Changed from GameObject to string for file loading
    public bool hideCharacter;
    public bool bounceCharacter;

    [Header("Timing")]
    public float pauseAfterLine;
}

// A dictionary to link character names in your CSV file to actual GameObjects in Unity
[System.Serializable]
public struct CharacterReference
{
    public string characterID;
    public GameObject characterObject;
}

public class VNManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainTextObject;
    public TextHandler textHandler;
    public TMP_Text nameText;

    [Header("Dialogue Content")]
    public string dialogueFileName = "dialogue.csv"; // Name of file in StreamingAssets
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    public List<CharacterReference> characterRoster = new List<CharacterReference>(); // Link IDs to Objects here
    private int currentLineIndex = 0;

    [Header("Controls")]
    public Key nextKey = Key.Space;

    private bool isPaused = false;
    private bool isFastForwarding = false;
    private bool isDialogueRunning = false;
    private bool isWaitingOnPause = false;

    private float inputCooldown = 0f;

    [Header("Game Integration")]
    public UnityEvent OnDialogueStarted;
    public UnityEvent OnDialogueEnded;

    // We use OnEnable so the dialogue starts immediately when you turn on the Overlay Prefab
    void OnEnable()
    {
        if (textHandler == null) textHandler = mainTextObject.GetComponent<TextHandler>();

        LoadDialogueFromFile(dialogueFileName);
        StartCoroutine(StartDialogueSequence());
    }

    // --- CSV FILE LOADING SYSTEM ---
    public void LoadDialogueFromFile(string fileName)
    {
        // Looks inside the Assets/StreamingAssets folder
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            dialogueLines.Clear();
            string[] lines = File.ReadAllLines(filePath);

            // Regex pattern: Splits by comma, but ignores commas inside double quotes
            string splitPattern = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";

            // Start at 1 to skip the header row in your file
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                // Split using Regex instead of standard string split
                string[] columns = Regex.Split(lines[i], splitPattern);

                if (columns.Length >= 6)
                {
                    // Clean up any extra quotes that Excel/Google Sheets might have added
                    for (int j = 0; j < columns.Length; j++)
                    {
                        columns[j] = columns[j].TrimStart('"').TrimEnd('"').Replace("\"\"", "\"");
                    }

                    DialogueLine newLine = new DialogueLine();
                    newLine.speakerName = columns[0];
                    newLine.text = columns[1];
                    newLine.characterID = columns[2];

                    // Parse booleans and floats safely
                    bool.TryParse(columns[3], out newLine.hideCharacter);
                    bool.TryParse(columns[4], out newLine.bounceCharacter);
                    float.TryParse(columns[5], out newLine.pauseAfterLine);

                    dialogueLines.Add(newLine);
                }
            }
            Debug.Log($"Loaded {dialogueLines.Count} lines from {fileName}");
        }
        else
        {
            Debug.LogError($"Could not find dialogue file at {filePath}. Make sure it's in a StreamingAssets folder!");
        }
    }

    IEnumerator StartDialogueSequence()
    {
        isDialogueRunning = true; // MOVE THIS TO THE TOP!
        currentLineIndex = 0;
        yield return new WaitForSeconds(0.1f);
        mainTextObject.SetActive(true);
        if (nameText != null && nameText.transform.parent != null)
            nameText.transform.parent.gameObject.SetActive(true);

        PlayNextLine();
    }

    void Update()
    {
        if (!isDialogueRunning || isPaused) return;

        // --- NEW: Cooldown Timer ---
        if (inputCooldown > 0f)
        {
            inputCooldown -= Time.deltaTime;
        }

        if (isFastForwarding && !textHandler.isTyping && !isWaitingOnPause)
        {
            PlayNextLine();
            return;
        }

        // --- TOUCH AND CLICK SUPPORT ---
        bool nextInputPressed = false;

        if (Keyboard.current != null && Keyboard.current[nextKey].wasPressedThisFrame)
            nextInputPressed = true;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            // We still keep this as a backup safety net
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return; // Stop checking this frame if we hit UI
            }
            nextInputPressed = true;
        }

        // NEW: Only advance/cancel if the cooldown is completely finished
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

            // --- CHARACTER DICTIONARY LOOKUP ---
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
                    if (currentLine.bounceCharacter)
                    {
                        StartCoroutine(BounceCharacter(activeCharacter));
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
            EndDialogue();
        }
    }

    GameObject GetCharacterObject(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var character in characterRoster)
        {
            if (character.characterID == id) return character.characterObject;
        }
        return null;
    }

    IEnumerator WaitBetweenLines(float waitTime)
    {
        isWaitingOnPause = true;

        // Wait for the text to finish typing out
        yield return new WaitUntil(() => !textHandler.isTyping);

        // Instead of a strict WaitForSeconds, we use a timer
        float timer = 0;
        while (timer < waitTime)
        {
            // If the player turns on Skip Mode, instantly cancel this pause!
            if (isFastForwarding)
            {
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isWaitingOnPause = false;

        // Notice we removed the "if (isFastForwarding) PlayNextLine();" from down here.
        // We don't need it anymore, because your Update() loop will automatically 
        // see that isWaitingOnPause is false and safely trigger the next line!
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

    public void UI_ToggleFullSkip()
    {
        isFastForwarding = !isFastForwarding;
        textHandler.currentTypeSpeed = isFastForwarding ? 0.005f : textHandler.defaultTypeSpeed;

        // NEW: Force the game to ignore screen taps for a fraction of a second so it doesn't instantly cancel!
        inputCooldown = 0.2f;
    }

    public void AdvanceDialogue()
    {
        if (isPaused) return;

        // --- NEW: CANCEL FAST-FORWARD ON CLICK ---
        if (isFastForwarding)
        {
            UI_ToggleFullSkip(); // This turns fast-forward off and resets normal typing speed
            return; // We return early so the player doesn't accidentally skip a line while trying to cancel
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

    // --- SKIP ENTIRE SEQUENCE ---
    public void UI_SkipEntireSequence()
    {
        Debug.Log("The skip button was successfully clicked!"); // ADD THIS AT THE TOP

        if (!isDialogueRunning) 
        {
            Debug.LogWarning("Skip canceled because isDialogueRunning is FALSE!");
            return;
        }

        textHandler.ForceFinish();
        EndDialogue();
        Debug.Log("Sequence completely skipped.");
    }

    private void EndDialogue()
    {
        isDialogueRunning = false;
        mainTextObject.SetActive(false);
        if (nameText != null && nameText.transform.parent != null)
            nameText.transform.parent.gameObject.SetActive(false);

        foreach (var character in characterRoster)
        {
            if (character.characterObject != null) character.characterObject.SetActive(false);
        }

        // Tell the game the dialogue is over!
        OnDialogueEnded?.Invoke();

        // Turn the VN Canvas off so we can see the game again
        gameObject.SetActive(false);
    }

    public void UI_TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
    }

    public void StartConversation(string newCsvFileName)
    {
        if (textHandler == null) textHandler = mainTextObject.GetComponent<TextHandler>();

        // Tell the game the dialogue has started!
        OnDialogueStarted?.Invoke();

        LoadDialogueFromFile(newCsvFileName);
        StartCoroutine(StartDialogueSequence());
    }
}