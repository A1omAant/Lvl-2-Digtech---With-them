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
    public float attackrange;
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


    [Header("Attacking")]
    public float attackRange;
    public float timeBetweenAttacks;
    public float attackFollowDistance;
    public float attackDamage;
    bool alreadyAttacked = false;

    [Header("Audio")]
    public List<AudioClip> alertSounds;
    public List<AudioClip> chaseSounds;
    public List<AudioClip> attackSounds;
    public List<AudioClip> idleSounds;
    public AudioSource EnemyAudioSource;

    
    [Header("states")]
   
    public EnemyState state;
    private EnemyHealth self;

    public enum EnemyState{
        Idle,
        Patroling,
        Alerted,
        Chasing,
        Attacking,
        Stunned
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

        PlayerInSight();
        SetState();    
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
                        EnemyAudioSource.Play();
                    }
                    break;
                case EnemyState.Patroling:
                    //patrol sounds
                    break;
                case EnemyState.Alerted:
                    if(alertSounds.Count > 0){
                        EnemyAudioSource.clip = alertSounds[Random.Range(0, alertSounds.Count)];
                        EnemyAudioSource.Play();
                    }
                    break;
                case EnemyState.Chasing:
                    if(chaseSounds.Count > 0){
                        EnemyAudioSource.clip = chaseSounds[Random.Range(0, chaseSounds.Count)];
                        EnemyAudioSource.Play();
                    }
                    break;
                case EnemyState.Attacking:
                    if(attackSounds.Count > 0){
                        EnemyAudioSource.clip = attackSounds[Random.Range(0, attackSounds.Count)];
                        EnemyAudioSource.Play();
                    }
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
                //Attack();
                agent.speed = chasingSpeed;
                break;
            case EnemyState.Stunned:
                //stun
                break;
            default:
                //handle werid things
                break;
        }
    }


    private void CanSeePlayer(Transform player){
        Vector3 DirectionToPlayer = player.position - transform.position;
        float DistanceToPlayer = DirectionToPlayer.magnitude;

        if(DistanceToPlayer > sightRange){
            return;
        }

        float AngleToPlayer = Vector3.Angle(transform.forward, DirectionToPlayer);
        if(AngleToPlayer > sightAngle/2f){
            return;
        }

        
        if(Physics.Raycast(transform.position, DirectionToPlayer.normalized, out RaycastHit hit, sightRange, ~0)){
            //Debug.Log($"hit {hit.transform.gameObject}");
            if (hit.collider.gameObject != player.gameObject) return;
   
            if (DistanceToPlayer <= attackrange){
                state = EnemyState.Attacking; // attack if very close and seen
            }else if (DistanceToPlayer <= aggroRange){
                state = EnemyState.Chasing; // chase if seen but close
            }else if (DistanceToPlayer <= alertRange){
                state = EnemyState.Alerted; // alert if seen but far
            }
            }

        return;
    }

    public void OnSoundDetected(float volume, Vector3 position, GameObject source, bool alertinsphere)
    {
        if (alertinsphere)
        {
            state = EnemyState.Chasing;
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


        agent.SetDestination(player.position);
    }
  
    public void Idle(){


        float distanceToIdle = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),new Vector3(idleSpot.x, 0, idleSpot.z));
        Scan(5f, true);
        Debug.Log(distanceToIdle);
        Debug.Log(agent.isStopped);

        //check if idlespot is on navmesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(idleSpot, out hit, 2f, NavMesh.AllAreas)) { // 2f is the max distance to sample
            idleSpot = hit.position; // snap to navmesh position
        }

        if(distanceToIdle > 5f){
            agent.isStopped = false;
            agent.SetDestination(idleSpot);
        }else{
            
                Scan(5f, true);
                float targetHeight = 0.1f;
                if(agent.baseOffset != targetHeight){
                    agent.baseOffset = Mathf.Lerp(agent.baseOffset, targetHeight, Time.deltaTime * 2f);
                }
                agent.isStopped = true;
                
                
            
            
            
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
        agent.SetDestination(hit.position);
        }
        float distanceToPoint = Vector3.Distance(transform.position, hit.position);

        if(distanceToPoint <= 5f){
            Scan(5f, false);
            AlertWaitTime -= Time.deltaTime;
            if(AlertWaitTime <= 0f){
                AlertWaitTime = 15f;
                state = EnemyState.Idle;
            }
        }



    }

    public void Attack(){

        
        transform.LookAt(player);

        if(!alreadyAttacked){

            //attack code here
            Debug.Log("Enemy Attacking Player");
            //Damage player
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if(playerHealth != null){
                playerHealth.TakeDamage(attackDamage);
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }

        state = EnemyState.Chasing;

        
    }
    private void ResetAttack(){
        alreadyAttacked = false;
    }

    public void Scan(float radius, bool CheckRandom){
        agent.isStopped = false;
        if(!hasScanTarget){
        float angle = Random.Range(0f,360f);
        float rad = angle * Mathf.Deg2Rad;
        Vector3 randomOffset = new Vector3(Mathf.Cos(rad),0f,Mathf.Sin(rad))*radius;
        scanTarget = transform.position + randomOffset;
        hasScanTarget=true;
        }

        Vector3 Dir = (scanTarget- transform.position).normalized;
        if(Dir != Vector3.zero){
            Quaternion lookRotation = Quaternion.LookRotation(Dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * IdleRotationSpeed);
        }

        float angleToTarget = Vector3.Angle(transform.forward, Dir);
        if(angleToTarget < 4f){ 
            hasScanTarget = false; 
        }

    }

    public void OnMeleeHit(float damage, float duration, bool stun){
        
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
        Debug.Log("Enemy Stunned");
        state = EnemyState.Stunned;
        agent.isStopped = true;
        GetComponentInChildren<Renderer>().material.color = Color.yellow;
        yield return new WaitForSeconds(duration);
        GetComponentInChildren<Renderer>().material.color = Color.white;
        state = EnemyState.Chasing;
        agent.isStopped = false;
    }
}