using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class Boss : Node2D
    {
        private Grid Grid;
        List<TileChangeInstruction> tileChangeInstructions;

        string CurrentBossLevelName = "Boss2";

        public override void _Ready()
        {
            SetGrid();
            //CreateBossTestData();
            //CreateBossTestData1();
            CreateBossTestData2();

            //Load resource
            tileChangeInstructions = LoadBossData(CurrentBossLevelName);
        }

        private void CreateBossTestData2()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init(CurrentBossLevelName);
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       2,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Direct,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(1, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       5,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.Save();
        }

        private void CreateBossTestData1()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init(CurrentBossLevelName);
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Score,
                    score:              1,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       2,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Jump,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       3,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       4,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Water,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       5,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       6,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Bounce,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       7,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       8,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Rock,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       9,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       10,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Direct,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(-1, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       11,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       12,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Goal,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       13,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.Save();
        }

        private void CreateBossTestData()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init("DefaultLevelName");
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(1, 1),
                    tileType:           Type.Score,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(1, 1),
                    tileType:           Type.Direct,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(-1, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Direct,
                    score:              0,
                    eaten:              false,                    
                    bounceDirection:    new Vector2(1, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Score,
                    score:              0,
                    eaten:              false,                    
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.Save();
        }

        public void SetGrid()
        {
            Grid = GetNode<Stage>("..").Grid;
        }


        public void UpdateTile(TileChangeInstruction tci)
        {
            //Vector2 gridPosition, Type type, int score, Vector2 BounceDirection
            Grid.UpdateTile(tci.TileGridPosition, tci.TileType, tci.Score, tci.BounceDirection, tci.Eaten);
        }

        public Vector2 Move(int hopsCompleted, Vector2 playerGridPosition)
        {
            Vector2 playerPositionOnChangedTile = new Vector2(-1, -1);
            int lastTurnForLoop = LastTurnForLoop();
            foreach (TileChangeInstruction I in tileChangeInstructions)
            {
                if (I.ActionOnTurn == hopsCompleted % lastTurnForLoop)
                {
                    if (I.TileGridPosition == playerGridPosition) playerPositionOnChangedTile = playerGridPosition;
                    UpdateTile(I);
                } 
            }
            return playerPositionOnChangedTile;
        }

        private int LastTurnForLoop()
        {
            int turn = 0;
            foreach (TileChangeInstruction I in tileChangeInstructions)
            {
                if (I.ActionOnTurn > turn) turn = I.ActionOnTurn;
            }
            return turn + 1;
        }
        
        public List<TileChangeInstruction> LoadBossData(string levelName)
        {
            string path = $"res://Levels/{levelName}_BossData.tres"; 
            BossBattleData bossData = ResourceLoader.Load<BossBattleData>(path);
            if (bossData == null) return null;
            return bossData.Deserialize();
        }

        public void UpdateInstruction(Tile tile, int hopsCompleted, bool eaten)
        {
/*             int turn = hopsCompleted % LastTurnForLoop(); 


            for (int i = tileChangeInstructions.Count - 1; i >= 0 ; i++)
            {
                if (tileChangeInstructions[i].ActionOnTurn == turn &&
                    tileChangeInstructions[i].TileGridPosition == GridPosition)
                {
                    tileChangeInstructions[i].Eaten = eaten;
                    tileChangeInstructions[i].BounceDirection = bounceDirection;
                }
            } */

            int hops = hopsCompleted;
            while (hops >= 0)
            {
                int turn = hops % LastTurnForLoop();
                for (int i = 0; i < tileChangeInstructions.Count; i++)
                {
                    if (tileChangeInstructions[i].ActionOnTurn == turn &&
                        tileChangeInstructions[i].TileGridPosition == tile.GridPosition &&
                        tileChangeInstructions[i].TileType == tile.Type)
                    {
                        tileChangeInstructions[i].Eaten = eaten;
                        tileChangeInstructions[i].BounceDirection = tile.BounceDirection;
                    }
                }
                hops--;
            }

        }
    }
}
