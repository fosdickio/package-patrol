using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    //This class is not currently in use. Audio is now managed centrally by the AudioManager.
    //This code inspired by this Jason Weimann tutorial. https://www.youtube.com/watch?v=Bnm8mzxnwP8

    [SerializeField] 
    private AudioClip[] walkingConrete;
    [SerializeField]
    private AudioClip[] shootPackage;


    private AudioSource audioSource;
    
     

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Step()
    {
        AudioClip clip = GetRandomClip(walkingConrete);
        audioSource.PlayOneShot(clip);
        
    }

    public void FireWeaponSound()
    {
        AudioClip clip = GetRandomClip(shootPackage);
        audioSource.PlayOneShot(clip);
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }
}
