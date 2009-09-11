using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;

namespace Kedrah
{
    public static class Extensions
    {
        public static Core Kedrah = null;

        public static int DistanceBetween(this Location location, Location destination)
        {
            return Math.Max(Math.Abs(location.X - destination.X), Math.Abs(location.Y - destination.Y));
        }

        public static int Distance(this Location location)
        {
            return location.DistanceBetween(Kedrah.Player.Location);
        }

        public static bool IsAdjacent(this Location location)
        {
            return location.IsAdjacentTo(Kedrah.Player.Location);
        }

        public static int DistanceBetween(this Creature creature, Location destination)
        {
            return Math.Max(Math.Abs(creature.Location.X - destination.X), Math.Abs(creature.Location.Y - destination.Y));
        }

        public static int Distance(this Creature creature)
        {
            return creature.DistanceBetween(Kedrah.Player.Location);
        }
    }
}
