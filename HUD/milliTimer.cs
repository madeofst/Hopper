using Godot;
using System;

public class milliTimer
{
    private ulong TimerLength;
    private float TimerLengthSecs
    {
        get { return (float)TimerLength / 1000; }
    }
    private ulong TimerEnd;
    public float TimeAdjustment
    {
        set
        {
            TimerLength += (ulong)(value * 1000);
        }
    }

    public void Start(float lengthSecs)
    {
        TimerLength = (ulong)(lengthSecs * 1000);
        TimerEnd = OS.GetSystemTimeMsecs() + TimerLength;
    }

    public bool Finished()
    {
        if (OS.GetSystemTimeMsecs() >= TimerEnd && TimerEnd != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float Remaining()
    {
        return (float)(((TimerEnd - OS.GetSystemTimeMsecs())/1000) + 1);
    }

    public void Reset()
    {
        Start(TimerLengthSecs);
    }

    public void Add(ulong timeSecs)
    {
        TimerEnd += timeSecs * 1000;
    }
}