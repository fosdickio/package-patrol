using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    //The structure of this class was inspired by a Brackeys Audio tutorial
    // found here: https://www.youtube.com/watch?v=6OT43pvUyfY

    public Sound[] sounds;
    public static AudioManager instance;
    AudioSource audioSource;

    void Awake()
    {
        if(instance == null) 
        { instance = this; } 
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);

        /*
        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }*/

    }

    //avoid double playing theme music
    public void playThemeMusic()
    {

        Sound s = Array.Find(sounds, sound => sound.name == "ThemeMusic");

        if (s.source && s.source.isPlaying)
        {
            return;
        }

        if (s.loop)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.clip = s.clips[0];
            s.source.Play();
        }
    }
    public void Play(string name)
    {
        //This method plays audio requested by scripts in the project.
        //If the item is not looped, the existing Audio Source is used.
        //If the audio loops, a separate Audio Source is created.

        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null) 
        {
            Debug.LogWarning($"Sound: {name} not found.");
            return; 
        }
        
        if (s.loop)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.clip = s.clips[0];
            s.source.Play();
        } else if(audioSource!=null)
        {
            audioSource.volume = s.volume;
            audioSource.pitch = s.pitch;
            audioSource.PlayOneShot(GetRandomClip(s.clips));
        }
    }

    public void Stop(string name)
    {      
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Unable to stop. Sound: {name} not found.");
            return;
        }
        s.source.Stop();

        if (s.source.loop == true)
        {
            Destroy(s.source);
        }
        

    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips.Length == 1) { return clips[0]; }
        else
        {
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }
        
    }
}
