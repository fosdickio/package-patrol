using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
  None,
  Idle,
  Walk,
  Chase,
  TargetSpotted,
  IsScared,
  Attack,
  RangeAttack,
  WaypointNav,
  RTB
}

[System.Serializable]
public struct AIData
{
  public NavMeshAgent agent;
  public Animator agentAnimController;
  public Transform target;
  public Vector3 targetVelocity;
  public AIState currentState;
  [HideInInspector] public AIState nextState;

  [Range(0, 100)] public float walkSpeed;
  [Range(0, 100)] public float chaseSpeed;
  [Range(0, 100)] public float attackSpeed;
  [Range(0, 100)] public float runSpeed;

  [Range(1, 500)] public float walkRadius;

  [Range(1, 100)] public float minChaseDistance;
  [Range(1, 100)] public float maxChaseDistance;
  [Range(1, 100)] public float setIsScaredDistance;
  [Range(1, 100)] public float minAttackDistance;

  public bool isScared;

  public int waypointNavDirection;
  public Waypoint currentWaypoint;

  public bool alreadyAttacked;
  public float timeBetweenAttacks;
  public GameObject SpawnLocation;
}
