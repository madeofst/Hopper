using Godot;
using System;

public class LevelSelectButton : TextureButton
{
    [Signal]
    public delegate void ChangeFocus(string name);
    [Signal]
    public delegate void AnimationComplete();
    [Signal]
    public delegate void LevelSelectionMade();

    public ShaderMaterial ThickShader {get; set; }
    public ShaderMaterial ThinShader {get; set; }
    private bool Animating;
    private readonly float Speed = 0.95f;
    private Sprite LevelSelector { get; set; }

    public override void _Ready()
    {
        ThickShader = GetNode<Sprite>("Branch1").Material as ShaderMaterial;
        ThinShader = GetNode<Sprite>("Branch2").Material as ShaderMaterial;
        LevelSelector = GetNode<Sprite>("LevelSelector");
    }

    public void GotFocus()
    {
        EmitSignal(nameof(ChangeFocus), Name);
        LevelSelector.Texture = GD.Load<Texture>("res://Menus/Resources/ExampleLeaf2.png");
    }

    public void LostFocus()
    {
        LevelSelector.Texture = GD.Load<Texture>("res://Menus/Resources/ExampleLeaf3.png");
    }

    public void AnimateBranch()
    {
        if (ThickShader != null && ThinShader != null)
        {
            ThickShader.SetShaderParam("fill", 0f);
            ThinShader.SetShaderParam("fill", -0f);
            Animating = true;
        }
    }

    public void SetAsAccessible(bool ShowBranches)
    {
        if (ShowBranches)
        {
            ThickShader.SetShaderParam("fill", 1f);
            ThinShader.SetShaderParam("fill", 1f);
        }

        Disabled = false;
        TextureNormal = GD.Load<Texture>("res://Menus/Resources/ExampleLeaf3.png");
        LevelSelector.Texture = GD.Load<Texture>("res://Menus/Resources/ExampleLeaf3.png");
    }

    public override void _PhysicsProcess(float delta)
    {
        if (ThickShader != null && ThinShader != null)
        {
            float thickFill = (float)ThickShader.GetShaderParam("fill");
            float thinFill = (float)ThinShader.GetShaderParam("fill");

            if (thickFill >= 1 && Animating)
            {
                Animating = false;
                Disabled = false;
                EmitSignal(nameof(AnimationComplete));
            }
            else if (thickFill >=0.75 && Animating)
            {
                TextureNormal = GD.Load<Texture>("res://Menus/Resources/ExampleLeaf2.png");
                LevelSelector.Texture = GD.Load<Texture>("res://Menus/Resources/ExampleLeaf2.png");
            }

            if (Animating)
            {
                ThickShader.SetShaderParam("fill", Mathf.Clamp(thickFill + delta * Speed, 0, 1));
                ThinShader.SetShaderParam("fill", Mathf.Clamp(thinFill + delta * Speed * 0.7f, 0, 1));
            }
        }
    }

    public void SelectionMade()
    {
        GD.Print("Level selected. " + Name);
        EmitSignal(nameof(LevelSelectionMade));
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") || 
            @event.IsActionPressed("ui_select"))
        {
            AcceptEvent();
        }
    }
}
