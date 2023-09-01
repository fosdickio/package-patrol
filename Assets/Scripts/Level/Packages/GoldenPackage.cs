using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenPackage : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.gameObject.ToString());
        
        if (other.gameObject.CompareTag("Player"))
        {
            PlayGoldenPackageSound();
            gameObject.SetActive(false);
        }
    }
    
    private void PlayGoldenPackageSound()
    {
        FindObjectOfType<AudioManager>()?.Play("PackageDelivered");
    }
}
