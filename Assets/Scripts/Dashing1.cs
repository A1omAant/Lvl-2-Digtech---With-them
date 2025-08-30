using UnityEngine;

public class Dashing1 : MonoBehaviour
{

    [Header("References")]
    public Transform orientation;
    public Transform playerCamera;
    private Rigidbody rb;
    private PlayerMovement pm;
    public PlayerCam cam;
    public float dashFOV;
    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;
    [Header("Input")]

    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("Settings")]
    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVelocities = true;

    [Header("Cooldown")]
    public float dashCooldown;
    public float dashCooldownTimer;


    private void Start(){

        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        dashCooldownTimer = dashCooldown;



    }

    void Update(){

        if(Input.GetKeyDown(dashKey)){
            Dash();
        }
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        
    }

    private void Dash(){

        
        if (dashCooldownTimer > 0)
            return;
        else
            dashCooldownTimer = dashCooldown;

        pm.dashing = true;

        Transform forwardT;

        cam.DoFOVAdjustment(dashFOV, dashDuration/2);

        if (useCameraForward)
            forwardT = playerCamera;
        else
            forwardT = orientation;

        Vector3 direction = GetDirection(forwardT);

        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        if(disableGravity){
            rb.useGravity=false;
        }
        rb.AddForce(forceToApply, ForceMode.Impulse);

        delayedForceApply = forceToApply;

        Invoke(nameof(DelayedResetDash), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);

    }

    private Vector3 delayedForceApply;
    private void DelayedResetDash(){
        
        rb.AddForce(delayedForceApply, ForceMode.Impulse);
        if(resetVelocities){
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void ResetDash(){
        cam.DoFOVAdjustment(cam.baseFOV, dashDuration/2);
        pm.dashing = false;

        if(disableGravity){
            rb.useGravity = true;
        }
    }

    private Vector3 GetDirection(Transform forwardT){

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3();
            if(allowAllDirections){
                direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
            }else{
                direction = forwardT.forward;

            }
            if(verticalInput == 0 && horizontalInput == 0){
                direction = forwardT.forward;
            }

            return direction.normalized;             
       }

    }


