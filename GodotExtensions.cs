using System;
using Godot;

namespace GodotExtension
{
    public static class GodotExtensions
    {
        public static int PathLength(this Vector2 vec)
        {
            return (int)(vec.Abs().x + vec.Abs().y);
        }
    }
} 