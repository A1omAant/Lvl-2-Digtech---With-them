using UnityEngine;

public class PlayerMovment : MonoBehaviour
{
    [Header("Movement Settings")]
    private float MoveSpeed;
    public float BaseMoveSpeed;
    public float MoveSpeedSprint;
    public float MoveSpeedCrouch;
    public float dashSpeed;
    [Header("Drag Settings")]
    public float Steathdrag;
    public float SprintDrag;
    public float CrouchDrag;
    public float dashDrag;

    [Header("Crouch Settings")]
    public float CrouchHeight;
    public float CroucStartHeight;

  

    [Header("Movmeent Keybinds")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode sprintKey = KeyCode.LeftControl;
    public KeyCode crouch = KeyCode.Space;


    [Header("Ground Check")]
    public float PlayerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    Vector3 MoveDirection;

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
        CroucStartHeight = transform.localScale.y;
    }

    private void Update(){
        StateHandler();
        SpeedControl();

        grounded = Physics.Raycast(transform.position, Vector3.down, PlayerHeight * 0.5f + 0.2f, whatIsGround);

        if(grounded){
            rb.linearDamping = Steathdrag;
        }
        PlayerInput();
        PlayerMove();

        if(Input.GetKeyDown(crouch)){
            transform.localScale = new Vector3(transform.localScale.x, CrouchHeight, transform.localScale.z);
            rb.AddForce(Vector3.down * (CroucStartHeight - CrouchHeight)*5, ForceMode.Impulse);
        }
        if(Input.GetKeyUp(crouch)){
            transform.localScale = new Vector3(transform.localScale.x, CroucStartHeight, transform.localScale.z);
        }

       
    }

    private void StateHandler(){

        if(grounded && Input.GetKey(sprintKey)){
            state = MovementState.sprinting;
            MoveSpeed = MoveSpeedSprint;
            Steathdrag = SprintDrag;
        }
        else if(grounded && Input.GetKey(dashKey)){
            state = MovementState.dashing;
            Steathdrag = dashDrag;
        }
        else if(grounded && Input.GetKey(crouch)){
            state = MovementState.crouching;
            
            MoveSpeed = MoveSpeedCrouch;
            Steathdrag = CrouchDrag;
        }
        else{
            state = MovementState.walk;
            MoveSpeed = BaseMoveSpeed;
            
        }
    }

    private void PlayerInput(){

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        
    }
    private void PlayerMove(){


        MoveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //rb.MovePosition(rb.position + MoveDirection * MoveSpeed * Time.fixedDeltaTime);
        rb.AddForce(MoveDirection.normalized * MoveSpeed * 10f, ForceMode.Force);

    }

    private void SpeedControl(){
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if(flatVel.magnitude > MoveSpeed){
            Vector3 limitedVel = flatVel.normalized * MoveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
}
