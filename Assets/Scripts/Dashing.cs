using UnityEngine;
using System.Collections;

public class Dashing : MonoBehaviour
{
public CharacterController controller;
public float dashDistance = 5f;
public float dashTime = 0.2f;
private bool isDashing = false;

void Start() {
    controller = GetComponent<CharacterController>();
}

void Update() {
    Vector3 InputDir = new Vector3(
    Input.GetAxisRaw("Horizontal"), 
    0, 
    Input.GetAxisRaw("Vertical")
    );
    if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing) {
    StartCoroutine(Dash(-InputDir));
}
}







IEnumerator Dash(Vector3 inputDir) {
    if (inputDir == Vector3.zero) {
        // If no input, default forward
        inputDir = Vector3.forward;
    }

    isDashing = true;
    float elapsed = 0f;
    Vector3 startPos = transform.position;
    Vector3 dashDir = inputDir.normalized; // purely input-based
    Vector3 targetPos = startPos + dashDir * dashDistance;

    while (elapsed < dashTime) {
        Vector3 move = Vector3.Lerp(startPos, targetPos, elapsed / dashTime) - transform.position;
        controller.Move(move);
        elapsed += Time.deltaTime;
        yield return null;
    }

    // Snap to exact position
    controller.Move(targetPos - transform.position);
    isDashing = false;
}
}