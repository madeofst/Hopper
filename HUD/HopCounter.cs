using Godot;
using System;

public class HopCounter : Control
{
    [Export]
    public int MaxHops = 3;
    [Export]
    public int RemainingHops = 3;

    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
