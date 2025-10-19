using UnityEngine;
using System.Collections;

public class Throwableobject : MonoBehaviour


{

    private float distance;
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float pickupRange;
    public KeyCode pickupKey = KeyCode.T;
    public bool Inrange;

    [Header("Throw Settings")]
    public KeyCode throwKey = KeyCode.Mouse1;
    public float throwForce = 15f;
    public bool IsThrowable = true;
    public float throwHeight = 2f;
    [Header("Ammo Settings")]
    public bool IsAmmo = false;
    public float AmmoPickupValue = 8f;
    
    [Header("Health Settings")]
    public float HealthPickupValue = 20f;
    public bool IsHealthPack = false;

    [Header("Highlight Settings")]
    public GameObject HighlightMesh;
    [SerializeField] 
    private LineRenderer lineRenderer;

    private Rigidbody rb;
    public bool isHeld = false;
    public bool physicsDrop = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    [Header("sound settings")]
    public float soundVolume = 1f;
    public bool EmmitsNoiseOnLanding = true;
    public bool EmmitsNoiseOnDrop = true;
    private bool IsThrown;

    public float refreshRate = 0.02f; // updates every 20ms
    private float lastUpdateTime;

   

    private void Start(){
        isHeld = false;
        rb = GetComponent<Rigidbody>();
        rb.useGravity=true;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        setHighlight(false);

    }

    private void Update(){    

        
        if(Input.GetKey(throwKey) && isHeld ){   
            transform.GetComponent<LineRenderer>().enabled = true;

            if (Time.time - lastUpdateTime > refreshRate)
            {
                Trajectory();
                lastUpdateTime = Time.time;
            }
        }

        distance = (transform.position - holdPoint.position).sqrMagnitude;
        Inrange = (distance < pickupRange * pickupRange);

        if(!isHeld){
            transform.GetComponent<LineRenderer>().enabled = false;
            setHighlight(Inrange);
        }
        if(IsAmmo){
            if(Inrange && Input.GetKeyDown(pickupKey)){
                PlayerShootHitScan playerShoot = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerShootHitScan>();
                if(playerShoot != null){
                    playerShoot.AddAmmo(8f);
                    GameplayAudio.Instance?.PlayGameplaySFX("AmmoBox");
                    Destroy(gameObject);
                    return;
                }
            }
            return;
        }

        if(IsHealthPack){
            if(Inrange && Input.GetKeyDown(pickupKey)){
                PlayerHealth playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
                if(playerHealth != null){
                    if( playerHealth.currentHealth >= playerHealth.maxHealth){
                        return;
                    }
                    playerHealth.Heal(HealthPickupValue);
                    GameplayAudio.Instance?.PlayGameplaySFX("HealthPack");
                    Destroy(gameObject);
                    return;
                }
            }
            return;
        }

        
        Inputs();

        if(isHeld){
            transform.position = holdPoint.position;
            transform.rotation = holdPoint.rotation;
            transform.GetComponent<BoxCollider>().enabled = false;
        }else{
            transform.GetComponent<BoxCollider>().enabled = true;
        }
        
        
    }

    private void setHighlight(bool Highlight){
       if (Highlight) {
           HighlightMesh.GetComponent<MeshRenderer>().enabled = true;
       }else{
           HighlightMesh.GetComponent<MeshRenderer>().enabled = false;
       }
    }

    private void Inputs(){
        if (Input.GetKeyDown(pickupKey)) {
            if (!isHeld){
                TryPickup();
            }else{
                Putdown();
            }
        }
        
        if(Input.GetKeyUp(throwKey)&&isHeld){
            if(IsThrowable){   
                Throw();
            }
        }
    }


    private void TryPickup(){

        

        if(distance <= pickupRange * pickupRange && !isHeld){
            isHeld = true;
            rb.useGravity = false;
            rb.isKinematic = true;
            transform.parent = holdPoint;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            setHighlight(false);
        }


    }
    private void Putdown(){
        isHeld = false;
        rb.isKinematic = false;
        transform.SetParent(null);
        if(physicsDrop){
            rb.useGravity = true;
        }else{
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
    }

    private void HandleAiming(){


    }
    private void Throw(){
        isHeld = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.SetParent(null);
        rb.AddForce(holdPoint.forward * throwForce + Vector3.up * throwHeight, ForceMode.Impulse);
        rb.angularVelocity = Random.insideUnitSphere * 10f;
        IsThrown=true;

    }
    public void Trajectory()
{
    if(lineRenderer == null) return;

    int resolution = 50;
    lineRenderer.positionCount = resolution;

    Vector3 start = holdPoint.position + holdPoint.forward * 0.5f;
    Vector3 velocity = holdPoint.forward * throwForce + Vector3.up * throwHeight;

    for(int i = 0; i < resolution; i++) {
        float t = i / (float)(resolution - 1);
        Vector3 pos = start + velocity * t + 0.5f * Physics.gravity * t * t;
        Vector3 previousPos = start;
        if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z)) {
            pos = start; // fallback so Unity doesn’t crash
        }
        if (Physics.Linecast(previousPos, pos, out RaycastHit hit))
        {
            lineRenderer.SetPosition(i, hit.point);

            //velocity = Vector3.Reflect(velocity, hit.normal) * 0.5f; // halve speed after bounce

            lineRenderer.positionCount = i + 1;
            break;
        }
        lineRenderer.SetPosition(i, pos);
    }
}
private void OnCollisionEnter(Collision collision)
    {
        if(IsThrown){
            IsThrown=false; 
            if(collision.gameObject.CompareTag("Player")){
                return;
            }
            if(collision.gameObject.CompareTag("Enemy")){
                EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
                if(enemy != null){
                    enemy.OnShotHit(20f, true, 3f);
            }
            SoundSystem.Instance.EmitSound(transform.position, 40f, 200f, 0.7f, true, gameObject);
            Debug.Log("Thrown object emmited sound at " + transform.position);
        }
    }



}
}


