using Godot;

namespace Hopper
{
    public class SaveData : Resource
    {
        [Export]
        public int CurrentLocationId { get; set; }

        [Export]
        public LocationData[] LocationDataList { get; set; }

        public void Init(int LocationCount)
        {
            LocationDataList = new LocationData[LocationCount];
        }
        
    }
}