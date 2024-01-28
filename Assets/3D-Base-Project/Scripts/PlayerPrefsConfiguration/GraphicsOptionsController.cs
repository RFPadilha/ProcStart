using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsOptionsController : MonoBehaviour
{
    [Header("Dropdown")]
    public TMP_Dropdown screenModeDropdown;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;

    [Header("Toggles")]
    public Toggle vsyncToggle;

    private Resolution[] supportedResolutions;
    private List<Resolution> filteredResolutions;

    public float currentRefreshRate;
    public int currentResolutionIndex = 0;
    public int currentWidth;
    public int currentHeight;
    public bool fullscreen = true;

    private void Start()
    {
        SetResolutions();
    }
    public void SetResolutions()
    {
        List<string> resolutionOptions = new List<string>();
        supportedResolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        resolutionDropdown.ClearOptions();
        currentRefreshRate = Screen.currentResolution.refreshRateRatio.numerator;

        for (int i = 0; i < supportedResolutions.Length; i++)
        {
            if (supportedResolutions[i].refreshRateRatio.numerator == currentRefreshRate)
            {
                filteredResolutions.Add(supportedResolutions[i]);
            }
        }
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string resolutionOption = $"{filteredResolutions[i].width}x{filteredResolutions[i].height}";
                //$"{filteredResolutions[i].refreshRateRatio.numerator}Hz";
            resolutionOptions.Add(resolutionOption);
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(resolutionOptions);
        UpdateResolution(currentResolutionIndex);
        UpdateScreenMode(fullscreen ? 0 : 1);
    }
    public void UpdateScreenMode(int screenModeIndex)
    {
        switch (screenModeIndex)
        {
            case 0:
                fullscreen = true;
                Screen.SetResolution(currentWidth, currentHeight, fullscreen);
                break;
            case 1:
                fullscreen = false;
                Screen.SetResolution(currentWidth, currentHeight, fullscreen);
                break;
            default:
                break;
        }
        screenModeDropdown.value = screenModeIndex;
    }
    public void UpdateResolution(int resolutionIndex)
    {
        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, fullscreen);
        currentHeight = resolution.height;
        currentWidth = resolution.width;
        resolutionDropdown.value = resolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    public void UpdateQuality()
    {
        int index = qualityDropdown.value;
        QualitySettings.SetQualityLevel(index, true);
    }
    public void VSyncToggle()
    {
        int value;
        if (vsyncToggle.isOn) value = 1;
        else value = 0;
        QualitySettings.vSyncCount = value;
    }
}
