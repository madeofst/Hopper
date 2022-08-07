using System;

namespace Hopper
{
    public class StageData
    {
        public int ID { get; set; }
        public string Pond { get; set; }
        public int LevelReached { get; set; }

        public StageData(int ID = 0,
                         string Pond = "",
                         int LevelReached = 0)
        {
            this.ID = ID;
            this.Pond = Pond;
            this.LevelReached = LevelReached;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}