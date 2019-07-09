using Harmony;
using Profane.Gameplay.NetworkObjects.Characters.Player;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Events
{
    class PlayerLoaded
    {
        [HarmonyPatch(typeof(PlayerOwner))]
        [HarmonyPatch("uLink_OnNetworkInstantiate")]
        class LoadHack
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                Trace.WriteLine("Player Loaded");
                Hack.onPlayerLoaded(null, null);
            }
        }

    }
}
