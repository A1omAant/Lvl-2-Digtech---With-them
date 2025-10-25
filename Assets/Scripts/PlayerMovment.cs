using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    private float moveSpeed;
    public float baseMoveSpeed;
    public float moveSpeedSprint;
    public float moveSpeedCrouch;
    public float dashSpeed;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;


    [Header("Drag Settings")]
    public float stealthDrag;
    public float sprintDrag;
    public float crouchDrag;
    public float dashDrag;

    [Header("Crouch Settings")]
    public float crouchHeight;
    public float crouchStartHeight;
    public float airMultiplier;

    [Header("Footstep Settings")]
    public float walkStepInterval = 1f;
    public float sprintStepInterval = 1.25f;
    public float crouchStepInterval = 1.6f;

    private float stepTimer = 0f;

  

    [Header("Movement Keybinds")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode sprintKey = KeyCode.LeftControl;
    public KeyCode crouchKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("sound settings")]
    public float walkNoise;
    public float SprintNoise;
    public float DashNoise;
    public float walkRad;
    public float Springrad;
    public float Dashrad;


    public Transform orientation;

    float horizontalInput;

   // public SoundSystem soundsystem;

    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    PlayerMovement playermovement;

    public MovementState state;

    public enum MovementState{

        walk,
        sprinting,
        dashing,
        crouching
    }

    public bool dashing;



    private void Start(){
        rb = GetComponent<Rigidbody>();     
        playermovement = GetComponent<PlayerMovement>();
        rb.freezeRotation = true;   
        crouchStartHeight = transform.localScale.y;
        
    }

    private void Update(){
        //Debug.Log(rb.linearVelocity);

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        PlayerInput();
        StateHandler();
        SpeedControl();
        HandleFootsteps();

        if(state == MovementState.walk || state == MovementState.crouching || state == MovementState.sprinting){
            rb.linearDamping = stealthDrag;
        }else{
            rb.linearDamping = 0;
        }

        
    }

        
        

    public string GroundTypeCheck(){
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.3f, whatIsGround)){
            Debug.Log("Ground type: " + hit.collider.tag);
            return hit.collider.tag.ToString();
        }
        return "Untagged";
    }

       

    private void FixedUpdate()
    {
        PlayerMove();
    }


    private void StateHandler(){


        if(dashing){
            state = MovementState.dashing;
            moveSpeed = dashSpeed;
            SoundSystem.Instance.EmitSound(transform.position, Dashrad, DashNoise, 0.7f, false, gameObject); // dashing volumes
             //Debug.Log("dashing, emmited sound");
        }

        else if(grounded && Input.GetKey(sprintKey)){
            state = MovementState.sprinting;
            moveSpeed = moveSpeedSprint;
            stealthDrag = sprintDrag;
            SoundSystem.Instance.EmitSound(transform.position, Springrad, SprintNoise, 0.7f, false, gameObject); // sprinting sound volumes
            // Debug.Log("sprinting, emmited sound");
        }
      
        else if(grounded && Input.GetKey(crouchKey)){
            state = MovementState.crouching;
            
            moveSpeed = moveSpeedCrouch;
            stealthDrag = crouchDrag;

        }
        else{
            state = MovementState.walk;
            moveSpeed = baseMoveSpeed;
            if (horizontalInput != 0 || verticalInput != 0)
            {
                SoundSystem.Instance.EmitSound(transform.position, walkRad, walkNoise, 0.7f, false, gameObject);
            } //walking sound volumes
            //Debug.Log("walking, emmited sound");
         
            
        }
    }

    private void PlayerInput(){

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

       
        if(Input.GetKeyDown(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if(Input.GetKeyUp(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, crouchStartHeight, transform.localScale.z);
        }

  
    }

    public void HandleFootsteps(){
        if(!playermovement.grounded){
            stepTimer = 0f;
            return;
        }   

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        if(speed < 0.1f){
            stepTimer = 0f;
            return;
        }

        float currentStepInterval = walkStepInterval;
        switch(state){
            case MovementState.walk:
                currentStepInterval = walkStepInterval;
                break;
            case MovementState.sprinting:
                currentStepInterval = sprintStepInterval;
                break;
            case MovementState.crouching:
                currentStepInterval = crouchStepInterval;
                break;
            case MovementState.dashing:
                stepTimer = 0f;
                return;
            default:
                currentStepInterval = walkStepInterval;
                break;
        }

        stepTimer += Time.deltaTime;

        if(stepTimer >= currentStepInterval){
            PlayFootStep();
            stepTimer -= currentStepInterval;
        }
    }

    private void PlayFootStep(){
        string groundType = GroundTypeCheck();
        if (groundType == "Untagged") groundType = "Concrete";
        string playerState = playermovement.state.ToString();
        if (playerState == "walk") playerState = "Walk";
        else if (playerState == "sprinting") playerState = "Run";
        else if (playerState == "crouching") playerState = "Walk";
        GameplayAudio.Instance?.PlayFootstep(groundType, playerState);
    }
    private void PlayerMove(){

        //rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);

       if (state == MovementState.dashing) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        if(OnSlope()){
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            //Vector3 slopeMove = GetSlopeMoveDirection() * moveSpeed;
            //slopeMove = Vector3.ClampMagnitude(new Vector3(slopeMove.x, 0f, slopeMove.z), moveSpeed);
            //rb.AddForce(slopeMove * slopeMultiplier, ForceMode.Force);

            if (rb.linearVelocity.y > 0){
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }

        }
        else if(grounded){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }

       
        else if(!grounded){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        }

        rb.useGravity = !OnSlope();

    }
    
    private void SpeedControl(){

        if(!OnSlope()){
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }else{
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if(flatVel.magnitude > moveSpeed){
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }



    private bool OnSlope(){

        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)){
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;


        }
        return false;

    }
    private Vector3 GetSlopeMoveDirection(){
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized; 
    }
}
