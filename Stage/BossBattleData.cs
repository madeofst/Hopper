using Godot;
using Godot.Collections;

namespace Hopper
{
    public class BossBattleData : Resource
    {
        [Export]
        public Array<int> TileChangeData { get; set; }
     
        public void Init()
        {
            TileChangeData = new Array<int>();
        }

        public void AddTileChange(int[] changeData)
        {
            TileChangeData.Add(changeData[0]);
            TileChangeData.Add(changeData[1]);
            TileChangeData.Add(changeData[2]);
            TileChangeData.Add(changeData[3]);
            TileChangeData.Add(changeData[4]);
            TileChangeData.Add(changeData[5]);
            TileChangeData.Add(changeData[6]);
        }
    }
}