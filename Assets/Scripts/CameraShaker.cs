using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour
{
    public Transform cam;
    private Vector3 originalPos;
    public float duration;
    public float magnitiude;

    private void Awake() {
        if(cam == null) cam = Camera.main.transform;
        originalPos = cam.localPosition;
    }


    public void Shake(float dur, float mag) {
        duration = dur;
        magnitiude = mag;
        StopAllCoroutines();
        StartCoroutine(DoShake());
    }
    
    private IEnumerator DoShake() {
        float elapsed = 0f;

        while (elapsed < duration) {
            Vector3 randomOffset = Random.insideUnitSphere * magnitiude;
            randomOffset.z = 0f; // optional: keep z fixed
            cam.localPosition = originalPos + randomOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.localPosition = originalPos;
    }


}
