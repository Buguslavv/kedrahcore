using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using Kedrah.Constants;

namespace Kedrah.Objects
{
    public class Waypoint : IComparable<Waypoint>
    {
        public Core Kedrah;
        public Location Location;
        public WaypointType Type;

        public Waypoint() { }

        public Waypoint(Location location, WaypointType type, Core kedrah)
        {
            Kedrah = kedrah;
            Location = location;
            Type = type;
        }

        public int CompareTo(Waypoint other)
        {
            int comparisson = other.Location.Distance().CompareTo(other.Location.Distance());

            return comparisson;
        }

        public override string ToString()
        {
            return Type.ToString() + ": " + Location.ToString();
        }
    }
}
