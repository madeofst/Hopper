using Godot;
using System;

namespace Hopper
{
    public class PondLinkPath : Path2D
    {
        [Export]
        public string[] Directions;

        [Export]
        private bool active;

        public bool Active 
        { 
            get => active; 
            set
            {
                active = value; 
                Visible = false;
                if (value == true) Visible = true;
            } 
        }

        public override void _Ready()
        {
            Visible = false;
        }

    }
}
