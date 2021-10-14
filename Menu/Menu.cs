using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
	public class Menu : MarginContainer
	{
		public override void _Ready()
		{
		}

		public void newGamePressed()
		{
			GD.Print("New game button pressed.");
			World world = (World)GD.Load<PackedScene>("res://World/World.tscn").Instance();
			GetTree().Root.AddChild(world);
			Hide();
		}

		public void highScoresPressed()
		{
			GD.Print("High scores button pressed.");
			HighScoreTable table = (HighScoreTable)GD.Load<PackedScene>("res://HighScores/HighScoreTable.tscn").Instance();
			GetTree().Root.AddChild(table);
			Hide();
		}

		public void EditorPressed()
		{
			LevelEditor editor = (LevelEditor)GD.Load<PackedScene>("res://Levels/Editor/LevelEditor.tscn").Instance();
			GetTree().Root.AddChild(editor);
			Hide();
		}
	}
}