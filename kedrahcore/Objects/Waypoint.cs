using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using Kedrah.Constants;
using System.Threading;

namespace Kedrah.Objects
{
    public class Waypoint : IComparable<Waypoint>
    {
        public Core Core;
        public Location Location;
        public WaypointType Type;

        public Waypoint() { }

        public Waypoint(Location location, WaypointType type, Core core)
        {
            Core = core;
            Location = location;
            Type = type;
        }

        public void SetAction(string code)
        {
            code = Script.GenerateCSharp(ToString(), code, Timeout.Infinite);
            Core.Modules.Scripter.LoadScriptFromSource(code, Core.Modules.Scripter.CSharpCodeProvider);
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
