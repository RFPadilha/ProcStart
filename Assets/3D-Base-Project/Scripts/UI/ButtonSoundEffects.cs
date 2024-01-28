using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class ButtonSoundEffects : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    AudioSource source;
    void Awake()
    {
        source = GetComponent<AudioSource>();
    }
    //Interface functions that must be implemented------------------------------------------------------------------------
    public void OnPointerEnter(PointerEventData eventData)
    {

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        source.Play();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        source.Stop();
    }

}
