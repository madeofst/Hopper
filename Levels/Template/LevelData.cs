using Godot;

namespace Hopper
{
    public class LevelData : Resource
    {
        [Export]
        public string Name { get; set; }
        
        [Export]
        public int Width { get; set; }

        [Export]
        public int Height { get; set; }

        [Export]
        public int TileSize { get; set; }

        [Export]
        public Type[] TileType;

        [Export]
        public int[] TilePointValue;

        [Export]
        public Vector2[] TileBounceDirection { get; set; }
/* 
        [Export]
        public int StartingHops { get; set; } */

        [Export]
        public int MaximumHops { get; set; }

        [Export]
        public int ScoreRequired { get; set; }

        [Export]
        public Vector2 PlayerStartPosition { get; set; }

        public LevelData(){}

        public void Init(int length)
        {
            TileType = new Type[length];
            TilePointValue = new int[length];
            TileBounceDirection = new Vector2[length];
        }

        public void UpdateTile(Tile tile)
        {
            int i = (int)((tile.GridPosition.y * Width) + tile.GridPosition.x); //TODO: make this a function
            TileType[i] = tile.Type;
            TilePointValue[i] = tile.PointValue;
            TileBounceDirection[i] = tile.BounceDirection;
        }

        public void BuildBounceDirectionArray()
        {
            TileBounceDirection = new Vector2[Width * Height];
            for (int i = 0; i < TileBounceDirection.Length; i++)
            {
                TileBounceDirection[i] = Vector2.Zero;
            }
        }
    }
}