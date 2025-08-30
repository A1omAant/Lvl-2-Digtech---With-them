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
  

    [Header("Movement Keybinds")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode sprintKey = KeyCode.LeftControl;
    public KeyCode crouchKey = KeyCode.Space;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;


    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState{

        walk,
        sprinting,
        dashing,
        crouching
    }


    private void Start(){
        rb = GetComponent<Rigidbody>();     
        rb.freezeRotation = true;   
        crouchStartHeight = transform.localScale.y;
    }

    private void Update(){
        Debug.Log(rb.linearVelocity);

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        PlayerInput();
        StateHandler();
        SpeedControl();
   
        
        if(grounded){
            rb.linearDamping = stealthDrag;
        }else{
            rb.linearDamping = 0;
        }
        }
        
        



       

    private void FixedUpdate()
    {
        PlayerMove();
    }


    private void StateHandler(){

        if(grounded && Input.GetKey(sprintKey)){
            state = MovementState.sprinting;
            moveSpeed = moveSpeedSprint;
            stealthDrag = sprintDrag;
        }
        else if(grounded && Input.GetKey(dashKey)){
            state = MovementState.dashing;
            stealthDrag = dashDrag;
        }
        else if(grounded && Input.GetKey(crouchKey)){
            state = MovementState.crouching;
            
            moveSpeed = moveSpeedCrouch;
            stealthDrag = crouchDrag;
        }
        else{
            state = MovementState.walk;
            moveSpeed = baseMoveSpeed;
            
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
    private void PlayerMove(){

        //rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);

       

        
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        if(OnSlope()){
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

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

        if(OnSlope()){
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
