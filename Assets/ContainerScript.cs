using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContainerScript : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        //empty
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //move player towards it
        //when a certain distance is reached
        //spawn something
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //highlight object
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //unshine object
    }

    public void OnPointerUp(PointerEventData eventData)
    {
       //empty
    }
}
