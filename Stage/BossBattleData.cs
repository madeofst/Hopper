using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System;

namespace Hopper
{
    public class BossBattleData : Resource
    {
        [Export]
        public string LevelName { get; set; }
        [Export]
        public Array<int> TileChangeData { get; set; }
        private readonly int NoOfElements = 9;
     
        public void Init(string levelName)
        {
            LevelName = levelName;
            TileChangeData = new Array<int>();
        }

        public Error Save()
        {
            //Assumes all required data saved in TileChangeData
            if (LevelName == null) LevelName = "DefaultLevelName";
            TakeOverPath($"res://Levels/{LevelName}_BossData.tres");
            Error error = ResourceSaver.Save($"res://Levels/{LevelName}_BossData.tres", this);
            return error;
        }


        public void AddTileChange(TileChangeInstruction tileChangeInstruction)
        {
            int[] instructions = tileChangeInstruction.Serialize();

            foreach (int i in instructions)
            {
                TileChangeData.Add(i);
            }
        }

        public List<TileChangeInstruction> Deserialize()
        {
            List<TileChangeInstruction> InstructionList = new List<TileChangeInstruction>();

            for (int i = 0; i < TileChangeData.Count / NoOfElements; i++)
            {
                int i2 = i * NoOfElements;
                InstructionList.Add(new TileChangeInstruction(
                    actionOnTurn:       TileChangeData[i2],
                    tileGridPosition:   new Vector2(TileChangeData[i2 + 1], 
                                                    TileChangeData[i2 + 2]),
                    tileType:           (Type)TileChangeData[i2 + 3],
                    score:              TileChangeData[i2 + 4],
                    eaten:              Convert.ToBoolean(TileChangeData[i2 + 5]),
                    bounceDirection:    new Vector2(TileChangeData[i2 + 6], 
                                                    TileChangeData[i2 + 7]),
                    activated:          Convert.ToBoolean(TileChangeData[i2 + 8])
                ));
            }

            return InstructionList;
        }
    }
}