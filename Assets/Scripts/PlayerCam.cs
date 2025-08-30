using UnityEngine;
using System.Collections;


public class PlayerCam : MonoBehaviour
{
    [Header("Player Camera Settings")]
    public float Xsens;
    public float Ysens;
     public float baseFOV;

    public Transform orientation;

    public float xRotation;
    public float yRotation;


    private void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Camera.main.fieldOfView = baseFOV;

    }

    private void Update(){
        float MouseX = Input.GetAxisRaw("Mouse X");
        float MouseY = Input.GetAxisRaw("Mouse Y");

        xRotation -= MouseY * Ysens/10;
        yRotation += MouseX * Xsens/10;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

        orientation.localRotation = Quaternion.Euler(0, yRotation, 0);
    }


    public void DoFOVAdjustment(float targetFOV, float duration)
    {
        
        StartCoroutine(AdjustFieldOfView(targetFOV, duration));
    }

    private IEnumerator AdjustFieldOfView( float targetFOV, float duration)
    {
        float startFOV = Camera.main.fieldOfView;
        float time = 0;

        while (time < duration)
        {
            Camera.main.fieldOfView = Mathf.Lerp(startFOV, targetFOV, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        Camera.main.fieldOfView = targetFOV;
    }

    
    
}
