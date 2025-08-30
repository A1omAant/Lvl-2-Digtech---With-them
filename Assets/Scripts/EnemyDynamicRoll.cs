using UnityEngine;

public class EnemyDynamicRoll : MonoBehaviour
{
    [SerializeField] private float maxLerpSpeed = 5f;    // Maximum rotation speed
    [SerializeField] private float maxTiltAngle = 20f;   // Maximum tilt for forward/sideways
    [SerializeField] private float smoothFactor = 0.1f;  // How quickly the lean follows movement

    private Vector3 lastPosition;
    private Vector3 smoothedVelocity;

    void Start()
    {
        lastPosition = transform.position;
        smoothedVelocity = Vector3.zero;
    }

    void Update()
    {
        // Calculate raw movement vector
        Vector3 movementVector = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Smooth out sudden spikes in velocity
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, movementVector, smoothFactor);

        // Convert to local space relative to the enemy
        Vector3 localMove = transform.InverseTransformDirection(smoothedVelocity);

        float forwardSpeed = localMove.z;  // forward/back lean
        float sideSpeed = localMove.x;     // left/right lean

        // Construct target rotation with lean
        Quaternion targetRotation = Quaternion.Euler(
            Mathf.Clamp(forwardSpeed * -maxTiltAngle, -maxTiltAngle, maxTiltAngle),
            transform.rotation.eulerAngles.y,
            Mathf.Clamp(sideSpeed * -maxTiltAngle, -maxTiltAngle, maxTiltAngle)
        );

        // Smoothly slerp to target rotation
        float lerpSpeed = Mathf.Clamp(smoothedVelocity.magnitude, 0f, maxLerpSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerpSpeed * Time.deltaTime);
        transform.Translate(Vector3.forward * Time.deltaTime * 5); // Move forward at a constant speed
    }
}