using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Packets;
using Tibia.Packets.Incoming;
using Tibia.Util;
using Tibia.Objects;
using System.Threading;
using Tibia.Constants;
using Kedrah.Constants;
using Kedrah.Objects;

namespace Kedrah.Modules
{
    public class Looter : Module
    {
        #region Variables/Objects

        private static ushort maxTries = 10;
        private Location lastBody = Location.Invalid;
        public OpenBodyRule OpenBodies = OpenBodyRule.Filtered;
        public bool EatFromMonsters = true;
        public bool IsLooting = false;
        public bool OpenDistantBodies = true;
        public bool OpenNextContainer = true;
        public List<LootItem> LootItems = new List<LootItem>();
        public List<Location> LootBodies = new List<Location>();

        #endregion

        #region Constructor/Destructor

        public Looter(ref Core core)
            : base(ref core)
        {
            Core.Proxy.ReceivedContainerOpenIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedContainerOpenIncomingPacket);
            Core.Proxy.ReceivedTileAddThingIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedTileAddThingIncomingPacket);
            Core.Proxy.ReceivedTextMessageIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedTextMessageIncomingPacket);

            #region Timers

            Timers.Add("looting", new Tibia.Util.Timer(100, false));
            Timers["looting"].Execute += new Tibia.Util.Timer.TimerExecution(Looting_OnExecute);

            #endregion
        }

        #endregion

        #region Proxy Hooks

        private bool Proxy_ReceivedContainerOpenIncomingPacket(IncomingPacket packet)
        {
            if (Enabled && Looting && LootItems.Count > 0)
            {
                ContainerOpenPacket p = (ContainerOpenPacket)packet;
                Thread handler = new Thread(new ThreadStart(delegate()
                {
                    AutoResetEvent ev = new AutoResetEvent(false);
                    Thread thread = new Thread(new ThreadStart(delegate()
                    {
                        IsLooting = true;
                        Core.Player.Stop();
                        Thread.Sleep(100);
                        Loot(p.Id);
                    }));

                    thread.Priority = ThreadPriority.AboveNormal;
                    thread.Start();

                    if (!ev.WaitOne(2000))
                    {
                        thread.Abort();
                    }

                    IsLooting = false;
                }));
                handler.Start();
            }

            return true;
        }

        bool Proxy_ReceivedTileAddThingIncomingPacket(Tibia.Packets.IncomingPacket packet)
        {
            TileAddThingPacket p = (TileAddThingPacket)packet;

            if (Enabled && Looting && OpenBodies != OpenBodyRule.None)
            {
                if (p.Item != null && (OpenDistantBodies || p.Position.IsAdjacent()))
                {
                    if (p.Item.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer) && p.Item.GetFlag(Tibia.Addresses.DatItem.Flag.IsCorpse) && p.Position.Z == Core.Player.Z)
                    {
                        if (OpenBodies == OpenBodyRule.All)
                        {
                            LootBodies.Add(p.Position);
                        }
                        else
                        {
                            if (lastBody.IsValid())
                            {
                                LootBodies.Add(lastBody);
                                lastBody = Location.Invalid;
                            }

                            lastBody = p.Position;
                        }
                    }
                }
            }

            return true;
        }

        bool Proxy_ReceivedTextMessageIncomingPacket(Tibia.Packets.IncomingPacket packet)
        {
            if (Enabled && Looting && lastBody.IsValid())
            {
                TextMessagePacket p = (TextMessagePacket)packet;

                if (OpenBodies == OpenBodyRule.Allowed)
                {
                    LootBodies.Add(lastBody);
                    lastBody = Location.Invalid;
                }
                else
                {
                    if (EatFromMonsters)
                    {
                        foreach (var item in ItemLists.Foods)
                        {
                            if (p.Message.ToLower().Contains(item.Value.Name.ToLower()))
                            {
                                LootBodies.Add(lastBody);
                                lastBody = Location.Invalid;
                                return true;
                            }
                        }
                    }

                    foreach (LootItem item in LootItems)
                    {
                        if (p.Message.ToLower().Contains(item.Description.ToLower()))
                        {
                            LootBodies.Add(lastBody);
                            lastBody = Location.Invalid;
                            return true;
                        }
                    }
                }

                lastBody = Location.Invalid;
            }

            return true;
        }

        #endregion

        #region Module Functions

        #region Add Loot

        public void AddLootByRatio(double ratio)
        {
            AddLootByRatio(ratio, (byte)LootContainer.Any);
        }

        public void AddLootByRatio(double ratio, byte container)
        {
            foreach (var i in ItemDataLists.AllItems)
            {
                if (i.Value.ValueRatio >= ratio)
                {
                    LootItem lootItem = new LootItem((ushort)i.Value.Id, container, i.Value.Name);
                    LootItems.Add(lootItem);
                }
            }
        }

        public void AddLootByName(string name)
        {
            AddLootByName(name, (byte)LootContainer.Any);
        }

        public void AddLootByName(string name, byte container)
        {
            if (ItemDataLists.AllItems.ContainsKey(name))
            {
                LootItem lootItem = new LootItem((ushort)ItemDataLists.AllItems[name].Id, container, ItemDataLists.AllItems[name].Name);
                LootItems.Add(lootItem);
            }
        }

        public void AddLootById(uint id)
        {
            AddLootById(id, (byte)LootContainer.Any);
        }

        public void AddLootById(uint id, byte container)
        {
            if (ItemLists.AllItems.ContainsKey(id))
            {
                LootItem lootItem = new LootItem((ushort)ItemLists.AllItems[id].Id, container, ItemLists.AllItems[id].Name);
                LootItems.Add(lootItem);
            }
        }

        #endregion

        private bool IsLootContainer(byte number)
        {
            Container container = Core.Client.Inventory.GetContainer(number);

            if ((number == 0) || (ItemLists.Container.ContainsKey((uint)container.Id) && !(container.Id == Items.Container.NormalBag.Id && container.HasParent)))
            {
                return false;
            }

            return true;
        }

        private void OpenNewContainer(Item container, byte number)
        {
            container.OpenAsContainer(number);
            Thread.Sleep(100);
        }

        private void GetItem(Item item, Container container)
        {
            if (item.GetFlag(Tibia.Addresses.DatItem.Flag.IsStackable))
            {
                var lootContainerItem = container.GetItems().FirstOrDefault(lCItem => lCItem.Id == item.Id && lCItem.Count < 100);

                if (lootContainerItem != null && (lootContainerItem.Count + item.Count <= 100 || container.Amount < container.Volume))
                {
                    item.Move(lootContainerItem.Location);
                }
                else if (lootContainerItem == null && container.Amount < container.Volume)
                {
                    var itemLocation = new ItemLocation();
                    itemLocation.Type = ItemLocationType.Container;
                    itemLocation.ContainerId = container.Number;
                    itemLocation.ContainerSlot = (byte)(container.Volume - 1);
                    item.Move(itemLocation);
                }
                else if (OpenNextContainer)
                {
                    if (lootContainerItem != null)
                    {
                        item.Move(lootContainerItem.Location);
                    }

                    var newContainer = container.GetItems().FirstOrDefault(newItemContainer => newItemContainer.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer));

                    if (newContainer != null)
                    {
                        OpenNewContainer(newContainer, container.Number);
                    }
                }
            }
            else
            {
                if (container.Amount < container.Volume)
                {
                    var itemLocation = new ItemLocation();
                    itemLocation.Type = ItemLocationType.Container;
                    itemLocation.ContainerId = container.Number;
                    itemLocation.ContainerSlot = (byte)(container.Volume - 1);
                    item.Move(itemLocation);
                }
                else if (OpenNextContainer)
                {
                    var newContainer = container.GetItems().FirstOrDefault(newItemContainer => newItemContainer.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer));

                    if (newContainer != null)
                    {
                        OpenNewContainer(newContainer, container.Number);
                    }
                }
            }
        }

        private Container GetLootContainer(Item item)
        {
            foreach (Container lootContainer in Core.Client.Inventory.GetContainers())
            {
                if (!IsLootContainer(lootContainer.Number))
                {
                    if (lootContainer.Amount < lootContainer.Volume)
                    {
                        return lootContainer;
                    }
                    else if (item.GetFlag(Tibia.Addresses.DatItem.Flag.IsStackable))
                    {
                        var lootContainerItem = lootContainer.GetItems().FirstOrDefault(lCItem => lCItem.Id == item.Id && lCItem.Count < 100);

                        if ((lootContainerItem != null && (lootContainerItem.Count + item.Count <= 100 || lootContainer.Amount < lootContainer.Volume)) ||
                            (lootContainerItem == null && lootContainer.Amount < lootContainer.Volume))
                        {
                            return lootContainer;
                        }
                    }

                    if (OpenNextContainer && lootContainer.Amount >= lootContainer.Volume)
                    {
                        var newContainer = lootContainer.GetItems().FirstOrDefault(newItemContainer => newItemContainer.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer));

                        if (newContainer != null)
                        {
                            OpenNewContainer(newContainer, lootContainer.Number);

                            return null;
                        }
                    }
                }
            }

            return null;
        }

        private void Loot(byte number)
        {
            Container container = Core.Client.Inventory.GetContainer(number);

            if (container == null || !IsLootContainer(number))
            {
                return;
            }

            IEnumerable<Item> containterEnumerable = container.GetItems();
            List<Item> containerItems = containterEnumerable.ToList();

            while (containerItems.Count > 0)
            {
                Item item = containerItems.Last();
                LootItem lootItem = LootItems.Find(delegate(LootItem loot) { return loot.Id == item.Id; });

                if (lootItem != null)
                {
                    if (lootItem.Container == (byte)LootContainer.Ground)
                    {
                        int startAmmount = container.Amount;

                        for (int i = 0; i < maxTries && container.Amount == startAmmount; i++)
                        {
                            item.Move(ItemLocation.FromLocation(Core.Player.Location));
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        Container lootContainer = null;

                        #region Select container

                        if (lootItem.Container == (byte)LootContainer.Any)
                        {
                            lootContainer = GetLootContainer(item);
                        }
                        else
                        {
                            lootContainer = Core.Client.Inventory.GetContainer(lootItem.Container);
                        }

                        #endregion

                        if (lootContainer == null)
                        {
                            continue;
                        }

                        int startAmmount = container.Amount;

                        for (int i = 0; i < maxTries && container.Amount == startAmmount; i++)
                        {
                            GetItem(item, lootContainer);
                            Thread.Sleep(100);
                        }
                    }
                }

                containerItems.Remove(item);
            }

            #region Eat Foot

            if (EatFromMonsters)
            {
                Item food = containterEnumerable.FirstOrDefault(i => ItemLists.Foods.ContainsKey(i.Id));

                if (food != null)
                    for (int i = 0; i < food.Count; i++)
                    {
                        food.Use();
                        Thread.Sleep(300);
                    }
            }

            #endregion

            #region Open bag / close container

            Item bag = containterEnumerable.LastOrDefault(i => i.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer));

            if (bag != null)
            {
                OpenNewContainer(bag, container.Number);
            }
            else
            {
                container.Close();
            }

            Thread.Sleep(100);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool Looting
        {
            get
            {
                if (Timers["looting"].State == Tibia.Util.TimerState.Running)
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
                    PlayTimer("looting");
                }
                else
                {
                    PauseTimer("looting");
                }
            }
        }

        #endregion

        #region Timers

        private void Looting_OnExecute()
        {
            if (Core.Player.IsWalking || IsLooting)
            {
                return;
            }

            #region Open Bodies

            if (LootBodies.Count > 0 && !Core.Player.IsWalking)
            {
                LootBodies.Sort(new Comparison<Location>(delegate(Location l1, Location l2) { return l1.Distance().CompareTo(l2.Distance()); }));

                if (Core.Modules.Cavebot.PerformWaypoint(new Waypoint(LootBodies[0], WaypointType.OpenBody, Core)))
                {
                    LootBodies.RemoveAt(0);
                }

                return;
            }

            #endregion
        }

        #endregion
    }
}
