using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource musicSource, sfxSource;

    public SoundScript[] music, sfx;
    
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic("Background");
    }

    public void PlayMusic(string name)
    {
        SoundScript s = Array.Find(instance.music, sound => sound.name == name);

        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        else
        {
            instance.musicSource.clip = s.clip;
            instance.musicSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        SoundScript s = Array.Find(instance.sfx, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        else
        {
            instance.sfxSource.PlayOneShot(s.clip);
        }
    }
}
