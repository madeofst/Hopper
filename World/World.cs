using Godot;
using System;

namespace Hopper
{
    public class World : Node2D
    {
        private LevelFactory levelFactory { get; set; } = new LevelFactory();
        
        //References to existing nodes
        public Level CurrentLevel { get; set; }
        private Grid Grid { get; set; }
        private Player Player { get; set; }
        
        //World parameters
        public bool GameOver = false;
        public milliTimer Timer;

        //List of levels FIXME: needs to be updated
/*         public Level[] Levels { get; set; } = new Level[] 
        {
            new Level(1, 5, 7, 1, 5),
            new Level(2, 7, 8, 2, 5),
            new Level(3, 7, 10, 2),
            new Level(4, 7, 10, 3),
            new Level(5, 7, 10, 4) 
        }; */

        //Signals
        [Signal]
        public delegate void TimeUpdate(float timeRemaining);

        public override void _Ready()
        {
            CallDeferred("Init");
        }

        private void Init()
        {
            NewLevel(new Vector2(0, 0));            
            NewPlayer();           

            Grid.Connect(nameof(Grid.NextLevel), this, "IncrementLevel");
            Player.Connect(nameof(Player.GoalReached), this, "IncrementLevel");
            Timer = new milliTimer();
            Timer.Start(100);
        }

        private void NewLevel(Vector2 playerPosition)
        {
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = levelFactory.Generate(playerPositionX: (int)playerPosition.x, 
                                                 playerPositionY: (int)playerPosition.y);
            AddChild(CurrentLevel);
            CurrentLevel.Build();
            Grid = CurrentLevel.Grid;
        }

        private void NewPlayer()
        {
            Player = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instance();
            AddChild(Player);
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
            NewLevel(Player.GridPosition); //TODO: also need to pass hops remaining
            MoveChild(Player, 4);
            Player.CurrentLevel = CurrentLevel;
            Player.Grid = CurrentLevel.Grid;
        }
    }
}
