using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using IronPython;
using IronPython.Modules;
using Microsoft.Scripting.Hosting;

namespace Mod.ScriptingEngine
{
    public class Repl
    {


        public Repl()
        {
            // create the engine like last time  
            var scriptEngine = IronPython.Hosting.Python.CreateEngine();
            var scriptScope = scriptEngine.CreateScope();
            // load the assemblies for unity, using the types of GameObject  
            // and Editor so we don't have to hardcoded paths  
            scriptEngine.Runtime.LoadAssembly(typeof(GameObject).Assembly);
            StringBuilder example = new StringBuilder();

            scriptEngine.Runtime.IO.SetOutput(new Writter.ScriptOutputStream(), Encoding.UTF8);

            example.AppendLine("import UnityEngine as unity");
            example.AppendLine("print 'Done!'");
            
            var ScriptSource = scriptEngine.CreateScriptSourceFromString(example.ToString());
            ScriptSource.Execute(scriptScope);


        }
        
        public static void PopulateScope(ScriptScope scope)
        {

            scope.SetVariable("Hack",Hack.hack);


        }

    }
}
