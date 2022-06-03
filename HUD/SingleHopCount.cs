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
    public Tween Tween; 
    public Sprite Sprite;

    public override void _Ready()
    {
        Tween = GetNode<Tween>("Tween");
        Sprite = GetNode<Sprite>("Sprite");
    }

    public void SpringIntoView(float delay)
    {
        Visible = true;
        Sprite.Scale = Vector2.Zero;
        Tween.InterpolateProperty(Sprite, 
                                  "scale", 
                                  Vector2.Zero, 
                                  Vector2.One, 
                                  0.5f, 
                                  Tween.TransitionType.Elastic, 
                                  Tween.EaseType.Out, 
                                  delay);
        Tween.Start();
    }
}

public enum CounterState
{
    Full,
    Empty
}