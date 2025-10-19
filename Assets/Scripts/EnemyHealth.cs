using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
   [Header("References")]
   //public ParticleSystem sparks;
   //public ParticleSystem smoke;

   [Header("Health")]
   public float health;
   public float maxHealth = 100f;

   [Header("Die")]
   public GameObject meshRoot;
   public Collider[] colliderToDisable;

   private bool dead = false;

   private void Awake(){
        health = maxHealth;
        //smoke.Stop();
        //sparks.Stop();

   }

    public void TakeDamage (float damage){

        if(dead)return;

        health -= damage;
        float HealthPercent = health/maxHealth;

       if (HealthPercent <= 0f){
            die();
        }

    }

    private void die(){
        dead = true;

        Destroy(gameObject);
    }



}
