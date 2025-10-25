using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;


public class PlayerHealth : MonoBehaviour
{
   public float maxHealth = 100f;
    public float currentHealth = 100f;
    //public GameObject deathScreenUI;
    public TextMeshProUGUI healthText;
    public Slider healthSlider; 
    public PauseMenuController pauseMenuController;
    
    public bool dead = false;

    

    private void Start(){
        currentHealth = maxHealth;
        
        
    }

    void Update(){
        if(currentHealth > maxHealth){
            currentHealth = maxHealth;
        }
        if(currentHealth <= 0f){
            Die();
            return;
        }
        float healthPercentage = (currentHealth / maxHealth) * 100f;
        healthText.text = Mathf.RoundToInt(currentHealth).ToString();
        healthSlider.value = currentHealth / maxHealth;
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
        if(currentHealth <= 0f){
            Debug.Log("Health <= 0, calling Die()");
        Die();
        return;
        }
      
    }

    private void Die(){
        if(dead)return;
        dead = true;
        //disable player controls here
        GetComponent<PlayerMovement>().enabled = false;
        //GetComponent<PlayerLook>().enabled = false;
        GetComponent<PlayerShootHitScan>().enabled = false;
        // Show death screen UI
        // cull audio
        GameplayAudio.Instance?.StopAllSounds();
        GameplayAudio.Instance?.PlayGameplaySFX("Player Death");
        GameplayAudio.Instance?.PlayGameplaySFX("PlayerDeath");
        //GameplayAudio.Instance?.StopAllSounds();

        Debug.Log("Player Died!");
        pauseMenuController.Die();
        // Implement death behavior here (e.g., respawn, game over screen, etc.)
    }




}
