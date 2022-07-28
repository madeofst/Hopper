using Godot;
using System;

namespace Hopper
{
    public class PondLinkPath : Path2D
    {
        [Export]
        public string[] Directions;

        [Export]
        public bool Active;
/* 
        [Export]
        public string Start;

        [Export]
        public string End; */
    }
}
