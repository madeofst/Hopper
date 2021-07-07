using System;
using Godot;
using GodotExtension;    

public class Grid : Node2D
{
    public Player Player { get; set; }

    public int GridWidth;
    public int GridHeight;
    public Tile[,] Tiles;
    public Vector2 Size;
    public Vector2 Offset;
    public Vector2 TileSize;
    private Vector2 GoalTileGridPosition;
    private Tile GoalTile;

    private RandomNumberGenerator rand = new RandomNumberGenerator();

    public Grid(){}

    public Grid(int x, Vector2 viewportSize)
    {
        Name = "Grid";
        GridWidth = x;
        GridHeight = x;
        Tiles = new Tile[GridWidth, GridHeight];

        Offset = viewportSize/9;            //this is a constant
        Size = viewportSize - (Offset*2);   //448
        TileSize = new Vector2(Size.x/GridWidth, Size.y/GridHeight);
    }

    public Tile Tile(Vector2 position)
    {
        return Tiles[(int)position.x, (int)position.y];
    }

    public override void _Ready()
    {
        rand.Randomize();
    }

    public void SetupGrid()
    {
        Player = GetNode<Player>("../Player");
        for (int i = 0; i < GridWidth; i++)
        {
            for (int j = 0; j < GridHeight; j++)
            {
                Tiles[i, j] = new Tile(); 
                AddChild(Tiles[i,j]);
                Tiles[i, j].BuildTile(Type.Blank, TileSize, Offset, new Vector2(i, j));
            }
        }
        UpdateGrid();
    }

    public void UpdateGrid()
    {
        //Clear existing tiles
        AssignTileTypes();
    }

    private void AssignTileTypes()
    {
        ClearTypes();
        AssignGoalTile(3, 2);
        AssignScoreTiles(1);
    }

    private void ClearTypes()
    {
        foreach (Tile t in Tiles)
        {
            t.Type = Type.Blank;
        }
    }

    private void AssignGoalTile(int maxStepsFromPlayer, int minStepsFromPlayer = 1)
    {
        int possibleSteps = rand.RandiRange(minStepsFromPlayer, maxStepsFromPlayer);
        int x = rand.RandiRange(0, possibleSteps);
        Vector2 relativeGridPosition = new Vector2(x, possibleSteps - x);
        Vector2 absoluteGridPosition;
        do
        {
            for (int i = 0; i <= rand.RandiRange(0,3); i++)
            {
                float tempY = relativeGridPosition.y;
                relativeGridPosition.y = relativeGridPosition.x * - 1;
                relativeGridPosition.x = tempY;
            }
            absoluteGridPosition = relativeGridPosition + Player.GridPosition;
        } while (absoluteGridPosition.x < 0 || absoluteGridPosition.x >= GridWidth ||
                 absoluteGridPosition.y < 0 || absoluteGridPosition.y >= GridHeight );

        GoalTileGridPosition = absoluteGridPosition;
        GoalTile = Tiles[(int)absoluteGridPosition.x, (int)absoluteGridPosition.y];
        GoalTile.Type = Type.Goal;
    }

    private void AssignScoreTiles(int Count)
    {
        Vector2 ScoreToGoal;
        Vector2 ScoreGridPosition;
        for (int i = 1; i <= Count; i++)
        {
            float totalSteps;
            Vector2 PlayerToScore;
            do
            {
                int possibleSteps = rand.RandiRange(-Player.MaxHops, Player.MaxHops);
                int x = rand.RandiRange(-possibleSteps, possibleSteps);
                PlayerToScore = new Vector2(x, possibleSteps - x);
                ScoreGridPosition = PlayerToScore + Player.GridPosition;
                ScoreToGoal = GoalTile.GridPosition - ScoreGridPosition;;
                totalSteps = PlayerToScore.PathLength() + ScoreToGoal.PathLength();
            } 
            while (ScoreGridPosition == Player.GridPosition ||
                   ScoreGridPosition == GoalTile.GridPosition ||
                   totalSteps >= Player.MaxHops || 
                   totalSteps <= 0 ||
                   ScoreGridPosition.x < 0 || 
                   ScoreGridPosition.x >= GridWidth ||
                   ScoreGridPosition.y < 0 || 
                   ScoreGridPosition.y >= GridHeight);
            
            Tile(ScoreGridPosition).Type = Type.Score;
            Tile(ScoreGridPosition).PointValue *= (int)totalSteps;
            Tile(ScoreGridPosition).Label.UpdateText(Tile(ScoreGridPosition).PointValue);
        }
    }
}