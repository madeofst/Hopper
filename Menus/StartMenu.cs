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

		public override void _Ready()
		{
			Music = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
			Tween = GetNode<Tween>("Tween");
			NewGameButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/CenterContainer3/NewGameButton");
			NewGameButton.GrabFocus();
		}

		public void newGamePressed()
		{
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
			Map = (Map)GD.Load<PackedScene>("res://Map/Map.tscn").Instance();
			Map.Modulate = new Color(1, 1, 1, 0);
			GetTree().Root.AddChildBelowNode(GetNode<LoadingScreen>("/root/LoadingScreen"), Map);
			Music.Stop();
			Map.FadeIn();
			GetTree().Root.MoveChild(this, 2);
			Hide();
		}
    }
}
