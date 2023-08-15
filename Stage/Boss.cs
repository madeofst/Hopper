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

        string CurrentBossLevelName = "Boss1";

        public override void _Ready()
        {
            SetGrid();
            //CreateBossTestData();
            //CreateBossTestData1();

            //Load resource
            tileChangeInstructions = LoadBossData(CurrentBossLevelName);
        }

        private void CreateBossTestData1()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init(CurrentBossLevelName);
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Score,
                    score:              1,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0)
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Bounce,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(-1, 0)
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

        public void Move(int HopsCompleted)
        {
            int lastTurnForLoop = LastTurnForLoop();
            foreach (TileChangeInstruction I in tileChangeInstructions)
            {
                if (I.ActionOnTurn == HopsCompleted % lastTurnForLoop) UpdateTile(I);
            }
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

        public void UpdateInstruction(Vector2 playerGridPosition, int HopsCompleted, bool eaten)
        {
            int turn = (HopsCompleted - 1) % LastTurnForLoop();

            for (int i = 0; i < tileChangeInstructions.Count; i++)
            {
                if (tileChangeInstructions[i].ActionOnTurn == turn &&
                    tileChangeInstructions[i].TileGridPosition == playerGridPosition)
                {
                    tileChangeInstructions[i].Eaten = eaten;
                }
            }
        }
    }
}
