using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Packets;
using Tibia.Packets.Incoming;
using Tibia.Util;
using Tibia.Objects;
using System.Threading;

namespace Kedrah.Modules {
    public class Looter : Module {
        #region Variables/Objects

        private static ushort maxTries = 10;
        private static AutoResetEvent lootEvent = new AutoResetEvent(false);
        private Queue<byte> lootContainers = new Queue<byte>();
        public bool EatFromMonsters = true;
        public bool OpenBodies = true;
        public bool OpenDistantBodies = true;
        public bool OpenNextContainer = true;
        public List<LootBody> LootBodies = new List<LootBody>();
        public List<LootItem> LootItems = new List<LootItem>();

        #endregion

        #region Constructor/Destructor

        public Looter(Core core)
            : base(core) {
            for (ushort i = 0; i < 9000; i++) {
                if (i != (ushort)Tibia.Constants.Items.Container.BagBrown.Id && i != (ushort)Tibia.Constants.Items.Food.Meat.Id && i != (ushort)Tibia.Constants.Items.Food.Ham.Id)
                    LootItems.Add(new LootItem(i, 0, ""));
            }

            Kedrah.Proxy.ReceivedContainerOpenIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedContainerOpenIncomingPacket);
            Kedrah.Proxy.ReceivedTileAddThingIncomingPacket += new Proxy.IncomingPacketListener(Proxy_ReceivedTileAddThingIncomingPacket);

            #region Timers

            Timers.Add("looting", new Tibia.Util.Timer(100, false));
            Timers["looting"].Execute += new Tibia.Util.Timer.TimerExecution(Looting_OnExecute);
            Timers.Add("openBodies", new Tibia.Util.Timer(100, false));
            Timers["openBodies"].Execute += new Tibia.Util.Timer.TimerExecution(OpenBodies_OnExecute);

            #endregion
        }

        #endregion

        #region Module Functions

        private void AdjustStackOrder(IEnumerable<Tibia.Objects.Item> cItems, Tibia.Objects.Item item) {
            foreach (Item i in cItems) {
                if (i.Location.StackOrder > item.Location.StackOrder) {
                    i.Location.StackOrder--;
                    i.Location.ContainerSlot--;
                }
            }
        }

        private bool IsLootContainer(byte number) {
            Tibia.Objects.Container container = Kedrah.Inventory.GetContainer(number);
            Tibia.Objects.Item item = new Tibia.Objects.Item(Kedrah.Client, (uint)container.Id);

            if ((number == 0) || (Tibia.Constants.ItemLists.Containers.ContainsKey(item.Id) && !(item.Id == Tibia.Constants.Items.Container.BagBrown.Id && container.HasParent)))
                return false;

            return true;
        }

        private bool Proxy_ReceivedContainerOpenIncomingPacket(IncomingPacket packet) {
            if (Looting && LootItems.Count > 0) {
                ContainerOpenPacket p = (ContainerOpenPacket)packet;

                if (IsLootContainer(p.Id))
                    lootContainers.Enqueue(p.Id);
            }

            return true;
        }

        bool Proxy_ReceivedTileAddThingIncomingPacket(Tibia.Packets.IncomingPacket packet) {
            if (Looting && OpenBodies) {
                TileAddThingPacket p = (TileAddThingPacket)packet;

                if (p.Item != null && (OpenDistantBodies || p.Position.IsAdjacentTo(Kedrah.Player.Location))) {
                    if (p.Item.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer))
                        LootBodies.Add(new LootBody(p.Item, Kedrah));
                }
            }

