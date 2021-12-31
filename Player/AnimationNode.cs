using System;
using Godot;

namespace Hopper
{
    public class AnimationNode
    {
        public Animation Animation;
        public Vector2 Movement;

        public AnimationNode(Animation Animation, Vector2 Movement)
        {
            this.Animation = Animation;
            this.Movement = Movement;
        }
    }
}