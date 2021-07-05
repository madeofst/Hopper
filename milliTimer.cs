using Godot;
using System;

public class milliTimer
{
    private ulong TimerLength;
    private ulong TimerEnd;

    public void Start(ulong lengthSecs = 10)
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
        Start();
    }

    public void Add(ulong timeSecs)
    {
        TimerEnd += timeSecs * 1000;
    }
}