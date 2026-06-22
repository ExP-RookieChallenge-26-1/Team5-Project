using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("What file should this trigger load?")]
    [Tooltip("Type the exact name of the file in StreamingAssets, e.g., dialogue.csv")]
    public string csvFileName = "dialogue.csv";

    [Header("System Reference")]
    public VNManager vnManager;

    // THIS is the method we will look for in the button!
    public void PlayThisDialogue()
    {
        if (vnManager != null)
        {
            vnManager.gameObject.SetActive(true);
            vnManager.StartConversation(csvFileName);
        }
        else
        {
            Debug.LogError("VNManager is not assigned on the DialogueTrigger!");
        }
    }
}