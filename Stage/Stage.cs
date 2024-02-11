using Godot;
using System;
using System.Collections.Generic;

namespace Hopper
{
	public class Stage : Node2D
		{
		//Classes for loading stuff
		private ResourceRepository Resources { get; set; }
		private LevelFactory LevelFactory { get; set; }
		
		//Visuals
		private TextureRect WaterShader { get; set; }
		private TextureRect Water { get; set; }
		private TextureRect Background { get; set; }

		//Player
		private Player Player { get; set; }

		//Pond
		private Pond Pond { get; set; }

		//Boss
		private Boss Boss { get; set; }

		//Undo Manager
		private UndoManager UndoManager { get; set; }

		//Level
		public Level CurrentLevel { get; set; }
		public Level NextLevel { get; set; }
		public Grid Grid { get; set; }
		
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
			LevelFactory = new LevelFactory(Resources);
			
			WaterShader = GetNode<TextureRect>("WaterShader");
			Water = GetNode<TextureRect>("Water");
			Background = GetNode<TextureRect>("Background");
			HUD = GetNode<HUD>("../HUD"); 
			UndoManager = GetNode<UndoManager>("UndoManager"); 
			LevelTitleScreen = GetNode<LevelTitleScreen>("../LevelTitleScreen");

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
				Background.Texture = GD.Load<Texture>("res://Stage/Resources/GardenPond.png");
				Music = GetNode<AudioStreamPlayer>("../AudioRepository/GardenPondMusic");
			}
			else if (StageData.Pond == "BelAir")
			{
				Background.Texture = GD.Load<Texture>("res://Stage/Resources/TestPondDrawing4.png");
				Music = GetNode<AudioStreamPlayer>("../AudioRepository/ForestPondMusic");
			}
			else if (StageData.Pond == "Liffey")
			{
				Background.Texture = GD.Load<Texture>("res://Stage/Resources/DesertPond.png");
				Music = GetNode<AudioStreamPlayer>("../AudioRepository/ForestPondMusic");
			}
			else if (StageData.Pond == "Idwal")
			{
				Background.Texture = GD.Load<Texture>("res://Stage/Resources/TestPondDrawing4.png");
				Music = GetNode<AudioStreamPlayer>("../AudioRepository/ForestPondMusic");
			}
			else if (StageData.Pond == "Boss")
			{
				Background.Texture = GD.Load<Texture>("res://Stage/Resources/BossPond.png");
				Music = GetNode<AudioStreamPlayer>("../AudioRepository/ForestPondMusic");
			}
			else
			{
				Background.Texture = GD.Load<Texture>("res://Stage/Resources/TestPondDrawing4.png");
				Music = GetNode<AudioStreamPlayer>("../AudioRepository/ForestPondMusic");
			}

			Music.Play();
			
			HUD.LockPosition(Position);

			ScoreBox ScoreBox = GetNode<ScoreBox>("../HUD/ScoreBox"); //FIXME: this basically defeats the object of signaling up I think

			if (Map != null) Map.ConnectSaveSignal(this);

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
			Player.Connect(nameof(Player.BossMove), this, nameof(BossMove));
			Player.Connect(nameof(Player.UpdateTileInstruction), this, nameof(UpdateTileInstruction));

