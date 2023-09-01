using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckCollision : MonoBehaviour
{
    float lastCollisionTime;
    float noDamageTimeSeconds = 1;
    [SerializeField] GameObject truckCollisionEffect;
    [SerializeField] CarController truckController;

    void Start()
    {
        lastCollisionTime = Time.time;
    }


    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.tag == "Player") { return; }
        if (collision.gameObject.tag == "Citizen") { return; }
        if (collision.gameObject.tag == "Dog") { return; }
        if (collision.gameObject.tag == "Package") { return; }

        HandleCollisionEffects(collision);

        if (collision.gameObject.tag == "StreetLamp")
        {
            collision.gameObject.GetComponent<Rigidbody>().freezeRotation = false;
        }
        
        var currCollisionTime = Time.time;
        if (currCollisionTime < lastCollisionTime + noDamageTimeSeconds)
        {
            return;
        }
        lastCollisionTime = currCollisionTime;

        if (truckController) { truckController.TakeDamage(20); };
        
    }

    private void HandleCollisionEffects(Collision collision)
    {
        FindObjectOfType<AudioManager>().Play("TruckCollision");
        GameObject collisionEffect = Instantiate(truckCollisionEffect, collision.GetContact(0).point, Quaternion.identity);
        Destroy(collisionEffect, 3f);
    }


}
