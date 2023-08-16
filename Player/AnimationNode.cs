using System;
using Godot;

namespace Hopper
{
    public class AnimationNode
    {
        public Animation Animation;
        public Vector2 Movement;
        public Curve Curve;
        public Vector2 BounceVector;
        public bool Free;

        public AnimationNode(Animation Animation, Vector2 Movement, Curve Curve, bool Free)
        {
            this.Animation = Animation;
            this.Movement = Movement;
            this.Curve = Curve;
            this.Free = Free;
        }
    }
}