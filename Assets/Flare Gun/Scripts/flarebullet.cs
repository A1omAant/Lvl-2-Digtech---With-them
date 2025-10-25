using UnityEngine;
using System.Collections;

public class flarebullet : MonoBehaviour {
			

	private Light flarelight;
	private AudioSource flaresound;
	private ParticleSystemRenderer smokepParSystem;
	private bool myCoroutine;
	private float smooth = 2.4f;
	public 	float flareTimer = 9;
	public AudioClip flareBurningSound;


	// Use this for initialization
	void Start () {

		StartCoroutine("flareLightoff");

		GetComponent<AudioSource>().PlayOneShot(flareBurningSound); // play sound until flare disabled
		flarelight = GetComponent<Light>();
		flaresound = GetComponent<AudioSource>();
		//smokepParSystem = GetComponent<ParticleSystemRenderer>();

		Invoke(nameof(DisableFlare), flareTimer + 1f);

		
	
	}
	void UpdateSound(){
		if(flareTimer > flareBurningSound.length){
			if(!flaresound.isPlaying){
				flaresound.PlayOneShot(flareBurningSound); // play sound until flare disabled
			}
		}
	}
	
	void DisableFlare()
	{
		gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {

		UpdateSound();

		if (myCoroutine == true)
			
		{
			flarelight.intensity = Random.Range(2f,6.0f);
			
		}else
			
		{
			flarelight.intensity =  Mathf.Lerp(flarelight.intensity,0f,Time.deltaTime * smooth);
			flarelight.range =  Mathf.Lerp(flarelight.range,0f,Time.deltaTime * smooth);			
			flaresound.volume = Mathf.Lerp(flaresound.volume,0f,Time.deltaTime * smooth);
			//smokepParSystem.maxParticleSize = Mathf.Lerp(smokepParSystem.maxParticleSize,0f,Time.deltaTime * 5);


		}

			
	}
	
	IEnumerator flareLightoff()
	{
		myCoroutine = true;
		yield return new WaitForSeconds(flareTimer);
		myCoroutine = false;

	}
}
