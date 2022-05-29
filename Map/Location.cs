using Godot;
using System;
using System.Collections.Generic;

public class Location : Node2D
{
    [Export]
    public int ID;
    
    [Export]
    public string[] Levels;

    [Export]
    public Texture Texture;
    private Sprite Sprite;
    private AnimationPlayer AnimationPlayer;

    [Export]
    public bool Active;

    [Export]
    public bool NewlyActivated;

    [Export]
    public bool Complete;

    [Export]
    public string[] LocationsToUnlock;

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>("Sprite");
        AnimationPlayer = Sprite.GetNode<AnimationPlayer>("AnimationPlayer");
        Sprite.Texture = Texture;
    }

    public override void _Process(float delta)
    {
        if (NewlyActivated) 
            AnimationPlayer.Play("New");
        else if (Complete)
            AnimationPlayer.Play("Complete");
        else if (Active) 
            AnimationPlayer.Play("Active");
        else
            AnimationPlayer.Play("Inactive");
    }

}
