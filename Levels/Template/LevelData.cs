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
        public int StartingHops { get; set; }

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
        }

        public void UpdateTile(Tile tile)
        {
            int i = (int)((tile.GridPosition.y * Width) + tile.GridPosition.x);
            TileType[i] = tile.Type;
            TilePointValue[i] = tile.PointValue;
        }
    }
}