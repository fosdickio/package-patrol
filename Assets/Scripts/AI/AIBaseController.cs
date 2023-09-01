using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBaseController : MonoBehaviour
{
  [SerializeField] protected AIData aIData;

  private void Idle() => RunAgent(transform.position, 0.0f);
  private void Walk() => RunAgent(RandomNavMeshLocation(), aIData.walkSpeed);
  private void Chase() => RunAgent(aIData.target.transform.position, aIData.chaseSpeed);
  private void TargetSpotted() => RunAgent(aIData.target.transform.position, aIData.chaseSpeed);
  private void Attack() => RunAgent(aIData.target.transform.position, aIData.attackSpeed);
  private void IsScared() => RunAgent(RandomNavMeshLocation(), aIData.runSpeed);
  //private void WaypointNav() => RunAgent(WaypointNavigation(), aIData.walkSpeed);
  private void WaypointNav() => WaypointNavigation();
  //   private void RangeAttack() => RangeProjectileAttack();

  protected void SetState(AIState state)
  {
    aIData.nextState = state;
    if (aIData.nextState != aIData.currentState)
    {
      aIData.currentState = aIData.nextState;
    }
  }

  protected void RunState()
  {
    switch (aIData.currentState)
    {
      case AIState.Idle:
        Idle();
        break;
      case AIState.Walk:
        Walk();
        break;
      case AIState.Chase:
        Chase();
        break;
      case AIState.TargetSpotted:
        TargetSpotted();
        break;
      case AIState.IsScared:
        IsScared();
        break;
      case AIState.Attack:
        Attack();
        break;
      //   case AIState.RangeAttack:
      //     RangeAttack();
      //     break;
      case AIState.WaypointNav:
        WaypointNav();
        break;
      default:
        break;
    }
  }

  private void RunAgent(Vector3 destination, float speed)
  {
    if (aIData.agent != null && aIData.agent.remainingDistance <= aIData.agent.stoppingDistance)
    {
      aIData.agent.speed = speed;
      aIData.agent.SetDestination(destination);
    }
  }

  // protected void RangeProjectileAttack()
  // {
  //     Debug.Log("<color=yellow>Enter Attack Mode!!! </color>" + aIData.alreadyAttacked);
  //     //Make sure enemy doesn't move
  //     aIData.agent.SetDestination(transform.position);

  //     transform.LookAt(aIData.target);

  //     if (!aIData.alreadyAttacked)
  //     {
  //         Debug.Log("<color=yellow>Start shooting!!! </color>" + aIData.alreadyAttacked);
  //         AttackBall();
  //         //StartCoroutine(ThrowPackage());

  //         aIData.alreadyAttacked = true;
  //         Invoke(nameof(ResetAttack), aIData.timeBetweenAttacks);
  //         Debug.Log("<color=yellow>Reset attack!!! </color>" + aIData.alreadyAttacked);
  //     }
  // }

  protected Vector3 RandomNavMeshLocation()
  {
    Vector3 finalPosition = Vector3.zero;
    Vector3 randomPosition = Random.insideUnitSphere * aIData.walkRadius;
    randomPosition += transform.position;
    if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, aIData.walkRadius, 1))
    {
      finalPosition = hit.position;
    }
    return finalPosition;
  }

  protected void WaypointNavigation()
  //protected Vector3 WaypointNavigation()
  {
    //Vector3 finalPosition = Vector3.zero;

    if ((aIData.agent.remainingDistance - aIData.agent.stoppingDistance) < 0 && aIData.agent.pathPending != true)
    {
      bool shouldBranch = false;
      //Debug.Log("<color=yellow>shouldBranch is now False: </color>");

      //Debug.Log("<color=orange>branches: </color>end line" + aIData.currentWaypoint.branches);
      if (aIData.currentWaypoint.branches != null && aIData.currentWaypoint.branches.Count > 0)
      {
        //Debug.Log("<color=orange>branches count > 0: </color>");
        shouldBranch = Random.Range(0f, 1f) <= aIData.currentWaypoint.branchRatio ? true : false;
      }

      if (shouldBranch)
      {
        //Debug.Log("<color=blue>shouldBranch if: </color>");
        aIData.currentWaypoint = aIData.currentWaypoint.branches[Random.Range(0, aIData.currentWaypoint.branches.Count - 1)];
      }
      else
      {
        //Debug.Log("<color=blue>shouldBranch else: </color>");
        if (aIData.waypointNavDirection == 0)
        {
          if (aIData.currentWaypoint.nextWaypoint != null)
          {
            //Debug.Log("<color=red>Go Next: </color>Entered if");
            aIData.currentWaypoint = aIData.currentWaypoint.nextWaypoint;
            //Debug.Log("<color=red>Go Next: </color>" + aIData.currentWaypoint);
          }
          else
          {
            //Debug.Log("<color=red>Go Prev: </color>Entered else");
            aIData.currentWaypoint = aIData.currentWaypoint.previousWaypoint;
            aIData.waypointNavDirection = 1;
            //Debug.Log("<color=red>Go Prev: </color>" + aIData.currentWaypoint);
          }

        }
        else if (aIData.waypointNavDirection == 1)
        {
          if (aIData.currentWaypoint.previousWaypoint != null)
          {
            //Debug.Log("<color=green>Go Prev: </color>Entered if");
            aIData.currentWaypoint = aIData.currentWaypoint.previousWaypoint;
            //Debug.Log("<color=green>Go Prev: </color>" + aIData.currentWaypoint);
          }
          else
          {
            //Debug.Log("<color=green>Go Next: </color>Entered else");
            aIData.currentWaypoint = aIData.currentWaypoint.nextWaypoint;
            aIData.waypointNavDirection = 0;
            //Debug.Log("<color=green>Go Next: </color>" + aIData.currentWaypoint);
          }
        }
      }

      aIData.agent.speed = aIData.walkSpeed;
      aIData.agent.SetDestination(aIData.currentWaypoint.GetPosition());
      //finalPosition = aIData.currentWaypoint.GetPosition();
    }

    //return finalPosition;
  }

  private void ResetAttack()
  {
    aIData.alreadyAttacked = false;
  }
}
