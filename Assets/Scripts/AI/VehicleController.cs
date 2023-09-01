using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class VehicleController : MonoBehaviour
{
  //[SerializeField] float rootMovementSpeed;

  private NavMeshAgent agent;
  private Animator VehicleAnimController;
  public Waypoint currentWaypoint;
  public float VehicleRoamSpeed;
  //private Rigidbody rigidBody;

  int direction;

  // Start is called before the first frame update
  void Start()
  {
    //direction = Mathf.RoundToInt(Random.Range(0f, 1f));
    direction = 0;
    //VehicleAnimController = GetComponent<Animator>();
    //rigidBody = GetComponent<Rigidbody>();

    agent = GetComponent<NavMeshAgent>();
    agent.SetDestination(currentWaypoint.GetPosition());
    VehicleRoamSpeed = Random.Range(5.0f, 10.0f);
    agent.speed = VehicleRoamSpeed;
    //VehicleAnimController.SetFloat("velocity", !agent.isStopped ? 0.5f : 0);
  }

  // Update is called once per frame
  void Update()
  {


    if ((agent.remainingDistance - agent.stoppingDistance) < 0 && agent.pathPending != true)
    {
      bool shouldBranch = false;

      if (currentWaypoint.branches != null && currentWaypoint.branches.Count > 0)
      {
        shouldBranch = Random.Range(0f, 1f) <= currentWaypoint.branchRatio ? true : false;
      }

      if (shouldBranch)
      {
        currentWaypoint = currentWaypoint.branches[Random.Range(0, currentWaypoint.branches.Count - 1)];
      }
      else
      {
        if (direction == 0)
        {
          if (currentWaypoint.nextWaypoint != null)
          {
            currentWaypoint = currentWaypoint.nextWaypoint;
            //Debug.Log("<color=orange>Car Go Next: </color>" + currentWaypoint);
          }
          else
          {
            currentWaypoint = currentWaypoint.previousWaypoint;
            direction = 1;
            //Debug.Log("<color=orange>Car Go Prev: </color>" + currentWaypoint);
          }

        }
        else if (direction == 1)
        {
          if (currentWaypoint.previousWaypoint != null)
          {
            currentWaypoint = currentWaypoint.previousWaypoint;
            //Debug.Log("<color=brown>Car Go Prev: </color>" + currentWaypoint);
          }
          else
          {
            currentWaypoint = currentWaypoint.nextWaypoint;
            direction = 0;
            //Debug.Log("<color=brown>Car Go Next: </color>" + currentWaypoint);
          }
        }
      }

      agent.SetDestination(currentWaypoint.GetPosition());
    }
  }
}
