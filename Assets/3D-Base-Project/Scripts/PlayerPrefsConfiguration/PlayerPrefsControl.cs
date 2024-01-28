using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsControl : MonoBehaviour
{
    public static PlayerPrefsControl instance;
    public GameObject optionsMenuRoot;
    //Component references
    SoundOptionsController soundOptionsController;
    GraphicsOptionsController graphicsOptionsController;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(this);
    }
    private void Start()
    {
        LoadPrefs();
    }
    public void LoadPrefs()
    {
        soundOptionsController = optionsMenuRoot.GetComponent<SoundOptionsController>();
        graphicsOptionsController = optionsMenuRoot.GetComponent<GraphicsOptionsController>();

        //Graphics preferences, 2nd param is default value
        graphicsOptionsController.currentRefreshRate = PlayerPrefs.GetFloat("currentRefreshRate", 60f);
        graphicsOptionsController.currentResolutionIndex = PlayerPrefs.GetInt("currentResolutionIndex", 0);
        graphicsOptionsController.currentWidth = PlayerPrefs.GetInt("currentWidth", 640);
        graphicsOptionsController.currentHeight = PlayerPrefs.GetInt("currentHeight", 480);

        graphicsOptionsController.qualityDropdown.value = PlayerPrefs.GetInt("qualityDropdownIndex", 0);
        graphicsOptionsController.screenModeDropdown.value = PlayerPrefs.GetInt("fullscreenDropdownIndex", 0);
        //resolution dropdown is set dinamically, no need to set its value because it detects the resolution

        graphicsOptionsController.fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1 ?  true : false;
        graphicsOptionsController.vsyncToggle.isOn = PlayerPrefs.GetInt("vsyncToggle", 1) == 1 ? true : false;

        //SoundPreferences
        soundOptionsController.masterSlider.value = PlayerPrefs.GetFloat("masterVolume", 1);
        soundOptionsController.musicSlider.value = PlayerPrefs.GetFloat("musicVolume", 1);
        soundOptionsController.sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume", 1);
        soundOptionsController.voiceSlider.value = PlayerPrefs.GetFloat("voiceVolume", 1);

        //1 means the toggle has a checkmark, which means the sounds are enabled, therefore "false" mute
        soundOptionsController.masterSoundsEnabled.isOn = PlayerPrefs.GetInt("masterSoundsEnabled", 0) == 1 ? false : true;
        soundOptionsController.musicEnabled.isOn = PlayerPrefs.GetInt("musicEnabled", 0) == 1 ? false : true;
        soundOptionsController.sfxEnabled.isOn = PlayerPrefs.GetInt("sfxEnabled", 0) == 1 ? false : true;
        soundOptionsController.voiceEnabled.isOn = PlayerPrefs.GetInt("voiceEnabled", 0) == 1 ? false : true;
    }


    public void SaveGraphicChanges()
    {
        //dropdown values
        PlayerPrefs.SetFloat("currentRefreshRate", graphicsOptionsController.currentRefreshRate);
        PlayerPrefs.SetInt("currentResolutionIndex", graphicsOptionsController.currentResolutionIndex);
        PlayerPrefs.SetInt("currentWidth", graphicsOptionsController.currentWidth);
        PlayerPrefs.SetInt("currentHeight", graphicsOptionsController.currentHeight);
        
        //update dropdown visualization
        PlayerPrefs.SetInt("qualityDropdownIndex", graphicsOptionsController.qualityDropdown.value);
        PlayerPrefs.SetInt("fullscreenDropdownIndex", graphicsOptionsController.screenModeDropdown.value);
        PlayerPrefs.SetInt("resolutionDropdownIndex", graphicsOptionsController.resolutionDropdown.value);

        //toggles
        PlayerPrefs.SetInt("fullscreen", graphicsOptionsController.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("vsyncToggle", graphicsOptionsController.vsyncToggle.isOn ? 1 : 0);
    }
    public void SaveSoundChanges()
    {
        //slider values
        PlayerPrefs.SetFloat("masterVolume", soundOptionsController.masterSlider.value);
        PlayerPrefs.SetFloat("musicVolume", soundOptionsController.musicSlider.value);
        PlayerPrefs.SetFloat("sfxVolume", soundOptionsController.sfxSlider.value);
        PlayerPrefs.SetFloat("voiceVolume", soundOptionsController.voiceSlider.value);

        //toggles
        PlayerPrefs.SetInt("masterSoundsEnabled", soundOptionsController.masterSoundsEnabled.isOn ? 0 : 1);
        PlayerPrefs.SetInt("musicEnabled", soundOptionsController.musicEnabled.isOn ? 0 : 1);
        PlayerPrefs.SetInt("sfxEnabled", soundOptionsController.sfxEnabled.isOn ? 0 : 1);
        PlayerPrefs.SetInt("voiceEnabled", soundOptionsController.voiceEnabled.isOn ? 0 : 1);
    }
}
