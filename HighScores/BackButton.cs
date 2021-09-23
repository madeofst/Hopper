using Godot;
using System;

public class BackButton : Button
{
    public override void _Ready()
    {
        GrabFocus();        
    }
}
