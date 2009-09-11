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
        private static AutoResetEvent lootEvent = new AutoResetEvent(false);
        private uint waitDrop = 0;
        private Queue<byte> lootContainers = new Queue<byte>();
        private Location lastBody = Location.Invalid;
        public bool EatFromMonsters = true;
        public OpenBodyRule OpenBodies = OpenBodyRule.Filtered;
        public bool OpenDistantBodies = true;
        public bool OpenNextContainer = true;
        public List<LootItem> LootItems = new List<LootItem>();

        #endregion

        #region Constructor/Destructor

        public Looter(ref Core core)
            : base(ref core)
        {
            Kedrah.Proxy.ReceivedContainerOpenIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedContainerOpenIncomingPacket);
            Kedrah.Proxy.ReceivedTileAddThingIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedTileAddThingIncomingPacket);
            Kedrah.Proxy.ReceivedTextMessageIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedTextMessageIncomingPacket);
            Kedrah.Proxy.ReceivedContainerAddItemIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedContainerAddItemIncomingPacket);

            #region Timers

            Timers.Add("looting", new Tibia.Util.Timer(100, false));
            Timers["looting"].Execute += new Tibia.Util.Timer.TimerExecution(Looting_OnExecute);

            #endregion
        }

        #endregion

        #region Proxy Hooks

        private bool Proxy_ReceivedContainerAddItemIncomingPacket(IncomingPacket packet)
        {
            lootEvent.Set();

            return true;
        }

        private bool Proxy_ReceivedContainerOpenIncomingPacket(IncomingPacket packet)
        {
            if (Looting && LootItems.Count > 0)
            {
                Kedrah.Modules.WaitStatus = WaitStatus.Idle;
                ContainerOpenPacket p = (ContainerOpenPacket)packet;
                lootContainers.Enqueue(p.Id);
            }

            return true;
        }

        bool Proxy_ReceivedTileAddThingIncomingPacket(Tibia.Packets.IncomingPacket packet)
        {
            TileAddThingPacket p = (TileAddThingPacket)packet;

            if (p.Position == Kedrah.Player.Location && p.Item != null && p.Item.Id == waitDrop)
            {
                waitDrop = 0;
                lootEvent.Set();
            }

            if (Looting && OpenBodies != OpenBodyRule.None)
            {
                if (p.Item != null && (OpenDistantBodies || p.Position.IsAdjacent()))
                {
                    if (p.Item.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer) && p.Item.GetFlag(Tibia.Addresses.DatItem.Flag.IsCorpse) && p.Position.Z == Kedrah.Player.Z)
                    {
                        if (OpenBodies == OpenBodyRule.All)
                        {
                            Kedrah.Modules.Cavebot.LootBodies.Add(p.Position);
                        }
                        else
                        {
                            if (lastBody.IsValid())
                            {
                                Kedrah.Modules.Cavebot.LootBodies.Add(lastBody);
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
            if (Looting && lastBody.IsValid())
            {
                TextMessagePacket p = (TextMessagePacket)packet;

                if (OpenBodies == OpenBodyRule.Allowed)
                {
                    Kedrah.Modules.Cavebot.LootBodies.Add(lastBody);
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
                                Kedrah.Modules.Cavebot.LootBodies.Add(lastBody);
                                lastBody = Location.Invalid;
                                return true;
                            }
                        }
                    }

                    foreach (LootItem item in LootItems)
                    {
                        if (p.Message.ToLower().Contains(item.Description.ToLower()))
                        {
                            Kedrah.Modules.Cavebot.LootBodies.Add(lastBody);
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

        private bool IsLootContainer(byte number)
        {
            Container container = Kedrah.Inventory.GetContainer(number);

            if ((number == 0) || (ItemLists.Container.ContainsKey((uint)container.Id) && !(container.Id == Items.Container.NormalBag.Id && container.HasParent)))
            {
                return false;
            }

            return true;
        }

        private void OpenNewContainer(Item container, byte number)
        {
            Kedrah.Modules.WaitStatus = WaitStatus.OpenContainer;
            container.OpenAsContainer(number);

            while (Kedrah.Modules.WaitStatus == WaitStatus.OpenContainer)
            {
                Thread.Sleep(100);
            }

            Kedrah.Modules.WaitStatus = WaitStatus.LootItems;
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
            foreach (Container lootContainer in Kedrah.Inventory.GetContainers())
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
                            newContainer.OpenAsContainer(lootContainer.Number);
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
            if (Kedrah.Modules.WaitStatus != WaitStatus.Idle)
            {
                return;
            }

            Container container = Kedrah.Inventory.GetContainer(number);

            if (container == null || !IsLootContainer(number))
                return;

            Kedrah.Modules.WaitStatus = WaitStatus.LootItems;
            Kedrah.Player.Stop();

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
                            waitDrop = item.Id;
                            item.Move(ItemLocation.FromLocation(Kedrah.Player.Location));

                            if (lootEvent.WaitOne(1000))
                            {
                                break;
                            }
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
                            lootContainer = Kedrah.Inventory.GetContainer(lootItem.Container);
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

                            if (lootEvent.WaitOne(1000))
                            {
                                break;
                            }
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
                Kedrah.Modules.WaitStatus = WaitStatus.OpenContainer;
            }
            else
            {
                container.Close();
                Kedrah.Modules.WaitStatus = WaitStatus.Idle;
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
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    PlayTimer("looting");
                else
                    PauseTimer("looting");
            }
        }

        #endregion

        #region Timers

        private void Looting_OnExecute()
        {
            if (Kedrah.Client.LoggedIn && lootContainers.Count > 0)
                Loot(lootContainers.Dequeue());
        }

        #endregion
    }
}
