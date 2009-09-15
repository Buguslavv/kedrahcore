using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kedrah.Objects
{
    public partial class Script : IScript
    {
        public string Name;
        public Core Core;

        public Script()
        {
            Name = GetType().Name;
        }

        public virtual bool Run(Core core)
        {
            Core = core;
            return true;
        }

        public virtual bool Stop()
        {
            return true;
        }

        public static string GenerateCSharp(string name, string code)
        {
            string script = "";
            script += "using System;\n";
            script += "using System.Collections.Generic;\n";
            script += "using Kedrah;\n";
            script += "\n";
            script += "namespace Kedrah\n";
            script += "{\n";
            script += " public class " + name + " : Objects.Script\n";
            script += " {\n";
            script += "     public override bool Run(Core core)\n";
            script += "     {\n";
            script += "         base.Run(core);\n";
            script += code;
            script += "         return true;\n";
            script += "     }\n";
            script += " }\n";
            script += "}\n";

            return script;
        }
    }
}
