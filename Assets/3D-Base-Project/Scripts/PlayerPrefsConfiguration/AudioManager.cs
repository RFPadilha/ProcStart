using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Sound Files")]
    public Sound[] musicSounds;
    public Sound[] sfxSounds;
    public Sound[] voiceSounds;

    [Header("Sound Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;

    private bool enableAll = true;
    private bool enableMusic = true;
    private bool enableSFX = true;
    private bool enableVoice = true;
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
            Debug.Log("More than one audio manager found in scene, destroying most recent");
        }
    }
    private void Start()
    {
        PlayMusic("DuneDreamer");
    }
    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, x => x.name == name);
        if(s == null)
        {
            Debug.LogWarning($"Music with name \"{name}\" not found.");
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }
    }
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning($"SFX with name \"{name}\" not found.");
        }
        else
        {
            sfxSource.PlayOneShot(s.clip);
        }
    }

    public void PlayVoice(string name)
    {
        Sound s = Array.Find(voiceSounds, x => x.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Voice clip with name \"{name}\" not found.");
        }
        else
        {
            voiceSource.PlayOneShot(s.clip);
        }
    }

    public void EnableAllSounds(bool value)
    {
        enableAll = value;
        musicSource.mute = !(enableAll && enableMusic);
        sfxSource.mute = !(enableAll && enableSFX);
        voiceSource.mute = !(enableAll && enableVoice);
    }

    public void EnableMusic(bool value)
    {
        enableMusic = value;
        musicSource.mute = !(enableAll && enableMusic);
    }
    public void EnableSFX(bool value)
    {
        enableSFX = value;
        sfxSource.mute = !(enableAll && enableSFX);
    }
    public void EnableVoice(bool value)
    {
        enableVoice = value;
        voiceSource.mute = !(enableAll && enableVoice);
    }


    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
    public void VoiceVolume(float volume)
    {
        voiceSource.volume = volume;
    }
}
