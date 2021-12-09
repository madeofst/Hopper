using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HopCounter : Control
{

    private int maxHops = 3;
    private int remainingHops = 3;

    [Export]
    public int MaxHops 
    { 
        get => maxHops; 
        set
        {
            maxHops = value;
            int i = 0;
            foreach (SingleHopCount h in HopCounters)
            {
                if (i > maxHops - 1)
                    h.Visible = false;
                else
                    h.Visible = true;
                i++;
            }
        } 
    }

    [Export]
    public int RemainingHops 
    { 
        get => remainingHops; 
        set 
        {
            remainingHops = value;
            int i = 0;
            foreach (SingleHopCount h in HopCounters)
            {
                if (i > remainingHops - 1)
                    h.State = CounterState.Empty;
                else
                    h.State = CounterState.Full;
                i++;
            }
        }
    }

    private List<SingleHopCount> HopCounters = new List<SingleHopCount>();

    public override void _Ready()
    {
        var arr = GetChildren();
        foreach (var h in arr)
        {
            HopCounters.Add((SingleHopCount)h);
        }
    }

    internal void UpdateHop(int hopsRemaining)
    {
        RemainingHops = hopsRemaining;
    }

    internal void SetMaxHops(int maxHops)
    {
        MaxHops = maxHops;
    }
}
