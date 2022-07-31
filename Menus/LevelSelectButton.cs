using Godot;
using System;

public class LevelSelectButton : TextureButton
{
    [Signal]
    public delegate void ChangeFocus(string name);

    public void GotFocus()
    {
        EmitSignal(nameof(ChangeFocus), Name);
    }
}
