using Godot;
using System;

public enum OverlayMenuMode
{
    Minimised,
    Menu,
    Map,
    LevelTitle,
    Stage
}

public class OverlayMenu : ColorRect
{
    public OverlayMenuButton BackButton { get; set; }
    public OverlayMenuButton RestartButton { get; set; }
    public OverlayMenuButton MapButton { get; set; }
    public OverlayMenuButton QuitButton { get; set; }
    public OverlayMenuButton LevelSelectOverlayButton { get; set; }
    public OverlayMenuButton ShowMenuButton { get; set; }
    public OverlayMenuButton UndoButton { get; set; }


    public OverlayMenuMode CurrentMode;

    public override void _Ready()
    {
        BackButton = GetNode<OverlayMenuButton>("MarginContainer/Buttons/Back");
        RestartButton = GetNode<OverlayMenuButton>("MarginContainer/Buttons/Restart");
        MapButton = GetNode<OverlayMenuButton>("MarginContainer/Buttons/Map");
        QuitButton = GetNode<OverlayMenuButton>("MarginContainer/Buttons/Quit");
        LevelSelectOverlayButton = GetNode<OverlayMenuButton>("MarginContainer/Buttons/LevelSelect");
        ShowMenuButton = GetNode<OverlayMenuButton>("MarginContainer/Buttons/ShowMenu");
        UndoButton = GetNode<OverlayMenuButton>("MarginContainer/Buttons/Undo");
        ChangeMode(OverlayMenuMode.Minimised);
    }

    public void ChangeMode(string modename)
    {
        OverlayMenuMode mode;
        OverlayMenuMode.TryParse(modename, out mode);
        ChangeMode(mode);
    }

    public void ChangeMode(OverlayMenuMode Mode)
    {
        if (Mode == OverlayMenuMode.Menu)
        {
            Hide();
        }
        else if (Mode == OverlayMenuMode.Minimised)
        {
            ShowMenuButton.ShowMenuButton();
            BackButton.Hide();
            RestartButton.ShowMenuButton();
            UndoButton.ShowMenuButton();
            MapButton.Hide();
            LevelSelectOverlayButton.Hide();
            QuitButton.Hide();

            //ColourRect.Hide();

            Show();
        }
        else if (Mode == OverlayMenuMode.Map)
        {
            ShowMenuButton.Hide();
            BackButton.Hide();
            RestartButton.Hide();
            UndoButton.Hide();
            MapButton.Hide();
            LevelSelectOverlayButton.Hide();
            QuitButton.ShowMenuButton();

            //ColourRect.Hide();

            Show();
        }
        else if (Mode == OverlayMenuMode.LevelTitle)
        {
            ShowMenuButton.Hide();
            BackButton.Hide();
            RestartButton.Hide();
            UndoButton.Hide();
            MapButton.ShowMenuButton();
            LevelSelectOverlayButton.Hide();
            QuitButton.Hide();

            //ColourRect.Hide();
            Show();
        }
        else if (Mode == OverlayMenuMode.Stage)
        {
            ShowMenuButton.Hide();
            BackButton.ShowMenuButton();
            RestartButton.Hide();
            UndoButton.Hide();
            MapButton.ShowMenuButton();
            LevelSelectOverlayButton.ShowMenuButton();
            QuitButton.ShowMenuButton();

            //ColourRect.Show();
            Show();

            BackButton.GrabFocus();
        }

        CurrentMode = Mode; //TODO: Check this is OK and what happens if no mode sent.
    }

    public override void _Input(InputEvent @event)
    {
        if (CurrentMode == OverlayMenuMode.Minimised && @event.IsActionPressed("ui_cancel"))
        {   
            GD.Print("Escape pressed.");
            ChangeMode(OverlayMenuMode.Stage);
            AcceptEvent();
        }
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if (CurrentMode == OverlayMenuMode.Stage)
        {
            AcceptEvent();  //Prevents propagating to player etc.
        }
    }
}
