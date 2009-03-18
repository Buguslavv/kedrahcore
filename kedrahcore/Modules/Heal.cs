using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah.Modules
{
    public class Heal : Module
    {
        #region Variables/Objects

        hList<ItemPercent> potionLife, runeLife, potionMana;
        hList<SpellPercent> spellLife, reverseSpellLife;
        DateTime spellNext = DateTime.Now, potionNext = DateTime.Now;
        bool poison = false, paralyze = false;
        uint potionExhaustion = 700, spellExhaustion = 1080, spellPoisonMana = 30;
        string spellPoison = Tibia.Constants.Spells.Antidote.Words;

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// Heal module constructor.
        /// </summary>
        public Heal(Core core)
            : base(core)
        {
            potionLife = new hList<ItemPercent>();
            runeLife = new hList<ItemPercent>();
            potionMana = new hList<ItemPercent>();
            spellLife = new hList<SpellPercent>();

            #region Timers
            
            // Main timer
            timers.Add("healer", new Tibia.Util.Timer(100, false));
            timers["healer"].Execute += new Tibia.Util.Timer.TimerExecution(healer_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool Healer
        {
            get
            {
                if (timers["healer"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    PlayTimer("healer");
                else
                    PauseTimer("healer");
            }
        }

        public bool Poison
        {
            get
            {
                return poison;
            }
            set
            {
                poison = value;
            }
        }

        public bool Paralyze
        {
            get
            {
                return paralyze;
            }
            set
            {
                paralyze = value;
            }
        }

        public string SpellPoisonMana
        {
            get
            {
                return spellPoison;
            }
            set
            {
                spellPoison = value;
            }
        }

        public uint SpellPoisonWords
        {
            get
            {
                return spellPoisonMana;
            }
            set
            {
                spellPoisonMana = value;
            }
        }

        public uint PotionExhaustion
        {
            get
            {
                return potionExhaustion;
            }
            set
            {
                potionExhaustion = value;
            }
        }

        public uint SpellExhaustion
        {
            get
            {
                return spellExhaustion;
            }
            set
            {
                spellExhaustion = value;
            }
        }

        public hList<ItemPercent> PotionLife
        {

            get
            {
                return potionLife;
            }
            set
            {
                potionLife = value;
            }
        }

        public hList<ItemPercent> PotionMana
        {
            get
            {
                return potionMana;
            }
            set
            {
                potionMana = value;
            }
        }

        public hList<ItemPercent> RuneLife
        {
            get
            {
                return runeLife;
            }
            set
            {
                runeLife = value;
            }
        }

        public hList<SpellPercent> SpellLife
        {
            get
            {
                spellLife.Sort(new Comparison<SpellPercent>(compareSpellPercents));
                reverseSpellLife = spellLife;
                reverseSpellLife.Reverse();
                return spellLife;
            }
            set
            {
                spellLife = value;
            }
        }

        #endregion

        #region Module Functions

        int compareSpellPercents(SpellPercent sp1, SpellPercent sp2)
        {
            return sp1.Percent == sp2.Percent ? 0 : sp1.Percent > sp2.Percent ? 1 : -1;
        }

        #endregion

        #region Timers

        void healer_OnExecute()
        {
            int cP = 0;

            if (!kedrah.Client.LoggedIn) return;

            if (potionNext.CompareTo(DateTime.Now) <= 0)
            {
                foreach (ItemPercent pot in potionLife)
                {
                    if (kedrah.Player.HPBar <= pot.Percent)
                        potionNext = kedrah.Inventory.UseItemOnSelf(pot.Item.Id) ? DateTime.Now.AddMilliseconds(potionExhaustion) : DateTime.Now;
                }
                foreach (ItemPercent pot in potionMana)
                {
                    cP = kedrah.Player.Mana * 100 / kedrah.Player.Mana_Max;
                    if (cP <= pot.Percent)
                        potionNext = kedrah.Inventory.UseItemOnSelf(pot.Item.Id) ? DateTime.Now.AddMilliseconds(potionExhaustion) : DateTime.Now;
                }
            }
            if (spellNext.CompareTo(DateTime.Now) <= 0)
            {
                foreach (ItemPercent rune in runeLife)
                {
                    if (kedrah.Player.HPBar <= rune.Percent)
                        spellNext = kedrah.Inventory.UseItemOnSelf(rune.Item.Id) ? DateTime.Now.AddMilliseconds(spellExhaustion) : DateTime.Now;
                }
                foreach (SpellPercent spell in reverseSpellLife)
                {
                    if (kedrah.Player.HPBar <= spell.Percent && kedrah.Player.Mana >= spell.Mana)
                        spellNext = kedrah.Console.Say(spell.Spell) ? DateTime.Now.AddMilliseconds(spellExhaustion) : DateTime.Now;
                }
                if (poison && kedrah.Player.HasFlag(Tibia.Constants.Flag.Poisoned) && kedrah.Player.Mana >= spellPoisonMana)
                    spellNext = kedrah.Console.Say(spellPoison) ? DateTime.Now.AddMilliseconds(spellExhaustion) : DateTime.Now;
                if (paralyze && kedrah.Player.HasFlag(Tibia.Constants.Flag.Paralyzed))
                    spellNext = kedrah.Console.Say(reverseSpellLife[0].Spell) ? DateTime.Now.AddMilliseconds(spellExhaustion) : DateTime.Now;
            }
        }

        #endregion
    }

    public class hList<T> : List<T>
    {
        public void RemoveDuplicates()
        {
            Dictionary<T, int> uniqueStore = new Dictionary<T, int>();
            List<T> inputList = new List<T>(this);
            this.Clear();

            foreach (T currValue in inputList)
            {
                if (!uniqueStore.ContainsKey(currValue))
                {
                    uniqueStore.Add(currValue, 0);
                    this.Add(currValue);
                }
            }
        }

        public void Insert(T item)
        {
            base.Add(item);
            this.RemoveDuplicates();
        }
    }

    public struct ItemPercent
    {
        public uint Percent;
        public Tibia.Objects.Item Item;

        public ItemPercent(uint percent, Tibia.Objects.Item item)
        {
            this.Percent = percent;
            this.Item = item;
        }
    }

    public struct SpellPercent
    {
        public uint Percent;
        public string Spell;
        public uint Mana;

        public SpellPercent(uint percent, string spell, uint mana)
        {
            this.Percent = percent;
            this.Spell = spell;
            this.Mana = mana;
        }
    }
}
