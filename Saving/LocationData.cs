using Godot;

namespace Hopper
{
    public class LocationData : Resource
    {
        [Export]
        public int ID { get; set; }

        [Export]
        public string CurrentLocationName { get; set; }

        [Export]
        public bool Active { get; set; }

        [Export]
        public bool NewlyActivated { get; set; }

        [Export]
        public bool Complete { get; set; }

        [Export]
        public int LevelReached { get; set; }

/*         public LocationData()
        {
            ID = 0;
            Active = false;
            newlyActivated = false;
            Complete = false;
            LevelReached = 0;
        } */
    }
}