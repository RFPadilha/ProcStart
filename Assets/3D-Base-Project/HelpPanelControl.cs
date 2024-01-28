using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HelpPanelControl : MonoBehaviour
{
    public static HelpPanelControl instance;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public Timer timeToFade;
    public bool closed = true;
    Animator animator;

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
        timeToFade = new Timer(0, false);
        timeToFade.NullTimer();
        closed = true;
        animator = GetComponent<Animator>();
        animator.SetBool("IsClosed", closed);
    }

    private void Update()
    {
        if (timeToFade.isActive)
        {
            if(closed)
            {
                closed = false;
                animator.SetBool("IsClosed", closed);
            }
            timeToFade.UpdateTimer(Time.deltaTime);
        }
        if(timeToFade.completed && !closed)
        {
            CloseTooltip();
        }
    }
    public void ActivateTooltip(string _title, string _description, float _time)
    {
        title.text = _title;
        description.text = _description;
        timeToFade = new Timer(_time, true);
    }
    public void CloseTooltip()
    {
        closed = true;
        animator.SetBool("IsClosed", closed);
        timeToFade.NullTimer();
    }
}
