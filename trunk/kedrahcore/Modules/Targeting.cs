using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Tibia.Objects;
using Tibia.Constants;

namespace Kedrah.Modules {
    public class Targeting : Module {
        #region Enums/Structures

        public struct Element {
            public string To;
            public int Percent;

            public Element(string to, int percent) {
                To = to;
                Percent = percent;
            }
        }

        public enum FightActions {
            Attack,
            Follow,
            None
        }

        public enum FightExtra {
            Spell,
            ItemEquip,
            ItemUse
        }

        public enum FightStances {
            Stand,
            Follow,
            Distance,
            ParryStand,
            ParryFollow
        }

        public enum FightSecurity {
            Wave,
            Beam,
            Both,
            Automatic,
            None
        }

        #endregion

        #region Variables/Objects

        private Creature creature;
        private Target target;

        public bool AttackedOnly, Reachable, Shootable;
        public byte Distance = 2;
        public byte OthersMonsters;
        public Dictionary<string, byte> TargetSelection = new Dictionary<string, byte>();
        public List<Target> Targets = new List<Target>();

        #endregion

        #region Constructor/Destructor

        public Targeting(ref Core core)
            : base(ref core) {
            Reachable = false;
            Reachable = true;
            Shootable = false;
            OthersMonsters = 0;
            TargetSelection.Add("distance", 0);
            TargetSelection.Add("health", 0);
            TargetSelection.Add("priority", 100);
            TargetSelection.Add("stick", 0);

            #region Timers

            Timers.Add("target", new Tibia.Util.Timer(500, false));
            Timers["target"].Execute += new Tibia.Util.Timer.TimerExecution(Target_OnExecute);
            Timers.Add("action", new Tibia.Util.Timer(2000, false));
            Timers["action"].Execute += new Tibia.Util.Timer.TimerExecution(Action_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public long ActionDelay {
            get {
                return Timers["action"].Interval;
            }
            set {
                Timers["action"].Interval = value;
            }
        }

        public bool Attacker {
            get {
                if (Timers["target"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set {
                if (value) {
                    PlayTimer("target");
                    PlayTimer("action");
                }
                else {
                    PauseTimer("target");
                    PauseTimer("action");
                }
            }
        }

        public long AttackDelay {
            get {
                return Timers["target"].Interval;
            }
            set {
                Timers["target"].Interval = value;
            }
        }

        #endregion

        #region Module Functions

        public void AddTarget(string name) {
            CreatureData c = null;

            if (CreatureLists.AllCreatures.ContainsKey(name))
                c = CreatureLists.AllCreatures[name];
            else
                c = new CreatureData(name, 0, 0, 0, 0, 0, false, false, FrontAttack.None, null, null, null, null, null);

            Targets.Add(new Target(c));
        }

        public void AddTarget(string name, FightActions action, byte priority, FightSecurity security, FightStances stance, Tibia.Constants.Attack attackMode, Tibia.Constants.Follow followMode) {
            CreatureData c = null;

            if (CreatureLists.AllCreatures.ContainsKey(name))
                c = CreatureLists.AllCreatures[name];
            else
                c = new CreatureData(name, 0, 0, 0, 0, 0, false, false, FrontAttack.None, null, null, null, null, null);

            this.Targets.Add(new Target(c, action, priority, security, stance, attackMode, followMode));
        }

        public override void Enable() {
            base.Enable();
        }

        public string GetBestMageSpell(CreatureData creatureData) {
            return GetBestMageSpell(creatureData, "exori mort", "exori tera", "exori vis", "exori flam", "exori frigo");
        }

        public string GetBestMageSpell(CreatureData creatureData, string deathSpell, string earthSpell, string energySpell, string fireSpell, string iceSpell) {
            DamageType damageType = creatureData.GetWeakness(new List<DamageType>() { DamageType.Death, DamageType.Earth, DamageType.Energy, DamageType.Fire, DamageType.Ice });

            switch (damageType) {
                case DamageType.Death:
                    return deathSpell;
                case DamageType.Earth:
                    return earthSpell;
                case DamageType.Energy:
                    return energySpell;
                case DamageType.Fire:
                    return fireSpell;
                case DamageType.Ice:
                    return iceSpell;
            }

            return iceSpell;
        }

        public string GetBestPaladinSpell(CreatureData creatureData) {
            return GetBestPaladinSpell(creatureData, "exori san", "exori con");
        }

        public string GetBestPaladinSpell(CreatureData creatureData, string holySpell, string physicalSpell) {
            DamageType damageType = creatureData.GetWeakness(new List<DamageType>() { DamageType.Holy, DamageType.Physical });

            switch (damageType) {
                case DamageType.Holy:
                    return holySpell;
                case DamageType.Physical:
                    return physicalSpell;
            }

            return holySpell;
        }

        public void SelectTarget() {
            if (!Kedrah.Client.LoggedIn)
                return;

            Tibia.Objects.Creature selected = null;
            Target selectedTarget = null;
            Dictionary<string, double[]> verifier = new Dictionary<string, double[]>(4);

            verifier.Add("distance", new double[2] { 0, 0 });
            verifier.Add("health", new double[2] { 0, 0 });
            verifier.Add("priority", new double[2] { 0, 0 });
            verifier.Add("stick", new double[2] { 0, 0 });
            List<KeyValuePair<string, byte>> items = TargetSelection.OrderByDescending(s => s.Value).ToList();

            foreach (Creature creature in Kedrah.BattleList.GetCreatures()) {
                Target target = Targets.Find(delegate(Target t) { 
                    return (string.Compare(t.Name, "All", false) == 0 || 
                        string.Compare(t.Name, creature.Name, true) == 0 && 
                        (t.HPRange[0] <= creature.HPBar && t.HPRange[1] <= creature.HPBar)); 
                });

                if (creature.IsSelf() || creature.Type != CreatureType.NPC)
                    continue;

                if (Reachable && !creature.IsReachable())
                    continue;

                if (AttackedOnly && !creature.IsAttacking())
                    continue;

                if (target == null)
                    continue;

                if (OthersMonsters > 0) {
                    var playersAround = Kedrah.BattleList.GetCreatures().ToList().FindAll(delegate(Tibia.Objects.Creature c) {
                        return c.DistanceTo(creature.Location) <= OthersMonsters && (c.Z == creature.Location.Z) && (c.Type == Tibia.Constants.CreatureType.Player) && (!c.IsSelf());
                    });

                    if (playersAround.Count > 0 && !creature.IsAttacking())
                        continue;
                }

                if (selected == null) {
                    selected = creature;
                    selectedTarget = target;
                    continue;
                }

                if (selectedTarget.Action != FightActions.Attack && target.Action == FightActions.Attack) {
                    selected = creature;
                    selectedTarget = target;
                    continue;
                }

                verifier["distance"][0] = selected.DistanceTo(Kedrah.Player.Location);
                verifier["health"][0] = (double)selected.HPBar;
                verifier["priority"][0] = (double)selectedTarget.Priority;
                verifier["stick"][0] = (double)selected.Id;
                verifier["distance"][1] = creature.DistanceTo(Kedrah.Player.Location);
                verifier["health"][1] = (double)creature.HPBar;
                verifier["priority"][1] = (double)target.Priority;
                verifier["stick"][1] = (double)creature.Id;

                foreach (KeyValuePair<string, byte> v in items) {
                    if (v.Key == "stick") {
                        if (creature.Id == Kedrah.Player.Target_ID) {
                            selected = creature;
                            selectedTarget = target;
                        }

                        break;
                    }

                    if (verifier[v.Key][0] > verifier[v.Key][1]) {
                        selected = creature;
                        selectedTarget = target;
                        break;
                    }
                }
            }

            this.target = selectedTarget;
            this.creature = selected;
        }

        #endregion

        #region Timers

        private void Target_OnExecute() {
            SelectTarget();

            if (this.target == null || this.creature == null)
                return;

            Kedrah.Client.FollowMode = this.target.FollowMode;
            Kedrah.Client.AttackMode = this.target.AttackMode;
            Tibia.Packets.Outgoing.FightModesPacket.Send(Kedrah.Client, (byte)Kedrah.Client.AttackMode, (byte)Kedrah.Client.FollowMode, (byte)Kedrah.Client.SafeMode);

            if (this.target.Action == FightActions.Attack) {
                if (creature.Id != Kedrah.Player.Target_ID)
                    Kedrah.Player.Stop();

                this.creature.Attack();
            }
            else if (target.Action == FightActions.Follow) {
                if (creature.Id != Kedrah.Player.Target_ID)
                    Kedrah.Player.Stop();

                this.creature.Follow();
            }
        }

        private void Action_OnExecute() {
            if (this.target == null || this.creature == null)
                return;

            foreach (FightExtraPair extra in this.target.Extra)
                extra.Execute(this.creature, Kedrah.Inventory);
        }

        #endregion

        #region Auxiliar Classes

        public class TargetFinder {
            private string name;
            private byte hpbar;
            private bool Sensitive;

            public TargetFinder(string n, byte hp) {
                name = n;
                hpbar = hp;
                Sensitive = true;
            }

            public TargetFinder(string n, byte hp, bool sensitive) {
                name = n;
                hpbar = hp;
                Sensitive = sensitive;
            }

            public Predicate<Target> Match {
                get {
                    return IsMatch;
                }
            }

            public bool IsMatch(Target s) {
                bool hp = (s.HPRange[0] <= hpbar && s.HPRange[1] <= hpbar);
                if (string.Compare(s.Name, "All", false) == 0)
                    return true;

                return (hp && string.Compare(s.Name, name, !Sensitive) == 0);
            }
        }

        public class FightExtraPair {
            private FightExtra Type;
            private Tibia.Objects.Item Item;
            private Tibia.Constants.SlotNumber Slot;
            private string Spell;

            public FightExtraPair(FightExtra type, Tibia.Objects.Item item) {
                this.Type = type;
                this.Item = item;
                this.Slot = Tibia.Constants.SlotNumber.Ammo;
                this.Spell = "";
            }

            public FightExtraPair(FightExtra type, Tibia.Objects.Item item, Tibia.Constants.SlotNumber slot) {
                this.Type = type;
                this.Item = item;
                this.Slot = slot;
                this.Spell = "";
            }

            public FightExtraPair(FightExtra type, string spell) {
                this.Type = type;
                this.Item = null;
                this.Slot = Tibia.Constants.SlotNumber.Ammo;
                this.Spell = spell;
            }

            public void Execute(Tibia.Objects.Creature creature, Tibia.Objects.Inventory inventory) {
                switch (this.Type) {
                    case FightExtra.ItemEquip:
                        Tibia.Objects.ItemLocation loc = new Tibia.Objects.ItemLocation();
                        this.Item.Move(inventory.GetItemInSlot(this.Slot).Location);
                        break;
                    case FightExtra.ItemUse:
                        this.Item.Use(creature);
                        break;
                    case FightExtra.Spell:
                        this.Item.Client.Console.Say(this.Spell);
                        break;
                }
            }
        }

        public class Target : CreatureData {
            public FightActions Action;
            public byte Priority;
            public byte[] HPRange = { 0, 100 };
            public FightSecurity Security;
            public FightStances Stance;
            public Tibia.Constants.Attack AttackMode;
            public Tibia.Constants.Follow FollowMode;
            public List<FightExtraPair> Extra;

            public Target(CreatureData c)
                : this(c, FightActions.None, 0, FightSecurity.Automatic, FightStances.Stand, Tibia.Constants.Attack.FullAttack, Tibia.Constants.Follow.DoNotFollow) {
            }

            public Target(CreatureData c, FightActions action, byte priority, FightSecurity security, FightStances stance, Tibia.Constants.Attack attackMode, Tibia.Constants.Follow followMode)
                : base(c.Name, c.HitPoints, c.ExperiencePoints, c.SummonMana, c.ConvinceMana, c.MaxDamage, c.CanIllusion, c.CanSeeInvisible, c.FrontAttack, c.Immunities, c.Strengths, c.Weaknesses, c.Sounds, c.Loot) {
                this.Action = action;
                this.Priority = priority;
                this.Security = security;
                this.Stance = stance;
                this.AttackMode = attackMode;
                this.FollowMode = followMode;
                this.Extra = new List<FightExtraPair>();
            }
        }
        #endregion
    }
}
