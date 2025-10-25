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
    public AudioSource FootstepAudioSource;
    public List<AudioClip> GameplayAudioClips;
    public List<AudioClip> HeartBeatAudioClips;
    public List<AudioClip> FootstepAudioClips;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
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
            AudioClip clip = HeartBeatAudioClips.Find(c => c.name == "HeartBeat_" + Random.Range(2,4).ToString());
            HeartBeatAudioSource.PlayOneShot(clip);
        }
    }


    public void PlayFootstep(string surfaceType, string speed)
    {
        AudioClip clip = FootstepAudioClips.Find(c => c.name == "Footsteps_" + surfaceType + "_" + speed + "_" + Random.Range(1, 5).ToString());

        if (clip != null)
        {
            float pitchVariation = Random.Range(0.9f, 1.1f);
            FootstepAudioSource.pitch = pitchVariation;

            if (speed == "Crouch")
            {
                FootstepAudioSource.volume = 1f;
            }
            else if (speed == "Walk")
            {
                FootstepAudioSource.volume = 1f;
            }
            else if (speed == "Run")
            {
                FootstepAudioSource.volume = 1f;
            }

                FootstepAudioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("Footstep clip not found for surface type: " + surfaceType);
            }
    }
    public void StopAllSounds()
    {
        HeartBeatAudioSource.Stop();
        FootstepAudioSource.Stop();
        GameplayAudioSource.Stop();
    }

  
    }