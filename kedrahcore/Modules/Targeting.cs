using System;
using System.Collections.Generic;
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

        public struct FightExtraPair {
            public FightExtra Type;
            public Tibia.Objects.Item Item;
            public string Spell;

            public FightExtraPair(FightExtra type, Tibia.Objects.Item item) {
                this.Type = type;
                this.Item = item;
                this.Spell = "";
            }

            public FightExtraPair(FightExtra type, string spell) {
                this.Type = type;
                this.Item = null;
                this.Spell = spell;
            }
        }

        public enum FightActions {
            Attack,
            Follow,
            None
        }

        public enum FightExtra {
            Spell,
            Item
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

        List<Monster> monsters = new List<Monster>();
        List<Target> targets = new List<Target>();

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// Targeting module constructor.
        /// </summary>
        public Targeting(Core core)
            : base(core) {
            LoadMonstersFromXmlResource();

            #region Timers

            timers.Add("avoidFront", new Tibia.Util.Timer(2500, false));
            timers["avoidFront"].Execute += new Tibia.Util.Timer.TimerExecution(avoidFront_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool AvoidFront {
            get {
                if (timers["avoidFront"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set {
                if (value)
                    PlayTimer("avoidFront");
                else
                    PauseTimer("avoidFront");
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

        #endregion

        #region Module Functions

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

        public string GetBestElementIn(Monster monster, string[] elements) {
            tElement current, best = new tElement("", 0);
            List<string> eles = elements.ToList();
            List<string> eleenum = elements.ToList();

            try {
                foreach (string element in eleenum) {
                    current = new tElement();
                    current = monster.Immunities.Find(delegate(tElement m) {
                        return m.To.ToLower() == element.ToLower();
                    });
                    if (current.To != null) {
                        eles.Remove(element);
                        continue;
                    }
                    current = monster.Strongnesses.Find(delegate(tElement m) {
                        return m.To.ToLower() == element.ToLower();
                    });
                    if (current.To != null && best.Percent <= 0) {
                        if (current.Percent >= best.Percent || best.To == "") {
                            best.Percent = -Math.Abs(current.Percent - 2);
                            best.To = current.To;
                        }
                        eles.Remove(element);
                        continue;
                    }
                    current = monster.Weaknesses.Find(delegate(tElement m) {
                        return m.To.ToLower() == element.ToLower();
                    });
                    if (current.To != null) {
                        if (current.Percent >= best.Percent) {
                            best.Percent = Math.Abs(current.Percent + 2);
                            best.To = current.To;
                        }
                        eles.Remove(element);
                        continue;
                    }
                }
            }
            catch {
            }

            if (best.Percent < 0 && eles.Count == 0)
                best.To = "";
            else
                best.To = eles.Last();

            return best.To.ToLower();
        }

        public Monster FindMonster(string name) {
            return FindMonster(name, false);
        }

        public Monster FindMonster(string name, bool sensitive) {
            return monsters.Find(new MonsterFinder(name, sensitive).Match);
        }

        public void LoadMonstersFromXmlResource() {
            XPathDocument doc;
            XPathNavigator nav, nav2, nav3;
            XPathExpression expr;
            XPathNodeIterator iterator, iterator2;
            System.IO.Stream file = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("KedrahCore.Monsters.xml");

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

        #endregion

        #region Timers

        void avoidFront_OnExecute() {
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
                switch (where) {
                    case "weakness":
                        Weaknesses.Add(attr);
                        break;
                    case "strongness":
                        Strongnesses.Add(attr);
                        break;
                    case "immunity":
                        Immunities.Add(attr);
                        break;
                    case "neutral":
                        Neutral.Add(attr);
                        break;
                }
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

        /// <summary>
        /// Represents a target information.
        /// </summary>
        public class Target : Monster {
            public FightActions Action;
            public FightSecurity Security;
            public FightStances Stance;
            public List<FightExtraPair> Extra;

            public Target(Monster monster)
                : this(monster.Name, FightActions.None, FightSecurity.Automatic, FightStances.Stand) {
                this.Immunities = monster.Immunities;
                this.Neutral = monster.Neutral;
                this.Strongnesses = monster.Strongnesses;
                this.Weaknesses = monster.Weaknesses;
                this.Beam = monster.Beam;
                this.Wave = monster.Wave;
            }
            public Target(string name, FightActions action, FightSecurity security, FightStances stance)
                : base(name) {
                this.Action = action;
                this.Security = security;
                this.Stance = stance;
                this.Extra = new List<FightExtraPair>();
            }
        }
        #endregion
    }
}
