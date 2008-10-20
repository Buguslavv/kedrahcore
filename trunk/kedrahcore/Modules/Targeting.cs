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



            #endregion
        }

        #endregion

        #region Get/Set Objects

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

        public string GetBestElementIn(Monster monster, string[] elements)
        {
            tElement current, best = new tElement("", 0);

            foreach (string element in elements)
            {
                current = new tElement();
                current = monster.Immunities.Find(delegate(tElement m) { return m.To.ToLower() == element.ToLower(); });
                if (current.To != null)
                    continue;
                current = monster.Strongnesses.Find(delegate(tElement m) { return m.To.ToLower() == element.ToLower(); });
                if (current.To != null && best.Percent <= 0)
                    if (current.Percent >= best.Percent || best.To == "")
                    {
                        best.Percent = -Math.Abs(current.Percent);
                        best.To = current.To;
                        continue;
                    }
                    else
                        continue;
                current = monster.Weaknesses.Find(delegate(tElement m) { return m.To.ToLower() == element.ToLower(); });
                if (current.To != null)
                    if (current.Percent >= best.Percent)
                    {
                        best.Percent = Math.Abs(current.Percent);
                        best.To = current.To;
                        continue;
                    }
                    else
                        continue;
            }

            return best.To;
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
                            int.TryParse(nav3.Value, out currentElement.Percent);
                            current.AddAttribute(nav2.Name, currentElement);
                            
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



        #endregion

        /// <summary>
        /// Represents a monster information.
        /// </summary>
        public class Monster
        {
            public List<tElement> Weaknesses;
            public List<tElement> Strongnesses;
            public List<tElement> Immunities;

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
