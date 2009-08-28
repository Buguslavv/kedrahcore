using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Kedrah.Modules {
    public class Targeting : Module {
        #region Enums/Structures

        public struct tElement {
            public string To;
            public int Percent;

            public tElement(string to, int percent) {
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

        byte distance = 2;
        List<Monster> monsters = new List<Monster>();
        List<Target> targets = new List<Target>();

        public bool AttackedOnly, Reachable, Shootable;
        public byte OthersMonsters;
        Dictionary<string, byte> targetSelection = new Dictionary<string, byte>();

        Target target;
        Tibia.Objects.Creature creature;

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// Targeting module constructor.
        /// </summary>
        public Targeting(Core core)
            : base(core) {
            LoadMonstersFromXmlResource();

            Reachable = false;
            Reachable = true;
            Shootable = false;
            OthersMonsters = 0;
            targetSelection.Add("distance", 0);
            targetSelection.Add("health", 0);
            targetSelection.Add("priority", 100);
            targetSelection.Add("stick", 0);

            #region Timers

            // Target selection timer
            timers.Add("target", new Tibia.Util.Timer(500, false));
            timers["target"].Execute += new Tibia.Util.Timer.TimerExecution(target_OnExecute);
            timers.Add("action", new Tibia.Util.Timer(2000, false));
            timers["action"].Execute += new Tibia.Util.Timer.TimerExecution(action_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public long ActionDelay {
            get {
                return timers["action"].Interval;
            }
            set {
                timers["action"].Interval = value;
            }
        }

        public bool Attacker {
            get {
                if (timers["target"].State == Tibia.Util.TimerState.Running)
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
                return timers["target"].Interval;
            }
            set {
                timers["target"].Interval = value;
            }
        }

        public byte Distance {
            get {
                return distance;
            }
            set {
                distance = value;
            }
        }

        public List<Monster> Monsters {
            get {
                return monsters;
            }
            set {
                monsters = value;
            }
        }

        public List<Target> Targets {
            get {
                return targets;
            }
            set {
                targets = value;
            }
        }

        public Dictionary<string, byte> TargetSelection {
            get {
                return targetSelection;
            }
            set {
                targetSelection = value;
            }
        }


        #endregion

        #region Module Functions

        public void AddTarget(string name) {
            Monster monster = FindMonster(name);

            if (monster == null)
                monster = new Monster(name);

            this.Targets.Add(new Target(monster));
        }

        public void AddTarget(string name, FightActions action, byte priority, FightSecurity security, FightStances stance, Tibia.Constants.Attack attackMode, Tibia.Constants.Follow followMode) {
            Monster monster = FindMonster(name);

            if (monster == null)
                monster = new Monster(name);

            this.Targets.Add(new Target(monster, action, priority, security, stance, attackMode, followMode));
        }

        /// <summary>
        /// Enables the Targeting module.
        /// </summary>
        public override void Enable() {
            base.Enable();
        }

        public Monster FindMonster(string name) {
            return FindMonster(name, false);
        }

        public Monster FindMonster(string name, bool sensitive) {
            return monsters.Find(new MonsterFinder(name, sensitive).Match);
        }

        public Target FindTarget(string name, byte hpbar) {
            return FindTarget(name, hpbar, false);
        }

        public Target FindTarget(string name, byte hpbar, bool sensitive) {
            return targets.Find(new TargetFinder(name, hpbar, sensitive).Match);
        }

        public string GetBestElementIn(Monster monster, string[] elements) {
            if (monster == null)
                return "";

            List<List<tElement>> elementLists = new List<List<tElement>>();

            elementLists.Add(monster.Weaknesses);
            elementLists.Add(monster.Neutral);
            elementLists.Add(monster.Strongnesses);

            foreach (List<tElement> elementList in elementLists)
                foreach (tElement element in elementList)
                    if (elements.Contains(element.To, StringComparer.OrdinalIgnoreCase))
                        return element.To;

            return "";
        }

        public string GetBestMageSpell(Monster monster) {
            return GetBestMageSpell(monster, "exori flam", "exori vis", "exori frigo", "exori tera", "exori mort");
        }

        public string GetBestMageSpell(Monster monster, string fireSpell, string energySpell, string iceSpell, string earthSpell, string deathSpell) {
            string element = GetBestElementIn(monster, new string[] { "fire", "energy", "ice", "earth", "death" });

            switch (element) {
                case "fire":
                    return fireSpell;
                case "energy":
                    return energySpell;
                case "ice":
                    return iceSpell;
                case "earth":
                    return earthSpell;
                case "death":
                    return deathSpell;
            }

            return deathSpell;
        }

        public string GetBestPaladinSpell(Monster monster) {
            return GetBestPaladinSpell(monster, "exori con", "exori san");
        }

        public string GetBestPaladinSpell(Monster monster, string physicalSpell, string holySpell) {
            string element = GetBestElementIn(monster, new string[] { "physical", "holy" });

            switch (element) {
                case "physical":
                    return physicalSpell;
                case "holy":
                    return holySpell;
            }

            return holySpell;
        }

        public void LoadMonstersFromXmlResource() {
            XPathDocument doc;
            XPathNavigator nav, nav2, nav3;
            XPathExpression expr;
            XPathNodeIterator iterator, iterator2;
            System.IO.Stream file = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Kedrah.Monsters.xml");

            doc = new XPathDocument(file);
            nav = doc.CreateNavigator();

            expr = nav.Compile("/monsters/monster/name");
            iterator = nav.Select(expr);

            Monster current;
            tElement currentElement;

            while (iterator.MoveNext()) {
                nav2 = iterator.Current.Clone();
                expr = nav.Compile("/monsters/monster[name='" + nav2.Value + "']");
                iterator2 = nav.Select(expr);
                current = new Monster(nav2.Value);

                while (iterator2.MoveNext()) {
                    nav2 = iterator2.Current.Clone();
                    nav2.MoveToFirstChild();

                    while (nav2.MoveToNext()) {
                        currentElement = new tElement();
                        nav3 = nav2.Clone();
                        nav3.MoveToFirstChild();
                        currentElement.To = nav3.Value;
                        nav3.MoveToNext();

                        if (nav2.Name == "beam")
                            current.Beam = (nav2.Value == "yes") ? true : false;
                        else if (nav2.Name == "wave") {
                            current.Wave = (nav2.Value == "yes") ? true : false;
                        }
                        else {
                            int.TryParse(nav3.Value, out currentElement.Percent);
                            current.AddAttribute(nav2.Name, currentElement);
                        }

                    }
                }

                monsters.Add(current);
            }
        }

        public void SelectTarget() {
            if (!kedrah.Client.LoggedIn)
                return;

            Tibia.Objects.Creature selected = null;
            Target selectedTarget = null;
            Dictionary<string, double[]> verifier = new Dictionary<string, double[]>(4);

            verifier.Add("distance", new double[2] { 0, 0 });
            verifier.Add("health", new double[2] { 0, 0 });
            verifier.Add("priority", new double[2] { 0, 0 });
            verifier.Add("stick", new double[2] { 0, 0 });
            List<KeyValuePair<string, byte>> items = targetSelection.OrderByDescending(s => s.Value).ToList();

            foreach (Tibia.Objects.Creature creature in kedrah.BattleList.GetCreatures()) {
                Target target = FindTarget(creature.Name, (byte)creature.HPBar);

                if (creature.IsSelf() || creature.Type != Tibia.Constants.CreatureType.NPC)
                    continue;

                if (Reachable && !creature.IsReachable())
                    continue;

                if (AttackedOnly && !creature.IsAttacking())
                    continue;

                if (target == null) {
                    target = FindTarget("All", 50);

                    if (target == null)
                        continue;
                }

                if (OthersMonsters > 0) {
                    var playersAround = kedrah.BattleList.GetCreatures().ToList().FindAll(delegate(Tibia.Objects.Creature c) {
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

                verifier["distance"][0] = selected.DistanceTo(kedrah.Player.Location);
                verifier["health"][0] = (double)selected.HPBar;
                verifier["priority"][0] = (double)selectedTarget.Priority;
                verifier["stick"][0] = (double)selected.Id;
                verifier["distance"][1] = creature.DistanceTo(kedrah.Player.Location);
                verifier["health"][1] = (double)creature.HPBar;
                verifier["priority"][1] = (double)target.Priority;
                verifier["stick"][1] = (double)creature.Id;

                foreach (KeyValuePair<string, byte> v in items) {
                    if (v.Key == "stick") {
                        if (creature.Id == kedrah.Player.Target_ID) {
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

        void target_OnExecute() {
            if (this.target == null || this.creature == null)
                return;

            kedrah.Client.FollowMode = this.target.FollowMode;
            kedrah.Client.AttackMode = this.target.AttackMode;

            if (this.target.Action == FightActions.Attack) {
                kedrah.Player.Stop();
                this.creature.Attack();
            }
            else if (this.target.Action == FightActions.Follow) {
                kedrah.Player.Stop();
                this.creature.Follow();
            }
        }

        void action_OnExecute() {
            if (this.target == null || this.creature == null)
                return;

            foreach (FightExtraPair extra in this.target.Extra)
                extra.Execute(this.creature, kedrah.Inventory);
        }

        #endregion

        #region Auxiliar Classes
        /// <summary>
        /// Represents a monster information.
        /// </summary>
        public class Monster {
            public List<tElement> Immunities;
            public List<tElement> Neutral;
            public List<tElement> Strongnesses;
            public List<tElement> Weaknesses;

            public bool Beam;
            public bool Wave;

            public string Name;

            /// <summary>
            /// </summary>
            public Monster(string name) {
                Weaknesses = new List<tElement>();
                Strongnesses = new List<tElement>();
                Immunities = new List<tElement>();
                Neutral = new List<tElement>();
                Name = name;
            }

            public bool HasFrontAttack() {
                return Wave || Beam;
            }

            public void AddAttribute(string where, tElement attr) {
                attr.To = attr.To.ToLower();

                switch (where) {
                    case "weakness":
                        Weaknesses.Add(attr);
                        Weaknesses.Sort(new Comparison<tElement>(compareElements));
                        break;
                    case "strongness":
                        Strongnesses.Add(attr);
                        Strongnesses.Sort(new Comparison<tElement>(compareElementsReverse));
                        break;
                    case "immunity":
                        Immunities.Add(attr);
                        break;
                    case "neutral":
                        Neutral.Add(attr);
                        break;
                }
            }

            int compareElements(tElement e1, tElement e2) {
                return e1.Percent == e2.Percent ? 0 : e1.Percent < e2.Percent ? 1 : -1;
            }

            int compareElementsReverse(tElement e1, tElement e2) {
                return e1.Percent == e2.Percent ? 0 : e1.Percent > e2.Percent ? 1 : -1;
            }
        }

        public class MonsterFinder {
            private string name;
            private bool Sensitive;

            public MonsterFinder(string n) {
                name = n;
                Sensitive = true;
            }

            public MonsterFinder(string n, bool sensitive) {
                name = n;
                Sensitive = sensitive;
            }

            public Predicate<Monster> Match {
                get {
                    return IsMatch;
                }
            }

            public bool IsMatch(Monster s) {
                if (!Sensitive)
                    return (s.Name.ToLower() == name.ToLower());
                else
                    return (s.Name == name);
            }
        }

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
                if (!Sensitive)
                    return (hp && s.Name.ToLower() == name.ToLower());
                else
                    return (hp && s.Name == name);
            }
        }

        /// <summary>
        /// Represents a extra action executer
        /// </summary>
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

        /// <summary>
        /// Represents a target information.
        /// </summary>
        public class Target : Monster {
            public FightActions Action;
            public byte Priority;
            public byte[] HPRange = { 0, 100 };
            public FightSecurity Security;
            public FightStances Stance;
            public Tibia.Constants.Attack AttackMode;
            public Tibia.Constants.Follow FollowMode;
            public List<FightExtraPair> Extra;

            public Target(Monster monster)
                : this(monster, FightActions.None, 0, FightSecurity.Automatic, FightStances.Stand, Tibia.Constants.Attack.FullAttack, Tibia.Constants.Follow.DoNotFollow) {
            }

            public Target(Monster monster, FightActions action, byte priority, FightSecurity security, FightStances stance, Tibia.Constants.Attack attackMode, Tibia.Constants.Follow followMode)
                : base(monster.Name) {
                this.Immunities = monster.Immunities;
                this.Neutral = monster.Neutral;
                this.Strongnesses = monster.Strongnesses;
                this.Weaknesses = monster.Weaknesses;
                this.Beam = monster.Beam;
                this.Wave = monster.Wave;
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
