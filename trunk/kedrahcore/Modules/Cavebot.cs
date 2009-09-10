using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using Tibia.Constants;
using System.Threading;

namespace Kedrah.Modules
{
    public class Cavebot : Module
    {
        #region Variables/Objects

        private int iterator = 0;
        public List<Waypoint> Waypoints = new List<Waypoint>();
        public int SkipNodes = 3;
        public Item Pick = Tibia.Constants.Items.Tool.Pick;
        public Item Rope = Tibia.Constants.Items.Tool.Rope;
        public Item Shovel = Tibia.Constants.Items.Tool.Shovel;
        public List<Item> LootBodies = new List<Item>();

        #endregion

        #region Constructor/Destructor

        public Cavebot(ref Core core)
            : base(ref core)
        {
            Pick = Tibia.Constants.Items.Tool.Pick;
            Rope = Tibia.Constants.Items.Tool.Rope;
            Shovel = Tibia.Constants.Items.Tool.Shovel;

            #region Timers

            Timers.Add("walk", new Tibia.Util.Timer(500, false));
            Timers["walk"].Execute += new Tibia.Util.Timer.TimerExecution(Walk_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool Walk
        {
            get
            {
                if (Timers["walk"].State == Tibia.Util.TimerState.Running)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    PlayTimer("walk");
                }
                else
                {
                    PauseTimer("walk");
                }
            }
        }

        #endregion

        #region Module Functions

        private Location MovableDirection()
        {
            List<Tile> tiles = new List<Tile>();
            tiles.Add(Kedrah.Map.GetTile(Kedrah.Player.Location.Offset(1, 0, 0)));
            tiles.Add(Kedrah.Map.GetTile(Kedrah.Player.Location.Offset(-1, 0, 0)));
            tiles.Add(Kedrah.Map.GetTile(Kedrah.Player.Location.Offset(0, 1, 0)));
            tiles.Add(Kedrah.Map.GetTile(Kedrah.Player.Location.Offset(0, -1, 0)));

            foreach (Tile tile in tiles)
            {
                if (!tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath) && !tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.Blocking) && !tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.Floorchange))
                {
                    return tile.Location;
                }
            }

