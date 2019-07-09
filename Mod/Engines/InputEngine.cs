using Harmony;
using Newtonsoft.Json;
using Profane.Core.Chat.Enums;
using Profane.Core.Chat.Message;
using Profane.Gameplay.NetworkObjects.Characters.Input;
using Profane.Gameplay.NetworkObjects.Characters.Input.Data;
using Profane.Gameplay.NetworkObjects.Characters.Interfaces;
using Profane.GUI.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Engines
{
    public class InputEngine
    {
        private static Queue<Func<ActionData, ActionData>> ActionQueue = new Queue<Func<ActionData, ActionData>>();
        private static bool locked = false;

        public static void Enqueue(Func<ActionData,ActionData> f)
        {
            lock (ActionQueue)
            {
                locked = true;
                ActionQueue.Enqueue(f);
                locked = false;
            }
        }

        public static void Enqueue(Func<ActionData,ActionData>[] fs)
        {
            lock (ActionQueue)
            {
                locked = true;
                foreach (var f in fs)
                {
                    ActionQueue.Enqueue(f);
                }
                locked = false;
            }
        }


        class Patches
        {

            [HarmonyPatch(typeof(PlayerInput<IConfigurableInput>))]
            [HarmonyPatch("ProcessInputs")]
            class DisableWalkHotkey
            {
                static void Postfix(ref ActionData __result)
                {
                    // Anti idle
                    __result.AnyAction = true;


                    if (!locked && ActionQueue.Count > 0)
                    {
                        //Trace.WriteLine("Queue Count:" + ActionQueue.Count.ToString());

                        var f = ActionQueue.Dequeue();


                        __result = f(__result);


                    }

                }
            }



        }

    }
}
