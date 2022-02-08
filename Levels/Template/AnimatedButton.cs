using Godot;
using System;

public class AnimatedButton : Button
{
    public Tween Tween;
    public override void _Ready()
    {
        Tween = GetNode<Tween>("Tween");
    }
}