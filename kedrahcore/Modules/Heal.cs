using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah.Modules {
    public class Heal : Module {
        #region Variables/Objects

        public bool Poison = false;
        public bool Paralyze = false;
        public DateTime PotionNext = DateTime.Now;
        public DateTime SpellNext = DateTime.Now;
        public HealList<ItemPercent> PotionLife;
        public HealList<ItemPercent> RuneLife;
        public HealList<ItemPercent> PotionMana;
        public HealList<SpellPercent> SpellLife;
        public string SpellPoisonWords = Tibia.Constants.Spells.Antidote.Words;
        public ushort PotionExhaustion = 700;
        public ushort SpellExhaustion = 1080;
        public ushort SpellPoisonMana = 30;

        #endregion

        #region Constructor/Destructor

        public Heal(Core core)
            : base(core) {
            PotionLife = new HealList<ItemPercent>();
            RuneLife = new HealList<ItemPercent>();
            PotionMana = new HealList<ItemPercent>();
            SpellLife = new HealList<SpellPercent>();

            #region Timers

            Timers.Add("healer", new Tibia.Util.Timer(100, false));
            Timers["healer"].Execute += new Tibia.Util.Timer.TimerExecution(Healer_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool Healer {
            get {
                if (Timers["healer"].State == Tibia.Util.TimerState.Running)
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

        private int compareSpellPercents(SpellPercent sp1, SpellPercent sp2) {
            return sp1.Percent == sp2.Percent ? 0 : sp1.Percent > sp2.Percent ? 1 : -1;
        }

        #endregion

        #region Timers

        private void Healer_OnExecute() {
            if (!Kedrah.Client.LoggedIn)
                return;

            if (PotionNext.CompareTo(DateTime.Now) <= 0) {
                foreach (ItemPercent potion in PotionLife)
                    if (Kedrah.Player.HPBar <= potion.Percent)
                        PotionNext = Kedrah.Inventory.UseItemOnSelf(potion.Item.Id) ? DateTime.Now.AddMilliseconds(PotionExhaustion) : DateTime.Now;

                foreach (ItemPercent potion in PotionMana)
                    if ((Kedrah.Player.Mana * 100 / Kedrah.Player.Mana_Max) <= potion.Percent)
                        PotionNext = Kedrah.Inventory.UseItemOnSelf(potion.Item.Id) ? DateTime.Now.AddMilliseconds(PotionExhaustion) : DateTime.Now;

            }
            if (SpellNext.CompareTo(DateTime.Now) <= 0) {
                foreach (ItemPercent rune in RuneLife)
                    if (Kedrah.Player.HPBar <= rune.Percent)
                        SpellNext = Kedrah.Inventory.UseItemOnSelf(rune.Item.Id) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;

                foreach (SpellPercent spell in SpellLife)
                    if (Kedrah.Player.HPBar <= spell.Percent && Kedrah.Player.Mana >= spell.Mana)
                        SpellNext = Kedrah.Console.Say(spell.Spell) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;

                if (Poison && Kedrah.Player.HasFlag(Tibia.Constants.Flag.Poisoned) && Kedrah.Player.Mana >= SpellPoisonMana)
                    SpellNext = Kedrah.Console.Say(SpellPoisonWords) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;

                if (Paralyze && Kedrah.Player.HasFlag(Tibia.Constants.Flag.Paralyzed))
                    SpellNext = Kedrah.Console.Say(SpellLife.Last().Spell) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;
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
