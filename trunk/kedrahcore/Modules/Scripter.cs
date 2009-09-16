using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.IO;
using System.Reflection;
using System.CodeDom.Compiler;
using Kedrah.Objects;
using System.Threading;

namespace Kedrah.Modules
{
    public class Scripter : Module
    {
        #region Variables/Objects

        private static CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
        private static VBCodeProvider vBCodeProvider = new VBCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
        private Dictionary<string, Script> loadedScripts = new Dictionary<string, Script>();

        public string ErrorLog;

        #endregion

        #region Constructor/Destructor

        public Scripter(ref Core core)
            : base(ref core)
        {
            Enable();
        }

        #endregion

        #region Get/Set Objects

        public CSharpCodeProvider CSharpCodeProvider
        {
            get
            {
                return cSharpCodeProvider;
            }
        }

        public VBCodeProvider VBCodeProvider
        {
            get
            {
                return vBCodeProvider;
            }
        }

        #endregion

        #region Module Function

        public void RunAll()
        {
            foreach (KeyValuePair<string, Script> script in loadedScripts)
            {
                Run(script.Key);
            }
        }

        public void Run(string name)
        {
            if (loadedScripts.ContainsKey(name))
            {
                Thread thread = new Thread(new ThreadStart(delegate()
                {
                    loadedScripts[name].Run(Core);
                }));
                thread.Start();
            }
        }

        public void StopAll(string name)
        {
            foreach (KeyValuePair<string, Script> script in loadedScripts)
            {
                script.Value.Stop();
            }
        }

        public void Stop(string name)
        {
            if (loadedScripts.ContainsKey(name))
            {
                loadedScripts[name].Stop();
            }
        }

        public void UnloadScript(string name)
        {
            loadedScripts[name].Stop();
            loadedScripts.Remove(name);
        }

        public void LoadScriptFromFile(string path)
        {
            Assembly assembly = null;
            switch (Path.GetExtension(path))
            {
                case ".dll":
                    assembly = LoadDll(path);
                    break;
                case ".cs":
                    assembly = CompileScriptFromFile(path, cSharpCodeProvider, ref ErrorLog);
                    break;
                case ".vb":
                    assembly = CompileScriptFromFile(path, vBCodeProvider, ref ErrorLog);
                    break;
            }

            LoadScriptFromAssembly(assembly);
        }

        public void LoadScriptFromSource(string source, CodeDomProvider provider)
        {
            Assembly assembly = null;
            assembly = CompileScriptFromSource(source, provider, ref ErrorLog);

            LoadScriptFromAssembly(assembly);
        }

        public void LoadScriptFromAssembly(Assembly assembly)
        {
            if (!Enabled) return;
            if (assembly != null)
            {
                foreach (Script script in FindScripts(assembly))
                {
                    if (loadedScripts.ContainsKey(script.Name))
                    {
                        loadedScripts.Remove(script.Name);
                    }
                    loadedScripts.Add(script.Name, script);
                }
            }
        }

        public static Assembly CompileScriptFromFile(string path, CodeDomProvider provider, ref string errors)
        {
            return CompileScriptFromSource(File.ReadAllText(path), provider, ref errors);
        }

        public static Assembly CompileScriptFromSource(string source, CodeDomProvider provider, ref string errors)
        {
            StringBuilder errorLog = new StringBuilder();
            errors = "";
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = true;
            compilerParameters.IncludeDebugInformation = false;
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            compilerParameters.ReferencedAssemblies.Add("KedrahCore.dll");
            compilerParameters.ReferencedAssemblies.Add("TibiaAPI.dll");
            compilerParameters.ReferencedAssemblies.Add(System.Reflection.Assembly.GetExecutingAssembly().Location);
            CompilerResults results = provider.CompileAssemblyFromSource(compilerParameters, source);
            if (!results.Errors.HasErrors)
            {
                return results.CompiledAssembly;
            }
            else
            {
                foreach (CompilerError error in results.Errors)
                {
                    errorLog.AppendLine(error.ErrorText);
                }
            }

            errors = errorLog.ToString();
            return null;
        }

        public static IEnumerable<Script> FindScripts(Assembly assembly)
        {
            foreach (Type t in assembly.GetTypes())
            {
                t.ToString();
                if (t.GetInterface("IScript", true) != null)
                {
                    yield return (Script)assembly.CreateInstance(t.FullName);
                }
            }
        }

        public static Assembly LoadDll(string path)
        {
            return Assembly.LoadFile(path);
        }

        #endregion
    }
}
