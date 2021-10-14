using Godot;
using System;

namespace Hopper
{
    public class World : Node2D
    {
        private LevelFactory levelFactory { get; set; } = new LevelFactory();
        
        //References to existing nodes
        private Level CurrentLevel { get; set; }
        private Grid Grid { get; set; }
        private Player Player { get; set; }
        
        //World parameters
        public bool GameOver = false;
        public milliTimer Timer;

        //List of levels FIXME: needs to be updated
        public Level[] Levels { get; set; } = new Level[] 
        {
            new Level(1, 5, 7, 1, 5),
            new Level(2, 7, 8, 2, 5),
            new Level(3, 7, 10, 2),
            new Level(4, 7, 10, 3),
            new Level(5, 7, 10, 4) 
        };

        //Signals
        [Signal]
        public delegate void TimeUpdate(float timeRemaining);

        public override void _Ready()
        {
            NewGrid(Levels[0]);
            NewPlayer();

            Grid.Connect(nameof(Grid.NextLevel), this, "IncrementLevel");

            Timer = new milliTimer();
            Timer.Start(100);
        }

        private void NewPlayer()
        {
            Player = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instance();
            AddChild(Player);
        }

        private void NewGrid(Level level)
        {
            Grid = new Grid(level);
            AddChild(Grid);
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
            EmitSignal(nameof(TimeUpdate), Timer.Remaining());
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
}
