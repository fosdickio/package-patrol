using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawProjection : MonoBehaviour
{
    //This code inspired by tutorial by Adam Konig
    // found here: https://www.youtube.com/watch?v=RnEO3MRPr5Y
    [SerializeField] GameObject weapon;
    Weapon weaponController;
    LineRenderer lineRenderer;

    [SerializeField] int numPoints = 50;
    [SerializeField] float timeBetweenPoints = 0.1f;
    

    [SerializeField] LayerMask collidableLayers;
    public bool showProjection;


    void Awake()
    {
        weaponController = weapon.GetComponent<Weapon>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(showProjection)
        {
            DrawLine();
        }
        
    }

    private void DrawLine()
    {
        lineRenderer.positionCount = numPoints;
        List<Vector3> points = new List<Vector3>();
        Vector3 startingPosition = transform.position;
        Vector3 startingVelocity = transform.forward * weaponController.weaponForce;

        for (float t = 0; t < numPoints; t += timeBetweenPoints)
        {
            Vector3 newPoint = startingPosition + t * startingVelocity;
            newPoint.y = startingPosition.y + startingVelocity.y * t + Physics.gravity.y / 2f * t * t;
            points.Add(newPoint);

            if(Physics.OverlapSphere(newPoint, 2, collidableLayers).Length > 0)
            {
                lineRenderer.positionCount = points.Count;
                break;
            }
        }
        lineRenderer.SetPositions(points.ToArray());
    }
}
