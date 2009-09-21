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
