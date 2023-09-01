using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityParticleSystem : MonoBehaviour
{
    [SerializeField] GameObject visibilityParticleSystem;
    [SerializeField] LayerMask mask;


    // Update is called once per frame
    void Update()
    {
        if(WeaponGrounded())
        {
            visibilityParticleSystem.SetActive(true);
        } else
        {
            if(visibilityParticleSystem)
            {
                visibilityParticleSystem.SetActive(false);
            }
            
        }
    }

    private bool WeaponGrounded()
    {
        if(!visibilityParticleSystem) { return false; }
        Ray ray = new Ray(visibilityParticleSystem.transform.position, Vector3.down);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo, 1f, mask))
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
            return true;
        } else
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.green);
            return false;
        }
    }
}
