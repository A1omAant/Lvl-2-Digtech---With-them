using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    public static SoundSystem Instance; //create instance

    private void Awake() //when the script is loaded
    {
        Debug.Log("SoundSystem Awake called");
        Instance = this; //create instance
    }

    public void EmitSound(Vector3 position, float radius, float magnitude, float attenuationValue, bool AutoAlertInSphere){
        Debug.Log("Sound emitted SoundSystem");
        Collider[] results = new Collider[32]; //new collider array up to 32 items
        int enemyLayerMask = LayerMask.GetMask("Enemy"); //enemy layermask
        if (enemyLayerMask == 0) {
            Debug.LogError("Enemy layer not found! Make sure it exists.");
            return;
        }
        int count = Physics.OverlapSphereNonAlloc(position, radius, results, enemyLayerMask);//overlap sphere from position, with radius radius, checking results on enemy layermask

        for ( int i = 0; i < count; i++ ) {

            EnemyAI enemy = results[i].GetComponent<EnemyAI>();
            
            if (enemy != null) { // Check if the enemy exists
            Debug.Log($"Found enemy: {enemy.name}"); // Get the EnemyAI component from the collider
                if(!AutoAlertInSphere){ // If not auto-alerting in sphere


                Vector3 dir = enemy.transform.position - position; // Direction from sound source to enemy
                if(Physics.Raycast(position, dir.normalized, out RaycastHit hitInfo, radius)) { //physics raycast, checking line of sight
                   if(hitInfo.collider.gameObject == enemy.gameObject || hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("PrefabTrans")) { //if hits enemy or if it hits a transparent prefab
                        float distance = Vector3.Distance(position, enemy.transform.position); // Distance from sound source to enemy
                        float normalizedDistance = (distance / radius); // Normalize distance
                        float volume = magnitude * (1 - normalizedDistance); // Calculate volume based on distance
                        volume = Mathf.Max(0, volume);// Ensure volume is not negative and not greater than intial magnitiude
                        enemy.OnSoundDetected(volume); // Notify enemy of detected sound
                        Debug.Log($"Sound emitted to enemy {enemy.name} with volume {volume}");
                    }
                    else if(hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Wall")||hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("PrefabSolid")||hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("ground")) { // If hits wall or a solid prefab or the ground

                        Vector3 hitpoint = hitInfo.point; // find the point where the raycast hit the wall
                        float distanceToObstacle = Vector3.Distance(position, hitpoint); // distance from the sound source to the wall
                        float distEnemyFromObstacle = Vector3.Distance(hitpoint, enemy.transform.position); //distance from the enemy to the wall

                        float volumeToObstacle = magnitude * (1-(distanceToObstacle/radius)); // Calculate volume carried to the wall
                        float volume = volumeToObstacle* (1 - distEnemyFromObstacle/radius); // Calculate volume based on distance from wall to enemy and the volume to the wall
                        volume *= (1-attenuationValue); // multiply by attenuation, fixed fraction of how absorband the wall is
                        volume = Mathf.Max(0, volume);// Ensure volume is not negative and not greater than intial magnitiude
                        enemy.OnSoundDetected(volume); // Notify enemy of detected sound
                    }

                }
            }else{
                float volume = magnitude; // if autodetect, no math, enemy gets propagated initial magnitude regardless of obsticals or line of sight
                enemy.OnSoundDetected(volume); // Notify enemy of detected sound
            }
            }


        }
    }
    
}
