using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Kedrah.Objects;
using Tibia.Constants;

namespace Kedrah.Modules
{
    public class Heal : Module
    {
        #region Variables/Objects

        public bool Poison = false;
        public bool Paralyze = false;
        public DateTime PotionNext = DateTime.Now;
        public DateTime SpellNext = DateTime.Now;
        public List<HealPercent> PotionLife;
        public List<HealPercent> PotionMana;
        public List<HealPercent> RuneLife;
        public List<HealPercent> SpellLife;
        public string SpellPoisonWords = Spells.Antidote.Words;
        public ushort PotionExhaustion = 700;
        public ushort SpellExhaustion = 1080;
        public ushort SpellPoisonMana = 30;

        #endregion

        #region Constructor/Destructor

        public Heal(ref Core core)
            : base(ref core)
        {
            PotionLife = new List<HealPercent>();
            PotionMana = new List<HealPercent>();
            RuneLife = new List<HealPercent>();
            SpellLife = new List<HealPercent>();

            #region Timers

            Timers.Add("healer", new Tibia.Util.Timer(100, false));
            Timers["healer"].Execute += new Tibia.Util.Timer.TimerExecution(Healer_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool Healer
        {
            get
            {
                if (Timers["healer"].State == Tibia.Util.TimerState.Running)
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
                    PotionLife.Sort();
                    PotionMana.Sort();
                    RuneLife.Sort();
                    SpellLife.Sort();
                    PlayTimer("healer");
                }
                else
                {
                    PauseTimer("healer");
                }
            }
        }

        #endregion

        #region Timers

        private void Healer_OnExecute()
        {
            if (!Core.Client.LoggedIn)
            {
                return;
            }

            if (PotionNext.CompareTo(DateTime.Now) <= 0)
            {
                foreach (HealPercent potion in PotionLife)
                {
                    if (Core.Player.HPBar <= Util.RandomGenerator.RandomNumber(potion.Percent, (int)(potion.Percent * Core.RandomRate)))
                    {
                        if (potion.Keys != null)
                        {
                            Core.Client.Input.SendKeys(potion.Keys);
                            PotionNext = DateTime.Now.AddMilliseconds(Util.RandomGenerator.RandomNumber(PotionExhaustion, (int)Math.Round(PotionExhaustion * Core.RandomRate)));
                        }
                        else
                            PotionNext = Core.Client.Inventory.UseItemOnSelf(potion.Item.Id) ? DateTime.Now.AddMilliseconds(PotionExhaustion) : DateTime.Now;
                    }
                }

                foreach (HealPercent potion in PotionMana)
                {
                    if ((Core.Player.Mana * 100 / Core.Player.Mana_Max) <= Util.RandomGenerator.RandomNumber(potion.Percent, (int)(potion.Percent * Core.RandomRate)))
                    {
                        if (potion.Keys != null)
                        {
                            Core.Client.Input.SendKeys(potion.Keys);
                            PotionNext = DateTime.Now.AddMilliseconds(Util.RandomGenerator.RandomNumber(PotionExhaustion, (int) Math.Round(PotionExhaustion * Core.RandomRate)));
                        }
                        else
                            PotionNext = Core.Client.Inventory.UseItemOnSelf(potion.Item.Id) ? DateTime.Now.AddMilliseconds(PotionExhaustion) : DateTime.Now;
                    }
                }
            }

            if (SpellNext.CompareTo(DateTime.Now) <= 0)
            {
                foreach (HealPercent rune in RuneLife)
                {
                    if (Core.Player.HPBar <= Util.RandomGenerator.RandomNumber(rune.Percent, (int) (rune.Percent * Core.RandomRate)))
                    {
                        if (rune.Keys != null)
                        {
                            Core.Client.Input.SendKeys(rune.Keys);
                            SpellNext = DateTime.Now.AddMilliseconds(Util.RandomGenerator.RandomNumber(SpellExhaustion, (int)Math.Round(SpellExhaustion * Core.RandomRate)));
                        }
                        else
                            SpellNext = Core.Client.Inventory.UseItemOnSelf(rune.Item.Id) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;
                    }
                }

                foreach (HealPercent spell in SpellLife)
                {
                    if (Core.Player.HPBar <= Util.RandomGenerator.RandomNumber(spell.Percent, (int)(spell.Percent * Core.RandomRate)) && Core.Player.Mana >= spell.Mana)
                    {
                        if (spell.Keys != null)
                        {
                            Core.Client.Input.SendKeys(spell.Keys);
                            SpellNext = DateTime.Now.AddMilliseconds(Util.RandomGenerator.RandomNumber(SpellExhaustion, (int)Math.Round(SpellExhaustion * Core.RandomRate)));
                        }
                        else
                            SpellNext = Core.Client.Console.Say(spell.Spell) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;
                    }
                }

                if (Poison && Core.Player.HasFlag(Flag.Poisoned) && Core.Player.Mana >= SpellPoisonMana)
                {
                    Core.Client.Input.SendString(SpellPoisonWords);
                    SpellNext = DateTime.Now.AddMilliseconds(Util.RandomGenerator.RandomNumber(SpellExhaustion, (int)Math.Round(SpellExhaustion * Core.RandomRate)));
                }

                if (Paralyze && Core.Player.HasFlag(Flag.Paralyzed))
                {
                    if (Util.RandomGenerator.RandomNumber(0, (int)(2 * Core.RandomRate)) == 0)
                    {
                        if (SpellLife.Last().Keys != null)
                        {
                            Core.Client.Input.SendKeys(SpellLife.Last().Keys);
                            SpellNext = DateTime.Now.AddMilliseconds(Util.RandomGenerator.RandomNumber(SpellExhaustion, (int)Math.Round(SpellExhaustion * Core.RandomRate)));
                        }
                        else
                            SpellNext = Core.Client.Console.Say(SpellLife.Last().Spell) ? DateTime.Now.AddMilliseconds(SpellExhaustion) : DateTime.Now;
                    }
                }
            }
        }

        #endregion
    }
}
