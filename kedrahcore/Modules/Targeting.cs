using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Kedrah.Modules
{
    public class Targeting : Module
    {
        #region Structures

        public struct tElement
        {
            public string To;
            public int Percent;

            public tElement(string to, int percent)
            {
                To = to;
                Percent = percent;
            }
        }

        #endregion

        #region Variables/Objects

        List<Monster> monsters = new List<Monster>();

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// Targeting module constructor.
        /// </summary>
        public Targeting(Core core)
            : base(core)
        {
            LoadMonstersFromXmlResource();

            #region Timers

            timers.Add("avoidFront", new Tibia.Util.Timer(2500, false));
            timers["avoidFront"].OnExecute += new Tibia.Util.Timer.TimerExecution(avoidFront_OnExecute);

            #endregion
        }

        #endregion

        #region Get/Set Objects

        public bool AvoidFront
        {
            get
            {
                if (timers["avoidFront"].State == Tibia.Util.TimerState.Running)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    PlayTimer("avoidFront");
                else
                    PauseTimer("avoidFront");
            }
        }

        public List<Monster> Monsters
        {
            get
            {
                return monsters;
            }
            set
            {
                monsters = value;
            }
        }

        #endregion

        #region Module Functions

        public bool WalkDiagonal()
        {
            Tibia.Objects.Creature creature = kedrah.BattleList.GetCreature(kedrah.Player.Target_ID);
            Monster monster = FindMonster(creature.Name);

            if (!monster.HasDiagonalAttack())
                return false;

            List<Tibia.Objects.Location> locations = new List<Tibia.Objects.Location>();
            List<Tibia.Objects.Location> affectedSqms = new List<Tibia.Objects.Location>();
            Tibia.Objects.Location location = new Tibia.Objects.Location();
            Tibia.Objects.Item item = new Tibia.Objects.Item(kedrah.Client, 0);

            if (monster.Beam)
            {
                int sumX = 0, sumY = 0;
                if (creature.Direction == Tibia.Constants.TurnDirection.Down)
                    sumY = 1;
                else if (creature.Direction == Tibia.Constants.TurnDirection.Up)
                    sumY = -1;
                else if (creature.Direction == Tibia.Constants.TurnDirection.Right)
                    sumX = 1;
                else if (creature.Direction == Tibia.Constants.TurnDirection.Left)
                    sumX = -1;
                for (int i = 1; i < 9; i++)
                    affectedSqms.Add(new Tibia.Objects.Location(creature.X + (sumX * i), creature.Y + (sumY * i), creature.Z));
            }
            else
            {
                int sumX = 0, sumY = 0;
                if (creature.Direction == Tibia.Constants.TurnDirection.Down)
                    sumY = 1;
                else if (creature.Direction == Tibia.Constants.TurnDirection.Up)
                    sumY = -1;
                else if (creature.Direction == Tibia.Constants.TurnDirection.Right)
                    sumX = 1;
                else if (creature.Direction == Tibia.Constants.TurnDirection.Left)
                    sumX = -1;
                for (int i = 1; i < 9; i++)
                    affectedSqms.Add(new Tibia.Objects.Location(creature.X + (sumX * i), creature.Y + (sumY * i), creature.Z));

                if (creature.Direction == Tibia.Constants.TurnDirection.Down || creature.Direction == Tibia.Constants.TurnDirection.Up)
                {
                    sumX = 1;
                    for (int i = 3; i < 9; i++)
                        affectedSqms.Add(new Tibia.Objects.Location(creature.X + sumX, creature.Y + (sumY * i), creature.Z));
                    sumX = -1;
                    for (int i = 3; i < 9; i++)
                        affectedSqms.Add(new Tibia.Objects.Location(creature.X + sumX, creature.Y + (sumY * i), creature.Z));
                    sumX = 2;
                    for (int i = 6; i < 9; i++)
                        affectedSqms.Add(new Tibia.Objects.Location(creature.X + sumX, creature.Y + (sumY * i), creature.Z));
                    sumX = -2;
                    for (int i = 6; i < 9; i++)
                        affectedSqms.Add(new Tibia.Objects.Location(creature.X + sumX, creature.Y + (sumY * i), creature.Z));
                }
                else if (creature.Direction == Tibia.Constants.TurnDirection.Right || creature.Direction == Tibia.Constants.TurnDirection.Left)
                {
                    sumY = 1;
                    for (int i = 3; i < 9; i++)
                        affectedSqms.Add(new Tibia.Objects.Location(creature.X + (sumX * i), creature.Y + sumY, creature.Z));
                    sumY = -1;
                    for (int i = 3; i < 9; i++)
                        affectedSqms.Add(new Tibia.Objects.Location(creature.X + (sumX * i), creature.Y + sumY, creature.Z));
                    sumY = 2;
                    for (int i = 6; i < 9; i++)
                        affectedSqms.Add(new Tibia.Objects.Location(creature.X + (sumX * i), creature.Y + sumY, creature.Z));
                    sumY = -2;
                    for (int i = 6; i < 9; i++)
                        affectedSqms.Add(new Tibia.Objects.Location(creature.X + (sumX * i), creature.Y + sumY, creature.Z));
                }
            }

            bool canWalk = false;

            if (affectedSqms.Contains(kedrah.Player.Location))
            {
                if (creature.Y == kedrah.Player.Y)
                {
                    location = kedrah.Player.Location;
                    location.Y -= 1;
                    locations.Add(location);
                    location = kedrah.Player.Location;
                    location.Y += 1;
                    locations.Add(location);
                }
                else if (creature.X == kedrah.Player.X)
                {
                    location = kedrah.Player.Location;
                    location.X -= 1;
                    locations.Add(location);
                    location = kedrah.Player.Location;
                    location.X += 1;
                    locations.Add(location);
                }
                else if (creature.Direction == Tibia.Constants.TurnDirection.Down || creature.Direction == Tibia.Constants.TurnDirection.Up)
                {
                    //if (creature.Y > kedrah.Player.Y)
                }
            }

            List<Tibia.Objects.Location> sLocations = new List<Tibia.Objects.Location>(locations.Count);
            int count = locations.Count;
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                int j = random.Next(locations.Count);
                sLocations.Add(locations[j]);
                locations.Remove(locations[j]);
            }

            foreach (Tibia.Objects.Location l in sLocations)
            {
                location = l;
                item.Id = kedrah.Map.CreateMapSquare(location).Tile.Id;
                canWalk = !item.GetFlag(Tibia.Addresses.DatItem.Flag.Blocking) && !item.GetFlag(Tibia.Addresses.DatItem.Flag.Floorchange) && kedrah.BattleList.GetCreaturesOnLoc(location).Count == 0;
                if (canWalk)
                    break;
            }
            if (canWalk)
                kedrah.Player.GoTo = location;
            else
                return false;
            return true;
        }

        public string GetBestMageSpell(Monster monster)
        {
            return GetBestMageSpell(monster, "exori flam", "exori vis", "exori frigo", "exori tera", "exori mort");
        }

        public string GetBestMageSpell(Monster monster, string fireSpell, string energySpell, string iceSpell, string earthSpell, string deathSpell)
        {
            string element = GetBestElementIn(monster, new string[] { "fire", "energy", "ice", "earth", "death" });
            switch (element)
            {
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

        public string GetBestPaladinSpell(Monster monster)
        {
            return GetBestPaladinSpell(monster, "exori con", "exori san");
        }

        public string GetBestPaladinSpell(Monster monster, string physicalSpell, string holySpell)
        {
            string element = GetBestElementIn(monster, new string[] { "physical", "holy" });
            switch (element)
            {
                case "physical":
                    return physicalSpell;
                case "holy":
                    return holySpell;
            }
            return holySpell;
        }

        public string GetBestElementIn(Monster monster, string[] elements)
        {
            tElement current, best = new tElement("", 0);
            List<string> eles = elements.ToList();
            List<string> eleenum = elements.ToList();

            try
            {
                foreach (string element in eleenum)
                {
                    current = new tElement();
                    current = monster.Immunities.Find(delegate(tElement m) { return m.To.ToLower() == element.ToLower(); });
                    if (current.To != null)
                    {
                        eles.Remove(element);
                        continue;
                    }
                    current = monster.Strongnesses.Find(delegate(tElement m) { return m.To.ToLower() == element.ToLower(); });
                    if (current.To != null && best.Percent <= 0)
                    {
                        if (current.Percent >= best.Percent || best.To == "")
                        {
                            best.Percent = -Math.Abs(current.Percent - 2);
                            best.To = current.To;
                        }
                        eles.Remove(element);
                        continue;
                    }
                    current = monster.Weaknesses.Find(delegate(tElement m) { return m.To.ToLower() == element.ToLower(); });
                    if (current.To != null)
                    {
                        if (current.Percent >= best.Percent)
                        {
                            best.Percent = Math.Abs(current.Percent + 2);
                            best.To = current.To;
                        }
                        eles.Remove(element);
                        continue;
                    }
                }
            }
            catch
            { }

            if (best.Percent < 0 && eles.Count == 0)
                best.To = "";
            else
                best.To = eles.Last();

            return best.To.ToLower();
        }

        public Monster FindMonster(string name)
        {
            return FindMonster(name, false);
        }

        public Monster FindMonster(string name, bool sensitive)
        {
            return monsters.Find(new MonsterFinder(name, sensitive).Match);
        }

        public void LoadMonstersFromXmlResource()
        {
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

            try
            {
                while (iterator.MoveNext())
                {
                    nav2 = iterator.Current.Clone();
                    expr = nav.Compile("/monsters/monster[name='" + nav2.Value + "']");
                    iterator2 = nav.Select(expr);
                    current = new Monster(nav2.Value);
                    while (iterator2.MoveNext())
                    {
                        nav2 = iterator2.Current.Clone();
                        nav2.MoveToFirstChild();
                        while (nav2.MoveToNext())
                        {
                            currentElement = new tElement();
                            nav3 = nav2.Clone();
                            nav3.MoveToFirstChild();
                            currentElement.To = nav3.Value;
                            nav3.MoveToNext();
                            if (nav2.Name == "beam")
                                current.Beam = (nav2.Value == "yes") ? true : false;
                            else if (nav2.Name == "wave")
                            {
                                current.Wave = (nav2.Value == "yes") ? true : false;
                            }
                            else
                            {
                                int.TryParse(nav3.Value, out currentElement.Percent);
                                current.AddAttribute(nav2.Name, currentElement);
                            }
                            
                        }
                    }
                    monsters.Add(current);
                }
            }
            catch
            {}
        }

        #endregion

        #region Timers

        void avoidFront_OnExecute()
        {
            try
            {
                if (!WalkDiagonal())
                    avoidFront_OnExecute();
            }
            catch
            { }
        }

        #endregion

        /// <summary>
        /// Represents a monster information.
        /// </summary>
        public class Monster
        {
            public List<tElement> Weaknesses;
            public List<tElement> Strongnesses;
            public List<tElement> Immunities;

            public bool Wave;
            public bool Beam;

            public string Name;

            /// <summary>
            /// Default constructor, same as Tibia.Objects.Creature.
            /// </summary>
            /// <param name="client">The client.</param>
            /// <param name="address">The address.</param>
            public Monster(string name)
            {
                Weaknesses = new List<tElement>();
                Strongnesses = new List<tElement>();
                Immunities = new List<tElement>();
                Name = name;
            }

            public bool HasDiagonalAttack()
            {
                return Wave || Beam;
            }

            public void AddAttribute(string where, tElement attr)
            {
                switch (where)
                {
                    case "weakness":
                        Weaknesses.Add(attr);
                        break;
                    case "strongness":
                        Strongnesses.Add(attr);
                        break;
                    case "immunity":
                        Immunities.Add(attr);
                        break;
                }
            }
        }

        public class MonsterFinder
        {
            private string name;
            private bool Sensitive;

            public MonsterFinder(string n)
            {
                name = n;
                Sensitive = true;
            }

            public MonsterFinder(string n, bool sensitive)
            {
                name = n;
                Sensitive = sensitive;
            }

            public Predicate<Monster> Match
            {
                get { return IsMatch; }
            }


            public bool IsMatch(Monster s)
            {
                if (!Sensitive)
                    return (s.Name.ToLower() == name.ToLower());
                else
                    return (s.Name == name);
            }
        }
    }
}
