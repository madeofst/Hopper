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

        private HopCounter HopCounter { get; set; }
        private HopCounterBar HopCounterBar { get; set; }
        private ScoreCounter ScoreCounter { get; set; }
        private TimeCounter TimeCounter { get; set; }
        private Stopwatch Stopwatch { get; set; }

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
            HopCounter = GetNode<HopCounter>("HUD/HBoxContainer/VBoxContainer2/HopCounter");
            HopCounterBar = GetNode<HopCounterBar>("HUD/HBoxContainer/VBoxContainer2/HopCounterBar");
            ScoreCounter = GetNode<ScoreCounter>("HUD/HBoxContainer/MarginContainer/VBoxContainer/ScoreCounter");
            TimeCounter = GetNode<TimeCounter>("HUD/HBoxContainer/MarginContainer/VBoxContainer/TimeCounter");
            Stopwatch = GetNode<Stopwatch>("HUD/HBoxContainer/MarginContainer/VBoxContainer/Stopwatch");

            Connect(nameof(World.TimeUpdate), TimeCounter, "UpdateText");
            Connect(nameof(World.TimeUpdate), Stopwatch, "UpdateStopwatch");


            NewPlayer();           
            Player.Connect(nameof(Player.GoalReached), this, "IncrementLevel");
            Player.Connect(nameof(Player.HopCompleted), HopCounter, "UpdateText");
            Player.Connect(nameof(Player.HopCompleted), HopCounterBar, "UpdateBar");
            Player.Connect(nameof(Player.ScoreUpdated), ScoreCounter, "UpdateText");

            NewLevel(new Vector2(0, 0));            
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
            MoveChild(Player, 4);
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
            NewLevel(Player.GridPosition);
            Player.CurrentLevel = CurrentLevel;
            Player.Grid = CurrentLevel.Grid;
        }
    }
}
