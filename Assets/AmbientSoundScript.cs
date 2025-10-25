using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AmbientSoundScript : MonoBehaviour
{
    public List<AudioClip> ambientSounds;
    public AudioSource audioSource;
    public float minDelay = 5f;
    public float maxDelay = 15f;
    
    private void Start() {
        if(audioSource == null){
            audioSource = GetComponentInChildren<AudioSource>();
        }
        StartCoroutine(PlayAmbientSounds());
    }

    private IEnumerator PlayAmbientSounds(){
        while(true){
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
            if(ambientSounds.Count > 0){
                AudioClip clip = ambientSounds[Random.Range(0, ambientSounds.Count)];
                Debug.Log("Playing ambient sound: " + clip.name);
                audioSource.PlayOneShot(clip);
            }
        }
    }
  
}
