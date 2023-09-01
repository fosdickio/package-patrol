using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICharacterEnemyController : AIEnemyBaseController
{
  // expose this variable to PedestrianSpawner since aIData is protected
  // public Waypoint currentWaypoint;
  //public float timeBetweenAttacks;
  //bool alreadyAttacked;
  public Vector3 worldDeltaPosition;
  public Vector2 groundDeltaPosition;
  public Vector2 velocity = Vector2.zero;
  private Rigidbody rigidBody;

  float moveSpeed = 0f;
  private float turnSpeed = 0f;
  private CharacterController characterController;
  private Transform playerTransform;
  public Vector3 targetPosition;


  // Start is called before the first frame update
  private void Awake()
  {
    //
  }
  void Start()
  {
    rigidBody = GetComponent<Rigidbody>();
    characterController = GetComponent<CharacterController>();
    aIData.agent = GetComponent<NavMeshAgent>();
    aIData.agentAnimController = GetComponent<Animator>();
    // aIData.agent.updatePosition = false;
    // aIData.agent.updateRotation = false;

    playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    // aIData.target = playerObject.transform;
    // aIData.targetVelocity = playerObject.GetComponent<Rigidbody>().velocity;
    aIData.target = playerTransform;
    aIData.isScared = false;

    if (aIData.agent != null)
    {
      SetState(AIState.Idle);
      HandleBeginIdle();
    }
  }

  // Update is called once per frame
  void Update()
  {
        if (aIData.SpawnLocation == null || transform == null || targetPosition == null)
        {
            return;
        }
    targetPosition = aIData.target.position;
    float distanceToTarget = Vector3.Distance(targetPosition, transform.position);
    float distanceToBase = Vector3.Distance(aIData.SpawnLocation.transform.position, transform.position);
    //Debug.Log("<color=orange>Enemy Controller: </color> distanceToTarget is " + distanceToTarget);

    if (aIData.isScared == false)
    {
      if (distanceToTarget > aIData.maxChaseDistance)
      {
        if (distanceToBase <= aIData.agent.stoppingDistance)
        {
          //Debug.Log("<color=green>Enter Idle Mode!!! </color>" + aIData.alreadyAttacked);
          targetPosition = aIData.target.position;
          SetState(AIState.Idle);
          HandleBeginIdle();
        }
        else
        {
          //Debug.Log("<color=orange>Enter RTB Mode!!! </color>" + aIData.alreadyAttacked);
          targetPosition = aIData.SpawnLocation.transform.position;
          SetState(AIState.RTB);
          FaceDirection((targetPosition - transform.position).normalized);
          HandleRTB();
        }
      }

      if (distanceToTarget <= aIData.maxChaseDistance && aIData.isScared == false)
      {
        //Debug.Log("<color=red>Enter Chase Mode!!! </color>" + aIData.alreadyAttacked);
        targetPosition = aIData.target.position;
        SetState(AIState.TargetSpotted);
        FaceDirection((targetPosition - transform.position).normalized);
        HandleBeginChase();


        // if (distanceToTarget <= aIData.minAttackDistance)
        // {
        //   Debug.Log("<color=brown>Enter min Attack Range!!! </color>");
        //   Debug.Log("<color=yellow>Enter Attack Mode!!! </color>" + aIData.alreadyAttacked);
        //   SetState(AIState.Attack);
        //   FaceDirection((targetPosition - transform.position).normalized);
        //   HandleBeginAttack();
        //   aIData.alreadyAttacked = true;
        //   //  Invoke(nameof(ResetAttack), timeBetweenAttacks);
        //   Debug.Log("<color=yellow>Finished Transition Into Attack Mode!!! </color>" + aIData.alreadyAttacked);
        // }

        //if (distanceToTarget <= aIData.setIsScaredDistance)
        //{
        //    aIData.isScared = true;
        //    SetState(AIState.IsScared);
        //    aIData.agentAnimController.SetFloat("velocity", !aIData.agent.isStopped ? 1f : 0);
        //    Debug.Log("<color=green>AI is Scared: </color>");
        //}
      }
    }
    else
    {
      if (distanceToTarget >= aIData.setIsScaredDistance)
      {
        Debug.Log("<color=green>Exit Flee Mode!!! </color>" + aIData.isScared);
        aIData.isScared = false;
      }
      else
      {
        Debug.Log("<color=red>Enter Flee Mode!!! </color>" + aIData.isScared);

        targetPosition = RandomNavMeshLocation();
        SetState(AIState.IsScared);
        FaceDirection((targetPosition - transform.position).normalized);
        HandleBeginFlee();
        
      }
    }

    if (aIData.agent)
    {
      aIData.agent.destination = targetPosition;
      aIData.agent.speed = moveSpeed;
      aIData.agent.angularSpeed = turnSpeed;
    }
    else
      characterController.SimpleMove(moveSpeed * UnityEngine.Vector3.ProjectOnPlane(targetPosition - transform.position, Vector3.up).normalized);


    RunState();

  }

  //private void ResetAttack()
  //{
  //    aIData.alreadyAttacked = false;
  //}

  private void OnDrawGizmosSelected()
  {
    // Debug.Log("<color=purple>OnDrawGizmosSelected: </color> 1");

    if (aIData.minAttackDistance > 0.0f)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(transform.position, aIData.minAttackDistance);
    }
    if (aIData.maxChaseDistance > 0.0f)
    {
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(transform.position, aIData.maxChaseDistance);
    }
    if (aIData.agent && aIData.agent.stoppingDistance > 0.0f)
    {
      Gizmos.color = Color.green;
      Gizmos.DrawWireSphere(transform.position, aIData.agent.stoppingDistance);
    }

    // Debug.Log("<color=purple>OnDrawGizmosSelected: </color> 2");
    if (aIData.target != null)
    {
      // Draws a blue line from this transform to the target
      // Debug.Log("<color=purple>OnDrawGizmosSelected: </color> 3");
      Gizmos.color = Color.blue;
      Gizmos.DrawLine(transform.position, targetPosition);
      // Debug.Log("<color=purple>OnDrawGizmosSelected: </color> 4");
    }
  }


  // private void OnAnimatorMove()
  // {
  //   //aIData.agent.transform.position = aIData.agentAnimController.deltaPosition;
  //   transform.position = aIData.agent.nextPosition;
  // }

  void FaceDirection(Vector3 facePosition)
  {
    transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.RotateTowards(transform.forward,
        facePosition, turnSpeed * Time.deltaTime * Mathf.Deg2Rad, 0f), Vector3.up), Vector3.up);
  }

  void HandleBeginIdle()
  {
    ClearAnimatorBools();
    // TrySetBool("isHowling", true);
    TrySetBool("isSitting", true);
    moveSpeed = 0f;
    //idleEvent.Invoke();
  }

  void HandleBeginChase()
  {
    SetMoveFast();
    GetComponent<Dog>().Chase();

    //movementEvent.Invoke();
  }

  void SetMoveFast()
  {
    //UnityEngine.Assertions.Assert.IsNotNull(moveState, string.Format("{0}'s wander script does not have any movement states.", gameObject.name));
    turnSpeed = 200f;
    moveSpeed = 6f;
    ClearAnimatorBools();
    TrySetBool("isRunning", true);
  }

  void HandleRTB()
  {
    turnSpeed = 200f;
    moveSpeed = 2f;
    ClearAnimatorBools();
    TrySetBool("isWalking", true);
  }

  void HandleBeginAttack()
  {
    aIData.agent.SetDestination(transform.position);
    turnSpeed = 120f;
    ClearAnimatorBools();
    TrySetBool("isAttacking", true);
    // attackingEvent.Invoke();
  }

  void ClearAnimatorBools()
  {
    TrySetBool("isRunning", false);
    TrySetBool("isWalking", false);
    TrySetBool("isSitting", false);
    TrySetBool("isBarking", false);
    // TrySetBool("isHowling", false);
    // TrySetBool("isAttacking", false);
    // TrySetBool("isDead", false);
  }

  void TrySetBool(string parameterName, bool value)
  {
    if (!string.IsNullOrEmpty(parameterName))
    {
      aIData.agentAnimController.SetBool(parameterName, value);
    }
  }

  void HandleBeginFlee()
  {
    SetMoveFast();
    //movementEvent.Invoke();
  }

  void OnParticleCollision(GameObject other)
  {
    GetComponent<Dog>().Flee();
    if (aIData.isScared != true)
    {
      aIData.isScared = true;
    }
    

    }
}
