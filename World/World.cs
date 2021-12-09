using Godot;
using System;

namespace Hopper
{
    public class World : Node2D
    {
        private ResourceRepository Resources { get; set; }
        private LevelFactory levelFactory { get; set; }
        
        //References to existing nodes
        public Level CurrentLevel { get; set; }
        private Grid Grid { get; set; }
        private Player Player { get; set; }
    
        private HopCounter HopCounterBar { get; set; }
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
            //Basic
            "StartingOut",
            "ArtAndSoul",
            //Introducing jumping
            "MovingOn",
            "MovingOn2",
            "DoubleJump",
            //Introducing water
            "SideWind"
        };

        public bool Temp { get; set; } = false;

        //Signals
        [Signal]
        public delegate void TimeUpdate(float timeRemaining);

        public override void _Ready()
        {
            Resources = new ResourceRepository();
            levelFactory = new LevelFactory(Resources);
        }

        public void Init(bool temp = false, string levelName = "")
        {
            HopCounterBar = GetNode<HopCounter>("HUD/HopCounter");
            ScoreCounter = GetNode<ScoreCounter>("HUD/TimeAndScoreSimple/VBoxContainer/ScoreCounter");
            TimeCounter = GetNode<TimeCounter>("HUD/TimeAndScoreSimple/VBoxContainer/TimeCounter");
            Stopwatch = GetNode<Stopwatch>("HUD/TimeAndScoreSimple/VBoxContainer/Stopwatch");

            Connect(nameof(World.TimeUpdate), TimeCounter, "UpdateText");
            Connect(nameof(World.TimeUpdate), Stopwatch, "UpdateStopwatch");
            
            NewPlayer();           
            Player.Connect(nameof(Player.GoalReached), this, nameof(IncrementLevel));
            Player.Connect(nameof(Player.HopCompleted), HopCounterBar, nameof(HopCounterBar.UpdateHop));
            Player.Connect(nameof(Player.ScoreUpdated), ScoreCounter, nameof(ScoreCounter.UpdateText));
            Player.Connect(nameof(Player.ScoreUpdated), this, nameof(UpdateGoalState));
            Player.Connect(nameof(Player.TileChanged), this, nameof(UpdateTile));

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
            CurrentLevel.Connect(nameof(Level.LevelBuilt), HopCounterBar, nameof(HopCounterBar.SetMaxHops));
            CurrentLevel.Build(Resources);
            Grid = CurrentLevel.Grid;
            MoveChild(Player, 4);
            Player.Init();
            if (Timer is null)
            {
                Timer = new milliTimer();
                Timer.Start(100);
            }
            else
            {
                Timer.Reset();
            }
        }

        private void NewPlayer()
        {
            Player = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instance();
            AddChild(Player);
        }

        public override void _Process(float delta)
        {
            if (CurrentLevel != null)
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
                    GameOver.Score = Player.TotalScore;
                    GameOver.ScoreLabel.Text = GameOver.Score.ToString();
                    QueueFree();
                }
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
            }
        }

        public void UpdateTile(Type NewType)
        {
            Tile NewTile = Resources.LoadByType(NewType).Instance() as Tile;
            CurrentLevel.Grid.ReplaceTile(Player.GridPosition, NewTile);
        }

        public void UpdateGoalState(int currentScore, int currentLevelScore)
        {
            CurrentLevel.UpdateGoalState(currentLevelScore, Resources.GoalOnScene.Instance() as Tile);
        }
    }
}
