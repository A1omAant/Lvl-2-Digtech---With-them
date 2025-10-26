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
    public GameObject RedScreenOverlay;
    
    public bool dead = false;

    

    private void Start(){
        currentHealth = maxHealth;
        
        
    }

    void Update(){
        RedScreen();
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
        //GameplayAudio.Instance?.StopAllSounds();

        Debug.Log("Player Died!");
        pauseMenuController.Die();
        // Implement death behavior here (e.g., respawn, game over screen, etc.)
    }

    private void RedScreen(){
        // fade in red screen using alpha relative to health
        float alpha = 1 - (currentHealth / maxHealth); 
        // alpha goes from 0 to 1 as health goes from max to 0 starting when health is less then 50%
        if(currentHealth < 50f){
            alpha = Mathf.Lerp(0f, 0.1f, (50f - currentHealth) / 50f);
        }
        else{
            alpha = 0f;
        }   
        Color color = RedScreenOverlay.GetComponent<Image>().color;
        color.a = alpha;
        RedScreenOverlay.GetComponent<Image>().color = color;
    }




}
