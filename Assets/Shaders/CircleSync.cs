using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSync : MonoBehaviour
{
    //Citation: This shader code is heavily inspired by a tutorial
    //from Hunterson Studio https://www.youtube.com/watch?v=S5gdvibmsV0

    public static int PosID = Shader.PropertyToID("_PlayerPosition");
    public static int SizeID = Shader.PropertyToID("_Size");

    public Material wallMaterial;
    public Camera cam;
    public LayerMask mask;
    public int cutoutSize = 1;

    void Update()
    {
        var dir = cam.transform.position - transform.position;
        var ray = new Ray(transform.position, dir.normalized);

        if(Physics.Raycast(ray, 3000, mask))
        {
            wallMaterial.SetFloat(SizeID, cutoutSize);
        } else
        {
            wallMaterial.SetFloat(SizeID, 0);
        }

        var view = cam.WorldToViewportPoint(transform.position);
        wallMaterial.SetVector(PosID, view);
    }
    
}
