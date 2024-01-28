using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadProgressBar : MonoBehaviour
{
    private Slider progressSlider;
    private void Awake()
    {
        progressSlider = GetComponent<Slider>();
    }
    private void Update()
    {
        progressSlider.value = Loader.GetLoadingProgress();
    }
}
