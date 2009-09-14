using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using Tibia.Constants;
using System.Threading;
using Kedrah.Objects;
using Kedrah.Constants;

namespace Kedrah.Modules
{
    public class Cavebot : Module
    {
        #region Variables/Objects

        private int iterator = 0;
        public List<Waypoint> Waypoints = new List<Waypoint>();
        public int SkipNodes = 3;
        public Item Pick = Items.Tool.Pick;
        public Item Rope = Items.Tool.Rope;
        public Item Shovel = Items.Tool.Shovel;

        #endregion

        #region Constructor/Destructor

        public Cavebot(ref Core core)
            : base(ref core)
        {
            Pick = Items.Tool.Pick;
            Rope = Items.Tool.Rope;
            Shovel = Items.Tool.Shovel;

            #region Timers

            Timers.Add("walk", new Tibia.Util.Timer(1000, false));
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
            tiles.Add(Core.Map.GetTile(Core.Player.Location.Offset(1, 0, 0)));
            tiles.Add(Core.Map.GetTile(Core.Player.Location.Offset(-1, 0, 0)));
            tiles.Add(Core.Map.GetTile(Core.Player.Location.Offset(0, 1, 0)));
            tiles.Add(Core.Map.GetTile(Core.Player.Location.Offset(0, -1, 0)));

            foreach (Tile tile in tiles)
            {
                if (!tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.BlocksPath) && !tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.Blocking) && !tile.Ground.GetFlag(Tibia.Addresses.DatItem.Flag.Floorchange))
                {
                    return tile.Location;
                }
            }

            return Core.Player.Location.Offset(1, 1, 0);
        }

        private Location NearestAjacent(Location location)
        {
            List<Location> locations = new List<Location> {
                location.Offset(-1,-1,0),
                location.Offset(-1,0,0),
                location.Offset(-1,1,0),
                location.Offset(0,-1,0),
                location.Offset(0,1,0),
                location.Offset(1,-1,0),
                location.Offset(1,0,0),
                location.Offset(1,1,0),
            };

            foreach (Location l in locations)
            {
                if (!location.IsValid() || l.Distance() < location.Distance()
)
                {
                    if (Core.Map.GetTile(l).Ground.GetFlag(Tibia.Addresses.DatItem.Flag.Blocking | Tibia.Addresses.DatItem.Flag.BlocksPath))
                    {
                        continue;
                    }

                    location = l;
                }
            }

            return location;
        }

        private Waypoint FloorChangerWaypoint(Location destination)
        {
            sbyte change = (sbyte)(destination.Z - Core.Player.Z);

            if (Math.Abs(change) > 1)
            {
                return null;
            }

            List<uint> list = null;
            Tile result = null;
            List<Tile> possible = Core.Map.GetTilesOnSameFloor().Where(t => t.Location.Distance() < 27).ToList();
            possible.Sort(new Comparison<Tile>(delegate(Tile t1, Tile t2) { return t1.Location.DistanceBetween(destination).CompareTo(t2.Location.DistanceBetween(destination)); }));

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
                return new Waypoint(result.Location, WaypointType.Stand, Core);
            }
            if (TileLists.DownUse.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Use, Core);
            }
            if (TileLists.Shovel.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Shovel, Core);
            }
            if (TileLists.Rope.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Rope, Core);
            }
            if (TileLists.Up.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Stand, Core);
            }
            if (TileLists.UpUse.Contains(result.Ground.Id))
            {
                return new Waypoint(result.Location, WaypointType.Use, Core);
            }

            return null;
        }

        public bool PerformWaypoint(Waypoint waypoint)
        {
            if (waypoint == null)
            {
                iterator++;
                return false;
            }

            if (Core.Player.Z != waypoint.Location.Z)
            {
                return true;
            }

            #region Verification + Actions

            switch (waypoint.Type)
            {
                case WaypointType.Action:
                    return true;
                case WaypointType.Approach:
                    if (waypoint.Location.IsAdjacent())
                    {
                        return true;
                    }
                    break;
                case WaypointType.OpenBody:
                    if (waypoint.Location.IsAdjacent())
                    {
                        Tile t = Core.Map.GetTile(waypoint.Location);
                        if (t.Items.Count > 0)
                        {
                            Item item = t.Items.FirstOrDefault(i => i.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer));
                            if (item != null)
                            {
                                item.Use();
                            }
                        }
                        return true;
                    }
                    break;
                case WaypointType.Use:
                    if (waypoint.Location.IsAdjacent())
                    {
                        Tile t = Core.Map.GetTile(waypoint.Location);
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
                    if (waypoint.Location.Distance() <= SkipNodes)
                    {
                        return true;
                    }
                    break;
                case WaypointType.Pick:
                    if (waypoint.Location.Distance() == 1)
                    {
                        Core.Inventory.UseItemOnTile(Pick.Id, Core.Map.GetTile(waypoint.Location));
                        return true;
                    }
                    else if (waypoint.Location == Core.Player.Location)
                    {
                        Core.Player.GoTo = MovableDirection();
                    }
                    break;
                case WaypointType.Rope:
                    if (waypoint.Location.IsAdjacent())
                    {
                        Core.Inventory.UseItemOnTile(Rope.Id, Core.Map.GetTile(waypoint.Location));
                        return true;
                    }
                    break;
                case WaypointType.Shovel:
                    if (waypoint.Location.Distance() == 1)
                    {
                        Core.Inventory.UseItemOnTile(Shovel.Id, Core.Map.GetTile(waypoint.Location));
                        return true;
                    }
                    else if (waypoint.Location == Core.Player.Location)
                    {
                        Core.Player.GoTo = MovableDirection();
                    }
                    break;
                case WaypointType.Stand:
                    if (waypoint.Location == Core.Player.Location)
                    {
                        return true;
                    }
                    break;
                case WaypointType.Walk:
                    return true;
            }

            #endregion


            if (Core.Modules.Targeting.IsTargeting)
            {
                return false;
            }

            if (waypoint.Type == WaypointType.Approach || waypoint.Type == WaypointType.OpenBody || waypoint.Type == WaypointType.Pick || waypoint.Type == WaypointType.Rope || waypoint.Type == WaypointType.Shovel || waypoint.Type == WaypointType.Use)
            {
                Core.Player.GoTo = NearestAjacent(waypoint.Location);
            }
            else if (waypoint.Type != WaypointType.Action)
            {
                Core.Player.GoTo = waypoint.Location;
            }

            return false;
        }

        #endregion

        #region Timers

        public void Walk_OnExecute()
        {
            if (Core.Player.IsWalking || Core.Modules.Targeting.IsTargeting || Core.Modules.Looter.IsLooting)
            {
                return;
            }

            if (Core.Modules.Looter.LootBodies.Count > 0 || Waypoints.Count <= 0)
            {
                return;
            }

            if (iterator >= Waypoints.Count)
            {
                iterator = 0;
            }

            Waypoint waypoint = Waypoints[iterator];

            if (!Core.Player.IsWalking)
            {
                if (waypoint.Type != WaypointType.Action && waypoint.Location.Z != Core.Player.Z)
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
}
