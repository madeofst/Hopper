using System;
using Godot;

namespace Hopper
{
    public class AnimationNode
    {
        public Animation Animation;
        public Vector2 Movement;
        public Curve Curve;

        public AnimationNode(Animation Animation, Vector2 Movement, Curve Curve)
        {
            this.Animation = Animation;
            this.Movement = Movement;
            this.Curve = Curve;
        }
    }
}