            return Kedrah.Player.Location.Offset(1, 1, 0);
        }

        private Location NearestAjacent(Location location)
        {
            Location result = new Location();

            foreach (Tile t in Kedrah.Map.GetTilesOnSameFloor())
            {
                if (t.Location.IsAdjacentTo(location) && t.Location.DistanceTo(Kedrah.Player.Location) < location.DistanceTo(Kedrah.Player.Location))
                {
                    result = t.Location;
                }
            }

            return result;
        }

        private Waypoint FloorChangerWaypoint(Location destination)
        {
            sbyte change = (sbyte)(destination.Z - Kedrah.Player.Z);

            if (Math.Abs(change) > 1)
            {
                return null;
            }

            List<uint> list = null;
            Tile result = null;
            List<Tile> possible = Kedrah.Map.GetTilesOnSameFloor().Where(t => t.Location.DistanceTo(Kedrah.Player.Location) < 27).ToList();
            possible.Sort(new Comparison<Tile>(delegate(Tile t1, Tile t2) { return t1.Location.DistanceTo(destination).CompareTo(t2.Location.DistanceTo(destination)); }));

            if (change < 0)
            {
                #region Going Up

                list = new List<uint>();
                list.AddRange(TileLists.Rope);
                list.AddRange(TileLists.Up);
                list.AddRange(TileLists.UpUse);
                result = possible.Find(delegate(Tile t)
                {
                    if (!list.Contains(t.Ground.Id) && t.Items.Count > 0)
                    {
                        t.Ground.Id = t.Items.First().Id;
                    }

                    return list.Contains(t.Ground.Id);
                });

                #endregion
            }
            else
            {
                #region Going Down

                list = new List<uint>();
                list.AddRange(TileLists.Down);
                list.AddRange(TileLists.DownUse);
                list.AddRange(TileLists.Shovel);
                result = possible.Find(delegate(Tile t)
                {
                    if (!list.Contains(t.Ground.Id) && t.Items.Count > 0)
                    {
                        t.Ground.Id = t.Items.First().Id;
                    }

                    return list.Contains(t.Ground.Id);
                });

                #endregion
            }

            if (result == null)
            {
                return null;
            }

            if (TileLists.Down.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Stand, Kedrah);
            }
            if (TileLists.DownUse.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Ladder, Kedrah);
            }
            if (TileLists.Shovel.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Shovel, Kedrah);
            }
            if (TileLists.Rope.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Rope, Kedrah);
            }
            if (TileLists.Up.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Stand, Kedrah);
            }
            if (TileLists.UpUse.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Ladder, Kedrah);
            }

            return null;
        }

        private bool PerformWaypoint(Waypoint waypoint)
        {
            if (waypoint == null)
            {
                iterator++;
                return false;
            }

            #region Verification + Actions

            switch (waypoint.Type)
            {
                case WaypointType.Action:
                    return true;
                case WaypointType.Approach:
                    if (waypoint.Location.DistanceTo(Kedrah.Player.Location) <= 1)
                    {
                        return true;
                    }
                    break;
                case WaypointType.Ladder:
                    if (waypoint.Location.DistanceTo(Kedrah.Player.Location) <= 1)
                    {
                        Tile t = Kedrah.Map.GetTile(waypoint.Location);
                        if (t.Items.Count > 0)
                        {
                            t.Items.First().Use();
                        }
                        else
                        {
                            t.Ground.Use();
                        }
                        return true;
                    }
                    break;
                case WaypointType.Node:
                    if (waypoint.Location.DistanceTo(Kedrah.Player.Location) <= SkipNodes)
                    {
                        return true;
                    }
                    break;
                case WaypointType.Pick:
                    if (waypoint.Location.DistanceTo(Kedrah.Player.Location) == 1)
                    {
                        Kedrah.Inventory.UseItemOnTile(Pick.Id, Kedrah.Map.GetTile(waypoint.Location));
                        return true;
                    }
                    else if (waypoint.Location == Kedrah.Player.Location)
                    {
                        Kedrah.Player.GoTo = MovableDirection();
                    }
                    break;
                case WaypointType.Rope:
                    if (waypoint.Location.DistanceTo(Kedrah.Player.Location) <= 1)
                    {
                        Kedrah.Inventory.UseItemOnTile(Rope.Id, Kedrah.Map.GetTile(waypoint.Location));
                        return true;
                    }
                    break;
                case WaypointType.Shovel:
                    if (waypoint.Location.DistanceTo(Kedrah.Player.Location) == 1)
                    {
                        Kedrah.Inventory.UseItemOnTile(Shovel.Id, Kedrah.Map.GetTile(waypoint.Location));
                        return true;
                    }
                    else if (waypoint.Location == Kedrah.Player.Location)
                    {
                        Kedrah.Player.GoTo = MovableDirection();
                    }
                    break;
                case WaypointType.Stand:
                    if (waypoint.Location == Kedrah.Player.Location)
                    {
                        return true;
                    }
                    break;
                case WaypointType.Walk:
                    return true;
            }

            #endregion

            if (!Kedrah.Player.IsWalking)
            {
                if (waypoint.Type == WaypointType.Approach || waypoint.Type == WaypointType.Ladder || waypoint.Type == WaypointType.Pick || waypoint.Type == WaypointType.Rope || waypoint.Type == WaypointType.Shovel)
                {
                    Kedrah.Player.GoTo = NearestAjacent(waypoint.Location);
                }
                else if (waypoint.Type != WaypointType.Action)
                {
                    Kedrah.Player.GoTo = waypoint.Location;
                }
            }

            return false;
        }

        #endregion

        #region Timers

        public void Walk_OnExecute()
        {
            if (Kedrah.Player.RedSquare != 0 || Kedrah.Modules.Targeting.IsTargeting || Kedrah.Modules.WaitStatus != WaitStatus.Idle)
            {
                return;
            }

            #region Open Bodies

            if (LootBodies.Count > 0)
            {
                LootBodies.RemoveAll(delegate(Item i) { return (i.Location.GroundLocation.Z != Kedrah.Player.Z); });
                LootBodies.Sort(new Comparison<Item>(delegate(Item i1, Item i2) { return i1.Location.GroundLocation.DistanceTo(Kedrah.Player.Location).CompareTo(i2.Location.GroundLocation.DistanceTo(Kedrah.Player.Location)); }));

                if (!LootBodies[0].Location.GroundLocation.IsAdjacentTo(Kedrah.Player.Location) && !Kedrah.Player.IsWalking)
                {
                    PerformWaypoint(new Waypoint(NearestAjacent(LootBodies[0].Location.GroundLocation), WaypointType.Approach, Kedrah));
                }
                else if (!Kedrah.Player.IsWalking)
                {
                    Kedrah.Modules.WaitStatus = WaitStatus.OpenContainer;
                    LootBodies[0].OpenAsContainer((byte)Kedrah.Inventory.GetContainers().Count());
                    LootBodies.RemoveAt(0);
                }

                return;
            }

            #endregion

            if (Waypoints.Count <= 0)
            {
                return;
            }

            if (iterator >= Waypoints.Count)
            {
                iterator = 0;
            }

            Waypoint waypoint = Waypoints[iterator];

            if (!Kedrah.Player.IsWalking)
            {
                if (waypoint.Type != WaypointType.Action && waypoint.Location.Z != Kedrah.Player.Z)
                {
                    PerformWaypoint(FloorChangerWaypoint(waypoint.Location));
                    return;
                }
            }

            if (PerformWaypoint(waypoint))
            {
                iterator++;
            }
        }

        #endregion
    }

    public enum WaypointType
    {
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
            int comparisson = other.Location.DistanceTo(Kedrah.Player.Location).CompareTo(other.Location.DistanceTo(Kedrah.Player.Location));

            return comparisson;
        }

        public override string ToString()
        {
            return Type.ToString() + ": " + Location.ToString();
        }
    }
}
