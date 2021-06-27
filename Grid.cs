using System;
using Godot;    

public class Grid : Node2D
{
    public Player Player { get; set; }

    public int GridWidth;
    public int GridHeight;
    public Tile[,] Tiles;
    public Vector2 Size;
    public Vector2 Offset;
    public Vector2 TileSize;

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

        for (int i = 0; i < GridWidth; i++)
        {
            for (int j = 0; j < GridHeight; j++)
            {   
                Tiles[i, j] = new Tile(); 
                Tiles[i, j].Type = Type.Black;
                Tiles[i, j].GridPosition = new Vector2(i, j);
                Tiles[i, j].Size = TileSize;
                Tiles[i, j].ScreenPosition = Tiles[i, j].GridPosition * TileSize + Offset + TileSize/2;
            }
        }
    }

    public override void _Ready()
    {
        rand.Randomize();
    }

    public void SetupGrid()
    {
        Player = GetNode<Player>("../Player");
        UpdateGrid();
    }

    public void UpdateGrid()
    {
        AssignTileTypes();
        Draw();
    }

    private void AssignTileTypes()
    {
        ClearTypes();
        AssignGoalTiles(1, 3);
    }

    private void ClearTypes()
    {
        foreach (Tile t in Tiles)
        {
            t.Type = Type.Black;
        }
    }

    private void AssignGoalTiles(int goalTileCount, int stepsFromPlayer)
    {
        int x = rand.RandiRange(0, stepsFromPlayer);
        Vector2 relativeGridPosition = new Vector2(x, stepsFromPlayer - x);
        Vector2 absoluteGridPosition;
        do
        {
            for (int i = 0; i <= rand.RandiRange(0,3); i++)
            {
                float tempY = relativeGridPosition.y;
                relativeGridPosition.y = relativeGridPosition.x * -1;
                relativeGridPosition.x = tempY;
            }
        absoluteGridPosition = relativeGridPosition + Player.GridPosition;
        } while (absoluteGridPosition.x < 0 || absoluteGridPosition.x >= GridWidth ||
                 absoluteGridPosition.y < 0 || absoluteGridPosition.y >= GridHeight );

        Tiles[(int)absoluteGridPosition.x, (int)absoluteGridPosition.y].Type = Type.White;
    }

    public void Draw()
    {
        foreach(Tile tile in Tiles)
        {
            Sprite sprite = new Sprite();
            sprite.Position = tile.ScreenPosition;
            sprite.Scale = new Vector2(0.95f,0.95f);
            if (tile.Type == Type.Black)
            {
                sprite.Texture = GD.Load<Texture>("res://BlackSquare.png");
            }
            else if (tile.Type == Type.White)
            {
                sprite.Texture = GD.Load<Texture>("res://WhiteSquare.png");
            }
            AddChild(sprite);
        }
    }
}