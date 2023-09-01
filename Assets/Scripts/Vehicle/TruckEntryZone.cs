using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckEntryZone : MonoBehaviour
{
    public GameObject entryIndicator;
    public CarController truckController;
    public List<Transform> stacks;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadPackage(GameObject packageToLoad)
    {
        Transform closestStack = null;
        float currClosestDistance = Mathf.Infinity;

        if (stacks.Count == 0) { return; }


        foreach (Transform stack in stacks)
        {
            
            float distance = (packageToLoad.transform.position - stack.transform.position).magnitude;

            if (distance < currClosestDistance)
            {
                closestStack = stack;
                currClosestDistance = distance;
            }
        }

        foreach(Transform slot in closestStack)
        {
            if (slot.childCount == 0)
            {
                packageToLoad.transform.parent = slot;
                packageToLoad.transform.position = slot.position;
                packageToLoad.transform.rotation = slot.rotation;
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerController>().inVehicleEntryZone = true;
            entryIndicator.SetActive(true);
            truckController.playerInEntryZone = true;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerController>().inVehicleEntryZone = false;
            entryIndicator.SetActive(false);
            truckController.playerInEntryZone = false;
        }
        

    }
}
