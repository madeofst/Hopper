using Godot;
using System;

namespace Hopper
{
    public class Stage : Node2D
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

        //Pond
        private Pond Pond { get; set; }

        //Level
        public Level CurrentLevel { get; set; }
        public Level NextLevel { get; set; }
        private Grid Grid { get; set; }
        
        //Menus
        public LevelTitleScreen LevelTitleScreen { get; private set; }

        //Audio
        public AudioStreamPlayer Music { get; set; }
        public AudioStreamPlayer FailLevel { get; private set; }
        public AudioStreamPlayer SucceedLevel { get; private set; }
        public AudioStreamPlayer GoalActivate { get; private set; }

        //HUD
        private HUD HUD { get; set; }        
        private Stopwatch Stopwatch { get; set; }

        //Timer
        public milliTimer Timer;

        //Parameters for Stage
        public int ID { get; set; }
        public bool Paused { get; private set; } = false;
        public bool TempForTesting { get; set; } = false;
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
        public StageData StageData;

        //List of levels
        public int iLevel { get; set; }
        public string[] Levels { get; set; }

        //Signals
        [Signal]
        public delegate void TimeUpdate(float timeRemaining);
        [Signal]
        public delegate void UnlockNextStage();
        [Signal]
        public delegate void UpdateLocationProgress();
        [Signal]
        public delegate void SaveState();

        public override void _Ready()
        {
            Resources = GetNode<ResourceRepository>("/root/ResourceRepository");
            levelFactory = new LevelFactory(Resources);
            
            WaterShader = GetNode<TextureRect>("WaterShader");
            Water = GetNode<TextureRect>("Water");
            Background = GetNode<TextureRect>("Background");
            HUD = GetNode<HUD>("../HUD"); 
            LevelTitleScreen = GetNode<LevelTitleScreen>("../LevelTitleScreen");

            Music = GetNode<AudioStreamPlayer>("../AudioRepository/Music");
            FailLevel = GetNode<AudioStreamPlayer>("../AudioRepository/FailLevel");
            SucceedLevel = GetNode<AudioStreamPlayer>("../AudioRepository/SucceedLevel");
            GoalActivate = GetNode<AudioStreamPlayer>("../AudioRepository/GoalActivate");
        }

