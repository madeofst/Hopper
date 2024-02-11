using System;
using Godot;
using System.Collections.Generic;

namespace Hopper
{
    public class GridState
    {
        public TileChangeInstruction[,] TileChangeInstructions;

        public GridState(int gridWidth, int gridHeight, Tile[,] tiles)
        {
            TileChangeInstructions = new TileChangeInstruction[gridWidth, gridHeight];

            for (int y = 1; y < gridHeight - 1; y++)
            {
                for (int x = 1; x < gridWidth - 1; x++)
                {
                    TileChangeInstructions[x, y] = new TileChangeInstruction(-1, tiles[x, y]);  //not setting action on turn as not relevant here
                }
            }
        }
    }
}