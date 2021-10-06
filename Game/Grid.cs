using System;
using System.Collections.Generic;
using Godot;
using GodotExtension;    

public class Grid : Control
{
    public Player Player { get; set; }

    public int GridWidth;
    public int GridHeight;
    public int GoalCount;
    
    private Vector2 ViewportSize;
    public Vector2 Offset;
    public Vector2 Size;
    public Vector2 TileSize;

    private Tile[,] Tiles;
    public RandomNumberGenerator rand = new RandomNumberGenerator();

    private Tile GoalTile;
    private Level currentLevel;
    public Level CurrentLevel 
    {
        get { return currentLevel; }
        set
        {
            currentLevel = value;
            int previousGridWidth = GridWidth;
            DefineGridParameters();
            if (Player != null)
            {
                SetupGrid();
                if (GridWidth != previousGridWidth) Player.GridPosition = new Vector2(0, 0);
            }
        }
    }

    [Signal]
    public delegate void NextLevel();

    public Grid(){}

    public Grid(Level level)
    {
        Name = "Grid";
        CurrentLevel = level;
    }

    public Tile Tile(Vector2 position)
    {
        return Tiles[(int)position.x, (int)position.y];
    }

    public override void _Ready()
    {
        rand.Randomize();
    }

    private void DefineGridParameters()
    {
        TileSize = new Vector2(32, 32);
        RectSize = TileSize * CurrentLevel.GridSize;
        SetPosition(new Vector2(
            68 + ((7 - CurrentLevel.GridSize) * TileSize.x)/2, 
            8 + ((7 - CurrentLevel.GridSize) * TileSize.y)/2
        ));

        GridWidth = GridHeight = CurrentLevel.GridSize;
        Tiles = new Tile[GridWidth, GridHeight];
    }

    public void InitializeGrid()
    {
        Player = GetNode<Player>("../Player");
        Player.Connect(nameof(Player.GoalReached), this, "IncrementGoalCount");
        SetupGrid();
    }

    private void SetupGrid()
    {
        InitializeTileArray();
        UpdateGrid();
    }

    private void InitializeTileArray()
    {
        ClearExistingChildren();
        for (int i = 0; i < GridWidth; i++)
        {
            for (int j = 0; j < GridHeight; j++)
            {
                Tiles[i, j] = new Tile();
                AddChild(Tiles[i, j]);
                Tiles[i, j].BuildTile(Type.Blank, TileSize, new Vector2(i, j));
            }
        }
    }

    private void ClearExistingChildren()
    {
        Godot.Collections.Array children = GetChildren();
        foreach (Node2D child in children)
        {
            RemoveChild(child);
            child.QueueFree();
        }
    }

    public void UpdateGrid()
    {
        AssignTileTypes();
    }

    private void AssignTileTypes()
    {
        ClearTypes();
        AssignGoalTile(CurrentLevel.HopsToAdd, 2);
        AssignScoreTiles(CurrentLevel.ScoreTileCount);
        AssignJumpTile(CurrentLevel.HopsToAdd, 2);
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
        int TopMax = (int)Math.Max(Player.GridPosition.x, Player.GridPosition.y);
        int BottomMax = (int)Math.Max(GridWidth - 1 - Player.GridPosition.x, GridHeight - 1 - Player.GridPosition.y);
        int OverallMax = Math.Max(TopMax, BottomMax);
        int limitedMaxStepsFromPlayer = maxStepsFromPlayer;
        if (OverallMax < limitedMaxStepsFromPlayer) limitedMaxStepsFromPlayer = OverallMax;

        int possibleSteps = rand.RandiRange(minStepsFromPlayer, limitedMaxStepsFromPlayer);
        int x = rand.RandiRange(0, possibleSteps);
        Vector2 relativeGridPosition = new Vector2(x, possibleSteps - x);
        Vector2 absoluteGridPosition;

        int limitCounter = 0;
        do
        {
            int limit = rand.RandiRange(0, 3);
            for (int i = 0; i <= limit; i++)
            {
                float tempY = relativeGridPosition.y;
                relativeGridPosition.y = relativeGridPosition.x * - 1;
                relativeGridPosition.x = tempY;
            }
            absoluteGridPosition = relativeGridPosition + Player.GridPosition;
            limitCounter++;

        } while ((absoluteGridPosition.x < 0 || absoluteGridPosition.x >= GridWidth ||
                 absoluteGridPosition.y < 0 || absoluteGridPosition.y >= GridHeight)
                 &&
                 (limitCounter < 100));

        GD.Print($"Limit: {limitCounter}"); //TODO: ERROR CHECK HERE

        GoalTile = Tiles[(int)absoluteGridPosition.x, (int)absoluteGridPosition.y];
        GoalTile.Type = Type.Goal;
    }

