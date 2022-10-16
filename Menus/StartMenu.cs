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
		private TextureButton NewGameButton;
		private Map Map;
		private HUD HUD;
		private bool EditorMode;

		public override void _Ready()
		{
			Music = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
			Tween = GetNode<Tween>("Tween");
			NewGameButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/CenterContainer3/NewGameButton");
		}


		public void newGamePressed()
		{
			EditorMode = false;
			if (!Tween.IsActive()) FadeOut();
		}

		public void highScoresPressed()
		{
			HighScoreTable table = (HighScoreTable)GD.Load<PackedScene>("res://HighScores/HighScoreTable.tscn").Instance();
			GetViewport().AddChild(table);
			Music.Stop();
			Hide();
		}

		public void EditorPressed()
		{
			EditorMode = true;
			LevelEditor editor = (LevelEditor)GD.Load<PackedScene>("res://Levels/Editor/LevelEditor.tscn").Instance();
			GetViewport().AddChild(editor);
			Music.Stop();
			FadeOut();
		}

		private void FadeOut()
		{
			Tween.InterpolateProperty(this, "modulate", new Color(1, 1, 1, 1), new Color(1, 1, 1, 0), 1f, Tween.TransitionType.Sine, Tween.EaseType.Out);
			Tween.Start();
		}

        internal void ShowMenu()
        {
			Modulate = new Color (1, 1, 1, 1);
			NewGameButton.GrabFocus();
            Show();
        }

		public void AfterFade(object x, string key)
		{
			if (!EditorMode)
			{
				Map = (Map)GD.Load<PackedScene>("res://Map/Map.tscn").Instance();
				Map.Modulate = new Color(1, 1, 1, 0);
				GetViewport().AddChildBelowNode(GetNode<LoadingScreen>("/root/GameContainer/ViewportContainer/Viewport/LoadingScreen"), Map);
			}

			HUD = (HUD)GD.Load<PackedScene>("res://HUD/HUD.tscn").Instance();
			GetViewport().AddChild(HUD);
			HUD.HideHopCounter();
			HUD.HideScoreBox();
			HUD.SetButtonToEnter();

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
			GetViewport().MoveChild(this, 2);
			Hide();
		}
    }
}
