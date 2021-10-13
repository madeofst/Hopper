using Godot;

public class LevelData : Resource
{
    [Export]
    public Type[] TileType;

    [Export]
    public int[] Score;

    [Export]
    public int MaximumHops { get; set; }

    [Export]
    public int ScoreRequired { get; set; }

    [Export]
    public Vector2 PlayerStartPosition { get; set; } = new Vector2(0, 0);

    public LevelData(){}

    public void Init(int length)
    {
        TileType = new Type[length];
        Score = new int[length];
    }
}