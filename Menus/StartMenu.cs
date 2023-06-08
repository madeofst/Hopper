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
		public TextureButton LoadButton;
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
			NewGameButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/CenterContainer3/NewGameButton");
			LoadButton = GetNode<TextureButton>("MarginContainer/VBoxContainer/HBoxContainer/CenterContainer4/LoadButton");
			EditorButton = GetNode<Button>("MarginContainer/VBoxContainer/MarginContainer/Button");
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

		public void newGamePressed()
		{
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
			if (!EditorMode) Map.FadeIn(); //TODO: might be able to remove this editor check?
			if (EditorMode) GD.Print("This should never happen I don't think.");
		}

		public void LoadPressed()
		{
			EditorMode = false;
			if (!Tween.IsActive()) FadeOut();
			LoadMap(ResourceLoader.Load<SaveData>("user://SaveFile.tres"));
            LoadHUD();
			if (!EditorMode) Map.FadeIn(); //TODO: might be able to remove this editor check?
			if (EditorMode) GD.Print("This should never happen I don't think.");
		}

        public void UpdateLoadButton()
        {
            File SaveFile = new File();
            if (!SaveFile.FileExists("user://SaveFile.tres"))
            {
                LoadButton.GetParent<Control>().Hide();
            }
			else
			{
				LoadButton.GetParent<Control>().Show();
			}
        }

		public void EditorPressed()
		{
			EditorMode = true;
			LevelEditor editor = (LevelEditor)GD.Load<PackedScene>("res://Levels/Editor/LevelEditor.tscn").Instance();
			LoadHUD();
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
