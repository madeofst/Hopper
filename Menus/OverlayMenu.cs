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

public class OverlayMenu : MarginContainer
{
    public OverlayMenuButton RestartButton { get; set; }
    public OverlayMenuButton MapButton { get; set; }
    public OverlayMenuButton QuitButton { get; set; }
    public OverlayMenuButton LevelSelectOverlayButton { get; set; }
    public OverlayMenuButton ShowMenuButton { get; set; }

    private ColorRect ColourRect { get; set; }

    private OverlayMenuMode CurrentMode;

    public override void _Ready()
    {
        RestartButton = GetNode<OverlayMenuButton>("Buttons/Restart");
        MapButton = GetNode<OverlayMenuButton>("Buttons/Map");
        QuitButton = GetNode<OverlayMenuButton>("Buttons/Quit");
        LevelSelectOverlayButton = GetNode<OverlayMenuButton>("Buttons/LevelSelect");
        ShowMenuButton = GetNode<OverlayMenuButton>("Buttons/ShowMenu");
        ColourRect = GetNode<ColorRect>("ColorRect");
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
            RestartButton.Hide();
            MapButton.Hide();
            LevelSelectOverlayButton.Hide();
            QuitButton.Hide();

            ColourRect.Hide();

            Show();
        }
        else if (Mode == OverlayMenuMode.Map)
        {
            ShowMenuButton.Hide();
            RestartButton.Hide();
            MapButton.Hide();
            LevelSelectOverlayButton.Hide();
            QuitButton.ShowMenuButton();

            ColourRect.Hide();

            Show();
        }
        else if (Mode == OverlayMenuMode.LevelTitle)
        {
            ShowMenuButton.Hide();
            RestartButton.Hide();
            MapButton.ShowMenuButton();
            LevelSelectOverlayButton.Hide();
            QuitButton.ShowMenuButton();

            ColourRect.Hide();

            Show();
        }
        else if (Mode == OverlayMenuMode.Stage)
        {
            ShowMenuButton.Hide();
            RestartButton.ShowMenuButton();
            MapButton.ShowMenuButton();
            LevelSelectOverlayButton.ShowMenuButton();
            QuitButton.ShowMenuButton();

            ColourRect.Show();
            Show();
        }

        CurrentMode = Mode; //TODO: Check this is OK and what happens if no mode sent.
    }

    public override void _Input(InputEvent @event)
    {
        if (CurrentMode == OverlayMenuMode.Minimised && @event.IsActionPressed("ui_cancel"))
        {   
            GD.Print("Escape pressed.");
            ChangeMode(OverlayMenuMode.Stage);
            RestartButton.GrabFocus();
            AcceptEvent();
        }
    }

    public override void _UnhandledKeyInput(InputEventKey @event)
    {
        if (CurrentMode == OverlayMenuMode.Stage)
        {
            AcceptEvent();
        }
    }
}
