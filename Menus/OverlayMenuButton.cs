using Godot;
using System;

[Tool]
public class OverlayMenuButton : MenuButton
{
    [Export]
    public string ButtonLabelText;

    public override void _Process(float delta)
    {
        UpdateLabel(ButtonLabelText);
    } 

    public void ShowMenuButton()
    {
        Show();
        FocusMode = FocusModeEnum.All;
    }

    public void OnFocus()
    {
        HighlightLabelText();
    }

    public void LostFocus()
    {
        DehighlightLabelText();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept"))
        {
            AcceptEvent();
        }
    }
}
