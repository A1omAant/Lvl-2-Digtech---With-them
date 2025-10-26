using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{

    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public LayerMask obstructions;
    public GameObject idlelocation;
    public Vector3 idleSpot; 
    public float IdleRotationSpeed;
    private Vector3 scanTarget;   
    private bool hasScanTarget = false;

    [Header("Patroling")]
    public Vector3 walkpoint;
    [SerializeField] private List<Transform> patrolPoints; 
    [SerializeField] private float waitTimeAtPoint = 2f;
    private float waittimer;
    private int currentPatrolIndex = 0;
    public float patrolSpeed;
    bool targetSet = false;

    [Header("Detection Visual")]
    public float sightRange;
    public float alertRange;
    public float aggroRange;
    public float sightAngle;
    public bool PlayerInSightRange, PlayerInAttackRange;
    public bool seen;

    [Header("detection Audio")]
    public float hearingSense;
    public float alertCutoff;
    public float aggroCutoff;
    public float aggroCutoffPlayer;

    [Header("Alert")]
    public float AlertWaitTime = 15f;
    public float AlertMoveSpeed;
    public Vector3 lastHeardPos;

    [Header("Chasing")]
    public float chasingSpeed;
    public float chasingMaxDistance; //optional, might amke it so they can only for max distance away from their position stored from their last state
    public float LostSightDuration = 5f;
    public float lostSightTimer = 0f;
    bool canSee = false;


    [Header("Attacking")]
    public float attackRange;
    public float timeBetweenAttacks;
    public float attackFollowDistance;
    public float attackDamage;
    bool alreadyAttacked = false;
    public ParticleSystem attackEffect;
    public Transform attackPoint;
    
    [Header("Idle Settings")]
    public bool idleScan;
    public GameObject DroneLight;

    [Header("Audio")]
    public List<AudioClip> alertSounds;
    public List<AudioClip> chaseSounds;
    public List<AudioClip> attackSounds;
    public List<AudioClip> idleSounds;
    public AudioSource EnemyAudioSource;

    
    [Header("states")]
   
    public EnemyState state;
    private EnemyHealth self;
    private EnemyState previousState;

    public enum EnemyState{
        Idle,
        Patroling,
        Alerted,
        Chasing,
        Attacking,
        Stunned,
        Dead
    }

    

   


    private void Awake(){
        seen = false;
        agent = GetComponent<NavMeshAgent>();
        self = GetComponent<EnemyHealth>();
        state = EnemyState.Idle;
        
        //patrolPoints = new List<Transform>();


        if (patrolPoints.Count > 0)
        {
            walkpoint = patrolPoints[currentPatrolIndex].position;
            agent.SetDestination(walkpoint);
        }
        player = GameObject.Find("Player Controller").transform;
        LayerMask whatIsPlayer = LayerMask.GetMask("Player");
        idleSpot = idlelocation.transform.position;

    }


    
    private void Update(){
        CheckHealth();
        if(state != EnemyState.Dead) {
        HandleAudio();
        PlayerInSight();
        SetState();    
        }
    } 

    private void PlayerInSight(){
         
        PlayerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        PlayerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        
        if(PlayerInSightRange){
            CanSeePlayer(player);
        }

    }

    public void HandleAudio(){
        if(!EnemyAudioSource.isPlaying){
            switch(state){
                case EnemyState.Idle:
                    if(idleSounds.Count > 0){
                        EnemyAudioSource.clip = idleSounds[Random.Range(0, idleSounds.Count)];
                        //EnemyAudioSource.Play();
                    }
                    break;
                case EnemyState.Patroling:
                    //patrol sounds
                    break;
                case EnemyState.Alerted:
                    if(alertSounds.Count > 0){
                        EnemyAudioSource.clip = alertSounds[Random.Range(0, alertSounds.Count)];
                        //EnemyAudioSource.Play();
                    }
                    break;
                case EnemyState.Chasing:
                    if(chaseSounds.Count > 0){
                        EnemyAudioSource.clip = chaseSounds[Random.Range(0, chaseSounds.Count)];
                        EnemyAudioSource.Play();
                    }
                    break;
                case EnemyState.Attacking:
                    //attack sounds handled in attack function
                    break;
                case EnemyState.Stunned:
                    //stun sounds
                    break;
                case EnemyState.Dead:
                    //death sounds
                    break;
                
                default:
                    break;
            }
        }
    }

    void SetState(){
        switch(state){
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patroling:
                agent.speed = patrolSpeed;
                //patrol();
                break;
            case EnemyState.Alerted:
                investigate();
                
                agent.speed = AlertMoveSpeed;
                break;
            case EnemyState.Chasing:
                chasePlayer();
                agent.speed = chasingSpeed;
                break;
            case EnemyState.Attacking:
                Attack();
                agent.speed = chasingSpeed;
                break;
            case EnemyState.Stunned:
                //stun
                break;
            case EnemyState.Dead:
                //dead
                break;
            default:
                //handle werid things
                break;
        }
    }


    private void CanSeePlayer(Transform player){
        Vector3 DirectionToPlayer = player.position - transform.position;
        float DistanceToPlayer = DirectionToPlayer.magnitude;
        float AngleToPlayer = Vector3.Angle(transform.forward, DirectionToPlayer);


        if(DistanceToPlayer <= sightRange && AngleToPlayer <= sightAngle/2f){
            
            if (Physics.Raycast(transform.position, DirectionToPlayer.normalized, out RaycastHit hit, sightRange, ~0))
            {
                Debug.Log($"Raycast hit {hit.transform.gameObject}");
                canSee = hit.collider.gameObject == player.gameObject;
            }

        }
        
        if(canSee){
             
            if(state == EnemyState.Idle && !idleScan){ // if idle and not scanning, don't notice player visually
                return;
            }
            //Debug.Log($"hit {hit.transform.gameObject}");
            lostSightTimer = 0f;
            if(DistanceToPlayer <= attackRange){
                state = EnemyState.Attacking; // attack if very close
            }else if (DistanceToPlayer <= aggroRange){
                state = EnemyState.Chasing; // chase if seen but close
            }

        }else if(state == EnemyState.Chasing || state == EnemyState.Attacking){
            lostSightTimer += Time.deltaTime;
            if(lostSightTimer >= LostSightDuration){
                state = EnemyState.Idle; // go to idle if lost sight for too long
            }
        }

        
    }

    public void OnSoundDetected(float volume, Vector3 position, GameObject source, bool alertinsphere)
    {
        if(state == EnemyState.Dead || state == EnemyState.Stunned)
            return;
        if (alertinsphere)
        {
            //state = EnemyState.Chasing;
            /*
            float distanceToPlayer = Vector3.Distance(transform.position, source.transform.position);
            if (distanceToPlayer <= aggroRange)
            {
                state = EnemyState.Chasing;
            }else if (distanceToPlayer <= alertRange)
            {
                lastHeardPos = position;
                state = EnemyState.Alerted;
            }
            */
            //check if player is on navmesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(source.transform.position, out hit, 2f, NavMesh.AllAreas)) {
                state = EnemyState.Chasing;
                lastHeardPos = hit.position;
                Debug.Log($"Enemy {name} auto-alerted to sound at position {hit.position} from gameobject {source}");
            }
            return;
        }

        bool ShouldUpdatePos = false;
        float heardVolume = volume * hearingSense;
        Debug.Log($"Enemy {name} detected sound with volume {heardVolume} at position {position} from gameobject {source}");
        if (state == EnemyState.Chasing || state == EnemyState.Attacking){
            lastHeardPos = position;
            return; 
        }    


        if (source.CompareTag("Player")){
            if (heardVolume > aggroCutoffPlayer){
                Debug.Log("aggro to player");
                state = EnemyState.Chasing; // chase if player makes loud noise
                ShouldUpdatePos = true;
                return;
            }
            else if (heardVolume > alertCutoff){
                lastHeardPos = position;
                state = EnemyState.Alerted; // investigate if player makes moderate noise
                ShouldUpdatePos = true;
                return;
            }
        }else{
            if (heardVolume > aggroCutoff){
                lastHeardPos = position;
                state = EnemyState.Alerted; // investigate if non player loud noise
                ShouldUpdatePos = true;
                return;
            }
            else if (heardVolume > alertCutoff){
                lastHeardPos = position;
                state = EnemyState.Alerted; // investigate if non player moderate noise
                ShouldUpdatePos = true;
                return;
            }
        }
        if(ShouldUpdatePos) lastHeardPos = position;

    }
    public void chasePlayer(){

        agent.isStopped = false;
        float targetHeight = 0.3f;

        agent.baseOffset = Mathf.Lerp(agent.baseOffset, targetHeight, Time.deltaTime * 2f);

        Vector3 DirectionToPlayer = (player.position - transform.position).normalized;
        Vector3 offset = DirectionToPlayer * 0.3f;
        //Vector3 RandomOffset = new Vector3(Random.Range(-2f,2f),0f,Random.Range(-2f,2f));
        Vector3 targetPosition = player.position + offset;
        NavMeshHit hit;
        if( NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas)) {
            targetPosition = hit.position;
        }
        agent.SetDestination(targetPosition);
    }
  
    public void Idle(){


        
        //Debug.Log(distanceToIdle);
        //Debug.Log(agent.isStopped);

        //check if idlespot is on navmesh
        NavMeshHit hit;

        if (NavMesh.SamplePosition(idleSpot, out hit, 2f, NavMesh.AllAreas)) { // 2f is the max distance to sample
            idleSpot = hit.position; // snap to navmesh position
        }

        float distanceToIdle = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z), new Vector3(idleSpot.x, 0f, idleSpot.z));


        if(distanceToIdle < 2f){
            agent.isStopped = true;
            
            float targetHeight = 0.1f;
            if(agent.baseOffset >= targetHeight+0.01f){
                agent.baseOffset = Mathf.Lerp(agent.baseOffset, targetHeight, Time.deltaTime * 2f);
            }
             if (idleScan)
            {
                DroneLight.SetActive(true);
                Debug.Log("Idle Scanning");
                Scan(5f, true);
            }
            else
            {
                DroneLight.SetActive(false);
            }

        }else{
            agent.isStopped = false;
            agent.SetDestination(idleSpot);

        }
    }

    

    public void patrol(){
        if (patrolPoints.Count == 0) return;

        Transform targetpoint = patrolPoints[currentPatrolIndex];

        if(!targetSet){
            agent.SetDestination(targetpoint.position);
            targetSet = true;
        }

        float distanceToPoint = Vector3.Distance(transform.position, targetpoint.position);
        if(distanceToPoint <= 2f){
            targetSet = false;

            waittimer += Time.deltaTime;
            Scan(5f, true);
            if(waittimer > waitTimeAtPoint){
                waittimer = 0f;
                currentPatrolIndex = (currentPatrolIndex+1)%patrolPoints.Count;
            }
        }
    }
    public void investigate(){

        agent.isStopped = false;
        float targetHeight = 0.2f;
        agent.baseOffset = Mathf.Lerp(agent.baseOffset, targetHeight, Time.deltaTime * 2f);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(lastHeardPos, out hit, 2f, NavMesh.AllAreas)) {
            agent.isStopped = false;
        agent.SetDestination(hit.position);
        }else if(!NavMesh.SamplePosition(lastHeardPos, out hit, 10f, NavMesh.AllAreas)){
            Debug.Log("Cannot reach investigation point");
            state = EnemyState.Idle;
            return;
        }
        float distanceToPoint = Vector3.Distance(transform.position, hit.position);

        if(distanceToPoint <= 5f){
            Debug.Log("Reached investigation point");
            Scan(5f, false);
            AlertWaitTime -= Time.deltaTime;
            if(AlertWaitTime <= 0f){
                AlertWaitTime = 15f;
                state = EnemyState.Idle;
            }
        }



    }

    public void Attack(){
        agent.isStopped = true;
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y; 
        float targetHeight = 0.3f;

        agent.baseOffset = Mathf.Lerp(agent.baseOffset, targetHeight, Time.deltaTime * 2f);
        transform.LookAt(lookPos);

        
        if(!alreadyAttacked){
            //attack code here
            Debug.Log("Enemy Attacking Player");
            //Damage player
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if(playerHealth != null){
                EnemyAudioSource.clip = attackSounds[Random.Range(0, attackSounds.Count)];
                EnemyAudioSource.Play();
                if(attackEffect != null){
                    Instantiate(attackEffect, attackPoint.position, attackPoint.rotation);
                }
                playerHealth.TakeDamage(attackDamage);
            }
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        
    }

    private void ResetAttack(){
        alreadyAttacked = false;
    }

    public void Scan(float radius, bool CheckRandom){
        //Debug.Log("Scanning");
        agent.isStopped = true;
        //if(!hasScanTarget){

        Vector3 DirToTarget = (scanTarget - transform.position);
        DirToTarget.y = 0f;


        if(scanTarget == Vector3.zero || DirToTarget.magnitude < 2f || Quaternion.Angle(transform.rotation, Quaternion.LookRotation(DirToTarget)) < 5f) // if no target or close to target, pick new target
        {
            float angle = Random.Range(0f,360f);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 randomOffset = new Vector3(Mathf.Cos(rad),0f,Mathf.Sin(rad))*radius;
            scanTarget = transform.position + randomOffset;
            //hasScanTarget=true;
            //Debug.Log($"New scan target at {scanTarget}");
        
        }

        
        DirToTarget = (scanTarget - transform.position).normalized;
        if(DirToTarget != Vector3.zero){
            Quaternion lookRotation = Quaternion.LookRotation(DirToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * IdleRotationSpeed);
            //Debug.Log($"Scanning towards {scanTarget}");
        }

        //float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
        //if(angleToTarget < 4f){ 
        //    hasScanTarget = false; 
        //}


    }

    public void OnMeleeHit(float damage, float duration, bool stun){

        if(state == EnemyState.Idle || state == EnemyState.Patroling){
            self.TakeDamage(damage*10);
            state = EnemyState.Chasing;
        }
        if(stun){
            StopAllCoroutines();
            StartCoroutine(Stun(duration));
        }

        self.TakeDamage(damage);
       // do a flinch animation or something
    }

    public void OnShotHit(float damage, bool stun, float duration ){
        Debug.Log("Enemy hit by shot");
        self.TakeDamage(damage);

        if(stun){
            StopAllCoroutines();
            StartCoroutine(Stun(duration));
        }
        state = EnemyState.Chasing;
        agent.isStopped = false;
    }

    public IEnumerator Stun(float duration){

        previousState = state;
        Debug.Log("Enemy Stunned");
        state = EnemyState.Stunned;
        float targetHeight = 0.1f;
        DroneLight.SetActive(false);

        agent.baseOffset = Mathf.Lerp(agent.baseOffset, targetHeight, Time.deltaTime * 2f);
        agent.isStopped = true;
        yield return new WaitForSeconds(duration);

        state = previousState;
        agent.isStopped = false;
        DroneLight.SetActive(true);
        float targetHeight2 = 0.3f;
        agent.baseOffset = Mathf.Lerp(agent.baseOffset, targetHeight2, Time.deltaTime * 2f);
        Debug.Log("Enemy Stun Ended");
    }
    public void CheckHealth(){
        if(self.health <= 0){
            state = EnemyState.Dead;
            StopAllCoroutines();
            agent.isStopped = true;

        }
    }
}