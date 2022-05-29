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
		public override void _Ready()
		{
			Music = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
			Tween = GetNode<Tween>("Tween");
		}

		public void newGamePressed()
		{
			Map Map = (Map)GD.Load<PackedScene>("res://Map/Map.tscn").Instance();
			GetTree().Root.AddChildBelowNode(GetNode<LoadingScreen>("/root/LoadingScreen"), Map);
			Music.Stop();
			Fade();
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
			Fade();
		}

		private void Fade()
		{
			Tween.InterpolateProperty(this, "modulate", new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), 0.5f, Tween.TransitionType.Sine, Tween.EaseType.In);
			Tween.Start();
		}

		public void Test(object Object, string nodePath)
		{
			//move behind and hide
			GetTree().Root.MoveChild(this, 2);
			Hide();
			//GD.Print("Test.");
		}

        internal void ShowMenu()
        {
			Modulate = new Color (1, 1, 1, 1);
            Show();
        }
    }
}
