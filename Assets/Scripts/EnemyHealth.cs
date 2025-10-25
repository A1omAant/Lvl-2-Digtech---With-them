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
        if (audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
             // play death sfxeffect
            meshRoot.GetComponent<MeshRenderer>().enabled = false;


        }
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        dead = true;
        GetComponent<EnemyAI>().enabled = false;
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
        GetComponent<UnityEngine.AI.NavMeshAgent>().velocity = Vector3.zero;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if(rb == null){
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.AddForce(Vector3.up * 50f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        //StartCoroutine(DieCoroutine());
        Destroy(gameObject, 1f);
        

       
    }

    private IEnumerator DieCoroutine(){
        dead = true;
        GetComponent<EnemyAI>().enabled = false;
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
        GetComponent<UnityEngine.AI.NavMeshAgent>().velocity = Vector3.zero;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        if(rb == null){
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.AddForce(Vector3.up * 50f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        foreach(Collider col in colliderToDisable){
            col.enabled = false;
        }
        if(deathEffect != null){
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        float fadetime = 3f;
        Renderer[] renderers = meshRoot.GetComponentsInChildren<Renderer>();
        float elapsed = 0f;
        while(elapsed < fadetime){
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadetime);
            foreach(Renderer rend in renderers){
                foreach(Material mat in rend.materials){
                    Color color = mat.color;
                    color.a = alpha;
                    mat.color = color;
                }
            }
            yield return null;
        }
        Destroy(gameObject);

    }



}
