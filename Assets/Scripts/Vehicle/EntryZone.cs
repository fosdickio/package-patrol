using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryZone : MonoBehaviour
{
    public TruckEntryZoneIndicator truckEntryZoneIndicator;
    public CarController truckController;
    public List<Transform> stacks;
    public GameObject packageLoadIndicator;

    void Start()
    {
        truckEntryZoneIndicator.activated = false;
    }

    void Update()
    {
        DisplayLoadIndicator();
    }

    private void DisplayLoadIndicator()
    {
        if (truckEntryZoneIndicator.activated)
        {
            packageLoadIndicator.SetActive(true);
            packageLoadIndicator.transform.position = GetIndicatorPosition();
        }
        else
        {
            packageLoadIndicator.SetActive(false);
        }
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

        foreach (Transform slot in closestStack)
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
            other.gameObject.GetComponent<CharacterMovement>().inVehicleEntryZone = true;
            truckEntryZoneIndicator.activated = true;
            truckController.playerInEntryZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponent<CharacterMovement>().inVehicleEntryZone = false;
            truckEntryZoneIndicator.activated = false;
            truckController.playerInEntryZone = false;
        }
    }

    Vector3 GetIndicatorPosition()
    {
        Transform closestStack = GetClosestStack();
        Transform highestSlot = GetHighestSlot(closestStack);

        if (highestSlot.childCount == 0)
        {
            return highestSlot.transform.position;
        } else
        {
            Vector3 offset = new Vector3(0, 0.15f, 0);
            return highestSlot.transform.position + offset;
        }
    }

    private Transform GetClosestStack()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        float currClosestStackDistance = Mathf.Infinity;
        Transform closestStack = null;

        foreach (Transform stack in stacks)
        {
            float distance = (stack.position - player.position).magnitude;

            if (distance < currClosestStackDistance)
            {
                closestStack = stack;
                currClosestStackDistance = distance;
            }
        }

        return closestStack;
    }

    Transform GetHighestSlot(Transform stack)
    {
        Transform highestSlot = stack.GetChild(stack.childCount - 1);
        float highestY = -Mathf.Infinity;

        foreach(Transform packageSlot in stack)
        {
            if(packageSlot.position.y > highestY && packageSlot.childCount > 0)
            {
                highestSlot = packageSlot;
                highestY = packageSlot.position.y;
            }
        }

        return highestSlot;
    }
}
