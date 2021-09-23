using Godot;
using System;

public class World : Node2D
{
    //References to existing nodes
    private Grid Grid { get; set; }
    private Player Player { get; set; }
    private Counter TimeCounter { get; set; }
    
    //World parameters
    public bool GameOver = false;
    public milliTimer Timer;
    public Level[] Levels { get; set; } = new Level[] {new Level(1, 5, 7, 1, 5),
                                                       new Level(2, 7, 8, 2, 5),
                                                       new Level(3, 7, 10, 2),
                                                       new Level(4, 7, 10, 3),
                                                       new Level(5, 7, 10, 4)};

    public override void _Ready()
    {
        NewGrid(Levels[0]);
        NewPlayer();
        CallDeferred("GetChildReferences");

        Grid.Connect(nameof(Grid.NextLevel), this, "IncrementLevel");

        Timer = new milliTimer();
        Timer.Start(10);
    }

    private void NewPlayer()
    {
        Player = new Player();
        AddChild(Player);
    }

    private void NewGrid(Level level)
    {
        Grid = new Grid(level, GetViewportRect().Size);
        AddChild(Grid);
    }

    public void GetChildReferences()
    {
        TimeCounter = GetNode<Counter>("TimeCounter");
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
            GameOver GameOver = (GameOver)GD.Load<PackedScene>("res://GameOver/GameOver.tscn").Instance();
            GetTree().Root.AddChild(GameOver);
            GameOver.Score = Player.Score;
            GameOver.ScoreLabel.Text = GameOver.Score.ToString();
            QueueFree();
        }
    }

    private void UpdateTimeRemaining()
    {
        TimeCounter.UpdateText(Timer.Remaining());
    }

    public void IncrementLevel()
    {
        int nextLevelElementID = Grid.CurrentLevel.ID - 1 + 1;
        if (nextLevelElementID <= Levels.Length - 1)
        {
            Grid.CurrentLevel = Levels[nextLevelElementID];
        }
    }

}
