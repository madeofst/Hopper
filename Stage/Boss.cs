using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hopper
{
    public class TileChangeInstruction
    {
        public int ActionOnTurn;
        public Vector2 TileGridPosition;
        public Type TileType;
        public int Score;
        public Vector2 BounceDirection;
    }

    public class Boss : Node2D
    {
        private Grid Grid;
        private BossBattleData TestResource;
        List<TileChangeInstruction> tileChangeInstructions;

        public override void _Ready()
        {
            SetGrid();

            TestResource = ResourceLoader.Load<BossBattleData>("res://Stage/BossBattleData.tres");

            // Create resource
            TestResource.Init();
            TestResource.AddTileChange(new int[] {0, 1, 1, (int)Type.Rock, 0, 0, 0});
            TestResource.AddTileChange(new int[] {1, 1, 1, (int)Type.Jump, 0, 0, 0});
            TestResource.AddTileChange(new int[] {0, 2, 2, (int)Type.Jump, 0, 0, 0});
            TestResource.AddTileChange(new int[] {1, 2, 2, (int)Type.Rock, 0, 0, 0});

            //Load resource
            tileChangeInstructions = new List<TileChangeInstruction>();
            int noOfElements = 7;
            for (int i = 0; i < TestResource.TileChangeData.Count / noOfElements; i++)
            {
                int i2 = i * noOfElements;
                tileChangeInstructions.Add( new TileChangeInstruction() 
                    {
                        ActionOnTurn = TestResource.TileChangeData[i2],
                        TileGridPosition = new Vector2(TestResource.TileChangeData[i2 + 1], 
                                                       TestResource.TileChangeData[i2 + 2]),
                        TileType = (Type)TestResource.TileChangeData[i2 + 3],
                        Score = TestResource.TileChangeData[i2 + 4],
                        BounceDirection = new Vector2(TestResource.TileChangeData[i2 + 5], 
                                                      TestResource.TileChangeData[i2 + 6]),
                    });
            }
        }

        public void SetGrid()
        {
            Grid = GetNode<Stage>("..").Grid;
        }


        public void UpdateTile(TileChangeInstruction tci)
        {
            //Vector2 gridPosition, Type type, int score, Vector2 BounceDirection
            Grid.UpdateTile(tci.TileGridPosition, tci.TileType, tci.Score, tci.BounceDirection);
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
    }
}
