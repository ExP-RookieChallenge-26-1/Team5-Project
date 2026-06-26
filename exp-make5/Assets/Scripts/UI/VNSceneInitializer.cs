using UnityEngine;

public class VNSceneInitializer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The exact name of your CSV file (including .csv) located inside the StreamingAssets folder.")]
    public string dialogueFileName = "intro_scene.csv";

    [Tooltip("Should the dialogue start immediately on Awake or wait for Start?")]
    public bool playOnStart = true;

    private void Start()
    {
        if (playOnStart)
        {
            StartVN();
        }
    }

    public void StartVN()
    {
        // Safely check if the VNManager instance exists in the scene
        if (VNManager.Instance != null)
        {
            VNManager.Instance.StartConversationWithFile(dialogueFileName);
        }
        else
        {
            Debug.LogError("VNSceneInitializer: Could not find VNManager.Instance in the scene! Make sure VNManager is attached to a GameObject.");
        }
    }
}