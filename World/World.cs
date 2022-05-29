using Godot;
using System;
using System.Threading;

namespace Hopper
{
    public class World : Node2D
    {
        //Classes for loading stuff
        private ResourceRepository Resources { get; set; }
        private LevelFactory levelFactory { get; set; }
        
        //Visuals
        private TextureRect WaterShader { get; set; }
        private TextureRect Water { get; set; }
        private TextureRect Background { get; set; }

        //Player
        private Player Player { get; set; }

        //Level
        public Level CurrentLevel { get; set; }
        public Level NextLevel { get; set; }
        private Grid Grid { get; set; }
        
        //Menus
        public LevelTitleScreen LevelTitleScreen { get; private set; }
        public PauseMenu PauseMenu { get; private set; }

        //Audio
        public AudioStreamPlayer2D Music { get; set; }
        public AudioStreamPlayer2D FailLevel { get; private set; }
        public AudioStreamPlayer2D SucceedLevel { get; private set; }
        public AudioStreamPlayer2D GoalActivate { get; private set; }

        //HUD
        private HUD HUD { get; set; }        
        private HopCounter HopCounterBar { get; set; }
        private Stopwatch Stopwatch { get; set; }
        private ScoreBox ScoreBox  { get; set; }

        //Timer
        public milliTimer Timer;

        //Parameters for World
        public bool Paused { get; private set; } = false;
        public bool TempForTesting { get; set; } = false;
        public bool HopsExhausted { get; set; } = false; //TODO: won't need this after sorting restart animation
        public bool ScoreAnimFinished = false;
        public bool GameOver = false;
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
        public string[] Levels { get; set; } /* = new string[] 
        {
             //Basic (no special tiles)
                //Instructional
                "StartingOut",
                "SecondOfLy",
                "Up",
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
                "BlueLine",
                //TODO: need some easier ones
            //Combined challenge
                "Retrace",
                "DivingIn1",      
                "DivingIn1a",
                "DivingIn6",
                "DivingInEfficiently1",
                "SideToSide",
                "Mazemerize",
                "TheSquare",
                "PondInPond",
                "SideWind",     
                "MiniMaze",
                "GettingAbout9",
        }; */


        //Signals
        [Signal]
        public delegate void TimeUpdate(float timeRemaining);
        [Signal]
        public delegate void UnlockNextWorld();

        public override void _Ready()
        {
            Resources = GetNode<ResourceRepository>("/root/ResourceRepository");
            levelFactory = new LevelFactory(Resources);
            
            WaterShader = GetNode<TextureRect>("WaterShader");
            Water = GetNode<TextureRect>("Water");
            Background = GetNode<TextureRect>("Background");
            HUD = GetNode<HUD>("HUD");
            LevelTitleScreen = GetNode<LevelTitleScreen>("../LevelTitleScreen");
            PauseMenu = GetNode<PauseMenu>("../PauseMenu");

            Music = GetNode<AudioStreamPlayer2D>("Audio/Music");
            FailLevel = GetNode<AudioStreamPlayer2D>("Audio/FailLevel");
            SucceedLevel = GetNode<AudioStreamPlayer2D>("Audio/SucceedLevel");
            GoalActivate = GetNode<AudioStreamPlayer2D>("Audio/GoalActivate");
        }

