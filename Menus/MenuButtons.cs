using Godot;
using System;

public class MenuButton : Button
{
    private RichTextLabel Label;

    public override void _Ready()
    {
        Label = GetNode<RichTextLabel>("Label");
    }

    public void ShowLabel()
    {
        Label.Show();
    }

    public void HideLabel()
    {
        Label.Hide();
    }

    public void UpdateLabel(string newText)
    {
        Label.BbcodeText = newText;
    }

    public void HighlightLabelText()
    {
        Label.Modulate = new Color(1, 1, 1, 1);
    }

    public void DehighlightLabelText()
    {
        Label.Modulate = new Color(0.14f, 0.2f, 0.16f, 1);
    }

}