			LevelTitleScreen.Connect(nameof(LevelTitleScreen.TitleScreenLoaded), this, nameof(OnTitleScreenLoaded), new Godot.Collections.Array { false });
			LevelTitleScreen.Connect(nameof(LevelTitleScreen.StartMusic), this, nameof(PlayMusic));
			LevelTitleScreen.Connect(nameof(LevelTitleScreen.SelectLevel), this, nameof(NewLevel));
			LevelTitleScreen.Connect(nameof(LevelTitleScreen.Resume), this, nameof(Resume));
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
			NextLevel = LevelFactory.Load(Levels[iLevel], true);
			BuildLevel(false);
		}

		private void NewLevel(string levelName, bool replay = false)
		{
			NextLevel = LevelFactory.Load(levelName, true);
			
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
			HUD.HideHopCounter();
			HUD.HideScoreBox();
			HUD.OverlayMenu.ChangeMode(OverlayMenuMode.LevelTitle);
			LevelTitleScreen.SetPosition(Position);
			LevelTitleScreen.Init(StageData,
								  Levels.Length,
								  iLevel + 1,
								  level.LevelData.MaximumHops,
								  level.LevelData.ScoreRequired); //FIXME: Need to change iLevel to get its number from the Level itself
			LevelTitleScreen.AnimateShow();
			
			//FIXME: not sure why but this creates an error !is_inside_tree = true
			if (HUD.TouchButtons.Visible) HUD.TouchButtons.Visible = false; 
		}

		private void OnTitleScreenLoaded(bool replay = false)
		{
			UpdateLevelReached();
			MoveToTop(HUD);
			//if (!Paused) BuildLevel(replay);
		}

		private void BuildLevel(bool replay = false)
		{
			MoveToTop(HUD);
			if (!replay && NextLevel == null) NextLevel = CurrentLevel;

			AddChildBelowNode(Background, NextLevel);
			NextLevel.Connect(nameof(Level.LevelBuilt), HUD, nameof(HUD.SetMaxHops));
			NextLevel.Build(Resources);           
			Grid = NextLevel.Grid;
			
			Grid.Connect(nameof(Grid.TileSlidUp), this, nameof(CalculatePlayerMovementAfterBossMove));
			
			if (NextLevel.ScoreRequired > 0)
				HUD.ShowScoreBox(NextLevel.ScoreRequired);
			else
				HUD.HideScoreBox();
				
			HUD.SetButtonToRestart();
			HUD.UpdateMinScore(NextLevel.ScoreRequired, false);
			HUD.CountInActiveHops();

			LevelTitleScreen.LevelNameLabel.Text = $" {NextLevel.LevelName}";

			Player.Init(NextLevel, replay);

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

			File file = new File();
			if (file.FileExists(NextLevel.LevelData.ResourcePath.Replace("_Data","_BossData")))
			{
				Boss = (Boss)GD.Load<PackedScene>("res://Stage/Boss.tscn").Instance();
				Boss.Load(NextLevel.LevelName);
				AddChildBelowNode(Background, Boss);
				Player.BossMode = true;
				Grid.BossMode = true;      
				UpdateBossIndicators(NextLevel);          
				//ModulateBackgrounds();
			}

			UpdateGoalStateAndScore(NextLevel.ScoreRequired);
			if (CurrentLevel != null) CurrentLevel.QueueFree();
			CurrentLevel = NextLevel;
			NextLevel = null;
			
			UndoManager.Init(Player, CurrentLevel);
			Visible = true;
		}
