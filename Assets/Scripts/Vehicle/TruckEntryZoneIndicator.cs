using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckEntryZoneIndicator : MonoBehaviour
{

    // The shader graph design for the renderer/material referenced here was found here:
    //https://gamedev.stackexchange.com/questions/167502/draw-a-perfect-circle-in-ui

    [SerializeField] float speed;
    public bool activated;
    private Renderer rend;
    private float t = 0.0f;
    private float scale;

    void Start()
    {
        rend = GetComponent<Renderer>();
        scale = 0.0f;
    }

    void Update()
    {
        //TODO consider putting these lerps in a coroutine. Checking this 
        //every frame could be expensive. Guidance here:
        //https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/#how_to_use_lerp_in_unity

        if (activated && t < 1)
        {
            scale = Mathf.Lerp(0, 1, t);
            rend.material.SetFloat("_Scale", scale);
            t += speed * Time.deltaTime;

            if (t > 1) { t = 1.0f; }

        } 
        else if (!activated && t > 0)
        {
            scale = Mathf.Lerp(0, 1, t);
            rend.material.SetFloat("_Scale", scale);
            t -= speed * Time.deltaTime;
            if (t < 0) { t = 0.0f; }
        }
    }

    IEnumerator ExpandEntryZone()
    {
        //This coroutine currently not in use.
        while ( t < 1 )
        {
            scale = Mathf.Lerp(0, 1, t);
            rend.material.SetFloat("_Scale", scale);
            t += speed * Time.deltaTime;

            yield return null;
        }  
    }
}