        public void Init(string[] levels, Vector2 position, bool tempWorldForTesting = false, string levelName = "") //TODO: Think I probs just need to pass an array of level names here
        {
            Levels = levels;
            Position = position - new Vector2(240, 135);

            HopCounterBar = GetNode<HopCounter>("HUD/HopCounter");
            Stopwatch = GetNode<Stopwatch>("HUD/TimeAndScoreSimple/VBoxContainer/Stopwatch");
            ScoreBox = GetNode<ScoreBox>("HUD/ScoreBox");

            Connect(nameof(World.TimeUpdate), Stopwatch, "UpdateStopwatch");

            ScoreBox.PlayerLevelScore.Connect(nameof(ScoreLabel.ScoreAnimationFinished), this, nameof(ScoreAnimationFinished));
            ScoreBox.PlayerLevelScore.Connect(nameof(ScoreLabel.ScoreAnimationStarted), this, nameof(ScoreAnimationStarted));

            PauseMenu.QuitButton.Connect("pressed", this, nameof(QuitToMenu));
            PauseMenu.Connect(nameof(PauseMenu.Quit), this, nameof(QuitToMenu));
            PauseMenu.MapButton.Connect("pressed", this, nameof(QuitToMap));
            PauseMenu.Connect(nameof(PauseMenu.Map), this, nameof(QuitToMap));
            PauseMenu.Connect(nameof(PauseMenu.Unpause), this, nameof(Unpause));
            
            NewPlayer();           
            Player.Connect(nameof(Player.Pause), this, nameof(Pause));
            Player.Connect(nameof(Player.GoalReached), this, nameof(GoalReached));
            Player.Connect(nameof(Player.IncrementLevel), this, nameof(IncrementLevel));
            Player.Connect(nameof(Player.HopCompleted), HopCounterBar, nameof(HopCounterBar.UpdateHop));
            Player.Connect(nameof(Player.HopsExhausted), this, nameof(OnHopsExhausted));
            Player.Connect(nameof(Player.ScoreUpdated), this, nameof(UpdateGoalStateAndScore));
            Player.Connect(nameof(Player.TileChanged), this, nameof(UpdateTile));
            Player.Connect(nameof(Player.MoveBehind), this, nameof(MovePlayerBehind));
            Player.Connect(nameof(Player.MoveToTop), this, nameof(MovePlayerToTop));
            Player.Connect(nameof(Player.Quit), this, nameof(QuitToMenu));
            Player.Connect(nameof(Player.PlayFailSound), FailLevel, "play");

            LevelTitleScreen.Connect(nameof(LevelTitleScreen.ActivatePlayer), Player, nameof(Player.Appear));
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.LoadNextLevel), this, nameof(BuildLevel), new Godot.Collections.Array { false });
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.StartMusic), this, nameof(PlayMusic));

            if (tempWorldForTesting)
            {
                TempForTesting = true;
                NewLevel(levelName, false);
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


        //Working with levels
        private void InitialiseLevelLoad(Level level, bool replay)
        {
            if (!replay && !TempForTesting)
            {
                LevelTitleScreen.SetPosition(Position);
                LevelTitleScreen.Init(iLevel + 1, level.LevelData.MaximumHops, level.LevelData.ScoreRequired); //FIXME: Need to change iLevel to get its number from the Level itself
                LevelTitleScreen.AnimateShow();
            }
            else
            {
                BuildLevel(replay);
                Player.Appear();
            }
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

        private void BuildLevel(bool replay = false)
        {
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
            ScoreBox.LevelMinScore.UpdateText(NextLevel.ScoreRequired.ToString(), false);
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
            UpdateGoalStateAndScore(0, 0, NextLevel.ScoreRequired);
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = NextLevel;
            NextLevel = null;
            Visible = true;
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
                        EmitSignal(nameof(UnlockNextWorld));
                        QuitToMap(); 
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

        //Working with player
        private void NewPlayer()
        {
            Player = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instance();
            AddChildBelowNode(Background, Player);
        }


        public void GoalReached()
        {
            Music.Stop();
            Player.ClearQueues();
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

        public void UpdateGoalStateAndScore(int currentScore, int currentLevelScore, int minScore)
        {
            Level level = null;
            if (NextLevel != null) level = NextLevel;
            else if (CurrentLevel != null) level = CurrentLevel;

            if (level != null)
            {
                ScoreBox.UpdatePlayerScore(currentScore, currentLevelScore, level.ScoreRequired);
                if (!level.Grid.GoalTile.Activated)
                {
                    bool MinScoreReached = level.UpdateGoalState(currentLevelScore, Resources.GoalOnScene.Instance() as Tile);
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
                HopsExhausted = true; //TODO: won't need this after sorting smoke animation
            }
        }
                
        public void RestartLevel(string levelName, bool fail = false)
        {
            Player.RestartingLevel = true;
            if (PauseMenu.Visible == true) PauseMenu.AnimateHide();
            NewLevel(levelName, true);
            if (fail)
            {
                HUD.ShowPopUp("Try again!");
                HopsExhausted = false;
            }
            Player.Appear();
        }

        //General utitlity methods
        public void UpdateTile(Type NewType)
        {
            Tile NewTile = Resources.LoadByType(NewType).Instance() as Tile;
            CurrentLevel.Grid.ReplaceTile(Player.GridPosition, NewTile);
        }

        private void UpdateTimeRemaining()
        {
            if (Timer != null) EmitSignal(nameof(TimeUpdate), Timer.Remaining());
        }

        private void ConnectRestartButton(Level currentLevel)
        {
            if (PauseMenu.RestartButton.IsConnected("pressed", this, nameof(RestartLevel))) PauseMenu.RestartButton.Disconnect("pressed", this, nameof(RestartLevel));
            PauseMenu.RestartButton.Connect("pressed", this, nameof(RestartLevel), new Godot.Collections.Array() { currentLevel.LevelName, false } );

            if (PauseMenu.IsConnected(nameof(PauseMenu.Restart), this, nameof(RestartLevel))) PauseMenu.Disconnect(nameof(PauseMenu.Restart), this, nameof(RestartLevel));
            PauseMenu.Connect(nameof(PauseMenu.Restart), this, nameof(RestartLevel), new Godot.Collections.Array() { currentLevel.LevelName, false } );

            if (Player.IsConnected(nameof(Player.Restart), this, nameof(RestartLevel))) Player.Disconnect(nameof(Player.Restart), this, nameof(RestartLevel));
            Player.Connect(nameof(Player.Restart), this, nameof(RestartLevel), new Godot.Collections.Array() { currentLevel.LevelName, false } );
        }

        private void ScoreAnimationFinished()   { ScoreAnimFinished = true; }
        private void ScoreAnimationStarted()    { ScoreAnimFinished = false; }

        public void ShowWorld()
        {
            WaterShader.Visible = true;
            Water.Visible = true;
            Background.Visible = true;
            HUD.Visible = true;
        }

        public void HideWorld()
        {
            WaterShader.Visible = false;
            Water.Visible = false;
            Background.Visible = false;
            HUD.Visible = false;
        }

        private void PlayMusic()    { Music.Play(); }

        private void MovePlayerToTop()  { MoveChild(Player, HUD.GetPositionInParent() - 1); }
        private void MovePlayerBehind() { MoveChild(Player, Background.GetPositionInParent()); }

        public override void _Process(float delta)
        {
            if (CurrentLevel != null)
            {
                UpdateTimeRemaining();

                if (Timer != null)
                {
                    if (Timer.Finished()) GameOver = true;
                }

                //if (HopsExhausted && ScoreAnimFinished) RestartLevel(CurrentLevel.LevelName, true); //TODO: won't need this after sorting animation

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

        private void Unpause()
        {
            Player.Activate();
            Paused = false;
        }

        private void Pause()
        {
            Paused = true;
            Player.Deactivate();
            PauseMenu.SetPosition(Position);
            PauseMenu.Visible = true;
            PauseMenu.SetMode(PauseMenu.PauseMenuMode.World);
            PauseMenu.AnimateShow();
        }

        private void QuitToMap()
        {
            if (PauseMenu.Visible == true) PauseMenu.AnimateHide();
            QueueFree();
            Map Map = GetNode<Map>("/root/Map");
            if (!TempForTesting)
            {
                Map.Show();
                Map.SetProcessInput(true);
                Map.GetNode<Pointer>("Pointer").SetProcessInput(true);
            }
        }
                
        public void QuitToMenu()
        {
            QueueFree();
            if (!TempForTesting)
            {
                GetNode<Map>("/root/Map").QueueFree();  
                GetNode<StartMenu>("/root/StartMenu").ShowMenu();
            }
        }
    }
}
