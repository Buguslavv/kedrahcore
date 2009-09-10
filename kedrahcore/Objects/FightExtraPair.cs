using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kedrah.Constants;
using Tibia.Objects;
using Tibia.Constants;

namespace Kedrah.Objects
{
    public class FightExtraPair
    {
        private FightExtra Type;
        private Item Item;
        private SlotNumber Slot;
        private string Spell;
        private Dictionary<DamageType, string> SpellTypes;

        public FightExtraPair(FightExtra type, Item item)
        {
            this.Type = type;
            this.Item = item;
            this.Slot = SlotNumber.Ammo;
            this.Spell = "";
        }

        public FightExtraPair(FightExtra type, Item item, SlotNumber slot)
        {
            this.Type = type;
            this.Item = item;
            this.Slot = slot;
            this.Spell = "";
        }

        public FightExtraPair(FightExtra type, string spell)
        {
            this.Type = type;
            this.Item = null;
            this.Slot = SlotNumber.Ammo;
            this.Spell = spell;
        }

        public FightExtraPair(FightExtra type, Dictionary<DamageType, string> spellTypes)
        {
            this.Type = type;
            this.Item = null;
            this.Slot = SlotNumber.Ammo;
            this.SpellTypes = spellTypes;
        }

        public void Execute(Creature creature, Inventory inventory)
        {
            switch (this.Type)
            {
                case FightExtra.ItemEquip:
                    ItemLocation loc = new ItemLocation();
                    this.Item.Move(inventory.GetItemInSlot(this.Slot).Location);
                    break;
                case FightExtra.ItemUse:
                    this.Item.Use(creature);
                    break;
                case FightExtra.Spell:
                    this.Item.Client.Console.Say(this.Spell);
                    break;
                case FightExtra.AutoSpell:
                    DamageType type = creature.Data.GetWeakness(SpellTypes.Keys.ToList());
                    this.Item.Client.Console.Say(SpellTypes[type]);
                    break;
            }
        }
    }
}
