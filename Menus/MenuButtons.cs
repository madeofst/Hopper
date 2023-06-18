using Godot;
using System;

public class MenuButton : Button
{
    private RichTextLabel Label;

    public override void _Ready()
    {
        Label = GetNode<RichTextLabel>("Label");
    }

    public void ShowLabel()
    {
        Label.Show();
    }

    public void HideLabel()
    {
        Label.Hide();
    }
}
