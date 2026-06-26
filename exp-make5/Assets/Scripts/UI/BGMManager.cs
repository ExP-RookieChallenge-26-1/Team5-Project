using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    private AudioSource audioSource;
    private Coroutine musicCoroutine;
    private string currentTrackName = ""; // Keeps track of what's playing

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps music playing across scenes
            audioSource = GetComponent<AudioSource>();

            audioSource.loop = true; // Forces the audio file to loop infinitely
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject); // Kills duplicates if you reload the scene
        }
    }

    public void PlayBGM(string fileName)
    {
        // If this exact song is already playing, do nothing!
        if (currentTrackName == fileName && audioSource.isPlaying)
        {
            return;
        }

        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
        }

        currentTrackName = fileName;
        musicCoroutine = StartCoroutine(LoadAndPlayAudio(fileName));
    }

    private IEnumerator LoadAndPlayAudio(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (!filePath.Contains("://"))
        {
            filePath = "file://" + filePath;
        }

        AudioType audioType = GetAudioType(fileName);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType))
        {
            ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"BGM Error: {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                }
            }
        }
    }

    private AudioType GetAudioType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();
        return extension switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            ".ogg" => AudioType.OGGVORBIS,
            _ => AudioType.UNKNOWN,
        };
    }
}