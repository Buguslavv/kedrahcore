using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using Tibia.Constants;
using Tibia;

namespace Kedrah
{
    public static class Extensions
    {
        public static Core Core = null;

        #region Client

        public static void SetModes(this Client client, Attack attack, Follow follow)
        {
            Core.Client.FollowMode = follow;
            Core.Client.AttackMode = attack;
            Tibia.Packets.Outgoing.FightModesPacket.Send(Core.Client, (byte)Core.Client.AttackMode, (byte)Core.Client.FollowMode, (byte)Core.Client.SafeMode);
        }

        #endregion

        #region Location

        public static int MonstersAround(this Location location)
        {
            return Core.Client.BattleList.GetCreatures().Count(c => c.Location.Z == location.Z && c.Location.IsAdjacentTo(location));
        }

        private static Location AdjustLocation(this Location loc, int xDiff, int yDiff)
        {
            int xNew = loc.X - xDiff;
            int yNew = loc.Y - yDiff;

            if (xNew > 17)
                xNew -= 18;
            else if (xNew < 0)
                xNew += 18;

            if (yNew > 13)
                yNew -= 14;
            else if (yNew < 0)
                yNew += 14;

            return new Location(xNew, yNew, loc.Z);
        }

        public static bool IsReachable(this Location location)
        {
            try
            {
                var tileList = Core.Client.Map.GetTilesOnSameFloor();
                var playerTile = tileList.GetTileWithPlayer(Core.Client);
                var creatureTile = Core.Client.Map.GetTile(location);

                if (playerTile == null || creatureTile == null)
                    return false;

                int xDiff, yDiff;
                int playerZ = Core.Client.Memory.ReadInt32(Tibia.Addresses.Player.Z);
                var creatures = Core.Client.BattleList.GetCreatures().Where(c => c.Z == playerZ);
                int playerId = Core.Client.Memory.ReadInt32(Tibia.Addresses.Player.Id);

                xDiff = (int)playerTile.MemoryLocation.X - 8;
                yDiff = (int)playerTile.MemoryLocation.Y - 6;

                playerTile.MemoryLocation = AdjustLocation(playerTile.MemoryLocation, xDiff, yDiff);
                creatureTile.MemoryLocation = AdjustLocation(creatureTile.MemoryLocation, xDiff, yDiff);

                foreach (Tile tile in tileList)
                {
                    tile.MemoryLocation = AdjustLocation(tile.MemoryLocation, xDiff, yDiff);

                    if (tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.Blocking) || tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath) ||
                        tile.Items.Any(i => i.GetFlag(Tibia.Addresses.DatItem.Flag.Blocking) || i.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath) || Core.Client.PathFinder.ModifiedItems.ContainsKey(i.Id)))
                    {
                        Core.Client.PathFinder.Grid[tile.MemoryLocation.X, tile.MemoryLocation.Y] = 0;
                    }
                    else
                    {
                        Core.Client.PathFinder.Grid[tile.MemoryLocation.X, tile.MemoryLocation.Y] = 1;
                    }
                }

                return Core.Client.PathFinder.FindPath(playerTile.MemoryLocation, creatureTile.MemoryLocation);
            }
            catch { return false; }
        }

        public static List<Location> StrikeAffectedLocations(this Location location)
        {
            List<Location> result = new List<Location>();

            result.Add(location.Offset(1, 0, 0));
            result.Add(location.Offset(-1, 0, 0));
            result.Add(location.Offset(0, 1, 0));
            result.Add(location.Offset(0, -1, 0));

            return result.Where(l => l.IsShootable()).ToList();
        }

        public static List<Location> BeamAffectedLocations(this Location location)
        {
            List<Location> result = new List<Location>();

            for (int i = 1; i <= 9; i++)
            {
                result.Add(location.Offset(i, 0, 0));
                result.Add(location.Offset(-i, 0, 0));
                result.Add(location.Offset(0, i, 0));
                result.Add(location.Offset(0, -i, 0));
            }

            return result.Where(l => l.IsShootable()).ToList();
        }

        public static List<Location> WaveAffectedLocations(this Location location)
        {
            List<Location> result = new List<Location>();

            for (int i = 1; i <= 7; i++)
            {
                result.Add(location.Offset(i, 0, 0));
                result.Add(location.Offset(-i, 0, 0));
                result.Add(location.Offset(0, i, 0));
                result.Add(location.Offset(0, -i, 0));
                if (i > 1)
                {
                    result.Add(location.Offset(i, 1, 0));
                    result.Add(location.Offset(-i, 1, 0));
                    result.Add(location.Offset(1, i, 0));
                    result.Add(location.Offset(1, -i, 0));
                    result.Add(location.Offset(i, -1, 0));
                    result.Add(location.Offset(-i, -1, 0));
                    result.Add(location.Offset(-1, i, 0));
                    result.Add(location.Offset(-1, -i, 0));
                }
                if (i > 4)
                {
                    result.Add(location.Offset(i, 2, 0));
                    result.Add(location.Offset(-i, 2, 0));
                    result.Add(location.Offset(2, i, 0));
                    result.Add(location.Offset(2, -i, 0));
                    result.Add(location.Offset(i, -2, 0));
                    result.Add(location.Offset(-i, -2, 0));
                    result.Add(location.Offset(-2, i, 0));
                    result.Add(location.Offset(-2, -i, 0));
                }
            }

            return result.Where(l => l.IsShootable()).ToList();
        }

        public static int DistanceBetween(this Location location, Location destination)
        {
            return Math.Max(Math.Abs(location.X - destination.X), Math.Abs(location.Y - destination.Y));
        }

        public static int Distance(this Location location)
        {
            return location.DistanceBetween(Core.Player.Location);
        }

        public static bool IsAdjacent(this Location location)
        {
            return location.IsAdjacentTo(Core.Player.Location);
        }

        public static bool IsShootable(this Location location)
        {
            int XSign = (location.X > Core.Player.Location.X) ? 1 : -1;
            int YSign = (location.Y > Core.Player.Location.Y) ? 1 : -1;
            double XDistance = Math.Abs(location.X - Core.Player.Location.X);
            double YDistance = Math.Abs(location.Y - Core.Player.Location.Y);
            double max = location.Distance();
            Location check;

            if (Math.Abs(XDistance) > 8 || Math.Abs(YDistance) > 5)
            {
                return false;
            }

            for (int i = 1; i <= max; i++)
            {
                check = Core.Player.Location.Offset((int)Math.Ceiling(i * XDistance / max) * XSign, (int)Math.Ceiling(i * YDistance / max) * YSign, 0);
                Tile tile;
                try { tile = Core.Client.Map.GetTile(check); }
                catch { tile = null; }

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
            return creature.DistanceBetween(Core.Player.Location);
        }

        public static void Attack(this Creature creature, bool packetOnly)
        {
            if (packetOnly)
            {
                Tibia.Packets.Outgoing.AttackPacket.Send(Core.Client, (uint)creature.Id);
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
                Tibia.Packets.Outgoing.FollowPacket.Send(Core.Client, (uint)creature.Id);
            }
            else
            {
                creature.Follow();
            }
        }

        public static bool IsTarget(this Creature creature)
        {
            return (creature.Id.Equals(Core.Player.RedSquare) || creature.Id.Equals(Core.Player.GreenSquare));
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
