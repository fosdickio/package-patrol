using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject minimapCam;
    public static bool isInTruck = false;
    public GameObject markerPrefab;
    public GameObject packageMarkerPrefab;
    public Material playerMarkerMaterial;

    void Start()
    {
        if (GameObject.FindGameObjectsWithTag("MinimapCamera").Length != 0)
        {

            minimapCam = GameObject.FindGameObjectsWithTag("MinimapCamera")[0];
        }

    }

    // Update is called once per frame
    void Update()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length==0)
        {
            return;
        }

        var player = players[0];


        bool attachedMarkers = GameObject.FindGameObjectsWithTag("markerPrefab").Length > 0;


        if (!attachedMarkers)
        {
            var inst = Instantiate(markerPrefab, player.transform);
            inst.layer = 7;//minimap layer
            var scale = inst.transform.localScale;
            //scale.y *=2;
            inst.transform.localScale = scale;
            inst.GetComponent<Renderer>().material = playerMarkerMaterial;

            foreach (var pkg in GameObject.FindGameObjectsWithTag("Package"))
            {

                inst = Instantiate(packageMarkerPrefab, pkg.transform);
                inst.layer = 7;//minimap layer
                scale = inst.transform.localScale;
                //scale.y *=2;
                inst.transform.localScale = scale*2;
                inst.GetComponent<Renderer>().material.color = pkg.GetComponent<Renderer>().material.color;
            }
        }


        var playerPos = player.transform.position;
        playerPos.y += 80;
        if(minimapCam)
            minimapCam.transform.position= playerPos; 
    }
}
