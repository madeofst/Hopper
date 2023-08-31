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

        public override void _Ready()
        {
            SetGrid();
            //CreateBossTestData();
            CreateBossTestData1();
            CreateBossTestData2();
            CreateBossTestData3();
            CreateBossTestData4();
            CreateBossTestData5();
        }

        private void CreateBossTestData1()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init("Boss1");
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Score,
                    score:              1,
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
            BossData.Save();
        }

        private void CreateBossTestData2()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init("Boss2");
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Score,
                    score:              1,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
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
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(2, 4),
                    tileType:           Type.Jump,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(2, 4),
                    tileType:           Type.Bounce,
                    score:              0,
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
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(4, 4),
                    tileType:           Type.Jump,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(6, 4),
                    tileType:           Type.Jump,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(6, 4),
                    tileType:           Type.Bounce,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.Save();
        }

        private void CreateBossTestData4()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init("Boss4");
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Goal,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Goal,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       2,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Bounce,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(1, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       3,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Direct,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(1, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       4,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Jump,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(1, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       5,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(1, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       6,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Rock,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(1, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       7,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Score,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(1, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       8,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Water,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(1, 0),
                    activated:          false
                ));
/*             BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       0,
                    tileGridPosition:   new Vector2(6, 2),
                    tileType:           Type.Rock,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 1),
                    activated:          false
                )); */
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(6, 2),
                    tileType:           Type.Score,
                    score:              1,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, -1),
                    activated:          false
                ));
            /*BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       3,
                    tileGridPosition:   new Vector2(6, 6),
                    tileType:           Type.Rock,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(-1, 0),
                    activated:          false
                )); */
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(2, 2),
                    tileType:           Type.Water,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            /* BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(6, 2),
                    tileType:           Type.Water,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(2, 6),
                    tileType:           Type.Water,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(6, 6),
                    tileType:           Type.Water,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                )); */
            BossData.Save();
        }

        private void CreateBossTestData5()
        {
            // Create resource
            BossBattleData BossData = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");
            BossData.Init("Boss5");
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(4, 5),
                    tileType:           Type.Lily,
                    score:              0,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       1,
                    tileGridPosition:   new Vector2(6, 2),
                    tileType:           Type.Score,
                    score:              1,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       3,
                    tileGridPosition:   new Vector2(4, 5),
                    tileType:           Type.Score,
                    score:              1,
                    eaten:              false,
                    bounceDirection:    new Vector2(0, 0),
                    activated:          false
                ));
            BossData.AddTileChange(new TileChangeInstruction(
                    actionOnTurn:       3,
                    tileGridPosition:   new Vector2(6, 2),
                    tileType:           Type.Lily,
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
            Grid.UpdateTile(tci);
        }

        public int Move(int hopsCompleted)
        {
            int lastTurnForLoop = LastTurnForLoop();
            int TileCount = 0;
            
            foreach (TileChangeInstruction I in TileChangeInstructions)
            {

                if (I.ActionOnTurn == (hopsCompleted - 1) % lastTurnForLoop)
                {
                    Grid.GetTile(I.TileGridPosition).SlideAcross(I);
                    TileCount ++;
                } 
            }
            return TileCount;
        }

        public void UpdateIndicatorAnimations(int hopsCompleted, string AnimationName)
        {
            int lastTurnForLoop = LastTurnForLoop();
            foreach (TileChangeInstruction I in TileChangeInstructions)
            {
                if (I.ActionOnTurn == hopsCompleted % lastTurnForLoop)
                {
                    Grid.GetTile(I.TileGridPosition).Animate(AnimationName);
                } 
            }
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
