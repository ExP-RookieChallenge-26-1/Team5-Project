using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioControl: MonoBehaviour
{
    [Header("Audio Mixer Reference")]
    [Tooltip("Drag your main AudioMixer here")]
    public AudioMixer mainMixer;

    [Header("UI Sliders (0 to 10 Scale)")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // 1. Load saved preferences (default to 10 if no save exists)
        masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 10f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVol", 10f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 10f);

        // 2. Add listeners so the audio changes when the user drags the sliders
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // --- Volume Control Methods ---

    public void SetMasterVolume(float sliderValue)
    {
        // Convert the 0-10 slider value to a logarithmic decibel scale (-80dB to 0dB)
        float decibelValue = ConvertToDecibels(sliderValue);
        mainMixer.SetFloat("Master", decibelValue);
        PlayerPrefs.SetFloat("MasterVol", sliderValue); // Save the 0-10 value
    }

    public void SetBGMVolume(float sliderValue)
    {
        float decibelValue = ConvertToDecibels(sliderValue);
        mainMixer.SetFloat("BGM", decibelValue);
        PlayerPrefs.SetFloat("BGMVol", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float decibelValue = ConvertToDecibels(sliderValue);
        mainMixer.SetFloat("SFX", decibelValue);
        PlayerPrefs.SetFloat("SFXVol", sliderValue);
    }

    /// <summary>
    /// Converts a linear slider value (0 to 10) to an AudioMixer decibel value.
    /// AudioMixers work on a logarithmic scale, not linearly!
    /// </summary>
    private float ConvertToDecibels(float sliderValue)
    {
        // If the slider is at 0, mute the audio entirely (-80dB is Unity's mute threshold)
        if (sliderValue <= 0)
        {
            return -80f;
        }

        // Normalize the 0-10 value to a 0.01-1 range, then convert to Decibels
        float normalizedValue = sliderValue / 10f;
        return Mathf.Log10(normalizedValue) * 20f;
    }
}