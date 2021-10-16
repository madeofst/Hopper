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
        public int iLevel { get; set; } = 0;
        public string[] Levels { get; set; } = new string[] 
        {
            //"RedRing",
            //"BlueLine"
        };
        public bool Temp { get; set; } = false;

        //Signals
        [Signal]
        public delegate void TimeUpdate(float timeRemaining);

        public override void _Ready()
        {
            //CallDeferred("Init");
        }

        public void Init(bool temp = false, string levelName = "")
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

            if (temp)
            {
                Temp = true;
                NewLevel(levelName);
            }
            else
            {
                if (Levels.Length <= 0 || iLevel > Levels.Length - 1)
                {
                    NewLevel(Player.GridPosition);
                    Timer = new milliTimer();
                    Timer.Start(100);
                }
                else
                {
                    NewLevel(Levels[iLevel]);
                }
            }
        }

        private void NewLevel(Vector2 playerPosition)
        {
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = levelFactory.Generate(playerPositionX: (int)playerPosition.x, 
                                                 playerPositionY: (int)playerPosition.y);
            BuildLevel();
        }

        private void NewLevel(string levelName)
        {
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = levelFactory.Load(levelName, true);
            BuildLevel();

        }

        private void BuildLevel()
        {
            AddChild(CurrentLevel);
            CurrentLevel.Build();
            Grid = CurrentLevel.Grid;
            MoveChild(Player, 4);
            Player.Init();
            if (Timer != null) Timer.Reset();
            //HopCounter.UpdateText(CurrentLevel.StartingHops);
            //HopCounterBar.UpdateBar(CurrentLevel.StartingHops);
        }

        private void NewPlayer()
        {
            Player = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instance();
            AddChild(Player);
        }

        public override void _Process(float delta)
        {
            UpdateTimeRemaining();

            if (Timer != null)
            {
                if (Timer.Finished()) GameOver = true;
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
            if (Timer != null) EmitSignal(nameof(TimeUpdate), Timer.Remaining());
        }

        public void IncrementLevel()
        {
            iLevel++;
            if (Temp)
            {
                GD.Print("Level complete.");
                QueueFree();
            }
            else
            {
                if (iLevel >= Levels.Length || Levels[iLevel] == null)
                {
                    NewLevel(Player.GridPosition);
                    
                }
                else
                {
                    NewLevel(Levels[iLevel]);
                }
                //Player.CurrentLevel = CurrentLevel;
                //Player.Grid = CurrentLevel.Grid;
            }
        }
    }
}
