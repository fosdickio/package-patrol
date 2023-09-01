using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    //The structure of this class is borrowed from a Brackeys Audio tutorial.
    // https://www.youtube.com/watch?v=6OT43pvUyfY

    public string name;
    public AudioClip[] clips;

    [Range(0f, 1f)]
    public float volume;

    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
