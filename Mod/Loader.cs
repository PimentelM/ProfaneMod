using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using InsanePluginLibrary;
using Harmony;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Mod
{
    class Loader
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();


        

        public static void Load()
        {

            AllocConsole();
            Trace.WriteLine("Console is ready!");

            Loader.Mod = new GameObject();
            Loader.Mod.AddComponent<Hack>();
            UnityEngine.Object.DontDestroyOnLoad(Loader.Mod);

            try
            {
                HarmonyInstance.DEBUG = true;
                var harmony = HarmonyInstance.Create("Hack");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            } catch (Exception ex)
            {
                Trace.WriteLine("Could not load all patches: " + ex.Message);
                Trace.WriteLine(ex.StackTrace);
                
                

            }


            //new TaskFactory().StartNew(() => 
            //{
            //    try
            //    {
            //        Trace.WriteLine("Loaded Console");
            //    }
            //    catch ( Exception ex)
            //    {
            //        Trace.WriteLine("Could not load console: " + ex.Message);
            //    }

            //}
            //);

        }



        private static GameObject Mod;

    }



}
