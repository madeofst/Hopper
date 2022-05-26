using Godot;
using System;
using System.Threading;

namespace Hopper
{
    public static class Worlds
    {
        public static string[] World1Levels { get; set; } = new string[] 
        {
             //Basic (no special tiles)
                //Instructional
                "StartingOut",
                "SecondOfLy",
                "Up",
                //Challenge
                "PointsPointsPoints6",
                "ArtAndSoul2",
        };

        public static string[] World2Levels { get; set; } = new string[] 
        {
            //Jumping (jump tile only)
                //Instructional 
                "MovingOn",
                "MovingOn2",
                //Challenge
                "DoubleJump",
                "WeirdMirror1",
                "Jumpington",
        };

        public static string[] World3Levels { get; set; } = new string[] 
        {
            //Water (jump + water tile)
                //Instructional
                "WaterIsIt1",
                "WaterIsIt2",
                "WaterIsIt3",
                "WaterIsIt4",
                //Challenge
                "BlueLine",
                //TODO: need some easier ones
        };

        public static string[] World4Levels { get; set; } = new string[] 
        {
            //Combined challenge
                "Retrace",
                "DivingIn1",      
                "DivingIn1a",
                "DivingIn6",
                "DivingInEfficiently1",
                "SideToSide",
                "Mazemerize",
                "TheSquare",
                "PondInPond",
                "SideWind",     
                "MiniMaze",
                "GettingAbout9",
        };

    }
}