        public void Init(StageData StageData,
                         string[] levels, 
                         Vector2 position, 
                         bool tempStageForTesting = false,
                         string levelName = "", 
                         Map Map = null) 
        {
            this.StageData = StageData;
            ID = StageData.ID;
            Levels = levels;
            if (StageData.LevelReached > Levels.Length - 1)
            {
                this.iLevel = 0;
            }
            else
            {
                this.iLevel = StageData.LevelReached;
            }
            //Position = position - new Vector2(240, 135);

            if (StageData.Pond == "Hawkins")
            {
                Background.Texture = GD.Load<Texture>("res://Stage/Resources/DesertPond.png");
            }
            else if (StageData.Pond == "BelAir")
            {
                Background.Texture = GD.Load<Texture>("res://Stage/Resources/TestPondDrawing4.png");
            }
            else if (StageData.Pond == "Liffey")
            {
                Background.Texture = GD.Load<Texture>("res://Stage/Resources/TestPondDrawing4.png");
            }
            else
            {
                Background.Texture = GD.Load<Texture>("res://Stage/Resources/TestPondDrawing4.png");
            }

            HUD.LockPosition(Position);

            //Connect(nameof(Stage.TimeUpdate), Stopwatch, "UpdateStopwatch"); //FIXME: need to sort out stopwatch at some point

            ScoreBox ScoreBox = GetNode<ScoreBox>("../HUD/ScoreBox"); //FIXME: this basically defeats the object of signaling up I think
            ScoreBox.PlayerLevelScore.Connect(nameof(ScoreLabel.ScoreAnimationFinished), this, nameof(ScoreAnimationFinished));
            ScoreBox.PlayerLevelScore.Connect(nameof(ScoreLabel.ScoreAnimationStarted), this, nameof(ScoreAnimationStarted));

            if (Map != null)
            {
                Map.ConnectSaveSignal(this);
            }

            NewPlayer();           
            Player.Connect(nameof(Player.Pause), this, nameof(Pause));
            Player.Connect(nameof(Player.GoalReached), this, nameof(GoalReached));
            Player.Connect(nameof(Player.IncrementLevel), this, nameof(IncrementLevel));
            Player.Connect(nameof(Player.HopCompleted), HUD, nameof(HUD.UpdateHop));
            Player.Connect(nameof(Player.HopsExhausted), this, nameof(OnHopsExhausted));
            Player.Connect(nameof(Player.ScoreUpdated), this, nameof(UpdateGoalStateAndScore));
            Player.Connect(nameof(Player.TileChanged), this, nameof(UpdateTile));
            Player.Connect(nameof(Player.MoveBehind), this, nameof(MovePlayerBehind));
            Player.Connect(nameof(Player.MoveToTop), this, nameof(MovePlayerToTop));
            Player.Connect(nameof(Player.BackToMap), this, nameof(QuitToMap));
            Player.Connect(nameof(Player.Quit), this, nameof(QuitToMenu));
            Player.Connect(nameof(Player.PlayFailSound), FailLevel, "play");

            LevelTitleScreen.Connect(nameof(LevelTitleScreen.ActivatePlayer), Player, nameof(Player.Appear));
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.TitleScreenLoaded), this, nameof(OnTitleScreenLoaded), new Godot.Collections.Array { false });
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.StartMusic), this, nameof(PlayMusic));
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.SelectLevel), this, nameof(NewLevel));
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.Unpause), this, nameof(Unpause));
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.BackToMap), this, nameof(QuitToMap));
            LevelTitleScreen.Connect(nameof(LevelTitleScreen.QuitToMenu), this, nameof(QuitToMenu));

            if (tempStageForTesting)
            {
                TempForTesting = true;
                NewLevel(levelName, false);
            }
            else
            {
                if (PuzzleMode)
                {
                    if (StageData.LevelReached >= Levels.Length)
                    {
                        NewLevel(Levels[0]);
                    }
                    else
                    {
                        NewLevel(Levels[StageData.LevelReached]);
                    }
                }
                else
                {
                    //NewLevel(Player.GridPosition);
                }
            }
        }

        //Working with levels

        private void NewLevel(int levelID)
        {
            iLevel = levelID;
            NextLevel = levelFactory.Load(Levels[iLevel], true);
            BuildLevel(false);
        }

        private void NewLevel(string levelName, bool replay = false)
        {
            NextLevel = levelFactory.Load(levelName, true);
            
            HUD.Visible = true;
            if (!replay && !TempForTesting)
            {
                InitialiseLevelTitle(NextLevel);
            }
            else
            {
                BuildLevel(replay);
                Player.Appear();
            }
        }

        private void InitialiseLevelTitle(Level level)
        {
            MoveToTop(LevelTitleScreen);
            MoveToTop(HUD);
            HUD.OverlayMenu.ChangeMode(OverlayMenuMode.LevelTitle);
            LevelTitleScreen.SetPosition(Position);
            LevelTitleScreen.Init(StageData,
                                  Levels.Length,
                                  iLevel + 1,
                                  level.LevelData.MaximumHops,
                                  level.LevelData.ScoreRequired); //FIXME: Need to change iLevel to get its number from the Level itself
            LevelTitleScreen.AnimateShow();
            if (HUD.TouchButtons.Visible)
                HUD.TouchButtons.Visible = false; //FIXME: not sure why but this creates an error !is_inside_tree = true
        }

        /*         private void NewLevel(Vector2 playerPosition)
                {
                    if (CurrentLevel != null) 
                        CurrentLevel.QueueFree();
                    CurrentLevel = levelFactory.Generate(playerPositionX: (int)playerPosition.x, 
                                                         playerPositionY: (int)playerPosition.y);
                    BuildLevel();
                } */

        private void OnTitleScreenLoaded(bool replay = false)
        {
            if (Paused)
            {

            }
            else
            {
                BuildLevel(replay);
            }
        }

        private void BuildLevel(bool replay = false)
        {
            GetViewport().MoveChild(HUD, GetViewport().GetChildCount());
            if (!replay)
            {
                if (NextLevel == null) NextLevel = CurrentLevel;
            }

            AddChildBelowNode(Background, NextLevel);
            NextLevel.Connect(nameof(Level.LevelBuilt), HUD, nameof(HUD.SetMaxHops));
            NextLevel.Build(Resources);           
            Grid = NextLevel.Grid;
            
            HUD.ShowScoreBox();
            HUD.SetButtonToRestart();
            HUD.UpdateMinScore(NextLevel.ScoreRequired, false);
            HUD.CountInActiveHops();
            Player.Init(NextLevel, replay);
            //MoveToTop(HUD);

            if (!PuzzleMode && !TempForTesting)
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
            ConnectOverlayMenu(NextLevel);
            UpdateGoalStateAndScore(NextLevel.ScoreRequired);
            if (CurrentLevel != null) CurrentLevel.QueueFree();
            CurrentLevel = NextLevel;
            NextLevel = null;
            Visible = true;
        }

        public void IncrementLevel()
        {
            if (TempForTesting)
            {
                QueueFree();
                HUD.Visible = false;
            }
            else
            {
                if (PuzzleMode)
                {
                    //Level reach will be 1 higher than available levels when complete
                    if (iLevel == StageData.LevelReached) UpdateLevelReached();
                    iLevel++;

                    if (iLevel >= Levels.Length)
                    {
                        EmitSignal(nameof(UnlockNextStage));
                        QuitToMap(); 
                    }
                    else 
                    {
                        NewLevel(Levels[iLevel]);
                    }
                    EmitSignal(nameof(SaveState));
                }
                else
                {
                    //NewLevel(Player.GridPosition);
                }
            }
        }

        private void UpdateLevelReached()
        {
            StageData.LevelReached++;
            LevelTitleScreen.UpdateLevelReached(StageData.LevelReached);
            EmitSignal(nameof(UpdateLocationProgress), StageData.LevelReached);
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
            SucceedLevel.Play();
        }

        public void UpdateGoalStateAndScore(int updatedScore)
        {
            Level level = null;
            if (NextLevel != null) level = NextLevel;
            else if (CurrentLevel != null) level = CurrentLevel;

            if (level != null)
            {
                HUD.UpdateScore(updatedScore);
                if (!level.Grid.GoalTile.Activated && updatedScore <= 0)
                {
                    level.UpdateGoalState(updatedScore, Resources.GoalOnScene.Instance() as Tile);
                    GoalActivate.Play();
                    HUD.AnimateScoreBox();
                }
            }
        }

        public void OnHopsExhausted()
        {
            if (Levels.Length <= 0 || iLevel > Levels.Length - 1)
            {
                GameOver = true;
            }
        }
                
        public void RestartLevel(string levelName, bool fail = false)
        {
            Player.RestartingLevel = true;
            NewLevel(levelName, true);
            if (fail) HUD.ShowPopUp("Try again!");
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
            if (Player.IsConnected(nameof(Player.Restart), this, nameof(RestartLevel))) 
                Player.Disconnect(nameof(Player.Restart), this, nameof(RestartLevel));
            Player.Connect(nameof(Player.Restart), this, nameof(RestartLevel), new Godot.Collections.Array() { currentLevel.LevelName, false } );
        }

        private void ConnectOverlayMenu(Level currentLevel)
        {
            if (HUD.OverlayMenu.RestartButton.IsConnected("pressed", this, nameof(RestartLevel)))
                HUD.OverlayMenu.RestartButton.Disconnect("pressed", this, nameof(RestartLevel));
            HUD.OverlayMenu.RestartButton.Connect("pressed", this, nameof(RestartLevel), new Godot.Collections.Array() { currentLevel.LevelName, false } );

            if (HUD.OverlayMenu.QuitButton.IsConnected("pressed", this, nameof(QuitToMenu)))
                HUD.OverlayMenu.QuitButton.Disconnect("pressed", this, nameof(QuitToMenu));
            HUD.OverlayMenu.QuitButton.Connect("pressed", this, nameof(QuitToMenu));

            if (HUD.OverlayMenu.MapButton.IsConnected("pressed", this, nameof(QuitToMap)))
                HUD.OverlayMenu.MapButton.Disconnect("pressed", this, nameof(QuitToMap));
            HUD.OverlayMenu.MapButton.Connect("pressed", this, nameof(QuitToMap));
        }

        private void ScoreAnimationFinished()   { ScoreAnimFinished = true; }
        private void ScoreAnimationStarted()    { ScoreAnimFinished = false; }

        public void ShowStage()
        {
            WaterShader.Visible = true;
            Water.Visible = true;
            Background.Visible = true;
            HUD.Visible = true;
        }

        public void HideStage()
        {
            WaterShader.Visible = false;
            Water.Visible = false;
            Background.Visible = false;
            HUD.Visible = false;
        }

        private void PlayMusic()    { Music.Play(); }

        private void MovePlayerToTop()  { MoveChild(Player, GetChildCount()); }
        private void MovePlayerBehind() { MoveChild(Player, Background.GetPositionInParent()); }

        private void MoveToTop(Node node = null)
        {
            if (node == null) node = this;
            GetViewport().MoveChild(node, GetViewport().GetChildCount());
        }

        private void Unpause()
        {
            HUD.OverlayMenu.ChangeMode(OverlayMenuMode.Stage);
            MoveToTop(HUD);
            Player.Activate();
            Paused = false;
        }

        private void Pause()
        {
            Paused = true;
            Player.Deactivate();
            
            if (!TempForTesting)
            {
                InitialiseLevelTitle(CurrentLevel);
            }
        }

        private void QuitToMap()
        {
            if (LevelTitleScreen.Visible == true) LevelTitleScreen.AnimateHide();

            HUD.HideHopCounter();
            HUD.HideScoreBox();
            HUD.SetButtonToEnter();
            HUD.UnlockPosition();
            HUD.OverlayMenu.ChangeMode(OverlayMenuMode.Map);
            QueueFree();
            if (!TempForTesting)
            {
                Map Map = GetNode<Map>("/root/MapContainer/ViewportContainer/Viewport/Map");
                Map.ConnectPauseSignals();
                Map.UpdateActivationState(StageData.LevelReached);
                Map.Show();
                Map.SetProcessInput(true);
                Map.GetNode<Pointer>("Pointer").SetProcessInput(true);
            }
            else
            {
                HUD.Visible = false;
            }
        }

        public void QuitToMenu()
        {
            if (LevelTitleScreen.Visible == true) LevelTitleScreen.AnimateHide();

            HUD.Close();
            QueueFree();
            if (!TempForTesting)
            {
                HUD.Visible = false;
                GetNode<Map>("/root/MapContainer/ViewportContainer/Viewport/Map").QueueFree();  
                StartMenu StartMenu = GetNode<StartMenu>("/root/StartMenu");
                StartMenu.UpdateLoadButton();
                StartMenu.ShowMenu();
            }
        }
    }
}
