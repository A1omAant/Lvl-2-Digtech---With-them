using UnityEngine;

public class PlayerShootHitScan : MonoBehaviour
{

    [Header("References")]
    public Transform origin;
    public Camera camera;
    public KeyCode shoot = KeyCode.F;

    [Header("shooting affinities")]
    public float damage = 10f;
    public float firerate = 0.5f;
    public float range = 20f;
    private float shootTimer=0;

    private void Awake(){
        
    }

    public void Update(){

       ShootInput();
    }
    
    public void Shoot(){

        if(Physics.Raycast(origin.position, Vector3.forward, out RaycastHit hit, range)){
            
            GameObject hitObject = hit.collider.gameObject;
            EnemyHealth enemy = hitObject.GetComponent<EnemyHealth>();

            Debug.Log(hit.collider.gameObject);
            enemy.TakeDamage(damage);

        }

    }

    private void ShootInput(){
        shootTimer += Time.deltaTime;

         if(Input.GetKey(shoot)&&shootTimer>firerate){

            Shoot();
            shootTimer=0;
            
            }else{
                return;
            }
    }

}
