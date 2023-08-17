using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class Boss : Node2D
    {
        private Grid Grid;
        List<TileChangeInstruction> TileChangeInstructions;

        string CurrentBossLevelName = "Boss2";

        public override void _Ready()
        {
            SetGrid();
            //CreateBossTestData();
            CreateBossTestData1();
            CreateBossTestData2();
            CreateBossTestData3();
        }

        private void CreateBossTestData1()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init("Boss1");
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Score,
                    score:              1,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Bounce,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.Save();
        }

        private void CreateBossTestData2()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init("Boss2");
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Score,
                    score:              1,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Water,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.Save();
        }

        private void CreateBossTestData3()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init("Boss3");
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(2, 4),
                    tileType:           Type.Jump,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(2, 4),
                    tileType:           Type.Bounce,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Bounce,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Jump,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(6, 4),
                    tileType:           Type.Jump,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(6, 4),
                    tileType:           Type.Bounce,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
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
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(1, 1),
                    tileType:           Type.Direct,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(-1, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Direct,
                    score:              0,
                    eaten:              false,                    
                    bounceDirection:    new Vector2(1, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Score,
                    score:              0,
                    eaten:              false,                    
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
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
            Grid.UpdateTile(tci.TileGridPosition, tci.TileType, tci.Score, tci.BounceDirection, tci.Eaten, tci.Activated);
        }

        public Vector2 Move(int hopsCompleted, Vector2 playerGridPosition)
        {
            Vector2 playerPositionOnChangedTile = new Vector2(-1, -1);
            int lastTurnForLoop = LastTurnForLoop();
            foreach (TileChangeInstruction I in TileChangeInstructions)
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
            foreach (TileChangeInstruction I in TileChangeInstructions)
            {
                if (I.ActionOnTurn > turn) turn = I.ActionOnTurn;
            }
            return turn + 1;
        }
        
        public List<TileChangeInstruction> Load(string levelName)
        {
            string path = $"res://Levels/{levelName}_BossData.tres"; 
            BossBattleData bossData = ResourceLoader.Load<BossBattleData>(path);
            if (bossData == null) return null;

            TileChangeInstructions = bossData.Deserialize();
            return TileChangeInstructions;
        }

        public void UpdateInstruction(Tile tile, int hopsCompleted, bool eaten)
        {
            int hops = hopsCompleted;
            while (hops >= 0)
            {
                int turn = hops % LastTurnForLoop();
                for (int i = 0; i < TileChangeInstructions.Count; i++)
                {
                    if (TileChangeInstructions[i].ActionOnTurn == turn &&
                        TileChangeInstructions[i].TileGridPosition == tile.GridPosition &&
                        TileChangeInstructions[i].TileType == tile.Type)
                    {
                        TileChangeInstructions[i].Eaten = eaten;
                        TileChangeInstructions[i].BounceDirection = tile.BounceDirection;
                    }
                }
                hops--;
            }
        }

        public void UpdateGoalInstructions(bool activated)
        {
            for (int i = 0; i < TileChangeInstructions.Count; i++)
            {
                if (TileChangeInstructions[i].TileType == Type.Goal)
                {
                    TileChangeInstructions[i].Activated = true;
                }
            }
        }
    }
}
