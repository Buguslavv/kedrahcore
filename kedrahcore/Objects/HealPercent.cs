using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;

namespace Kedrah.Objects
{
    public class HealPercent : IComparable<HealPercent>
    {
        public byte Percent;
        public Item Item;
        public string Spell;
        public uint Mana;

        public HealPercent(byte percent)
        {
            Percent = percent;
        }

        public HealPercent(byte percent, Item item)
            : this(percent)
        {
            Item = item;
        }

        public HealPercent(byte percent, string spell, uint mana)
            : this(percent)
        {
            Spell = spell;
            Mana = mana;
        }

        public int CompareTo(HealPercent other)
        {
            return Percent.CompareTo(other.Percent);
        }
    }
}
