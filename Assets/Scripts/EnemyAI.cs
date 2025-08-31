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
    public float AlertWaitTime;
    public float AlertMoveSpeed;
    public Vector3 lastHeardPos;

    [Header("Chasing")]
    public float chasingSpeed;
    public float chasingMaxDistance; //optional, might amke it so they can only for max distance away from their position stored from their last state


    [Header("Attacking")]
    public float attackRange;
    public float attackDamage;
    bool alreadyAttacked;
    
    [Header("states")]
   
    public EnemyState state;

    public enum EnemyState{
        Idle,
        Patroling,
        Alerted,
        Chasing,
        Attacking
    }

    

   


    private void Awake(){
        seen = false;
        agent = GetComponent<NavMeshAgent>();

        if (patrolPoints.Count > 0)
        {
            walkpoint = patrolPoints[currentPatrolIndex].position;
            agent.SetDestination(walkpoint);
        }
        player = GameObject.Find("Player Controller").transform;
        LayerMask whatIsPlayer = LayerMask.GetMask("Player");

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
                //investigate();
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
            Debug.Log($"hit {hit.transform.gameObject}");
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

    public void OnSoundDetected(float volume, Vector3 position, GameObject source)
    {
        bool ShouldUpdatePos = false;
        float heardVolume = volume * hearingSense;
        Debug.Log($"Enemy {name} detected sound with volume {heardVolume} at position {position} from gameobject {source}");
        if (state == EnemyState.Chasing || state == EnemyState.Attacking){
            lastHeardPos = position;
            return; 
        }    


        if (source.CompareTag("Player")){
            if (heardVolume > aggroCutoffPlayer){
                state = EnemyState.Chasing; // chase if player makes loud noise
                ShouldUpdatePos = true;
                return;
            }
            else if (heardVolume > alertCutoff){
                state = EnemyState.Alerted; // investigate if player makes moderate noise
                ShouldUpdatePos = true;
                return;
            }
        }else{
            if (heardVolume > aggroCutoff){
                state = EnemyState.Alerted; // investigate if non player loud noise
                ShouldUpdatePos = true;
                return;
            }
            else if (heardVolume > alertCutoff){
                state = EnemyState.Alerted; // investigate if non player moderate noise
                ShouldUpdatePos = true;
                return;
            }
        }
        if(ShouldUpdatePos) lastHeardPos = position;

    }
    public void chasePlayer(){
        agent.isStopped = false;
        float targetHeight = 0.2f;
        agent.baseOffset = Mathf.Lerp(agent.baseOffset, targetHeight, Time.deltaTime * 2f);
        agent.SetDestination(player.position);
    }

    public void Idle(){


        float distanceToIdle = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),new Vector3(idleSpot.x, 0, idleSpot.z));

        //Debug.Log(distanceToIdle);
        if(distanceToIdle <1f){
            agent.isStopped = false;
            agent.SetDestination(idleSpot);
        }else{
            
            
                float targetHeight = 0.1f;
                agent.baseOffset = Mathf.Lerp(agent.baseOffset, targetHeight, Time.deltaTime * 2f);
                agent.isStopped = true;
                Scan(5f);
            
            
            
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
        if(distanceToPoint <= 0.5f){
            targetSet = false;

            waittimer += Time.deltaTime;
            Scan(5f);
            if(waittimer > waitTimeAtPoint){
                waittimer = 0f;
                currentPatrolIndex = (currentPatrolIndex+1)%patrolPoints.Count;
            }
        }
    }
    public void investigate(){
        agent.isStopped = false;
    }
    public void Attack(){
        
    }
    public void Scan(float radius){
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
}