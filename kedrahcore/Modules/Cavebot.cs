using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using Tibia.Constants;

namespace Kedrah.Modules {
    public class Cavebot : Module {
        #region Variables/Objects

        private int Iterator = 0;
        public List<Waypoint> Waypoints = new List<Waypoint>();
        public int SkipNodes = 2;
        public Item Pick = Tibia.Constants.Items.Tool.Pick;
        public Item Rope = Tibia.Constants.Items.Tool.Rope;
        public Item Shovel = Tibia.Constants.Items.Tool.Shovel;

        #endregion

        #region Constructor/Destructor

        public Cavebot(Core core)
            : base(core) {

            #region Timers

            Timers.Add("walk", new Tibia.Util.Timer(500, false));
            Timers["walk"].Execute += new Tibia.Util.Timer.TimerExecution(Walk_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool Walk {
            get {
                if (Timers["walk"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set {
                if (value)
                    PlayTimer("walk");
                else
                    PauseTimer("walk");
            }
        }

        #endregion

        #region Module Functions

        private Location MovableDirection() {
            List<Tile> tiles = new List<Tile>();
            tiles.Add(Kedrah.Map.GetTile(Kedrah.Player.Location.Offset(1, 0, 0)));
            tiles.Add(Kedrah.Map.GetTile(Kedrah.Player.Location.Offset(-1, 0, 0)));
            tiles.Add(Kedrah.Map.GetTile(Kedrah.Player.Location.Offset(0, 1, 0)));
            tiles.Add(Kedrah.Map.GetTile(Kedrah.Player.Location.Offset(0, -1, 0)));

            foreach (Tile tile in tiles)
                if (!tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath) && !tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.Blocking) && !tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.Floorchange))
                    return tile.Location;

            return Kedrah.Player.Location.Offset(1, 1, 0);
        }

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

        #region Timers

        private void Walk_OnExecute() {
            if (Kedrah.Player.Target_ID != 0)
                return;

            if (Kedrah.Modules.Looter.LootBodies.Count > 0) {
                for (int i = 0; i < Kedrah.Modules.Looter.LootBodies.Count; i++)
                    if (!Kedrah.PathFinder.Reachable(Kedrah.Modules.Looter.LootBodies[i].Body.Location.GroundLocation))
                        Kedrah.Modules.Looter.LootBodies.RemoveAt(i--);

                Kedrah.Modules.Looter.LootBodies.Sort();
                LootBody body = Kedrah.Modules.Looter.LootBodies[0];

                if (!body.Body.Location.GroundLocation.IsAdjacentTo(Kedrah.Player.Location) && !Kedrah.Player.IsWalking)
                    Kedrah.Player.GoTo = body.Body.Location.GroundLocation;

                return;
            }

            if (Waypoints.Count <= 0)
                return;

            Waypoint waypoint = Waypoints[Iterator];

            if (!Reachable(waypoint.Location))
                Iterator++;
            else {

                if (!Kedrah.Player.IsWalking && waypoint.Type != WaypointType.Action)
                    Kedrah.Player.GoTo = waypoint.Location;

                switch (waypoint.Type) {
                    case WaypointType.Action:
                        Iterator++;
                        break;
                    case WaypointType.Approach:

                        break;
                    case WaypointType.Ladder:
                        if (waypoint.Location.DistanceTo(Kedrah.Player.Location) <= 1) {
                            Kedrah.Map.GetTile(waypoint.Location).Ground.Use();
                            Iterator++;
                        }
                        break;
                    case WaypointType.Node:
                        if (waypoint.Location.DistanceTo(Kedrah.Player.Location) <= SkipNodes)
                            Iterator++;
                        break;
                    case WaypointType.Pick:
                        if (waypoint.Location.DistanceTo(Kedrah.Player.Location) == 1) {
                            Pick.Use(Kedrah.Map.GetTile(waypoint.Location));
                            Iterator++;
                        }
                        else if (waypoint.Location == Kedrah.Player.Location)
                            Kedrah.Player.GoTo = MovableDirection();
                        break;
                    case WaypointType.Rope:
                        if (waypoint.Location.DistanceTo(Kedrah.Player.Location) <= 1) {
                            Rope.Use(Kedrah.Map.GetTile(waypoint.Location));
                            Iterator++;
                        }
                        break;
                    case WaypointType.Shovel:
                        if (waypoint.Location.DistanceTo(Kedrah.Player.Location) == 1) {
                            Shovel.Use(Kedrah.Map.GetTile(waypoint.Location));
                            Iterator++;
                        }
                        else if (waypoint.Location == Kedrah.Player.Location)
                            Kedrah.Player.GoTo = MovableDirection();
                        break;
                    case WaypointType.Stand:
                        if (waypoint.Location == Kedrah.Player.Location)
                            Iterator++;
                        break;
                    case WaypointType.Walk:
                        Iterator++;
                        break;
                }

                if (Iterator >= Waypoints.Count)
                    Iterator = 0;
            }
        }

        #endregion
    }

    public enum WaypointType {
        Action,
        Approach,
        Ladder,
        Node,
        Pick,
        Rope,
        Shovel,
        Stand,
        Walk
    }

    public class Waypoint : IComparable<Waypoint> {
        public Core Kedrah;
        public Location Location;
        public WaypointType Type;

        public Waypoint() { }

        public Waypoint(Location location, WaypointType type, Core kedrah) {
            Kedrah = kedrah;
            Location = location;
            Type = type;
        }

        public int CompareTo(Waypoint other) {
            int comparisson = other.Location.DistanceTo(Kedrah.Player.Location).CompareTo(other.Location.DistanceTo(Kedrah.Player.Location));

            return comparisson;
        }

        public override string ToString() {
            return Type.ToString() + ": " + Location.ToString();
        }
    }
}
