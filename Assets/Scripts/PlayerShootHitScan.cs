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
    public ParticleSystem muzzleFlash;
    public GameObject impactEffectPrefab;
    public GameObject GunModel;
    public GameObject flashlightModel;
    public GameObject hitImpactEffectPrefab;
   

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
    private float nextTimeToFire = 0f;

    [Header("Melee references")]
    public Image MeleeIcon;
    public Image GunIcon;
    public Transform meleeOrigin;
    public GameObject MeleeModel;
    public KeyCode melee = KeyCode.Mouse0;

    [Header("Melee References")]
    public Transform AxeTransform;
    public Transform AxeHitOrigin;
    public float MeleeDamage = 40f;
    public float AxeHitRange = 2f;
    public float swingDuration = 0.5f;
    public float swingSpeed = 5f;
    public float swingAngle = 90f;
    public float stunDuration = 10f;
    public float MeleeCooldown = 1f;
    public float currentMeleeCooldown = 0f;
    public float MeleeDelay = 5f;
    bool isSwinging = false;
    public ParticleSystem AxeSwingEffect;
    

    

    public enum WeaponMode{
        Gun,
        Melee
    }

    public WeaponMode currentMode = WeaponMode.Gun;


    private void Awake(){
        if (camera == null)
        camera = Camera.main;
        UpdateWeaponModels();
    }

    public void Update(){
        if (Cursor.lockState != CursorLockMode.Locked) // Pause or other menu open, dont shoot
        return;
        ammoSlider.value = ammo;
        ammoText.text = Mathf.RoundToInt(ammo).ToString() + " | " + Mathf.RoundToInt(maxAmmo).ToString();
        heldAmmoText.text = " | " + Mathf.RoundToInt(HeldAmmo).ToString();

         if(Input.GetKeyDown(KeyCode.Alpha1)) {currentMode = WeaponMode.Gun; UpdateWeaponModels();}
        if(Input.GetKeyDown(KeyCode.Alpha2)) {currentMode = WeaponMode.Melee; UpdateWeaponModels();}

        if(isReloading) return;
        if(currentMode == WeaponMode.Gun){
            ShootInput();
            ReloadInput();
        }
        else if(currentMode == WeaponMode.Melee){
            MeleeInput();
        }
    }

    private void UpdateWeaponModels(){
        if(currentMode == WeaponMode.Gun){
            
            
            GunIcon.color = new Color(GunIcon.color.r, GunIcon.color.g, GunIcon.color.b, 1f);
            MeleeIcon.color = new Color(MeleeIcon.color.r, MeleeIcon.color.g, MeleeIcon.color.b, .3f);

            MeleeModel.SetActive(false);
            GunModel.SetActive(true);
            flashlightModel.SetActive(true);
            
        }else if(currentMode == WeaponMode.Melee){
            MeleeModel.SetActive(true);
            GunModel.SetActive(false);
            flashlightModel.SetActive(false);
            GunIcon.color = new Color(GunIcon.color.r, GunIcon.color.g, GunIcon.color.b, 0.3f);
            MeleeIcon.color = new Color(MeleeIcon.color.r, MeleeIcon.color.g, MeleeIcon.color.b, 1f);
            
        }
    }
    
    public void Shoot(){
        
        RaycastHit hit;
        ammo -= 1f;
        if(AlertsEnemies){
            SoundSystem.Instance.EmitSound(origin.position, 50f, 200f, 0.7f, AutoAlert, gameObject);
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
            //SpawnImpactEffect(hit.point, hit.normal);
            Instantiate(hitImpactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            
            Debug.Log("Hit " + hit.transform.name);
            EnemyAI enemy = hit.transform.GetComponent<EnemyAI>();
            if(enemy != null){
            
            enemy.OnShotHit(damage, false, 0f);
            GameplayAudio.Instance?.PlayGameplaySFX("Enemy Hit");
            }else{
                GameplayAudio.Instance?.PlayGameplaySFX("Impact SFX");
            }
        }
        muzzleFlash.Emit(1);

    }
    /*
    public void SpawnImpactEffect(Vector3 position, Vector3 normal){
        ParticleSystem impactEffect = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal)).GetComponent<ParticleSystem>();
        impactEffect.Play();
        Destroy(impactEffect.gameObject, impactEffect.main.duration);
    }
    */

    private void ShootInput(){
         if(Input.GetKey(shoot) && Time.time >= nextTimeToFire){
        Shoot();
        nextTimeToFire = Time.time + firerate;
    }
    }

    private void MeleeInput(){
        Debug.Log("meleeinput");

        currentMeleeCooldown += Time.deltaTime;
        //currentMeleeCooldown = Mathf.Min(currentMeleeCooldown, MeleeDelay);

        if(Input.GetKey(melee) && currentMeleeCooldown>MeleeDelay){
            Debug.Log("melee attampt");
            SwingAxe();
            currentMeleeCooldown = 0f;
        }
    }

    public void SwingAxe(){
        if (isSwinging) return;
        isSwinging = true;
        GameplayAudio.Instance?.PlayGameplaySFX("Swing Axe");
        StartCoroutine(SwingAxeCoroutine());
    }

    private IEnumerator SwingAxeCoroutine(){
        float elapsed = 0f;
        Quaternion initialRotation = AxeTransform.localRotation;
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0f, 0f, swingAngle);

        if(AxeSwingEffect != null) AxeSwingEffect.Play();

        HashSet<EnemyAI> hitEnemies = new HashSet<EnemyAI>();

        while(elapsed < swingDuration){

            
            elapsed += Time.deltaTime;
            float t = elapsed / swingDuration;

            AxeTransform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, (elapsed / swingDuration) * swingSpeed);
            

            Collider[] results =  new Collider[32];
            int enemyLayerMask = LayerMask.GetMask("Enemy"); 
            int count = Physics.OverlapSphereNonAlloc(AxeHitOrigin.position, AxeHitRange, results, enemyLayerMask);
            
            if (enemyLayerMask == 0) {
                Debug.LogError("Enemy layer not found! Make sure it exists.");
                yield return null;
            }
            if(AxeHitOrigin == null){
                Debug.LogError("Melee origin not set!");
                yield return null;
            }

            for ( int i = 0; i < count; i++ ) {

                EnemyAI enemy = results[i].GetComponent<EnemyAI>();
                if(enemy != null && !hitEnemies.Contains(enemy)){


                    GameplayAudio.Instance?.PlayGameplaySFX("Melee");
                    enemy.OnMeleeHit(MeleeDamage, stunDuration, true);
                    hitEnemies.Add(enemy);
                    Debug.Log("Melee hit " + enemy.name);
                }
            }

            yield return null;

        }

        float returnTimer = 0f;

        while (returnTimer < swingDuration * 0.5f)
        {
            returnTimer += Time.deltaTime;
            float t = returnTimer / (swingDuration * 0.5f);
            AxeTransform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, t);
            yield return null;
        }

        AxeTransform.localRotation = initialRotation;
        isSwinging = false;
    }


    public void MeleeAttack(){

        Debug.Log("attacking melee");



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
