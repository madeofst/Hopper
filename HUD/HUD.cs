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
        Restart = GetNode<Button>("Buttons/Restart");
        Quit = GetNode<Button>("Buttons/Quit");
    }

    public void ShowPopUp(string text)
    {
        PopUp.BbcodeText = $"[center]{text}[/center]";
        AnimationPlayer.Play("ShowPopUpText");
    }
}
