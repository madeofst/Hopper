using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
	public class StartMenu : MarginContainer
	{
		public AudioStreamPlayer2D Music;
		private Tween Tween;
		private TextureButton NewGameButton;
		private Map Map;
		private HUD HUD;
		private bool EditorMode;

		public override void _Ready()
		{
			Music = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
			Tween = GetNode<Tween>("Tween");
			NewGameButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/CenterContainer3/NewGameButton");
			NewGameButton.GrabFocus();
		}

		public void newGamePressed()
		{
			EditorMode = false;
			FadeOut();
		}

		public void highScoresPressed()
		{
			HighScoreTable table = (HighScoreTable)GD.Load<PackedScene>("res://HighScores/HighScoreTable.tscn").Instance();
			GetTree().Root.AddChild(table);
			Music.Stop();
			Hide();
		}

		public void EditorPressed()
		{
			EditorMode = true;
			LevelEditor editor = (LevelEditor)GD.Load<PackedScene>("res://Levels/Editor/LevelEditor.tscn").Instance();
			GetTree().Root.AddChild(editor);
			Music.Stop();
			FadeOut();
		}

		private void FadeOut()
		{
			Tween.InterpolateProperty(this, "modulate", new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In);
			Tween.Start();
		}

        internal void ShowMenu()
        {
			Modulate = new Color (1, 1, 1, 1);
            Show();
        }

		public void AfterFade(object x, string key)
		{
			if (!EditorMode)
			{
				Map = (Map)GD.Load<PackedScene>("res://Map/Map.tscn").Instance();
				Map.Modulate = new Color(1, 1, 1, 0);
				GetTree().Root.AddChildBelowNode(GetNode<LoadingScreen>("/root/LoadingScreen"), Map);
			}

			HUD = (HUD)GD.Load<PackedScene>("res://HUD/HUD.tscn").Instance();
			GetTree().Root.AddChild(HUD);
			HUD.HideHopCounter();
			HUD.HideScoreBox();

			Music.Stop();
			if (!EditorMode)
			{
				HUD.UnlockPosition();
				HUD.Visible = true;
				Map.FadeIn();
			}
			else
			{
				HUD.Visible = false;
			}
			GetTree().Root.MoveChild(this, 2);
			Hide();
		}
    }
}
