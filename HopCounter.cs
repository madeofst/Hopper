using Godot;
using System;

public class HopCounter : RichTextLabel
{
    public void UpdateText(string text)
    {
        Text = text;
    }
}