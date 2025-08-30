using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour
{
   public Transform cameraPosition;

   private void Update(){
    transform.position = cameraPosition.position;
    }
    

}

