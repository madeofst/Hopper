using Godot;
using System;

public class ScoreLabel : RichTextLabel
{
    private milliTimer timer { get; set; }
    public string PlainText { get; set; }

    [Signal]
    public delegate void ScoreAnimationFinished();
    [Signal]
    public delegate void ScoreAnimationStarted();

    public void UpdateText(string plainText, bool postGoal)
    {
        PlainText = plainText;
        if (postGoal)
        {
            BbcodeText = $"[color=#4ab3ff]{plainText}[/color]";
        }
        else
        {
            BbcodeText = $"[color=#ffffff]{plainText}[/color]";
        }
    }

    public void Shake()
    {
        EmitSignal(nameof(ScoreAnimationStarted));
        BbcodeText = $"[shake rate=35 level=10][color=#ff0000]{PlainText}[/color][/shake]";
        timer = new milliTimer();
        timer.Start(0.4f);
        //GD.Print($"{this.Name} - {BbcodeText} - {timer.Remaining()}");
    }

    public void EndShake()
    {
        BbcodeText = $"[color=#4ab3ff]{PlainText}[/color]";
        EmitSignal(nameof(ScoreAnimationFinished));
    }

    public override void _PhysicsProcess(float delta)
    {
        if (timer != null) 
        {
            if (timer.Finished())
            {
                timer = null;
                EndShake();
                //GD.Print($"{this.Name} - {BbcodeText}");
            }
        }
    }
}
