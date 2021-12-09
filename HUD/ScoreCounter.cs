using Godot;
using System;

namespace Hopper
{
    public class ScoreCounter : Counter
    {
        public ScoreCounter(){}

        public ScoreCounter(Vector2 size) : base(size)
        {
        }

        public override void MakeConnections()
        {
            //Player.Connect(nameof(Player.ScoreUpdated), this, "UpdateText");
        }

    }
}