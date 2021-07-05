using Godot;
using System;

public class World : Node2D
{
    public bool GameOver = false;
    private Sprite GameOverLabel { get; set; }
    private Grid Grid { get; set; }
    private Player Player { get; set; }
    private Counter TimeCounter { get; set; }
    public milliTimer Timer;
    

    public override void _Ready()
    {
        Grid = new Grid(7, GetViewportRect().Size);
        AddChild(Grid);
        Player = new Player();
        AddChild(Player);
        CallDeferred("GetChildReferences");
        Timer = new milliTimer();
        Timer.Start(10);
    }


    public override void _Process(float delta)
    {
        UpdateTimeRemaining();

        if (Timer.Finished())
        {
            GameOver = true;
        }

        if (GameOver)
        {
            MoveChild(GameOverLabel, GetChildCount());
            GameOverLabel.Visible = true;
        }
    }

    private void UpdateTimeRemaining()
    {
        GD.Print(Timer.Remaining());
        TimeCounter.UpdateText(Timer.Remaining());
    }

    public void GetChildReferences()
    {
        GameOverLabel = GetNode<Sprite>("GameOverLabel");
        TimeCounter = GetNode<Counter>("TimeCounter");
    }

}
