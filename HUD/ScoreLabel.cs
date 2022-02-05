using Godot;
using System;

public class ScoreLabel : RichTextLabel
{
    private milliTimer timer { get; set; }
    private string PlainText { get; set; }

    [Signal]
    public delegate void ScoreAnimationFinished();
    [Signal]
    public delegate void ScoreAnimationStarted();

    public void Shake()
    {
        EmitSignal(nameof(ScoreAnimationStarted));
        PlainText = BbcodeText;
        BbcodeText = $"[shake rate=35 level=10][color=#ff0000]{PlainText}[/color][/shake]";
        timer = new milliTimer();
        timer.Start(0.4f);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (timer != null) 
        {
            if (timer.Finished())
            {
                timer = null;
                EndShake();
            }
        }
    }

    public void EndShake()
    {
        BbcodeText = $"[color=#4ab3ff]{PlainText}[/color]";
        EmitSignal(nameof(ScoreAnimationFinished));
    }
}
