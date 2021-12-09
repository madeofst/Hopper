using Godot;
using System;

public class SingleHopCount : Control
{
    private CounterState state;
    [Export]
    public CounterState State 
    { 
        get => state; 
        set
        {
            if (value == CounterState.Empty)
            {
                GetNode<Sprite>("Sprite").Frame = 1;
            }
            else
            {
                GetNode<Sprite>("Sprite").Frame = 0;
            }
            state = value;
        } 
    }

    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

public enum CounterState
{
    Full,
    Empty
}