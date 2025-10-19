using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
   public float maxHealth = 100f;
    public float currentHealth = 100f;

    private void Start(){
        currentHealth = maxHealth;
    }

    void Update(){
        if(currentHealth > maxHealth){
            currentHealth = maxHealth;
        }
        if(currentHealth <= 0){
            Die();
            return;
        }
        float healthPercentage = (currentHealth / maxHealth) * 100f;

        if(healthPercentage < 50f){
            GameplayAudio.Instance?.PlayHeartBeat();
            GameplayAudio.Instance?.SetHeartbeatVolume(1 - healthPercentage/100f);
        }

    }

    public void Heal(float amount){
        currentHealth += amount;
        if(currentHealth > maxHealth){
            currentHealth = maxHealth;
        }
        Debug.Log("Healed! Current Health: " + currentHealth);
    }

    public void TakeDamage(float damage){
        currentHealth -= damage;
        Debug.Log("Took Damage! Current Health: " + currentHealth);
        if(currentHealth <= 0){
        Die();
        return;
        }
        return;
    }

    private void Die(){
        GameplayAudio.Instance?.PlayGameplaySFX("PlayerDeath");
        Debug.Log("Player Died!");
        // Implement death behavior here (e.g., respawn, game over screen, etc.)
    }


}
