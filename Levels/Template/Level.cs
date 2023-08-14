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
        //public int StartingHops { get; set; }
        public int MaximumHops { get; set; }
        public int ScoreRequired { get; set; }
        public Vector2 PlayerStartPosition { get; set; }
        public string LevelName { get; set; }

        [Signal]
        public delegate void LevelParametersUpdated();

        [Signal]
        public delegate void LevelBuilt(int maxHops);

        public Level(){}

        public override void _Ready()
        {
            Name = "Level";
            Grid = GetNode<Grid>("Grid");
        }
        
        public void Build(ResourceRepository resources)
        {
            LevelName = LevelData.Name;
            //StartingHops = LevelData.StartingHops;
            MaximumHops = LevelData.MaximumHops;
            ScoreRequired = LevelData.ScoreRequired;
            PlayerStartPosition = LevelData.PlayerStartPosition + Vector2.One;
            Grid.Resources = resources;
            Grid.DefineGrid(LevelData.TileSize, LevelData.Width, LevelData.Height);
            Grid.ClearExistingChildren();
            Grid.PopulateGrid(LevelData);
            Grid.Connect(nameof(Grid.ConnectTileRightClick), this, nameof(ConnectTileRightClick));
            ConnectTiles();
            EmitSignal(nameof(LevelBuilt), MaximumHops);
        }

        private void ConnectTiles()
        {
            foreach (Tile t in Grid.Tiles)
            {
                Grid.ConnectTile(t);
            }
        }

        private void ConnectTileRightClick(Tile t)
        {
            t.Connect(nameof(Tile.PlayerStartUpdated), this, nameof(UpdatePlayerStart));
        }


        public void UpdateLevelData()
        {
            int i = 0;
            for (int y = 1; y < Grid.GridHeight - 1; y++)
            {
                for (int x = 1; x < Grid.GridWidth - 1; x++)
                {
                    LevelData.TileType[i] = Grid.Tiles[x, y].Type;
                    LevelData.TilePointValue[i] = Grid.Tiles[x, y].PointValue;
                    LevelData.TileBounceDirection[i] = Grid.Tiles[x, y].BounceDirection;
                    i++;
                }
            }
            LevelData.Name = LevelName;
            //LevelData.StartingHops = StartingHops;
            LevelData.MaximumHops = MaximumHops;
            LevelData.ScoreRequired = ScoreRequired;
            LevelData.PlayerStartPosition = PlayerStartPosition;
        }

        public bool UpdateGoalState(int currentScore, Tile activatedGoalTile)
        {
            Grid.ReplaceTile(Grid.GoalTile.GridPosition, activatedGoalTile);
            return true;
        }

        private void UpdatePlayerStart(Vector2 gridPosition)
        {
            PlayerStartPosition = gridPosition;
            EmitSignal(nameof(LevelParametersUpdated));
            //GD.Print("LevelParametersUpdated");
        }

    }
}