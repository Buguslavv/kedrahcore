using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah.Objects
{
    public class LootItem
    {
        public ushort Id;
        public byte Container;
        public string Description;

        public LootItem() { }

        public LootItem(ushort id, byte container, string description)
        {
            Id = id;
            Container = container;
            Description = description;
        }

        public override string ToString()
        {
            return Id.ToString() + " Container " + (Container + 1).ToString() + " (" + Description + ")";
        }
    }
}
