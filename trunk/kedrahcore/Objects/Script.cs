using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using Tibia.Objects;
using System.IO;
using Tibia.Constants;

namespace Kedrah.Objects
{
    public partial class Script : IScript
    {
        public string Name;
        protected Core core;
        protected Player player;
        protected Client client;

        #region Custom Variables

        protected int mp { get { return player.Mana; } }
        protected int maxmp { get { return player.Mana_Max; } }
        protected int mppc { get { return (100 * player.Mana / player.Mana_Max); } }
        protected int hp { get { return player.HP; } }
        protected int maxhp { get { return player.HP_Max; } }
        protected int hppc { get { return player.HPBar; } }
        protected int cap { get { return player.Cap; } }
        protected int exp { get { return player.Exp; } }
        protected int level { get { return player.Level; } }
        protected int mlevel { get { return player.MagicLevel; } }
        protected int posx { get { return player.X; } }
        protected int posy { get { return player.Y; } }
        protected int posz { get { return player.Z; } }
        protected int soul { get { return player.Soul; } }
        protected int stamina { get { return player.Stamina; } }
        //protected int count { get { return 0; } } // NEED MODULE
        protected int screenleft { get { return client.Window.Size.Left; } }
        protected int screenright { get { return client.Window.Size.Rigth; } }
        protected int screentop { get { return client.Window.Size.Top; } }
        protected int screenbottom { get { return client.Window.Size.Bottom; } }
        protected string name { get { return player.Name; } }
        protected int time { get { return DateTime.Now.Second; } }
        protected int timems { get { return DateTime.Now.Millisecond; } }
        protected double deltatime { get { return DateTime.Now.Subtract(core.StartTime).TotalSeconds; } }
        protected double deltatimems { get { return DateTime.Now.Subtract(core.StartTime).TotalMilliseconds; } }
        //protected int exptnl { get { return 0; } } // NEED MODULE
        //protected int exph { get { return 0; } } // NEED MODULE
        //protected int expgained { get { return 0; } } // NEED MODULE
        //protected int timetnl { get { return 0; } } // NEED MODULE
        //protected int exptolevel { get { return 0; } } // FUNCTION
        //protected int timetolevel { get { return 0; } } // FUNCTION
        //protected int monstersaround { get { return 0; } } // FUNCTION
        //protected int playersaround { get { return 0; } } // FUNCTION
        //protected int sbtime { get { return 0; } }  // NEED MODULE
        //protected int formattime { get { return 0; } } // FUNCTION
        //protected int formatnum { get { return 0; } } // FUNCTION
        //protected int itemcount { get { return 0; } } // FUNCTION
        protected bool poisoned { get { return player.HasFlag(Flag.Poisoned); } }
        //protected int poisondmg { get { return 0; } } // NEED MODULE
        protected Item ringslot { get { return client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Ring)); } }
        protected Item beltslot { get { return client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Ammo)); } }
        protected Item backslot { get { return client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Backpack)); } }
        protected Item rhandslot { get { return client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Right)); } }
        protected Item lhandslot { get { return client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Left)); } }
        protected Item amuletslot { get { return client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Necklace)); } }
        protected Item bootsslot { get { return client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Feet)); } }
        protected Item legsslot { get { return client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Legs)); } }
        protected Item chestslot { get { return client.Inventory.GetItem(ItemLocation.FromSlot(SlotNumber.Armor)); } }
        protected bool manashielded { get { return player.HasFlag(Flag.ProtectedByMagicShield); } }
        protected bool drunk { get { return player.HasFlag(Flag.Drunk); } }
        protected bool hasted { get { return player.HasFlag(Flag.Hasted); } }
        protected bool paralyzed { get { return player.HasFlag(Flag.Paralyzed); } }
        protected bool connected { get { return client.LoggedIn; } }
        //protected int pkname { get { return 0; } } // FUNCTION
        //protected int fileisline { get { return 0; } } // FUNCTION
        //protected int filerandomline { get { return 0; } } // FUNCTION
        //protected int fileline { get { return 0; } } // FUNCTION
        //protected int token { get { return 0; } } // FUNCTION
        //protected int cutstr { get { return 0; } } // FUNCTION
        //protected int mshieldtime { get { return 0; } } // NEED MODULE
        //protected int hastetime { get { return 0; } } // NEED MODULE
        //protected int invistime { get { return 0; } } // NEED MODULE
        //protected int strengthtime { get { return 0; } } // NEED MODULE
        protected bool invisible { get { return !player.IsVisible; } }
        //protected int dmgs { get { return 0; } } // NEED MODULE
        //protected int enemycount { get { return 0; } } // NEED MODULE
        //protected int friendcount { get { return 0; } } // NEED MODULE
        protected int fishspots { get { return client.Map.GetTiles().Count(t => t.Location.Z == player.Z && Tiles.Water.GetNoFishIds().Contains(t.Ground.Id)); } }
        protected bool waypointson { get { return core.Modules.Cavebot.Walk; } }
        protected bool targetingon { get { return core.Modules.Targeting.Attacker; } }
        //protected bool autocomboon { get { return false; } } // NEED MODULE
        //protected bool caveboton { get { return waypointson; } }
        //protected int ping { get { return 0; } } // NEED MODULE
        //protected int idlerecvtime { get { return 0; } } // NEED MODULE
        //protected int standtime { get { return 0; } } // NEED MODULE
        protected string systime { get { return DateTime.Now.ToLongTimeString(); } }
        protected string sysdate { get { return DateTime.Now.ToLongDateString(); } }
        protected bool battlesign { get { return player.HasFlag(Flag.InBattle); } }
        protected bool redbattlesign { get { return player.HasFlag(Flag.CannotLogoutOrEnterProtectionZone); } }
        protected bool inpz { get { return player.HasFlag(Flag.WithinProtectionZone); } }
        //protected int rand { get { return 0; } } // FUNCTION
        //protected int sstime { get { return 0; } } // NEED MODULE
        //protected int winitemcount { get { return 0; } } // FUNCTION
        //protected int topitem { get { return 0; } } // FUNCTION
        //protected int istileitem { get { return 0; } } // FUNCTION
        //protected int lastdmg { get { return 0; } } // NEED MODULE
        //protected int lastdmgtype { get { return 0; } } // NEED MODULE
        //protected int mcount { get { return 0; } } // FUNCTION
        //protected int pcount { get { return 0; } } // FUNCTION
        //protected int screencount { get { return 0; } } // FUNCTION
        //protected int lastdmgername { get { return 0; } } // NEED MODULE
        //protected int lastdmgtime { get { return 0; } } // NEED MODULE
        //protected int isleader { get { return 0; } } // FUNCTION
        //protected int isfriend { get { return 0; } } // FUNCTION
        //protected int issubfriend { get { return 0; } } // FUNCTION
        //protected int isenemy { get { return 0; } } // FUNCTION
        //protected int issubenemy { get { return 0; } } // FUNCTION
        protected Player self { get { return player; } }
        protected Creature target { get { return client.BattleList.GetCreatures().FirstOrDefault(c => c.Id == player.RedSquare); } }
        protected Creature followed { get { return client.BattleList.GetCreatures().FirstOrDefault(c => c.Id == player.GreenSquare); } }
        protected Creature attacked { get { return target; } }
        //protected Creature attacker { get { return 0; } }
        //protected Creature pk { get { return 0; } }
        //protected Creature lastdmger { get { return 0; } }
        //protected Creature pattacker { get { return 0; } }
        //protected Creature mttacker { get { return 0; } }
        //protected Creature enemy { get { return 0; } }
        //protected Creature friend { get { return 0; } }
        //protected Creature subenemy { get { return 0; } }
        //protected Creature subfriend { get { return 0; } }
        //protected Creature anyenemy { get { return 0; } }
        //protected Creature anyfriend { get { return 0; } }
        //protected Creature coretarget { get { return 0; } }
        //protected Creature triggertarget { get { return 0; } }
        //protected Creature autoaimtarget { get { return 0; } }
        //protected int creature { get { return 0; } }
        //protected int mostexposed { get { return 0; } }
        //protected int mostshot { get { return 0; } }
        //protected int curmsg { get { return 0; } }
        protected string lastmsg { get { return client.Memory.ReadString(Tibia.Addresses.Client.LastMSGText); } }

        #endregion

        public Script()
        {
            Name = GetType().Name;
        }

        public virtual void Run(Core core)
        {
            this.core = core;
            client = core.Client;
            player = core.Player;
        }

        public virtual void Stop()
        {
            Thread.CurrentThread.Abort();
        }

        protected void reconnect()
        {
            client.Logout();
            Thread.Sleep(500);
            client.Login.Login(client.Memory.ReadString(Tibia.Addresses.Client.LoginAccount), client.Memory.ReadString(Tibia.Addresses.Client.LoginPassword), client.Login.CharacterList[client.Login.SelectedChar].CharName);
        }

        public static string RegexReplaceLoop(string code, List<string> expressions, List<string> replaces)
        {
            Regex regex;
            if (expressions.Count != replaces.Count) return code;

            for (int i = 0; i < expressions.Count; i++)
            {
                regex = new Regex(expressions[i]);
                while (regex.IsMatch(code))
                {
                    code = regex.Replace(code, replaces[i]);
                }
            }

            return code;
        }

        public static string RegexReplace(string code, List<string> expressions, List<string> replaces)
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

            expressions.AddRange(new string[] { @"'([^']*?)'", @"([^;])$", @"(\"".*)(\$[a-z][a-z0-9]+)(.*\"")" });
            replaces.AddRange(new string[] { @"""$1""", @"$1;", @"$1"" + $2.ToString() + ""$3" });

            expressionsLoop.AddRange(new string[] { @"'([^']*?)'", @"([^\|])\|([^\|])", @"\[([^\[]*?)\]" });
            replacesLoop.AddRange(new string[] { @"""$1""", @"$1;$2", @"($1)" });
            expressionsLoop.AddRange(new string[] { @"\$([a-z][a-z0-9]+)", "msgbox" });
            replacesLoop.AddRange(new string[] { @"$1", "System.Windows.Forms.MessageBox.Show" });

            code = RegexReplace(code, expressions, replaces);
            code = RegexReplaceLoop(code, expressionsLoop, replacesLoop);

            return GenerateCSharp(name, code, timeout);
        }

        public static string GenerateCSharp(string name, string code, int timeout)
        {
            System.Windows.Forms.MessageBox.Show(code);
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
            script += "         Timer Timer = new Timer(new TimerCallback(delegate(object o)\n";
            script += "         {\n";
            script += "             " + code + "\n";
            script += "         ;}), null, 0, " + timeout.ToString() + ");\n";
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
