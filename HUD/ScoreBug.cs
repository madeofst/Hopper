using Godot;
using System;

public class ScoreBug : TextureRect
{
    public TextureRect BugTexture;
    public Tween Tween;
    private CPUParticles2D CPUParticles2D;

    private bool _ticked;
    public bool Ticked
    {
        get { return _ticked; }
        set 
        {
            _ticked = value; 
            if (value)
            {
                BugTexture.Texture = GD.Load<Texture>("res://HUD/Resources/ScoreBoxTicked.png");
            }
            else
            {
                BugTexture.Texture = GD.Load<Texture>("res://HUD/Resources/ScoreBox.png");
            }
        }
    }

    public override void _Ready()
    {
        BugTexture = GetNode<TextureRect>("BugTexture");
        Tween = GetNode<Tween>("Tween");
        CPUParticles2D = GetNode<CPUParticles2D>("CPUParticles2D");
    }

    public void Pop()
    {
        Ticked = true;
        Tween.InterpolateProperty(BugTexture,
                                "rect_scale", 
                                new Vector2(0.5f, 0.5f), 
                                Vector2.One, 
                                0.2f, 
                                Tween.TransitionType.Back, 
                                Tween.EaseType.Out);
        CPUParticles2D.Emitting = true;
		Tween.Start();

    }
}
