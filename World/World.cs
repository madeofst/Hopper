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
        private HUD HUD { get; set; }
        private AnimationPlayer PopUp { get; set; }
        
        public AudioStreamPlayer2D Music { get; set; }
        public AudioStreamPlayer2D FailLevel { get; private set; }
        public AudioStreamPlayer2D SucceedLevel { get; private set; }
        public AudioStreamPlayer2D GoalActivate { get; private set; }

        private HopCounter HopCounterBar { get; set; }
        private ScoreCounter ScoreCounter { get; set; }
        private TimeCounter TimeCounter { get; set; }
        private Stopwatch Stopwatch { get; set; }
        private ScoreBox ScoreBox  { get; set; }

        //World parameters
        public bool GameOver = false;
        public milliTimer Timer;
        private bool _PuzzleMode = true;
        public bool PuzzleMode
        {
            get
            {
                if (Levels == null || Levels.Length <= 0) return false;
                return true;
            }
            set
            {
                _PuzzleMode = value;
            }
        }

        //List of levels
        public int iLevel { get; set; } = 0;
        public string[] Levels { get; set; } = new string[] 
        {
            //Basic (no special tiles)
                //Instructional
                "StartingOut",
                "SecondOfLy",
                //Challenge
                "PointsPointsPoints6",
                "ArtAndSoul2",
            //Jumping (jump tile only)
                //Instructional
                "MovingOn",
                "MovingOn2",
                //Challenge
                "DoubleJump",
                "WeirdMirror1",
                "Jumpington",
            //Water (jump + water tile)
                //Instructional
                "WaterIsIt1",
                "WaterIsIt2",
                "WaterIsIt3",
                "WaterIsIt4",
                //Challenge
                "Retrace",
                "SideWind",
                "MiniMaze"
        };

        public bool TempForTesting { get; set; } = false;

        //Signals
        [Signal]
        public delegate void TimeUpdate(float timeRemaining);

        public override void _Ready()
        {
            Resources = GetNode<ResourceRepository>("/root/ResourceRepository");
            levelFactory = new LevelFactory(Resources);
            
            HUD = GetNode<HUD>("HUD");

            Music = GetNode<AudioStreamPlayer2D>("Music");
            FailLevel = GetNode<AudioStreamPlayer2D>("FailLevel");
            SucceedLevel = GetNode<AudioStreamPlayer2D>("SucceedLevel");
            GoalActivate = GetNode<AudioStreamPlayer2D>("GoalActivate");
        }

        public void Init(bool tempWorldForTesting = false, string levelName = "")
        {
            HopCounterBar = GetNode<HopCounter>("HUD/HopCounter");
            ScoreCounter = GetNode<ScoreCounter>("HUD/TimeAndScoreSimple/VBoxContainer/ScoreCounter");
            TimeCounter = GetNode<TimeCounter>("HUD/TimeAndScoreSimple/VBoxContainer/TimeCounter");
            Stopwatch = GetNode<Stopwatch>("HUD/TimeAndScoreSimple/VBoxContainer/Stopwatch");
            ScoreBox = GetNode<ScoreBox>("HUD/ScoreBox");

            Connect(nameof(World.TimeUpdate), TimeCounter, "UpdateText");
            Connect(nameof(World.TimeUpdate), Stopwatch, "UpdateStopwatch");
            
            NewPlayer();           
            Player.Connect(nameof(Player.QuitToMenu), this, nameof(QuitToMenu));
            Player.Connect(nameof(Player.GoalReached), this, nameof(GoalReached));
            Player.Connect(nameof(Player.IncrementLevel), this, nameof(IncrementLevel));
            Player.Connect(nameof(Player.HopCompleted), HopCounterBar, nameof(HopCounterBar.UpdateHop));
            Player.Connect(nameof(Player.HopsExhausted), this, nameof(OnHopsExhausted));
            Player.Connect(nameof(Player.ScoreUpdated), ScoreCounter, nameof(ScoreCounter.UpdateText));
            Player.Connect(nameof(Player.ScoreUpdated), this, nameof(UpdateGoalState));
            Player.Connect(nameof(Player.TileChanged), this, nameof(UpdateTile));

            if (tempWorldForTesting)
            {
                TempForTesting = true;
                NewLevel(levelName);
            }
            else
            {
                if (PuzzleMode)
                {
                    NewLevel(Levels[iLevel]);
                }
                else
                {
                    NewLevel(Player.GridPosition);
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
            MoveChild(HUD, GetChildCount());
            MoveChild(Player, GetChildCount());
            Player.Init(CurrentLevel);
            ScoreBox.LevelMinScore.BbcodeText = CurrentLevel.ScoreRequired.ToString();
            if (!PuzzleMode)
            {
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
  
            Music.Play();
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
                    Music.Stop();
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


        public void GoalReached()
        {
            Music.Stop();
            Player.Active = false;
            Player.MoveInputQueue.Clear();
            Player.AnimationPlayer.Play("LevelComplete");

            if (iLevel >= Levels.Length - 1)
            {
                HUD.ShowPopUp("Game Complete");
            }
            else
            {
                HUD.ShowPopUp("Level complete!");
                SucceedLevel.Play();
            }
        }

        public void QuitToMenu()
        {
            QueueFree();
            GetNode<Menu>("/root/Menu").Show();
        }

        public void IncrementLevel()
        {
            iLevel++;
            if (TempForTesting)
            {
                QueueFree();
            }
            else
            {
                if (PuzzleMode)
                {
                    if (iLevel >= Levels.Length)
                    {
                        QuitToMenu();
                    }
                    else 
                    {
                        NewLevel(Levels[iLevel]);
                    }
                }
                else
                {
                    NewLevel(Player.GridPosition);
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
            ScoreBox.UpdatePlayerScore(currentScore, currentLevelScore);
            if (!CurrentLevel.Grid.GoalTile.Activated)
            {
                bool MinScoreReached = CurrentLevel.UpdateGoalState(currentLevelScore, Resources.GoalOnScene.Instance() as Tile);
                if (MinScoreReached && currentLevelScore != 0)
                {
                    GoalActivate.Play();
                    ScoreBox.Animate();
                }
            }
        }

        public void OnHopsExhausted()
        {
            if (TempForTesting)
            {
                QueueFree();
            }
            else if (Levels.Length <= 0 || iLevel > Levels.Length - 1)
            {
                GameOver = true;
            }
            else
            {
                FailLevel.Play();
                HUD.ShowPopUp("Try again!");
                NewLevel(Levels[iLevel]);
            }
        }
    }
}
