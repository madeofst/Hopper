using Godot;
using System;

namespace Hopper
{
    public class HopCounterLabel : Counter
    {
        public HopCounterLabel(){}
        
        public HopCounterLabel(Vector2 size) : base(size)
        {
        }

        public override void MakeConnections()
        {
            //Player.Connect(nameof(Player.HopCompleted), this, "UpdateText");
        }

    }
}