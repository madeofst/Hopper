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

    public override void _Ready()
    {
        Tween = GetNode<Tween>("Tween");
    }

    public void SpringIntoView(float delay)
    {
        Visible = true;
        RectScale = Vector2.Zero;
        Tween.InterpolateProperty(this, 
                                  "rect_scale", 
                                  new Vector2(0.7f, 0.7f), 
                                  Vector2.One, 
                                  0.3f, 
                                  Tween.TransitionType.Expo, 
                                  Tween.EaseType.In, 
                                  delay);
        Tween.Start();
    }
}

public enum CounterState
{
    Full,
    Empty
}