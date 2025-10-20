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
    public GameObject deathScreenUI;
    public TextMeshProUGUI healthText;
    public Slider healthSlider; 

    

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
        if(currentHealth <= 0){
        Die();
        return;
        }
        return;
    }

    private void Die(){
        GameplayAudio.Instance?.PlayGameplaySFX("PlayerDeath");

        deathScreenUI.SetActive(true);
        Debug.Log("Player Died!");


        // Implement death behavior here (e.g., respawn, game over screen, etc.)
    }


}
