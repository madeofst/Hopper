using Godot;
using System;

public class ScoreLabel : RichTextLabel
{
    private milliTimer timer { get; set; }
    private string PlainText { get; set; }

    public void Shake()
    {
        PlainText = BbcodeText;
        BbcodeText = $"[shake rate=35 level=10][color=#ff0000]{PlainText}[/color][/shake]";
        timer = new milliTimer();
        timer.Start(0.5f);
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
        
    }
}
