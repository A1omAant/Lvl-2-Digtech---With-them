using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
   [Header("References")]
   public ParticleSystem sparks;
   public ParticleSystem smoke;
   public ParticleSystem deathEffect;
   public AudioClip deathSound;
   public AudioSource audioSource;

   [Header("Health")]
   public float health;
   public float maxHealth = 100f;

   [Header("Die")]
   public GameObject meshRoot;
   public Collider[] colliderToDisable;
   

   public bool dead = false;

   private void Awake(){
        health = maxHealth;
        //smoke.Stop();
        //sparks.Stop();

   }
   void Update(){
        if(health <= maxHealth * 0.5f && !smoke.isPlaying){
            smoke.Play();
        }
        if(health <= maxHealth * 0.25f && !sparks.isPlaying){
            sparks.Play();
        }
        if(health > maxHealth * 0.5f){
            smoke.Stop();
        }
        if(health > maxHealth * 0.25f){
            sparks.Stop();
        }
        checkHealth();
        
   }

    public void TakeDamage (float damage){

        if(dead)return;

        health -= damage;
        checkHealth();
       

    }
    private void checkHealth(){
        if(health <= 0){
            die();
        }
    }

    private void die(){
        
        if(dead) return;
        dead = true;

        GetComponent<EnemyAI>().enabled = false;
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
        GetComponent<UnityEngine.AI.NavMeshAgent>().ResetPath();
        GetComponent<UnityEngine.AI.NavMeshAgent>().velocity = Vector3.zero;
        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        


        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
             // play death sfxeffect
        }

        if(deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(deathEffect.gameObject, 5f);
        }
        if(smoke.isPlaying){
            smoke.Stop();
        }
        if(sparks.isPlaying){
            sparks.Stop();
        }
        

        DoPhysicsdeath(gameObject);

        if(meshRoot != null){ // detach children from root
            foreach(Transform child in meshRoot.transform){
                child.parent = null;
                DoPhysicsdeath(child.gameObject);
                StartCoroutine(DieCoroutine(child.gameObject));
            }

        }
        StartCoroutine(DieCoroutine(gameObject));
        

       
    }

    private void DoPhysicsdeath(GameObject obj){

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if(rb == null){
            rb = obj.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;

        rb.useGravity = true;
        rb.mass = 1f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.5f;
        Vector3 ForceDir = (Vector3.up * 3f) + (Random.insideUnitSphere * 1.5f);
        rb.AddForce(ForceDir * 3f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);

    }

    private IEnumerator DieCoroutine(GameObject obj){
        yield return new WaitForSeconds(5f);

        foreach(Collider col in colliderToDisable){
            col.enabled = false;
        }

        Destroy(obj);
    }


}
