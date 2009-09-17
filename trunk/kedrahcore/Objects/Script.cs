using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using Tibia.Objects;
using System.IO;
using Tibia.Constants;
using System.Globalization;

namespace Kedrah.Objects
{
    public partial class Script : IScript
    {
        public string Name;
        protected Core core;
        protected Player player;
        protected Client client;
        protected Dictionary<string, object> symbols = new Dictionary<string, object>();

        public Script()
        {
            Name = GetType().Name;
        }

        public virtual void Run(Core core)
        {
            this.core = core;
            client = core.Client;
            player = core.Player;

            #region Register Variables

            #endregion
        }

        public virtual void Stop()
        {
            Thread.CurrentThread.Abort();
        }

        protected object GetVariable(string name)
        {
            string[] variable = name.Split('.');
            int param1 = -1;
            Item item;
            Random random = new Random((int)DateTime.Now.Ticks);
            string[] lines;

            switch (variable[0])
            {
                case "mp": return player.Mana;
                case "maxmp": return player.Mana_Max;
                case "mppc": return (100 * player.Mana / player.Mana_Max);
                case "hp": return player.HP;
                case "maxhp": return player.HP_Max;
                case "hppc": return player.HPBar;
                case "cap": return player.Cap;
                case "exp": return player.Exp;
                case "level": return player.Level;
                case "mlevel": return player.MagicLevel;
                case "posx": return player.X;
                case "posy": return player.Y;
                case "posz": return player.Z;
                case "soul": return player.Soul;
                case "stamina": return player.Stamina;
                case "count": break;
                case "screenleft": return client.Window.Size.Left;
                case "screenright": return client.Window.Size.Rigth;
                case "screentop": return client.Window.Size.Top;
                case "screenbottom": return client.Window.Size.Bottom;
                case "name": return player.Name;
                case "time": return DateTime.Now.Second;
                case "timems": return DateTime.Now.Millisecond;
                case "deltatime": return DateTime.Now.Subtract(core.StartTime).TotalSeconds;
                case "deltatimems": return DateTime.Now.Subtract(core.StartTime).TotalMilliseconds;
                case "exptnl": break;
                case "exph": break;
                case "expgained": break;
                case "timetnl": break;
                case "exptolevel": break;
                case "timetolevel": break;
                case "monstersaround":
                    if (variable.Length > 1)
                    {
                        int.TryParse(variable[1], out param1);
                    }
                    return client.BattleList.GetCreatures().Count(c => c.Type == CreatureType.NPC && c.Z == player.Z && (param1 < 1 || c.Distance() <= param1));
                case "playersaround":
                    if (variable.Length > 1)
                    {
                        int.TryParse(variable[1], out param1);
                    }
                    return client.BattleList.GetCreatures().Count(c => c.Type == CreatureType.Player && c.Z == player.Z && (param1 < 1 || c.Distance() <= param1));
                case "sbtime": break;
                case "formattime":
                    if (variable.Length > 1)
                    {
                        int.TryParse(variable[1], out param1);
                    }
                    return (param1 / 3600) + ":" + ((param1 % 3600) / 60) + ":" + ((param1 % 3600) % 60);
                case "formatnum":
                    if (variable.Length > 1)
                    {
                        int.TryParse(variable[1], out param1);
                    }
                    return param1.ToString("#,#", CultureInfo.InvariantCulture);
                case "itemcount":
                    if (variable.Length > 1)
                    {
                        int.TryParse(variable[1], out param1);
                        if (param1 > 0)
                        {
                            return client.Inventory.CountItems((uint)param1);
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                case "poisoned": return player.HasFlag(Flag.Poisoned);
                case "poisondmg": break;
                case "ringslot":
                    item = client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Ring));
                    if (variable.Length > 1)
                    {
                        if (string.Compare(variable[1], "id", true) == 0)
                        {
                            return item.Id;
                        }
                        else if (string.Compare(variable[1], "count", true) == 0)
                        {
                            return item.Count;
                        }
                    }
                    return item.Name;
                case "beltslot":
                    item = client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Ammo));
                    if (variable.Length > 1)
                    {
                        if (string.Compare(variable[1], "id", true) == 0)
                        {
                            return item.Id;
                        }
                        else if (string.Compare(variable[1], "count", true) == 0)
                        {
                            return item.Count;
                        }
                    }
                    return item.Name;
                case "backslot":
                    item = client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Backpack));
                    if (variable.Length > 1)
                    {
                        if (string.Compare(variable[1], "id", true) == 0)
                        {
                            return item.Id;
                        }
                        else if (string.Compare(variable[1], "count", true) == 0)
                        {
                            return item.Count;
                        }
                    }
                    return item.Name;
                case "rhandslot":
                    item = client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Right));
                    if (variable.Length > 1)
                    {
                        if (string.Compare(variable[1], "id", true) == 0)
                        {
                            return item.Id;
                        }
                        else if (string.Compare(variable[1], "count", true) == 0)
                        {
                            return item.Count;
                        }
                    }
                    return item.Name;
                case "lhandslot":
                    item = client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Left));
                    if (variable.Length > 1)
                    {
                        if (string.Compare(variable[1], "id", true) == 0)
                        {
                            return item.Id;
                        }
                        else if (string.Compare(variable[1], "count", true) == 0)
                        {
                            return item.Count;
                        }
                    }
                    return item.Name;
                case "amuletslot":
                    item = client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Necklace));
                    if (variable.Length > 1)
                    {
                        if (string.Compare(variable[1], "id", true) == 0)
                        {
                            return item.Id;
                        }
                        else if (string.Compare(variable[1], "count", true) == 0)
                        {
                            return item.Count;
                        }
                    }
                    return item.Name;
                case "bootsslot":
                    item = client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Feet));
                    if (variable.Length > 1)
                    {
                        if (string.Compare(variable[1], "id", true) == 0)
                        {
                            return item.Id;
                        }
                        else if (string.Compare(variable[1], "count", true) == 0)
                        {
                            return item.Count;
                        }
                    }
                    return item.Name;
                case "legsslot":
                    item = client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Legs));
                    if (variable.Length > 1)
                    {
                        if (string.Compare(variable[1], "id", true) == 0)
                        {
                            return item.Id;
                        }
                        else if (string.Compare(variable[1], "count", true) == 0)
                        {
                            return item.Count;
                        }
                    }
                    return item.Name;
                case "chestslot":
                    item = client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Armor));
                    if (variable.Length > 1)
                    {
                        if (string.Compare(variable[1], "id", true) == 0)
                        {
                            return item.Id;
                        }
                        else if (string.Compare(variable[1], "count", true) == 0)
                        {
                            return item.Count;
                        }
                    }
                    return item.Name;
                case "manashielded": return player.HasFlag(Flag.ProtectedByMagicShield);
                case "drunk": return player.HasFlag(Flag.Drunk);
                case "hasted": return player.HasFlag(Flag.Hasted);
                case "paralyzed": return player.HasFlag(Flag.Paralyzed);
                case "connected": return client.LoggedIn;
                case "pkname": break;
                case "fileisline":
                    if (variable.Length > 2)
                    {
                        if (!File.Exists(variable[1])) return false;
                        if (File.ReadAllLines(variable[1]).Contains(variable[2])) return true;
                    }
                    return false;
                case "filerandomline":
                    if (variable.Length > 1)
                    {
                        if (!File.Exists(variable[1])) return "";
                        lines = File.ReadAllLines(variable[1]);
                        return lines[random.Next(lines.Count() - 1)];
                    }
                    return "";
                case "fileline":
                    if (variable.Length > 2)
                    {
                        if (!File.Exists(variable[1])) return "";
                        int.TryParse(variable[2], out param1);
                        if (param1 > 0)
                        {
                            lines = File.ReadAllLines(variable[1]);
                            if (lines.Count() > param1)
                            {
                                return lines[param1];
                            }
                        }
                    }
                    return "";
                case "token":
                    param1 = 1;
                    if (variable.Length > 2)
                    {
                        int.TryParse(variable[2], out param1);
                        if (param1 < 1)
                        {
                            param1 = 1;
                        }
                    }
                    if (variable.Length > 1)
                    {
                        MatchCollection matches = Regex.Matches(variable[1], @"\""([^\""]*?)\""| ([^\""]*?) |^([^\""]*?) | ([^\""]*?)$|^([^\""]*?)$");
                        return matches[param1 - 1].Value.Trim('"');
                    }
                    return "";
                case "cutstr": break;
                case "mshieldtime": break;
                case "hastetime": break;
                case "invistime": break;
                case "strengthtime": break;
                case "invisible": return !player.IsVisible;
                case "dmgs": break;
                case "enemycount": break;
                case "friendcount": break;
                case "fishspots": return client.Map.GetTiles().Count(t => t.Location.Z == player.Z && Tiles.Water.GetNoFishIds().Contains(t.Ground.Id));
                case "waypointson": return core.Modules.Cavebot.Walk;
                case "targetingon": return core.Modules.Targeting.Attacker;
                case "autocomboon": break;
                case "caveboton": break;
                case "ping": break;
                case "idlerecvtime": break;
                case "standtime": break;
                case "systime": return DateTime.Now.ToLongTimeString();
                case "sysdate": return DateTime.Now.ToLongDateString();
                case "battlesign": return player.HasFlag(Flag.InBattle);
                case "redbattlesign": return player.HasFlag(Flag.CannotLogoutOrEnterProtectionZone);
                case "inpz": return player.HasFlag(Flag.WithinProtectionZone);
                case "rand": break;
                case "sstime": break;
                case "winitemcount": break;
                case "fired": break;
                case "synctime": break;
                case "navion": break;
                case "exectime": break;
                case "topitem": break;
                case "istileitem": break;
                case "lastdmg": break;
                case "lastdmgtype": break;
                case "mcount": break;
                case "pcount": break;
                case "screencount": break;
                case "lastdmgername": break;
                case "lastdmgtime": break;
                case "isleader": break;
                case "isfriend": break;
                case "issubfriend": break;
                case "isenemy": break;
                case "issubenemy": break;
                case "self": return player;
                case "target": return client.BattleList.GetCreatures().FirstOrDefault(c => c.Id == player.RedSquare);
                case "followed": return client.BattleList.GetCreatures().FirstOrDefault(c => c.Id == player.GreenSquare);
                case "attacked": return client.BattleList.GetCreatures().FirstOrDefault(c => c.Id == player.RedSquare);
                case "attacker": break;
                case "pk": break;
                case "lastdmger": break;
                case "pattacker": break;
                case "mttacker": break;
                case "enemy": break;
                case "friend": break;
                case "subenemy": break;
                case "subfriend": break;
                case "anyenemy": break;
                case "anyfriend": break;
                case "coretarget": break;
                case "triggertarget": break;
                case "autoaimtarget": break;
                case "creature": break;
                case "mostexposed": break;
                case "mostshot": break;
                case "curmsg": break;
                case "lastmsg": return client.Memory.ReadString(Tibia.Addresses.Client.LastMSGText);
                case "lastnavmsg": break;
            }

            return null;
        }

        protected void reconnect()
        {
            client.Logout();
            Thread.Sleep(500);
            client.Login.Login(client.Memory.ReadString(Tibia.Addresses.Client.LoginAccount), client.Memory.ReadString(Tibia.Addresses.Client.LoginPassword), client.Login.CharacterList[client.Login.SelectedChar].CharName);
        }

        public static string RegexReplaceLoop(string code, string expression, string replace)
        {
            Regex regex;

            regex = new Regex(expression);
            while (regex.IsMatch(code))
            {
                code = regex.Replace(code, replace);
            }

            return code;
        }

        public static string RegexReplaceLoopList(string code, List<string> expressions, List<string> replaces)
        {
            if (expressions.Count != replaces.Count) return code;

            for (int i = 0; i < expressions.Count; i++)
            {
                code = RegexReplaceLoop(code, expressions[i], replaces[i]);
            }

            return code;
        }

        public static string RegexReplaceList(string code, List<string> expressions, List<string> replaces)
        {
            Regex regex;
            if (expressions.Count != replaces.Count) return code;

            for (int i = 0; i < expressions.Count; i++)
            {
                regex = new Regex(expressions[i]);
                if (regex.IsMatch(code))
                {
                    code = regex.Replace(code, replaces[i]);
                }
            }

            return code;
        }

        public static string ParseVariables(string code)
        {
            string result = "";
            bool variable = false;
            bool parameter = false;
            bool parameterStr = false;
            bool parameterStart = false;
            bool str = false;
            bool varInStr = false;

            foreach (char c in code)
            {
                switch (c)
                {
                    case '$':
                        variable = true;
                        if (str)
                        {
                            varInStr = true;
                            result += "\" + ";
                        }
                        result += "GetVariable(\"";
                        break;
                    case '.':
                        if (variable)
                        {
                            parameter = true;
                        }
                        result += c;
                        break;
                    case '"':
                        if (parameterStr)
                        {
                            result += "\\\"";
                        }
                        else
                        {
                            result += c;
                        }
                        break;
                    case '\'':
                        if (parameter && !parameterStart)
                        {
                            if (parameterStr)
                            {
                                parameter = false;
                                parameterStr = false;
                            }
                            else
                            {
                                parameterStr = true;
                            }
                        }
                        else
                        {
                            if (variable)
                            {
                                variable = false;
                                result += "\")";
                                if (varInStr)
                                {
                                    varInStr = false;
                                    result += " + \"";
                                }
                            }
                            str = !str;
                            result += c;
                        }
                        break;
                    default:
                        if (!(char.IsLetterOrDigit(c) || c == '_'))
                        {
                            if (parameter && !parameterStr)
                            {
                                parameter = false;
                                parameterStart = false;
                            }
                            if (variable && !parameter)
                            {
                                variable = false;
                                result += "\")";
                                if (varInStr)
                                {
                                    varInStr = false;
                                    result += " + \"";
                                }
                            }
                        }
                        else if (parameter && !parameterStr)
                        {
                            parameterStart = true;
                        }
                        result += c;
                        break;
                }
            }
            return result;
        }

        public static string GenerateFromScript(string name, string code)
        {
            Regex regex;
            regex = new Regex(@"auto ([0-9]+) ");
            int timeout = Timeout.Infinite;

            if (regex.IsMatch(code))
            {
                int.TryParse(regex.Match(code).Groups[1].Value, out timeout);
                code = regex.Replace(code, "");
            }

            List<string> expressions = new List<string>();
            List<string> replaces = new List<string>();
            List<string> expressionsLoop = new List<string>();
            List<string> replacesLoop = new List<string>();

            expressions.AddRange(new string[] { @"([^;])$" });
            replaces.AddRange(new string[] { @"$1;" });

            expressionsLoop.AddRange(new string[] { @"'([^']*?)'", @"([^\|])\|([^\|])", @"\[([^\[]*?)\]" });
            replacesLoop.AddRange(new string[] { @"""$1""", @"$1;$2", @"($1)" });
            expressionsLoop.AddRange(new string[] { "msgbox" });
            replacesLoop.AddRange(new string[] { "System.Windows.Forms.MessageBox.Show" });

            code = ParseVariables(code);
            code = RegexReplaceList(code, expressions, replaces);
            code = RegexReplaceLoopList(code, expressionsLoop, replacesLoop);

            return GenerateCSharp(name, code, timeout);
        }

        public static string GenerateCSharp(string name, string code, int timeout)
        {
            System.Windows.Forms.MessageBox.Show(code + " ---\n" + timeout);
            string script = "";
            script += "using System;\n";
            script += "using System.Collections.Generic;\n";
            script += "using System.Threading;\n";
            script += "using Kedrah;\n";
            script += "\n";
            script += "namespace Kedrah\n";
            script += "{\n";
            script += " public class " + name + " : Objects.Script\n";
            script += " {\n";
            script += "     public override void Run(Core core)\n";
            script += "     {\n";
            script += "         base.Run(core);\n";
            if (timeout != Timeout.Infinite)
            {
                script += "         Timer Timer = new Timer(new TimerCallback(delegate(object o)\n";
                script += "         {\n";
            }
            script += "             " + code + "\n";
            if (timeout != Timeout.Infinite)
            {
                script += "         }), null, 0, " + timeout.ToString() + ");\n";
            }
            script += "     }\n";
            script += " }\n";
            script += "}\n";

            return script;
        }

        public static string GenerateVBNet(string name, string code)
        {
            string script = "";
            script += "Imports System\n";
            script += "Imports System.Collections.Generic\n";
            script += "Imports Kedrah\n";
            script += "\n";
            script += "Namespace Kedrah\n";
            script += " Public Class " + name + "\n";
            script += "     Inherits Objects.Script\n";
            script += "     Public Overloads Overrides Sub Run(ByVal core As Core)\n";
            script += "         MyBase.Run(core)\n";
            script += code + "\n";
            script += "     End Sub\n";
            script += " End Class\n";
            script += "End Namespace\n";

            return script;
        }
    }
}
