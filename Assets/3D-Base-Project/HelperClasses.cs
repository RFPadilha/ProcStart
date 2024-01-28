using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public float time;
    public float currentTime;
    public bool isActive;
    public bool completed;

    public Timer(float time, bool timerActive)
    {
        this.time = time;
        this.currentTime = time;
        this.isActive = timerActive;
        this.completed = false;
    }
    public void UpdateTimer(float step)
    {
        if (isActive)
        {
            currentTime -= step;
        }
        else return;
        if (currentTime <= 0)
        {
            StopTimer();
        }
    }
    public void RestartTimer()
    {
        currentTime = time;
        completed = false;
        isActive = true;
    }
    public void StopTimer()
    {
        currentTime = 0;
        isActive = false;
        completed = true;
    }
    public void NullTimer()
    {
        time = 0;
        currentTime = 0;
        isActive = false;
        completed = false;
    }
}

