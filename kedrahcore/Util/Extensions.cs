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

        public static bool IsShootable(this Location location)
        {
            int XSign = (location.X > Kedrah.Player.Location.X) ? 1 : -1;
            int YSign = (location.Y > Kedrah.Player.Location.Y) ? 1 : -1;
            double XDistance = Math.Abs(location.X - Kedrah.Player.Location.X);
            double YDistance = Math.Abs(location.Y - Kedrah.Player.Location.Y);
            double max = location.Distance();
            Location check;

            if (Math.Abs(XDistance) > 8 || Math.Abs(YDistance) > 5)
            {
                return false;
            }

            for (int i = 1; i <= max; i++)
            {
                check = Kedrah.Player.Location.Offset((int)Math.Ceiling(i * XDistance / max) * XSign, (int)Math.Ceiling(i * YDistance / max) * YSign, 0);
                Tile tile = Kedrah.Map.GetTile(check);

                if (tile != null)
                {
                    if (tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksMissiles))
                    {
                        return false;
                    }

                    Item item = tile.Items.FirstOrDefault(tileItem => tileItem.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksMissiles));

                    if (item != null)
                    {
                        return false;
                    }
                }
            }

            return true;
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

        #region Inventory

        public static int CountItems(this Inventory inventory, uint id)
        {
            int count = 0;
            
            foreach (Item item in inventory.GetItems().Where(i => i.Id == id))
            {
                count += item.Count;
            }

            return count;
        }

        public static void Stack(this Inventory inventory)
        {
            List<Item> items = inventory.GetItems().Where(i => i.GetFlag(Tibia.Addresses.DatItem.Flag.IsStackable) && i.Count < 100).ToList();
            
            foreach (Item item in items)
            {
                Item last = items.LastOrDefault(i => i.Id == item.Id);
                
                if (last != item)
                {
                    last.Move(item.Location);
                    return;
                }
            }
        }

        #endregion
    }
}
