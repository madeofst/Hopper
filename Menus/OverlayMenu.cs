using Godot;
using System;

public enum OverlayMenuMode
{
    Menu,
    Map,
    LevelTitle,
    Stage
}

public class OverlayMenu : MarginContainer
{
    public RestartButton RestartButton { get; set; }
    public MapButton MapButton { get; set; }
    public QuitButton QuitButton { get; set; }

    public override void _Ready()
    {
        RestartButton = GetNode<RestartButton>("Buttons/Restart");
        MapButton = GetNode<MapButton>("Buttons/Map");
        QuitButton = GetNode<QuitButton>("Buttons/Quit");
    }

    public void ChangeMode(OverlayMenuMode Mode)
    {
        HideAllText();

        if (Mode == OverlayMenuMode.Menu)
        {
            Hide();
        }
        else if (Mode == OverlayMenuMode.Map)
        {
            RestartButton.Hide();
            MapButton.Hide();
            QuitButton.Show();
            Show();
        }
        else if (Mode == OverlayMenuMode.LevelTitle)
        {
            RestartButton.Hide();
            MapButton.Show();
            QuitButton.Show();
            Show();
        }
        else if (Mode == OverlayMenuMode.Stage)
        {
            RestartButton.Show();
            MapButton.Show();
            QuitButton.Show();
            Show();
        }
    }

    public void HideAllText()
    {
        RestartButton.HideLabel();
        MapButton.HideLabel();
        QuitButton.HideLabel();
    } 
}
