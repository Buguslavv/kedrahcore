using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah.Modules {
    public class Heal : Module {
        #region Variables/Objects

        private DateTime spellNext;
        private DateTime potionNext;
        private bool poison;
        private bool paralize;
        private ushort potionExhaustion;
        private ushort spellExhaustion;
        private ushort spellPoisonMana;
        private string spellPoison;

        public HealList<ItemPercent> PotionLife { 
            get;
            set;
        }
        public HealList<ItemPercent> RuneLife {
            get;
            set;
        }
        public HealList<ItemPercent> PotionMana {
            get;
            set;
        }
        public HealList<SpellPercent> SpellLife {
            get;
            set;
        }
        public DateTime SpellNext {
            get {
                return spellNext;
            }
            set {
                spellNext = DateTime.Now;
            }
        }
        public DateTime PotionNext {
            get {
                return potionNext;
            }
            set {
                 potionNext = DateTime.Now;
            }
        }
        public bool Poison {
            get {
                return poison;
            }
            set {
                poison = false;
            }
        }
        public bool Paralyze {
            get {
                return paralize;
            }
            set {
                 paralize = false;
            }
        }
        public ushort PotionExhaustion {
            get {
                return potionExhaustion;
            }
            set {
                potionExhaustion = 700;
            }
        }
        public ushort SpellExhaustion {
            get {
                return spellExhaustion;
            }
            set {
                spellExhaustion = 1080;
            }
        }
        public ushort SpellPoisonMana  {
            get {
                return spellPoisonMana;
            }
            set {
                spellPoisonMana = 30;
            }
        }
        public string SpellPoison {
            get {
                return spellPoison;
            }
            set {
                spellPoison = Tibia.Constants.Spells.Antidote.Words;
                }
        }

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// Heal module constructor.
        /// </summary>
        public Heal(Core core)
            : base(core) {
            PotionLife = new HealList<ItemPercent>();
            RuneLife = new HealList<ItemPercent>();
            PotionMana = new HealList<ItemPercent>();
            SpellLife = new HealList<SpellPercent>();

            #region Timers

            // Main timer
            timers.Add("healer", new Tibia.Util.Timer(100, false));
            timers["healer"].Execute += new Tibia.Util.Timer.TimerExecution(healer_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool Healer {
            get {
                if (timers["healer"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set {
                if (value)
                    PlayTimer("healer");
                else
                    PauseTimer("healer");
            }
        }

        #endregion

        #region Module Functions

        private int CompareSpellPercents(SpellPercent sp1, SpellPercent sp2) {
            return sp1.Percent == sp2.Percent ? 0 : sp1.Percent > sp2.Percent ? 1 : -1;
        }

        #endregion

        #region Timers

        void healer_OnExecute() {
            if (!kedrah.Client.LoggedIn)
                return;

            if (potionNext.CompareTo(DateTime.Now) <= 0) {
                foreach (ItemPercent potion in PotionLife)
                    if (kedrah.Player.HPBar <= potion.Percent)
                        PotionNext = kedrah.Inventory.UseItemOnSelf(potion.Item.Id) ? DateTime.Now.AddMilliseconds(PotionExhaustion) : DateTime.Now;

                foreach (ItemPercent potion in PotionMana)
                    if ((kedrah.Player.Mana * 100 / kedrah.Player.Mana_Max) <= potion.Percent)
                        PotionNext = kedrah.Inventory.UseItemOnSelf(potion.Item.Id) ? DateTime.Now.AddMilliseconds(PotionExhaustion) : DateTime.Now;

            }
            if (SpellNext.CompareTo(DateTime.Now) <= 0) {
                foreach (ItemPercent rune in RuneLife)
                    if (kedrah.Player.HPBar <= rune.Percent)
                        SpellNext = kedrah.Inventory.UseItemOnSelf(rune.Item.Id) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;

                foreach (SpellPercent spell in SpellLife)
                    if (kedrah.Player.HPBar <= spell.Percent && kedrah.Player.Mana >= spell.Mana)
                        SpellNext = kedrah.Console.Say(spell.Spell) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;

                if (Poison && kedrah.Player.HasFlag(Tibia.Constants.Flag.Poisoned) && kedrah.Player.Mana >= SpellPoisonMana)
                    SpellNext = kedrah.Console.Say(SpellPoison) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;

                if (Paralyze && kedrah.Player.HasFlag(Tibia.Constants.Flag.Paralyzed))
                    SpellNext = kedrah.Console.Say(SpellLife.Last().Spell) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;
            }
        }

        #endregion
    }

    public class HealList<T> : List<T> {
        public void RemoveDuplicates() {
            Dictionary<T, int> uniqueStore = new Dictionary<T, int>();
            List<T> inputList = new List<T>(this);
            this.Clear();

            foreach (T currentValue in inputList) {
                if (!uniqueStore.ContainsKey(currentValue)) {
                    uniqueStore.Add(currentValue, 0);
                    this.Add(currentValue);
                }
            }
        }

        public void Insert(T item) {
            base.Add(item);
            this.RemoveDuplicates();
        }
    }

    public struct ItemPercent {
        public uint Percent;
        public Tibia.Objects.Item Item;

        public ItemPercent(uint percent, Tibia.Objects.Item item) {
            this.Percent = percent;
            this.Item = item;
        }
    }

    public struct SpellPercent {
        public uint Percent;
        public string Spell;
        public uint Mana;

        public SpellPercent(uint percent, string spell, uint mana) {
            this.Percent = percent;
            this.Spell = spell;
            this.Mana = mana;
        }
    }
}
