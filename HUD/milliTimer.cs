using Godot;
using System;

public class milliTimer
{
    private ulong TimerLength;
    private ulong TimerLengthSecs
    {
        get { return TimerLength / 1000; }
    }
    private ulong TimerEnd;
    public ulong TimeAdjustment
    {
        set
        {
            TimerLength += value * 1000;
        }
    }

    public void Start(ulong lengthSecs)
    {
        TimerLength = lengthSecs * 1000;
        TimerEnd = OS.GetSystemTimeMsecs() + TimerLength;
    }

    public bool Finished()
    {
        if (OS.GetSystemTimeMsecs() >= TimerEnd)
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