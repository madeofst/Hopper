using System;
using Godot;

namespace Hopper
{
    public class Level : Node2D
    {
        public LevelData LevelData { get; set; }
        public Grid Grid { get; set; }

        private bool editable = false;
        public bool Editable 
        { 
            get => editable; 
            set
            {
                editable = value;
                Grid.Editable = value;
            } 
        }

        //New params which relate to save file
        public int StartingHops { get; set; }
        public int MaximumHops { get; set; }
        public int ScoreRequired { get; set; }
        public Vector2 PlayerStartPosition { get; set; }
        public string LevelName { get; set; }

        public Level(){}

        public override void _Ready()
        {
            Name = "Level";
            Grid = GetNode<Grid>("Grid");
        }
        
        public void Build(ResourceRepository resources)
        {
            LevelName = LevelData.Name;
            StartingHops = LevelData.StartingHops;
            MaximumHops = LevelData.MaximumHops;
            ScoreRequired = LevelData.ScoreRequired;
            PlayerStartPosition = LevelData.PlayerStartPosition;
            Grid.Resources = resources;
            Grid.DefineGrid(LevelData.TileSize, LevelData.Width, LevelData.Height);
            Grid.ClearExistingChildren();
            Grid.PopulateGrid(LevelData);
        }

        public void UpdateLevelData()
        {
            int i = 0;
            for (int y = 0; y < Grid.GridHeight; y++)
            {
                for (int x = 0; x < Grid.GridWidth; x++)
                {
                    LevelData.TileType[i] = Grid.Tiles[x, y].Type;
                    LevelData.TilePointValue[i] = Grid.Tiles[x, y].PointValue;
                    i++;
                }
            }
            LevelData.Name = LevelName;
            LevelData.StartingHops = StartingHops;
            LevelData.MaximumHops = MaximumHops;
            LevelData.ScoreRequired = ScoreRequired;
            LevelData.PlayerStartPosition = PlayerStartPosition;
        }

        public void UpdateGoalState(int currentScore, Tile newTile)
        {
            if (currentScore >= ScoreRequired && !Grid.GoalTile.Activated)
            {
                Grid.ReplaceTile(Grid.GoalTile.GridPosition, newTile);
            }
            else
            {
                newTile.QueueFree();
            }
        }
    }
}