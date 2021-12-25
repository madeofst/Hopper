using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
	public class Menu : MarginContainer
	{
		public AudioStreamPlayer2D Music;
		public override void _Ready()
		{
			Music = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
		}

		public void newGamePressed()
		{
			World world = (World)GD.Load<PackedScene>("res://World/World.tscn").Instance();
			GetTree().Root.AddChild(world);
			Music.Stop();
			Hide();
			world.Init();
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
			Hide();
		}
	}
}
