using Godot;
using System;

namespace Hopper
{
    public class PondLinkPath : Path2D
    {
        [Export]
        public string[] Directions;

        [Export]
        private bool active = false;

        [Signal]
        public delegate void PathAnimated();

        public bool Active 
        { 
            get => active; 
            set
            {
                active = value;
                if (Shader != null) Shader.SetShaderParam("fill", 1f);
                Visible = true;
            } 
        }

        private ShaderMaterial Shader;
        private bool Animating = false;
        private float FillDirection;
        private float Speed;
        private Sprite Sprite;

        public override void _Ready()
        {
            Visible = false;
            Shader = (ShaderMaterial)Material;
            if (Shader != null) Shader.SetShaderParam("fill", 0f);

            if (GetChildCount() > 0)
            {
                Sprite = GetNode<Sprite>("Sprite");
                if (Sprite != null)
                {
                    Vector2 TextureSize = Sprite.Texture.GetSize();
                    Speed = 60 / Mathf.Max(TextureSize.x, TextureSize.y);
                }
            }
        }

        internal void AnimateReveal()
        {
            if (Shader != null)
            {
                Shader.SetShaderParam("fill", 0f);
                FillDirection = 1;
                Animating = true;
            }
            else
            {
                Active = true;
            }
            Visible = true;
        }

        public override void _PhysicsProcess(float delta)
        {
            if (Shader != null)
            {
                float fill = (float)Shader.GetShaderParam("fill");

                if (FillDirection == 1 && fill >= 1 && Animating)
                {
                    Animating = false;
                    active = true;
                    EmitSignal(nameof(PathAnimated), new Godot.Collections.Array{Name});
                }

                if (Animating)
                {
                    Shader.SetShaderParam("fill", Mathf.Clamp(fill + delta * Speed * FillDirection, 0, 1));
                }
            }
        }
    }
}