    private void AssignJumpTile(int maxStepsFromPlayer, int minStepsFromPlayer = 1)
    {
        if (rand.RandiRange(1, 3) == 3)
        {
        int TopMax = (int)Math.Max(Player.GridPosition.x, Player.GridPosition.y);
        int BottomMax = (int)Math.Max(GridWidth - 1 - Player.GridPosition.x, GridHeight - 1 - Player.GridPosition.y);
        int OverallMax = Math.Max(TopMax, BottomMax);
        int limitedMaxStepsFromPlayer = maxStepsFromPlayer;
        if (OverallMax < limitedMaxStepsFromPlayer) limitedMaxStepsFromPlayer = OverallMax;

        int possibleSteps = rand.RandiRange(minStepsFromPlayer, limitedMaxStepsFromPlayer);
        int x = rand.RandiRange(0, possibleSteps);
        Vector2 relativeGridPosition = new Vector2(x, possibleSteps - x);
        Vector2 absoluteGridPosition;

        int limitCounter = 0;
        do
        {
            int limit = rand.RandiRange(0, 3);
            for (int i = 0; i <= limit; i++)
            {
                float tempY = relativeGridPosition.y;
                relativeGridPosition.y = relativeGridPosition.x * - 1;
                relativeGridPosition.x = tempY;
            }
            absoluteGridPosition = relativeGridPosition + Player.GridPosition;
            limitCounter++;

        } while ((absoluteGridPosition.x < 0 || 
                  absoluteGridPosition.x >= GridWidth ||
                  absoluteGridPosition.y < 0 || 
                  absoluteGridPosition.y >= GridHeight ||
                  Tile(absoluteGridPosition).Type != Type.Blank)
                  && (limitCounter < 100)
                  );
        if (limitCounter < 100) Tile(absoluteGridPosition).Type = Type.Jump;
        }
        //GD.Print($"Limit: {limitCounter}"); //TODO: ERROR CHECK HERE
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
                int possibleSteps = rand.RandiRange(-CurrentLevel.MaxHops, CurrentLevel.MaxHops);
                int x = rand.RandiRange(-possibleSteps, possibleSteps);
                PlayerToScore = new Vector2(x, possibleSteps - x);
                ScoreGridPosition = PlayerToScore + Player.GridPosition;
                ScoreToGoal = GoalTile.GridPosition - ScoreGridPosition;;
                totalSteps = PlayerToScore.PathLength() + ScoreToGoal.PathLength();
            } 
            while (ScoreGridPosition == Player.GridPosition ||
                   ScoreGridPosition == GoalTile.GridPosition ||
                   totalSteps >= CurrentLevel.MaxHops || 
                   totalSteps <= 0 ||
                   ScoreGridPosition.x < 0 || 
                   ScoreGridPosition.x >= GridWidth ||
                   ScoreGridPosition.y < 0 || 
                   ScoreGridPosition.y >= GridHeight ||
                   Tile(ScoreGridPosition).Type == Type.Score);

            Tile(ScoreGridPosition).Type = Type.Score;
            Tile(ScoreGridPosition).PointValue *= (int)totalSteps;
        }
    }

    public void IncrementGoalCount()
    {
        GoalCount += 1;
        if (GoalCount % CurrentLevel.GoalsToNextLevel == 0)
        {
            GoalCount = 0;
            EmitSignal(nameof(NextLevel));
        }
    }

    public void PrintGrid()
    {
        float row = 0;
        string currentRowstring = "[ ";
        for (int y = 0; y < GridWidth; y++)
        {
            for (int x = 0; x < GridHeight; x++)
            {
                if (Tiles[x, y].GridPosition.y > row)
                {
                    row += 1;
                    GD.Print(currentRowstring + " ]");
                    currentRowstring = "[ ";
                }

                currentRowstring += $"{Tiles[x, y].GridPosition} {Tiles[x, y].Type} {Tiles[x, y].PointValue}";
            }
        }
        GD.Print(currentRowstring + " ]");
    }
}