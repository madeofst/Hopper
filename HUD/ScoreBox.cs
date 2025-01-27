using Godot;
using System;
using System.Collections.Generic;

public class ScoreBox : Control
{
	private List<ScoreBug> ScoreBugs { get; set; }

	public override void _Ready()
	{
		ScoreBugs = new List<ScoreBug>();
	}

	public void Init(int score)
	{
		ScoreBugs = new List<ScoreBug>();

		foreach (ScoreBug scoreBug in GetTree().GetNodesInGroup("ScoreBugs"))
		{
			scoreBug.Texture = GD.Load<Texture>("res://HUD/Resources/ScoreBox.png");
			scoreBug.Visible = false;
		}

		for (int i = 1; i <= score; i++)
		{
			ScoreBug scoreBug = GetNode<ScoreBug>($"Bugs/Bug{i}");
			scoreBug.Visible = true;
			ScoreBugs.Add(scoreBug);
		}
	}

	public void UpdatePlayerScore(int levelScore)
	{
		for (int i = 0; i < ScoreBugs.Count; i++)
		{
			if (levelScore > i)
			{
				if (i + 1 == levelScore && !ScoreBugs[i].Ticked)
				{
					ScoreBugs[i].Pop();
				}
			}
			else
			{
				ScoreBugs[i].Ticked = false;
			}
		}
	}

	internal void Animate()
	{
		//ScoreImages.Shake();
		//BugsRemaining.Shake();
	}
}
