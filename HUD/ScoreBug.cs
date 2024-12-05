using Godot;
using System;

public class ScoreBug : TextureRect
{
    public TextureRect BugTexture;

    public override void _Ready()
    {
        BugTexture = GetNode<TextureRect>("BugTexture");
    }


}
