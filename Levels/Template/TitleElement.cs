using Godot;
using System;

public class TitleElement : HBoxContainer
{
    public Tween Tween;
    public override void _Ready()
    {
        Tween = GetNode<Tween>("Tween");
    }
}
