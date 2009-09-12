using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using Tibia.Constants;

namespace Kedrah
{
    public static class Extensions
    {
        public static Core Kedrah = null;

        #region Client

        public static void SetModes(this Client client, Attack attack, Follow follow)
        {
            Kedrah.Client.FollowMode = follow;
            Kedrah.Client.AttackMode = attack;
            Tibia.Packets.Outgoing.FightModesPacket.Send(Kedrah.Client, (byte)Kedrah.Client.AttackMode, (byte)Kedrah.Client.FollowMode, (byte)Kedrah.Client.SafeMode);
        }

        #endregion

        #region Location

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

        #endregion

        #region Creature

        public static int DistanceBetween(this Creature creature, Location destination)
        {
            return Math.Max(Math.Abs(creature.Location.X - destination.X), Math.Abs(creature.Location.Y - destination.Y));
        }

        public static int Distance(this Creature creature)
        {
            return creature.DistanceBetween(Kedrah.Player.Location);
        }

        public static void Attack(this Creature creature, bool packetOnly)
        {
            if (packetOnly)
            {
                Tibia.Packets.Outgoing.AttackPacket.Send(Kedrah.Client, (uint)creature.Id);
            }
            else
            {
                creature.Attack();
            }
        }

        public static void Follow(this Creature creature, bool packetOnly)
        {
            if (packetOnly)
            {
                Tibia.Packets.Outgoing.FollowPacket.Send(Kedrah.Client, (uint)creature.Id);
            }
            else
            {
                creature.Follow();
            }
        }

        public static bool IsTarget(this Creature creature)
        {
            return (creature.Id.Equals(Kedrah.Player.RedSquare) || creature.Id.Equals(Kedrah.Player.GreenSquare));
        }

        #endregion
    }
}
