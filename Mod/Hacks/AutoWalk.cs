using Harmony;
using Mod.Helpers;
using Profane.Gameplay.NetworkObjects.Characters.Input;
using Profane.Gameplay.NetworkObjects.Characters.Input.Data;
using Profane.Gameplay.NetworkObjects.Characters.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Hacks
{
    public class AutoWalk : IHack
    {
        public override bool IsActivated { get; set; }
        public static AutoWalk autoWalk;

        public AutoWalk()
        {
            autoWalk = this;
        }

        public override void drawGUI()
        {
            autoWalk.IsActivated = UIHelper.CheckBox(autoWalk.IsActivated, "Auto Walk");
        }
        
        class Patches
        {
            [HarmonyPatch(typeof(PlayerInput<IConfigurableInput>))]
            [HarmonyPatch("ProcessInputs")]
            class InputProcessing
            {
                static void Postfix(ref ActionData __result)
                {
                    if (autoWalk != null && autoWalk.IsActivated)
                    {
                        __result.moveDirection.y = __result.moveDirection.y + 1f;
                    }
                }
            }

        }


    }
}
