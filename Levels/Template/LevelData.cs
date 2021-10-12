using Godot;

public class LevelData : Resource
{
    [Export]
    public Type[] TileType; //TODO: make an array of objects with type and score value for each

    [Export]
    public int MaximumHops { get; set; }

    [Export]
    public int ScoreRequired { get; set; }

    [Export]
    public Vector2 PlayerStartPosition { get; set; }

}