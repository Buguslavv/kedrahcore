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
        public List<Monster> Monsters = new List<Monster>();
        public List<Target> Targets = new List<Target>();

        #endregion

        #region Constructor/Destructor

        public Targeting(Core core)
            : base(core) {
            LoadMonstersFromXmlResource();

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

        public override void Enable() {
            base.Enable();
        }

        public Monster FindMonster(string name) {
            return FindMonster(name, false);
        }

        public Monster FindMonster(string name, bool sensitive) {
            return Monsters.Find(new MonsterFinder(name, sensitive).Match);
        }

        public Target FindTarget(string name, byte hpbar) {
            return FindTarget(name, hpbar, false);
        }

        public Target FindTarget(string name, byte hpbar, bool sensitive) {
            return Targets.Find(new TargetFinder(name, hpbar, sensitive).Match);
        }

        public string GetBestElementIn(Monster monster, string[] elements) {
            if (monster == null)
                return "";

            List<List<Element>> elementLists = new List<List<Element>>();

            elementLists.Add(monster.Weaknesses);
            elementLists.Add(monster.Neutral);
            elementLists.Add(monster.Strongnesses);

            foreach (List<Element> elementList in elementLists)
                foreach (Element element in elementList)
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
            Element currentElement;

            while (iterator.MoveNext()) {
                nav2 = iterator.Current.Clone();
                expr = nav.Compile("/monsters/monster[name='" + nav2.Value + "']");
                iterator2 = nav.Select(expr);
                current = new Monster(nav2.Value);

                while (iterator2.MoveNext()) {
                    nav2 = iterator2.Current.Clone();
                    nav2.MoveToFirstChild();

                    while (nav2.MoveToNext()) {
                        currentElement = new Element();
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

                Monsters.Add(current);
            }
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
                Target target = FindTarget(creature.Name, (byte)creature.HPBar);

                if (creature.IsSelf() || creature.Type != CreatureType.NPC)
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
            if (this.target == null || this.creature == null)
                return;

            Kedrah.Client.FollowMode = this.target.FollowMode;
            Kedrah.Client.AttackMode = this.target.AttackMode;

            if (this.target.Action == FightActions.Attack) {
                Kedrah.Player.Stop();
                this.creature.Attack();
            }
            else if (this.target.Action == FightActions.Follow) {
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
        public class Monster {
            public List<Element> Immunities;
            public List<Element> Neutral;
            public List<Element> Strongnesses;
            public List<Element> Weaknesses;

            public bool Beam;
            public bool Wave;

            public string Name;

            /// <summary>
            /// </summary>
            public Monster(string name) {
                Weaknesses = new List<Element>();
                Strongnesses = new List<Element>();
                Immunities = new List<Element>();
                Neutral = new List<Element>();
                Name = name;
            }

            public bool HasFrontAttack() {
                return Wave || Beam;
            }

            public void AddAttribute(string where, Element attr) {
                attr.To = attr.To.ToLower();

                switch (where) {
                    case "weakness":
                        Weaknesses.Add(attr);
                        Weaknesses.Sort(new Comparison<Element>(compareElements));
                        break;
                    case "strongness":
                        Strongnesses.Add(attr);
                        Strongnesses.Sort(new Comparison<Element>(compareElementsReverse));
                        break;
                    case "immunity":
                        Immunities.Add(attr);
                        break;
                    case "neutral":
                        Neutral.Add(attr);
                        break;
                }
            }

            int compareElements(Element e1, Element e2) {
                return e1.Percent == e2.Percent ? 0 : e1.Percent < e2.Percent ? 1 : -1;
            }

            int compareElementsReverse(Element e1, Element e2) {
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
