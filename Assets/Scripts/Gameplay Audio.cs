using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;


public class GameplayAudio : MonoBehaviour
{
    [Header("Audio References")]
    //public AudioMixer audioMixer;
    public static GameplayAudio Instance;

    public AudioSource GameplayAudioSource;
    public AudioSource HeartBeatAudioSource;
    public List<AudioClip> GameplayAudioClips;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayGameplaySFX(string clipName)
    {
        AudioClip clip = GameplayAudioClips.Find(c => c.name == clipName);
        if (clip != null)
        {
            GameplayAudioSource.PlayOneShot(clip);
             //sfxAudioSource.PlayOneShot(sfxClips.Find(c => c.name == "UISliderMove"));
        }
        else
        {
            Debug.LogWarning("Gameplay SFX clip not found: " + clipName);
        }
    }

    public void SetHeartbeatVolume(float volume)
    {
        HeartBeatAudioSource.volume = volume;
        //audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        //PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    public void PlayHeartBeat()
    {
        if(!HeartBeatAudioSource.isPlaying){
            HeartBeatAudioSource.PlayOneShot(GameplayAudioClips.Find(c => c.name == "HeartBeat"));
        }
    }

}
