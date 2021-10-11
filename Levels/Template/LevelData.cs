using Godot;

public class LevelData : Resource
{
    [Export]
    public Type[] TileType;
    
    [Export]
    public int Number { get; set; }

    [Export]
    public int MaximumHops { get; set; }

    [Export]
    public int ScoreRequired { get; set; }
}