using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("What file should this trigger load?")]
    [Tooltip("Type the exact name of the file in StreamingAssets, e.g., dialogue.csv")]
    public string csvFileName = "dialogue.csv";

    [Header("System Reference")]
    public VNManager vnManager;

    public void PlayThisDialogue()
    {
        if (vnManager != null)
        {
            // Combined file loading and sequence initialization into one call
            vnManager.StartConversationWithFile(csvFileName);
        }
        else
        {
            Debug.LogError("VNManager is not assigned on the DialogueTrigger!");
        }
    }
}