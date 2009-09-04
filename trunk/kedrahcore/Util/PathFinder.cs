using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah.Util {
    public class PathFinder {
        #region Variables/Objects
        
        public Core Kedrah;

        #endregion

        #region Constructor/Destructor

        public PathFinder(Core core) {
            Kedrah = core;
        }

        #endregion

        #region Methods

        private static Tibia.Objects.Location AdjustLocation(Tibia.Objects.Location location, int xDiff, int yDiff) {
            int xNew = location.X - xDiff;
            int yNew = location.Y - yDiff;

            if (xNew > 17)
                xNew -= 18;
            else if (xNew < 0)
                xNew += 18;

            if (yNew > 13)
                yNew -= 14;
            else if (yNew < 0)
                yNew += 14;

            return new Tibia.Objects.Location(xNew, yNew, location.Z);
        }

        public bool Reachable(Tibia.Objects.Location location) {
            IEnumerable<Tibia.Objects.Tile> tileList = Kedrah.Map.GetTilesOnSameFloor();
            Tibia.Objects.Tile playerTile = Kedrah.Map.GetTileWithPlayer();
            Tibia.Objects.Tile destinationTile = Kedrah.Map.GetTile(location);

            if (playerTile == null || destinationTile == null)
                return false;

            int xDiff, yDiff;
            int playerZ = Kedrah.Client.Memory.ReadInt32(Tibia.Addresses.Player.Z);
            var creatures = Kedrah.Client.BattleList.GetCreatures().Where(c => c.Z == playerZ);
            int playerId = Kedrah.Client.Memory.ReadInt32(Tibia.Addresses.Player.Id);

            xDiff = (int)playerTile.MemoryLocation.X - 8;
            yDiff = (int)playerTile.MemoryLocation.Y - 6;

            playerTile.MemoryLocation = AdjustLocation(playerTile.MemoryLocation, xDiff, yDiff);
            destinationTile.MemoryLocation = AdjustLocation(destinationTile.MemoryLocation, xDiff, yDiff);

            foreach (Tibia.Objects.Tile tile in tileList) {
                tile.MemoryLocation = AdjustLocation(tile.MemoryLocation, xDiff, yDiff);

                if (tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.Blocking) || tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath) ||
                    tile.Items.Any(i => i.GetFlag(Tibia.Addresses.DatItem.Flag.Blocking) || i.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath) || Kedrah.Client.PathFinder.ModifiedItems.ContainsKey(i.Id)) ||
                    creatures.Any(c => tile.Objects.Any(o => o.Data == c.Id && o.Data != playerId))) {
                    Kedrah.Client.PathFinder.Grid[tile.MemoryLocation.X, tile.MemoryLocation.Y] = 0;
                }
                else {
                    Kedrah.Client.PathFinder.Grid[tile.MemoryLocation.X, tile.MemoryLocation.Y] = 1;
                }
            }

            return Kedrah.Client.PathFinder.FindPath(playerTile.MemoryLocation, destinationTile.MemoryLocation);
        }

        #endregion
    }
}
