using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : AIBaseController
{
    // Start is called before the first frame update
    void Start()
    {
        aIData.agent = GetComponent<NavMeshAgent>();
        aIData.target = GameObject.FindGameObjectWithTag("Player").transform;
        if (aIData.agent != null)
        {
            SetState(AIState.Walk);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(aIData.target.transform.position, transform.position);

        if (distance > aIData.maxChaseDistance)
        {
            if (aIData.isScared == true)
            {
                aIData.isScared = false;
                SetState(AIState.Walk);
            }
        }

        if (distance <= aIData.maxChaseDistance && aIData.isScared == false)
        {
            SetState(AIState.TargetSpotted);

            if (distance <= aIData.minAttackDistance)
            {
                SetState(AIState.Attack);
            }

            if (distance <= aIData.setIsScaredDistance)
            {
                aIData.isScared = true;
                SetState(AIState.IsScared);
            }
        }
        RunState();
    }

}
