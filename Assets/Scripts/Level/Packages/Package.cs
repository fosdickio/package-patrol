using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour
{
    public ParticleSystem smokeTrail;
    public LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        smokeTrail.Stop();
    }

    public void playSmokeTrail()
    {
        smokeTrail.Play();
    }

    void OnCollisionEnter(Collision collision)
    {
        smokeTrail.Stop();
        if(collision.gameObject.layer != 6) { return; } //6 is the ground layer, unable to reference with layer mask
        FindObjectOfType<AudioManager>()?.Play("PackageCollision");
    }
}
