using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
	public class StartMenu : MarginContainer
	{
		public AudioStreamPlayer Music;
		private Tween Tween;
		private Button NewGameButton;
		public Button LoadButton;
		public Button EditorButton;
		private Map Map;
		private HUD HUD;
		private bool EditorMode;
		private Viewport MapViewport;
		private bool DEVMODE;

		public override void _Ready()
		{
			MapViewport = GetNode<Viewport>("/root/MapContainer/ViewportContainer/Viewport");
			Music = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
			Tween = GetNode<Tween>("Tween");
			NewGameButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/NewGameButton");
			LoadButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/LoadButton");
			EditorButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/EditorButton");
			DEVMODE = GetNode<ResourceRepository>("/root/ResourceRepository").DEVMODE;

			if (DEVMODE)
			{
				EditorButton.Show();
			}
			else
			{
				EditorButton.Hide();
			}
		}

		public void NewGamePressed()
		{
			NewGameButton.Disabled = true;
			LoadButton.Disabled = true;

			EditorMode = false;
			if (!Tween.IsActive()) FadeOut();

			if (DEVMODE)
			{
				LoadMap(ResourceLoader.Load<SaveData>("res://Saving/DevMode/SaveFile.tres"));
			}
			else
			{
				LoadMap(ResourceLoader.Load<SaveData>("res://Saving/DefaultSaveFile.tres"));
			}

			LoadHUD();
			HUD.OverlayMenu.ChangeMode(OverlayMenuMode.Map);

			Map.FadeIn();
		}

		public void LoadPressed()
		{
			NewGameButton.Disabled = true;
			LoadButton.Disabled = true;

			EditorMode = false;
			if (!Tween.IsActive()) FadeOut();

			LoadMap(ResourceLoader.Load<SaveData>("user://SaveFile.tres"));

			LoadHUD();
			HUD.OverlayMenu.ChangeMode(OverlayMenuMode.Map);

			Map.FadeIn();
		}

		public void UpdateLoadButton()
		{
			File SaveFile = new File();
			if (!SaveFile.FileExists("user://SaveFile.tres"))
			{
				LoadButton.Hide();
				NewGameButton.GrabFocus();
			}
			else
			{
				LoadButton.Show();
				LoadButton.GrabFocus();
			}
		}

		public void OptionsPressed()
		{
			OptionsMenu optionsMenu = (OptionsMenu)GD.Load<PackedScene>("res://Menus/OptionsMenu.tscn").Instance();
			GetViewport().AddChild(optionsMenu);
		}

		public void EditorPressed()
		{
			EditorMode = true;
			LevelEditor editor = (LevelEditor)GD.Load<PackedScene>("res://Levels/Editor/LevelEditor.tscn").Instance();
			LoadHUD();
			HUD.OverlayMenu.ChangeMode(OverlayMenuMode.Menu);
			GetViewport().AddChild(editor);
			Music.Stop();
			FadeOut();
		}

		public void QuitPressed()
		{
			GetTree().Quit();
		}

		private void FadeOut()
		{
			Tween.InterpolateProperty(this, "modulate", new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), 1f, Tween.TransitionType.Sine, Tween.EaseType.Out);
			Tween.Start();
		}

		internal void ShowMenu()
		{
			//HUD.OverlayMenu.ChangeMode(OverlayMenuMode.Menu);
			Modulate = new Color (1, 1, 1, 1);
			Show();

			NewGameButton.Disabled = false;
			LoadButton.Disabled = false;
		}

		public void AfterFade(object x, string key)
		{
			Music.Stop();
			if (!EditorMode)
			{
				HUD.UnlockPosition();
				HUD.Visible = true;
			}
			else
			{
				HUD.Visible = false;
			}
			//GetViewport().MoveChild(this, 2);
			Hide();
		}

		private void LoadHUD()
		{
			HUD = (HUD)GD.Load<PackedScene>("res://HUD/HUD.tscn").Instance();
			GetViewport().AddChild(HUD);
			HUD.HideHopCounter();
			HUD.HideScoreBox();
			HUD.SetButtonToEnter();
		}

		private void LoadMap(SaveData SaveData)
		{
			if (!EditorMode)
			{
				Map = (Map)GD.Load<PackedScene>("res://Map/Map.tscn").Instance();
				Map.Modulate = new Color(1, 1, 1, 0);
				MapViewport.AddChild(Map);
				Map.Init(SaveData);
			}
		}
	}
}
