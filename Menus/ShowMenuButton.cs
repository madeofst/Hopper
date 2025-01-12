using Godot;
using System;

public class ShowMenuButton : OverlayMenuButton
{
    public override void MenuButtonPressed()
    {
        GD.Print("Menu button press overridden");
    }

}
