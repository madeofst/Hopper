using Godot;
using System;

public class HUD : Control
{
    private AnimationPlayer AnimationPlayer;
    private RichTextLabel PopUp;

    public Button Restart;
    public Button Quit;

    public override void _Ready()
    {
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");    
        PopUp = GetNode<RichTextLabel>("PopUpText/RichTextLabel");
    }

    public void ShowPopUp(string text)
    {
        PopUp.BbcodeText = $"[center]{text}[/center]";
        AnimationPlayer.Play("ShowPopUpText");
    }
}
