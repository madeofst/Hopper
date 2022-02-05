using Godot;
using System;
using System.Threading;

namespace Hopper
{
    public class World : Node2D
    {
        private ResourceRepository Resources { get; set; }
        private LevelFactory levelFactory { get; set; }
        
        //References to existing nodes
        private TextureRect WaterShader { get; set; }
        private TextureRect Water { get; set; }
        private TextureRect Background { get; set; }
        public Level CurrentLevel { get; set; }
        public Level NextLevel { get; set; }
        private Grid Grid { get; set; }
        private Player Player { get; set; }
        private HUD HUD { get; set; }
        private AnimationPlayer PopUp { get; set; }
        public LevelTitleScreen LevelTitleScreen { get; private set; }
        
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
                "WaterIsIt1",   //FIXME: need to revisit these
                "WaterIsIt2",
                "WaterIsIt3",
                "WaterIsIt4",
                //Challenge
                //TODO: need some easier ones
                //"DivingIn6", //FIXME: rewrite needed for new system
                //"Retrace",      //FIXME: will need rewriting
                //"DivingInEfficiently2",
                "DivingInEfficiently1",
                "SideWind",     
                "MiniMaze", //FIXME: can currently use the jump at the bottom twice
                "GettingAbout9",
        };

        public bool TempForTesting { get; set; } = false;

        //Signals
        [Signal]
        public delegate void TimeUpdate(float timeRemaining);

        public override void _Ready()
        {
            Resources = GetNode<ResourceRepository>("/root/ResourceRepository");
            levelFactory = new LevelFactory(Resources);
            
            WaterShader = GetNode<TextureRect>("WaterShader");
            Water = GetNode<TextureRect>("Water");
            Background = GetNode<TextureRect>("Background");
            HUD = GetNode<HUD>("HUD");
            LevelTitleScreen = GetNode<LevelTitleScreen>("LevelTitleScreen");

            Music = GetNode<AudioStreamPlayer2D>("Audio/Music");
            FailLevel = GetNode<AudioStreamPlayer2D>("Audio/FailLevel");
            SucceedLevel = GetNode<AudioStreamPlayer2D>("Audio/SucceedLevel");
            GoalActivate = GetNode<AudioStreamPlayer2D>("Audio/GoalActivate");
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

            HUD.Quit.Connect("pressed", this, "QuitToMenu");
            
            NewPlayer();           
            Player.Connect(nameof(Player.QuitToMenu), this, nameof(QuitToMenu));
            Player.Connect(nameof(Player.GoalReached), this, nameof(GoalReached));
            Player.Connect(nameof(Player.IncrementLevel), this, nameof(IncrementLevel));
            Player.Connect(nameof(Player.HopCompleted), HopCounterBar, nameof(HopCounterBar.UpdateHop));
            Player.Connect(nameof(Player.HopsExhausted), this, nameof(OnHopsExhausted));
            Player.Connect(nameof(Player.ScoreUpdated), ScoreCounter, nameof(ScoreCounter.UpdateText));
            Player.Connect(nameof(Player.ScoreUpdated), this, nameof(UpdateGoalState));
            Player.Connect(nameof(Player.TileChanged), this, nameof(UpdateTile));
            Player.Connect(nameof(Player.MoveBehind), this, nameof(MovePlayerBehind));
            Player.Connect(nameof(Player.MoveToTop), this, nameof(MovePlayerToTop));

            LevelTitleScreen.Connect(nameof(LevelTitleScreen.ActivatePlayer), Player, nameof(Player.Activate));
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.LoadNextLevel), this, nameof(BuildLevel), new Godot.Collections.Array { false });
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.StartMusic), this, nameof(PlayMusic));

            if (tempWorldForTesting)
            {
                TempForTesting = true;
                NewLevel(levelName, true);
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

        private void ShowWorld()
        {
            WaterShader.Visible = true;
            Water.Visible = true;
            Background.Visible = true;
            HUD.Visible = true;
            Player.Visible = true;
        }

        private void PlayMusic()
        {
            Music.Play();
        }

        private void MovePlayerToTop()
        {
            MoveChild(Player, GetChildCount());
        }

        private void MovePlayerBehind()
        {
            MoveChild(Player, 4);
        }

        private void NewLevel(Vector2 playerPosition)
        {
            if (CurrentLevel != null) 
                CurrentLevel.QueueFree();
            CurrentLevel = levelFactory.Generate(playerPositionX: (int)playerPosition.x, 
                                                 playerPositionY: (int)playerPosition.y);
            BuildLevel();
        }

        private void NewLevel(string levelName, bool replay = false)
        {
            NextLevel = levelFactory.Load(levelName, true);
            InitialiseLevelLoad(NextLevel, replay);
        }

        private void InitialiseLevelLoad(Level level, bool replay)
        {
            if (!replay)
            {
                LevelTitleScreen.Init(iLevel + 1, level.LevelData.MaximumHops, level.LevelData.ScoreRequired); //FIXME: Need to change iLevel to get its number from the Level itself
                LevelTitleScreen.AnimateShow();
            }
            else
            {
                BuildLevel(replay);
                //BuildLevelInThread(replay);
            }
        }

        private void ConnectRestartButton(Level currentLevel)
        {
            if (HUD.Restart.IsConnected("pressed", this, "NewLevel")) HUD.Restart.Disconnect("pressed", this, "NewLevel");
            HUD.Restart.Connect("pressed", this, "NewLevel", new Godot.Collections.Array() { currentLevel.LevelName, true } );
        }

/*         private void BuildLevelInThread(bool replay = false)
        {
            ShowWorld();
            var th = new System.Threading.Thread(BuildLevel);
            th.Start(false);
        } */

        private void BuildLevel(bool replay = false)
        {
/*             bool replay = false;
            try {
                replay = (bool)obj;
            }
            catch (InvalidCastException) {
                replay = false;
            } */
            ShowWorld();
            if (!replay)
            {
                if (NextLevel == null)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if (NextLevel == null) NextLevel = CurrentLevel;
                }
            }

            AddChildBelowNode(Background, NextLevel);
            NextLevel.Connect(nameof(Level.LevelBuilt), HopCounterBar, nameof(HopCounterBar.SetMaxHops));
            NextLevel.Build(Resources);           
            Grid = NextLevel.Grid;
            Player.Init(NextLevel, replay);
            ScoreBox.LevelMinScore.BbcodeText = NextLevel.ScoreRequired.ToString();
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
            ConnectRestartButton(NextLevel);
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = NextLevel;
            NextLevel = null;
        }

        private void NewPlayer()
        {
            Player = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instance();
            AddChildBelowNode(Background, Player);
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

                if (LevelTitleScreen.Animating || LevelTitleScreen.Visible)
                {
                    Player.Active = false;
                }
                else
                {
                    Player.Active = true;
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
            Player.ClearQueues();
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
            if (CurrentLevel != null)
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
                NewLevel(Levels[iLevel], true);
            }
        }
    }
}
