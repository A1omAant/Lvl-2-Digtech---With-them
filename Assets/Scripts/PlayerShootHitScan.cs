using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class PlayerShootHitScan : MonoBehaviour
{

    [Header("References")]
    public Transform origin;
    public Camera camera;
    public KeyCode shoot = KeyCode.Mouse0;
    public Slider ammoSlider;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI heldAmmoText;
   

    [Header("shooting affinities")]
    public float damage = 20f;
    public float firerate = 0.5f;
    public float range = 20f;
    private float shootTimer=0;
    public float ammo = 5f;
    public float maxAmmo = 8f;
    public float reloadTime = 3f;
    public float reloadEmptyTimer = 4f;
    public float MaxHeldAmmo = 24f;
    public float HeldAmmo = 16f;
    public bool isReloading = false;
    public bool fullAuto = false;
    public bool AutoAlert = true;
    public bool AlertsEnemies = true;


    private void Awake(){
        if (camera == null)
        camera = Camera.main;
    }

    public void Update(){
        ammoSlider.value = ammo;
        ammoText.text = Mathf.RoundToInt(ammo).ToString() + " | " + Mathf.RoundToInt(maxAmmo).ToString();
        heldAmmoText.text = " | " + Mathf.RoundToInt(HeldAmmo).ToString();
   
        if(isReloading) return;
       ShootInput();
       ReloadInput();
    }
    
    public void Shoot(){
        
        RaycastHit hit;
        ammo -= 1f;
        if(AlertsEnemies){
            SoundSystem.Instance.EmitSound(origin.position, 40f, 200f, 0.7f, AutoAlert, gameObject);
        }
        Debug.Log("Shot fired! with sound emitted.");
        if(ammo < 0f){
            ammo = 0f;
            Debug.Log("Out of ammo!");
            GameplayAudio.Instance?.PlayGameplaySFX("dryfire");
            return;
        }

        GameplayAudio.Instance?.PlayGameplaySFX("shoot");

        if(Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, range)){
            Debug.Log("Hit " + hit.transform.name);
            EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
            if(enemy != null){
            enemy.OnShotHit(damage, false, 0f);
            }
        }

    }

    private void ShootInput(){
        shootTimer += Time.deltaTime;

         if(Input.GetKey(shoot)&&shootTimer>firerate){

            Shoot();
            shootTimer = 0f;
        }
    }
    private void ReloadInput(){
        if(Input.GetKeyDown(KeyCode.R)){

            if(!isReloading && ammo < maxAmmo && HeldAmmo > 0f){
                Debug.Log("Reloading...");
                isReloading = true;
                if(ammo <= 0f){
                    ammoText.text =  " 0 | " + Mathf.RoundToInt(maxAmmo).ToString();
                    Invoke("Reload", reloadEmptyTimer);
                    GameplayAudio.Instance?.PlayGameplaySFX("IAmReloading");
                    GameplayAudio.Instance?.PlayGameplaySFX("reloading from empty");
                }else{
                    ammoText.text =  " 0 | " + Mathf.RoundToInt(maxAmmo).ToString();
                    Invoke("Reload", reloadTime);
                    GameplayAudio.Instance?.PlayGameplaySFX("reload");
                }
            }
        }
    }

    private void Reload(){

        if(HeldAmmo <= 0f){
            Debug.Log("No held ammo to reload!");
        }else{
            float ammoNeeded = maxAmmo - ammo;
            if(HeldAmmo >= ammoNeeded){
                ammo += ammoNeeded;
                HeldAmmo -= ammoNeeded;
            }else{
                ammo += HeldAmmo;
                HeldAmmo = 0f;
            }
        }

        isReloading = false;
        Debug.Log("Reloaded!");
    }

    public void AddAmmo(float amount){
        HeldAmmo += amount;
        if(HeldAmmo > MaxHeldAmmo){
            HeldAmmo = MaxHeldAmmo;
        }
        Debug.Log("Picked up ammo. Held Ammo: " + HeldAmmo);
    }


    


}
