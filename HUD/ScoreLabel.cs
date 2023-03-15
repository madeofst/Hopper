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
            BbcodeText = $"[color=#d9f5f1]{plainText}[/color]";
        }
        else
        {
            BbcodeText = $"[color=#d9f5f1]{plainText}[/color]";
        }
    }

    public void Shake()
    {
        EmitSignal(nameof(ScoreAnimationStarted));
        BbcodeText = $"[shake rate=35 level=10][color=#6c9331]{PlainText}[/color][/shake]";
        timer = new milliTimer();
        timer.Start(0.4f);
        //GD.Print($"{this.Name} - {BbcodeText} - {timer.Remaining()}");
    }

    public void EndShake()
    {
        BbcodeText = $"[color=#d9f5f1]{PlainText}[/color]";
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
