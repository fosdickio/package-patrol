using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WaypointNavigator : MonoBehaviour
{
    //[SerializeField] float rootMovementSpeed;

    private NavMeshAgent agent;
    private Animator NPCAnimController;
    public Waypoint currentWaypoint;
    public float NPCWalkSpeed;
    //private Rigidbody rigidBody;

    int direction;

    // Start is called before the first frame update
    void Start()
    {
        direction = Mathf.RoundToInt(Random.Range(0f, 1f));
        //direction = 0;
        NPCAnimController = GetComponent<Animator>();
        //rigidBody = GetComponent<Rigidbody>();

        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(currentWaypoint.GetPosition());
        NPCWalkSpeed = Random.Range(1.0f, 4.0f);
        agent.speed = NPCWalkSpeed;
        NPCAnimController.SetFloat("velocity", !agent.isStopped ? 0.5f : 0);
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
                        Debug.Log("<color=red>Go Next: </color>" + currentWaypoint);
                    }
                    else
                    {
                        currentWaypoint = currentWaypoint.previousWaypoint;
                        direction = 1;
                        Debug.Log("<color=red>Go Prev: </color>" + currentWaypoint);
                    }
                    
                }
                else if (direction == 1)
                {
                    if (currentWaypoint.previousWaypoint != null)
                    {
                        currentWaypoint = currentWaypoint.previousWaypoint;
                        Debug.Log("<color=green>Go Prev: </color>" + currentWaypoint);
                    }
                    else
                    {
                        currentWaypoint = currentWaypoint.nextWaypoint;
                        direction = 0;
                        Debug.Log("<color=green>Go Next: </color>" + currentWaypoint);
                    }
                }
            }

            agent.SetDestination(currentWaypoint.GetPosition());
        }
    }

    //void OnAnimatorMove()
    //{
    //    Vector3 newRootPosition;

    //    newRootPosition = new Vector3(NPCAnimController.rootPosition.x, this.transform.position.y, NPCAnimController.rootPosition.z);
    //    newRootPosition = Vector3.LerpUnclamped(this.transform.position, newRootPosition, rootMovementSpeed);

    //    rigidBody.MovePosition(newRootPosition);
    //}
}