            return true;
        }

        private void GetItem(Item item, Container container) {
            if (item.GetFlag(Tibia.Addresses.DatItem.Flag.IsStackable)) {
                var lootContainerItem = container.GetItems().FirstOrDefault(lCItem => lCItem.Id == item.Id && lCItem.Count < 100);

                if (lootContainerItem != null && (lootContainerItem.Count + item.Count <= 100 || container.Amount < container.Volume)) {
                    item.Move(lootContainerItem.Location);
                    AdjustStackOrder(container.GetItems(), item);
                }
                else if (lootContainerItem == null && container.Amount < container.Volume) {
                    var itemLocation = new ItemLocation();
                    itemLocation.Type = Tibia.Constants.ItemLocationType.Container;
                    itemLocation.ContainerId = container.Number;
                    itemLocation.ContainerSlot = (byte)(container.Volume - 1);
                    item.Move(itemLocation);
                    AdjustStackOrder(container.GetItems(), item);
                }
                else if (OpenNextContainer) {
                    if (lootContainerItem != null) {
                        item.Move(lootContainerItem.Location);
                    }

                    var newContainer = container.GetItems().FirstOrDefault(newItemContainer => newItemContainer.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer));

                    if (newContainer != null) {
                        newContainer.OpenAsContainer(container.Number);
                    }
                }
            }
            else {
                if (container.Amount < container.Volume) {
                    var itemLocation = new ItemLocation();
                    itemLocation.Type = Tibia.Constants.ItemLocationType.Container;
                    itemLocation.ContainerId = container.Number;
                    itemLocation.ContainerSlot = (byte)(container.Volume - 1);
                    item.Move(itemLocation);
                    AdjustStackOrder(container.GetItems(), item);
                }
                else if (OpenNextContainer) {
                    var newContainer = container.GetItems().FirstOrDefault(newItemContainer => newItemContainer.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer));

                    if (newContainer != null) {
                        newContainer.OpenAsContainer(container.Number);
                    }
                }
            }
        }

        private Container GetLootContainer(Item item) {
            foreach (Container lootContainer in Kedrah.Inventory.GetContainers()) {
                if (!IsLootContainer(lootContainer.Number)) {
                    if (lootContainer.Amount < lootContainer.Volume) {
                        return lootContainer;
                    }
                    else if (item.GetFlag(Tibia.Addresses.DatItem.Flag.IsStackable)) {
                        var lootContainerItem = lootContainer.GetItems().FirstOrDefault(lCItem => lCItem.Id == item.Id && lCItem.Count < 100);

                        if ((lootContainerItem != null && (lootContainerItem.Count + item.Count <= 100 || lootContainer.Amount < lootContainer.Volume)) ||
                            (lootContainerItem == null && lootContainer.Amount < lootContainer.Volume)) {
                            return lootContainer;
                        }
                    }

                    if (OpenNextContainer && lootContainer.Amount >= lootContainer.Volume) {
                        var newContainer = lootContainer.GetItems().FirstOrDefault(newItemContainer => newItemContainer.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer));

                        if (newContainer != null) {
                            newContainer.OpenAsContainer(lootContainer.Number);
                            Thread.Sleep(100);
                            return null;
                        }
                    }
                }
            }

            return null;
        }

        private void Loot(byte number) {
            Kedrah.Player.Stop();

            Container container = Kedrah.Inventory.GetContainer(number);

            if (container == null || !IsLootContainer(number))
                return;

            IEnumerable<Item> containterEnumerable = container.GetItems();
            List<Item> containerItems = containterEnumerable.ToList();

            while (containerItems.Count > 0) {
                Item item = containerItems.Last();
                LootItem lootItem = LootItems.Find(delegate(LootItem loot) { return loot.Id == item.Id; });

                if (lootItem != null) {
                    Container lootContainer = null;

                    #region Select container

                    if (lootItem.Container == 0)
                        lootContainer = GetLootContainer(item);
                    else
                        lootContainer = Kedrah.Inventory.GetContainer(lootItem.Container);

                    #endregion

                    if (lootContainer == null)
                        continue;

                    int startAmmount = container.Amount;

                    for (int i = 0; i < maxTries && container.Amount == startAmmount; i++) {
                        GetItem(item, lootContainer);
                        Thread.Sleep(100);
                    }
                }

                containerItems.Remove(item);
            }

            #region Eat Foot

            if (EatFromMonsters) {
                Item food = containterEnumerable.FirstOrDefault(i => Tibia.Constants.ItemLists.Foods.ContainsKey(i.Id));

                if (food != null)
                    for (int i = 0; i < food.Count; i++) {
                        food.Use();
                        Thread.Sleep(100);
                    }
            }

            #endregion

            #region Open bag / close container

            Item bag = containterEnumerable.LastOrDefault(i => i.GetFlag(Tibia.Addresses.DatItem.Flag.IsContainer));

            if (bag != null) {
                bag.OpenAsContainer(container.Number);
            }
            else {
                container.Close();
            }
            Thread.Sleep(100);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool Looting {
            get {
                if (Timers["looting"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set {
                if (value) {
                    PlayTimer("looting");
                    PlayTimer("openBodies");
                }
                else {
                    PauseTimer("looting");
                    PauseTimer("openBodies");
                }
            }
        }

        #endregion

        #region Timers

        private void Looting_OnExecute() {
            if (Kedrah.Client.LoggedIn && lootContainers.Count > 0)
                Loot(lootContainers.Dequeue());
        }

        public void OpenBodies_OnExecute() {
            if (!Kedrah.Client.LoggedIn)
                return;
            if (Kedrah.Player.IsWalking)
                return;

            if (lootContainers.Count == 0 && LootBodies.Count > 0)
                LootBodies.Sort();
            else
                return;

            if (LootBodies[0].Body.Location.GroundLocation.IsAdjacentTo(Kedrah.Player.Location) && !Kedrah.Player.IsWalking) {
                LootBodies[0].Body.OpenAsContainer((byte)Kedrah.Inventory.GetContainers().Count());
                Thread.Sleep(100);
                LootBodies.RemoveAt(0);
            }
        }

        #endregion
    }

    public class LootItem {
        public ushort Id;
        public byte Container;
        public string Description;

        public LootItem() { }

        public LootItem(ushort id, byte container, string description) {
            Id = id;
            Container = container;
            Description = description;
        }

        public override string ToString() {
            return Id.ToString() + " Container " + (Container + 1).ToString() + " (" + Description + ")";
        }
    }

    public class LootBody : IComparable<LootBody> {
        public Item Body;
        public Core Kedrah;

        public LootBody(Item body, Core core) {
            Body = body;
            Kedrah = core;
        }

        public int CompareTo(LootBody other) {
            int comparisson = Body.Location.GroundLocation.DistanceTo(Kedrah.Player.Location).CompareTo(other.Body.Location.GroundLocation.DistanceTo(Kedrah.Player.Location));

            if (comparisson == 0) {
                List<Item> tileItems = Kedrah.Map.GetTile(Body.Location.GroundLocation).Items;
                List<Item> tileOtherItems = Kedrah.Map.GetTile(other.Body.Location.GroundLocation).Items;

                if (tileItems.Count > 0)
                    comparisson = (tileItems.Last().Id == Body.Id) ? -1 : 0;

                if (comparisson == 0 && tileOtherItems.Count > 0)
                    comparisson = (tileOtherItems.Last().Id == other.Body.Id) ? 1 : 0;
            }

            return comparisson;
        }
    }
}
