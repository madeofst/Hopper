using Godot;
using System;
using System.Collections.Generic;

public class ScoreBox : Control
{
	private List<TextureRect> ScoreImages { get; set; }

	public override void _Ready()
	{
		ScoreImages = new List<TextureRect>();
	}

	public void Init(int score)
	{
		ScoreImages = new List<TextureRect>();

		foreach (TextureRect tr in GetTree().GetNodesInGroup("BugTextures"))
		{
			tr.Texture = GD.Load<Texture>("res://HUD/Resources/ScoreBox.png");
			tr.Visible = false;
		}

		for (int i = 1; i <= score; i++)
		{
			TextureRect tr = GetNode<TextureRect>($"HBoxContainer/Bug{i}");
			tr.Visible = true;
			ScoreImages.Add(tr);
		}
	}

	public void UpdatePlayerScore(int levelScore)
	{
		for (int i = 0; i < ScoreImages.Count; i++)
		{
			if (levelScore > i)
			{
				ScoreImages[i].Texture = GD.Load<Texture>("res://HUD/Resources/ScoreBoxTicked.png");
			}
			else
			{
				ScoreImages[i].Texture = GD.Load<Texture>("res://HUD/Resources/ScoreBox.png");
			}
		}
	}

	internal void Animate()
	{
		//ScoreImages.Shake();
		//BugsRemaining.Shake();
	}
}
