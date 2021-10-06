using Godot;
using System;

public class HopCounter : Counter
{
    public HopCounter(){}
    
    public HopCounter(Vector2 size) : base(size)
    {
    }

    public override void MakeConnections()
    {
        Player.Connect(nameof(Player.HopCompleted), this, "UpdateText");
    }

}