using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCutout : MonoBehaviour
{
    //Citation: This shader graph associated with this script is heavily inspired by a tutorial
    //from Daniel Ilett https://www.youtube.com/watch?v=jidloC6gyf8

    //Citation: This code below is heavily inspired by a tutorial
    //from Hunterson Studio https://www.youtube.com/watch?v=S5gdvibmsV0

    private Camera mainCamera;

    public Transform targetObject;
    [SerializeField] private LayerMask wallMask;
    //[SerializeField] private Material wallMaterial;
    [SerializeField] private Material[] buildingMaterials;

    [SerializeField] float cutoutSize = 0.2f;
    [SerializeField] float falloffSize = 0.05f;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();

    }


    private void Update()
    {
        CreateCutout();
    }
    /*
    private void CreateCutout()
    {
        Vector3 dir = targetObject.transform.position - transform.position;
        Ray ray = new Ray(transform.position, dir.normalized);
        RaycastHit hitinfo;

        if (Physics.Raycast(ray, out hitinfo, 3000, wallMask))
        {
            wallMaterial.SetFloat("_CutoutSize", cutoutSize);
                    
        }
        else
        {
            wallMaterial.SetFloat("_CutoutSize", 0);
        }

        Vector3 view = mainCamera.WorldToViewportPoint(targetObject.transform.position);
        wallMaterial.SetVector("_CutoutPosition", view);
        wallMaterial.SetFloat("_FalloffSize", falloffSize);
    }*/

    private void CreateCutout()
    {
        if (!targetObject) { return; }
        Vector3 dir = targetObject.transform.position - transform.position;
        Ray ray = new Ray(transform.position, dir.normalized);
        RaycastHit hitinfo;
        bool buildingHit = Physics.Raycast(ray, out hitinfo, dir.magnitude, wallMask);
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red);

        if (buildingHit)
        {
            foreach (Material material in buildingMaterials)
            {
                var view = mainCamera.WorldToViewportPoint(targetObject.transform.position);
                material.SetVector("_CutoutPosition", view);
                material.SetFloat("_CutoutSize", cutoutSize);
                material.SetFloat("_FalloffSize", falloffSize);
            }

        }
        else
        {
            foreach (Material material in buildingMaterials)
            {
                material.SetFloat("_CutoutSize", 0);
            }
        }
    }
}


