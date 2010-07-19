using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tibia.Objects;
using System.Windows.Forms;

namespace Kedrah.Objects
{
    public class HealPercent : IComparable<HealPercent>
    {
        public byte Percent;
        public Item Item;
        public string Spell;
        public Keys[] Keys;
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

        public HealPercent(byte percent, params Keys[] keys)
            : this(percent)
        {
            Keys = keys;
        }

        public HealPercent(byte percent, uint mana, params Keys[] keys)
            : this(percent)
        {
            Keys = keys;
            Mana = mana;
        }

        public int CompareTo(HealPercent other)
        {
            return Percent.CompareTo(other.Percent);
        }
    }
}
