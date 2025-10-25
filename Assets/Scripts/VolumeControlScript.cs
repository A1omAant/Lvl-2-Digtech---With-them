using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;

public class VolumeControlScript : MonoBehaviour
{
    [Header("Audio References")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider sfxSlider;
    public Slider musicSlider;
    public Button restoreDefaultsButton;

    void Awake()
    {
    }


    private void Start()
    {
        InitializeSliders();

      
    }
    void InitializeSliders(){
        // Load saved volume settings
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        
        masterSlider.value = PlayerPrefs.GetFloat("Master", 0.75f);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX", 0.75f);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        musicSlider.value = PlayerPrefs.GetFloat("Music", 0.75f);
        restoreDefaultsButton.onClick.AddListener(OnRestoreDefaultsButtonPressed);
        


        SetMasterVolume(masterSlider.value);
        SetSFXVolume(sfxSlider.value);
        SetMusicVolume(musicSlider.value);


    }

    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f); // Prevent log(0)
        float dB = Mathf.Log10(volume) * 20;
        Debug.Log($"Setting MasterVolume to {volume} (dB: {dB})");
        audioMixer.SetFloat("Master", dB);
        PlayerPrefs.SetFloat("Master", volume);
    }
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f); // Prevent log(0)
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFX", volume);
    }
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f); // Prevent log(0)
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("Music", volume);
    }

    public void RestoreDefaultVolumes()
    {
        float defaultVolume = 0.75f;
        masterSlider.value = defaultVolume;
        sfxSlider.value = defaultVolume;
        musicSlider.value = defaultVolume;

        SetMasterVolume(defaultVolume);
        SetSFXVolume(defaultVolume);
        SetMusicVolume(defaultVolume);
    }

    public void OnRestoreDefaultsButtonPressed()
    {
        RestoreDefaultVolumes();
    }

    
}