/* 
		private void ModulateBackgrounds()
		{
		   var backgrounds = GetTree().GetNodesInGroup("ModulateForBoss");
		   foreach (TextureRect t in backgrounds)
		   {
				t.Modulate = Color.Color8(194, 149, 149, 0);
		   }
		} */

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
					//Level reached will be 1 higher than available levels when complete
					if (iLevel == StageData.LevelReached)
					{
						StageData.LevelReached++;
						if (StageData.LevelReached < Levels.Length)
						{
							LevelUnlocked(StageData.LevelReached);
						}
						EmitSignal(nameof(UpdateLocationProgress), StageData.LevelReached);
					}
					iLevel++;

					if (iLevel >= Levels.Length)
					{
						bool StageComplete = true;
						if (StageData.Pond == "Boss")
						{
							QuitToMenu();
							VictoryScreen VC = (VictoryScreen)GD.Load<PackedScene>("res://Menus/VictoryScreen.tscn").Instance();
							GetViewport().AddChild(VC);
						}
						else
						{
							QuitToMap(StageComplete); 
						}
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

		private void LevelUnlocked(int levelReached)
		{
			LevelTitleScreen.LevelUnlocked = levelReached;
		}

		private void UpdateLevelReached()
		{
			//TODO: Potentially add a flag for pause mode to prevent rebuilding the level
			LevelTitleScreen.UpdateLevelReached(StageData.LevelReached);
		}

		private void UpdateTileInstruction(Tile tile)
		{
			if (Boss != null)
			{
				Boss.UpdateInstruction(tile, CurrentLevel.MaximumHops - Player.HopsRemaining, false);
			}
		}

		private void BossMove()
		{
			if (Boss != null)
			{
				List<Tile> tiles = Boss.Move(CurrentLevel.MaximumHops - Player.HopsRemaining);

				if (tiles.Count == 0)
				{
					CalculatePlayerMovementAfterBossMove(new Vector2(-1, -1));
				}
				else
				{
					foreach (Tile t in tiles)
					{
						if (t.GridPosition == Player.GridPosition) Player.Shock();
					}
				}
			}
		}

		public void UpdateBossIndicators(Level level)
		{
			if (Boss != null)
			{
				Boss.UpdateIndicatorAnimations(level.MaximumHops - Player.HopsRemaining, "Highlight");
			}
		}

		private void CalculatePlayerMovementAfterBossMove(Vector2 TileGridPosition)
		{
			Tile tile = Grid.GetTile(TileGridPosition);
			if (tile != null)
			{
				if (tile.GridPosition == Player.GridPosition) // Player is on one of changed tiles
				{
					if (tile.Type == Type.Bounce || tile.Type == Type.Rock) 
					{
						Player.CalculateMovement(Player.PreviousAnimationNode.Movement, true);
					}
					else if (tile.Type == Type.Water)
					{
						Player.CalculateMovement(Player.PreviousAnimationNode.Movement, true);
					}
					else if (tile.Type == Type.Direct)
					{
						Player.CalculateMovement(tile.BounceDirection, true);
						tile.RotateBounceDirection();
						UpdateTileInstruction(tile);
						tile.RotateBounceDirectionVisual();
					}
					else if (tile.Type == Type.Score)
					{
						tile.Eat();
						Player.AfterAnimation(Player.PreviousAnimationNode.Animation.ResourceName, true);
					}
					else if (tile.Type == Type.Goal && tile.Activated)
					{
						Player.CalculateMovement(Player.PreviousAnimationNode.Movement, true);
					}
					else //Type.Jump || Type.Lily 
					{
						//nothing
					}
				}
			}
			Player.CheckHopsAndFinaliseAnimation(); //TODO: may be called multiple times wrongly but that might be ok
			UpdateBossIndicators(CurrentLevel);
			Player.Activate();
		}
		//Working with player

		private void NewPlayer()
		{
			Player = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instance();
			AddChildBelowNode(Background, Player);
		}


		public void GoalReached()
		{
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
				HUD.UpdateScore(level.ScoreRequired - updatedScore);
				
				if (Boss != null)
				{
					// This is only changing the eaten value of a score tile that has just appeared

					//Boss.UpdateInstruction(Grid.GetTile(Player.GridPosition), level.MaximumHops - Player.HopsRemaining, true);
				}

				if (updatedScore <= 0)
				{
					if (Boss != null) Boss.UpdateGoalInstructions(true);
					if (!level.GoalActive && updatedScore <= 0)
					{
						level.UpdateGoalState(true);
						
						if (level.MaximumHops - Player.HopsRemaining > 0)
						{
							GoalActivate.Play();
							HUD.AnimateScoreBox();
						}
					}
				}
				else if (level.GoalActive && updatedScore > 0)
				{
					level.UpdateGoalState(false);
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
			if (Boss!= null) Boss.QueueFree();
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
			if (HUD.OverlayMenu.RestartButton.IsConnected("pressed", Player, nameof(Player.RestartPressed)))
				HUD.OverlayMenu.RestartButton.Disconnect("pressed", Player, nameof(Player.RestartPressed));
			HUD.OverlayMenu.RestartButton.Connect("pressed", Player, nameof(Player.RestartPressed));

			if (HUD.OverlayMenu.QuitButton.IsConnected("pressed", this, nameof(QuitToMenu)))
				HUD.OverlayMenu.QuitButton.Disconnect("pressed", this, nameof(QuitToMenu));
			HUD.OverlayMenu.QuitButton.Connect("pressed", this, nameof(QuitToMenu));

			if (HUD.OverlayMenu.MapButton.IsConnected("pressed", this, nameof(QuitToMap)))
				HUD.OverlayMenu.MapButton.Disconnect("pressed", this, nameof(QuitToMap));
			HUD.OverlayMenu.MapButton.Connect("pressed", this, nameof(QuitToMap));

			if (HUD.OverlayMenu.LevelSelectButton.IsConnected("pressed", this, nameof(Pause)))
				HUD.OverlayMenu.LevelSelectButton.Disconnect("pressed", this, nameof(Pause));
			HUD.OverlayMenu.LevelSelectButton.Connect("pressed", this, nameof(Pause));
		}

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

		public void MovePlayerToTop()  { MoveChild(Player, GetChildCount()); }
		private void MovePlayerBehind() { MoveChild(Player, Background.GetPositionInParent()); }

		private void MoveToTop(Node node = null)
		{
			if (node == null) node = this;
			GetViewport().MoveChild(node, GetViewport().GetChildCount());
		}

		private void Resume()
		{
			HUD.OverlayMenu.ChangeMode(OverlayMenuMode.Stage);
			MoveToTop(HUD);
			if (Player.HopsRemaining == CurrentLevel.MaximumHops)
			{
				Player.Appear();
			} 
			else
			{
				Player.Activate();
			}
			Paused = false;
		}

		private void Pause()
		{
			Paused = true;
			
			if (!TempForTesting) InitialiseLevelTitle(CurrentLevel);
		}

		private void QuitToMap() { QuitToMap(false); }

		private void QuitToMap(bool StageComplete)
		{
			if (LevelTitleScreen.Visible == true) LevelTitleScreen.AnimateHide();

			Music.Stop();

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
				if (StageComplete)
				{
					EmitSignal(nameof(UnlockNextStage));
				} 
				else
				{
					Map.Pointer.SetProcessInput(true);
				}
			}
			else
			{
				HUD.Visible = false;
			}
		}

		public void QuitToMenu()
		{
			if (LevelTitleScreen.Visible == true) LevelTitleScreen.AnimateHide();

			Music.Stop();
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
