using Godot;
using System;

public class UserNameEntry : LineEdit
{
    public override void _Ready()
    {
        GrabFocus();   
    }
}
