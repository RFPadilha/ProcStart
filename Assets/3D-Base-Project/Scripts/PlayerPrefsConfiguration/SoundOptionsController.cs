using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundOptionsController : MonoBehaviour
{
    [Header("Mute/Unmute Toggles")]
    public Toggle masterSoundsEnabled;
    public Toggle musicEnabled;
    public Toggle sfxEnabled;
    public Toggle voiceEnabled;

    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider voiceSlider;

    private void Start()
    {
        AudioManager.instance.EnableAllSounds(masterSoundsEnabled.isOn);
        AudioManager.instance.EnableMusic(musicEnabled.isOn);
        AudioManager.instance.EnableSFX(sfxEnabled.isOn);
        AudioManager.instance.EnableVoice(voiceEnabled.isOn);
    }
    public void MasterVolume()
    {
        AudioManager.instance.MusicVolume(musicSlider.value * masterSlider.value);
        AudioManager.instance.SFXVolume(sfxSlider.value * masterSlider.value);
        AudioManager.instance.VoiceVolume(voiceSlider.value * masterSlider.value);
    }
    public void ToggleMaster()
    {
        bool enableAllSounds = masterSoundsEnabled.isOn;
        AudioManager.instance.EnableAllSounds(enableAllSounds);
    }

    public void ToggleMusic()
    {
        bool enableMusic = musicEnabled.isOn;
        AudioManager.instance.EnableMusic(enableMusic);
    }
    public void MusicVolume()
    {
        AudioManager.instance.MusicVolume(musicSlider.value * masterSlider.value);
    }


    public void ToggleSFX()
    {
        bool enableSFX = sfxEnabled.isOn;//inversed because "isOn" changes when function is called
        AudioManager.instance.EnableSFX(enableSFX);
    }
    public void SFXVolume()
    {
        AudioManager.instance.SFXVolume(sfxSlider.value * masterSlider.value);
    }


    public void ToggleVoice()
    {
        bool enableVoice = voiceEnabled.isOn;
        AudioManager.instance.EnableVoice(enableVoice);
    }
    public void VoiceVolume()
    {
        AudioManager.instance.VoiceVolume(voiceSlider.value * masterSlider.value);
    }
}
