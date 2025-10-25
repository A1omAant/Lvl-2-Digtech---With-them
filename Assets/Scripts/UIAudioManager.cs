using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;

public class UIAudioManager : MonoBehaviour
{
    [Header("Audio References")]
    //public AudioMixer audioMixer;
    public static UIAudioManager Instance;

    public AudioSource sfxAudioSource;
    public List<AudioClip> sfxClips;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(string clipName)
    {
        AudioClip clip = sfxClips.Find(c => c.name == clipName);
        if (clip != null)
        {
            sfxAudioSource.PlayOneShot(clip);
             //sfxAudioSource.PlayOneShot(sfxClips.Find(c => c.name == "UISliderMove"));
        }
        else
        {
            Debug.LogWarning("SFX clip not found: " + clipName);
        }
    }

    public void SetSFXVolume(float volume)
    {
        //audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        //PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    public void PlaySliderMove()
    {
        sfxAudioSource.PlayOneShot(sfxClips.Find(c => c.name == "UISliderMove"));
    }


    
}
