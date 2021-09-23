using Godot;
using System;

public class Counter : RichTextLabel
{
    public Counter(){}

    public Counter(Vector2 size)
    {
        RectSize = size;
    }

    public void UpdateText(string text)
    {
        Text = text;
    }

    public void UpdateText(int number)
    {
        Text = number.ToString();
    }

    public void UpdateText(float number)
    {
        Text = number.ToString();
    }
}