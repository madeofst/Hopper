using Godot;
using System;

public class ClickableArea : Area2D
{
    public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton && @event.IsPressed())
        {
            GD.Print("Clicked");
        }
    }


}
