using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICharacterController : AIBaseController
{
  // expose this variable to PedestrianSpawner since aIData is protected
  public Waypoint currentWaypoint;
  public Vector3 worldDeltaPosition;
  public Vector2 groundDeltaPosition;
  public Vector2 velocity = Vector2.zero;

  // Start is called before the first frame update
  void Start()
  {
    aIData.agent = GetComponent<NavMeshAgent>();
    aIData.agentAnimController = GetComponent<Animator>();
    aIData.agent.updatePosition = false;

    aIData.waypointNavDirection = Mathf.RoundToInt(Random.Range(0f, 1f));
    if (currentWaypoint != null) aIData.currentWaypoint = currentWaypoint;

    aIData.walkSpeed = Random.Range(1.0f, 4.0f);
    aIData.runSpeed = Random.Range(4.0f, 6.0f);
    aIData.agentAnimController.SetFloat("velocity", !aIData.agent.isStopped ? 0.5f : 0);

    aIData.target = GameObject.FindGameObjectWithTag("Player").transform;
    if (aIData.agent != null)
    {
      SetState(AIState.WaypointNav);
    }
  }

  // Update is called once per frame
  void Update()
  {
    worldDeltaPosition = aIData.agent.nextPosition - transform.position;
    groundDeltaPosition.x = Vector3.Dot(transform.right, worldDeltaPosition);
    groundDeltaPosition.y = Vector3.Dot(transform.forward, worldDeltaPosition);
    velocity = (Time.deltaTime > 1e-5f) ? groundDeltaPosition / Time.deltaTime : velocity = Vector2.zero;

    float distance = Vector3.Distance(aIData.target.transform.position, transform.position);

    if (distance > aIData.maxChaseDistance)
    {
      if (aIData.isScared == true)
      {
        aIData.isScared = false;
        //SetState(AIState.Walk);
        SetState(AIState.WaypointNav);
        //aIData.agentAnimController.SetFloat("velocity", !aIData.agent.isStopped ? 0.5f : 0);
        aIData.agentAnimController.SetFloat("velocity", velocity.y);
        //aIData.agentAnimController.SetFloat("velocity", aIData.agent.velocity.y);
        Debug.Log("<color=red>Scared AI back to navigation: </color>");
      }
    }

    if (distance <= aIData.maxChaseDistance && aIData.isScared == false)
    {
      //SetState(AIState.TargetSpotted);

      //if (distance <= aIData.minAttackDistance)
      //{
      //    SetState(AIState.Attack);
      //}

      if (distance <= aIData.setIsScaredDistance)
      {
        aIData.isScared = true;
        SetState(AIState.IsScared);
        //aIData.agentAnimController.SetFloat("velocity", !aIData.agent.isStopped ? 1f : 0);
        aIData.agentAnimController.SetFloat("velocity", velocity.y);
        //aIData.agentAnimController.SetFloat("velocity", aIData.agent.velocity.y);
        Debug.Log("<color=green>AI is Scared: </color>");
      }
    }
    RunState();
  }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.tag == "Truck")
        {
            print("Truck hit...");
            FindObjectOfType<AudioManager>().Play("NPCScream");
        }
    }



    private void OnAnimatorMove()
  {
    //aIData.agent.transform.position = aIData.agentAnimController.deltaPosition;
    transform.position = aIData.agent.nextPosition;
  }

}
