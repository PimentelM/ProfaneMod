using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Profane.GUI.PlayerHP;
using Harmony;
using InsanePluginLibrary;
using System.Reflection;
using System.Diagnostics;
using Profane.Gameplay.NetworkObjects.Status.Karma;
using Profane.Gameplay.NetworkObjects.Characters;
using Mod.Helpers;

namespace Mod.Hacks
{
    public class EspHack : IHack
    {
        public static EspHack espHack;

        static Assembly assembly = Assembly.GetExecutingAssembly();

        public EspHack()
        {
            espHack = this;

            Disable();
        }

        public override void drawGUI()
        {
            espHack.IsActivated = UIHelper.CheckBox(espHack.IsActivated, "Esp Hack");

            if (espHack.IsActivated)
            {
                espHack.showPlayers = UIHelper.CheckBox(espHack.showPlayers, "# Show Players");
                espHack.showPartyBar = UIHelper.CheckBox(espHack.showPartyBar, "# Show PartyBar");
                espHack.showMonsters = UIHelper.CheckBox(espHack.showMonsters, "# Show Monsters");
                espHack.showBlueCreatures = UIHelper.CheckBox(espHack.showBlueCreatures, "# Show Blue Creatures");


            }
        }

        private bool _isActivated = false;

        public override bool IsActivated
        {
            get
            {
                return _isActivated;

            }

            set
            {
                if (_isActivated != value)
                {
                    if (value)
                    {
                        Enable();
                    }
                    else
                    {
                        Disable();
                    }
                }

                _isActivated = value;
            }
        }

        private void Disable()
        {
            showBlueCreatures = 
            showMonsters = 
            showPlayers = 
            showPartyBar = false;

        }

        private void Enable()
        {
            showMonsters = 
            showPlayers = 
            showPartyBar = true;
        }

        private void RemoveCreatureNameBrackets()
        {
            var Monsters = from HealthBarWindow bar in (UnityEngine.Object.FindObjectsOfType<HealthBarWindow>() as HealthBarWindow[])
            where bar.entityType == EntityType.Monster
            select bar;

            foreach ( HealthBarWindow monsterHelthBar in Monsters)
            {
                // Remove os brackets ao redor do nome, hehe
                monsterHelthBar.SetName(Traverse.Create(monsterHelthBar).Field("nameText").GetValue<string>().Replace("[","").Replace("]",""));
            }

        }


        public bool showBlueCreatures { get; set; }

        public bool showMonsters { get; set; }


        public bool showPlayers { get; set; }

        public bool showPartyBar { get; set; }

        class HealthBarPatches
        {
            static Dictionary<HealthBarWindow, KarmaType> karmaMap = new Dictionary<HealthBarWindow, KarmaType>();

            // This whole code is being used in order to get and store karma information about the HealthBarWindow owner.
            [HarmonyPatch(typeof(HealthBarWindow))]
            [HarmonyPatch("SetNameColorByKarma")]
            class SetNameColorByKarma
            {
                [HarmonyPrefix]
                static void Prefix(HealthBarWindow __instance, KarmaType type)
                {
                    if (__instance.entityType != EntityType.Monster)
                        return;
                    try
                    {
                        // Stores karma type by instance everytime it is set.
                        if (__instance != null )
                        {
                            karmaMap[__instance] = type;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("ESP.SetNameColorByKarma: " + ex.Message);
                    }


                }


            }


            [HarmonyPatch(typeof(HealthBarWindow))]
            [HarmonyPatch("UpdateVisibilityInfo")]
            class UpdateVisibilityInfo
            {
                
                [HarmonyPostfix]
                static void Postfix(HealthBarWindow __instance, ref bool  __result)
                {
                    bool returnTrue = false;

                    if (__instance == null) return;
                    if (espHack == null || !espHack.IsActivated) return;

                    


                    Traverse traverse = Traverse.Create(__instance);

                    void makeVisible()
                    {
                        try
                        {
                            __instance.SetTextGroupAlpha(1f);
                            __instance.SetCanvasGroupAlpha(1f);

                            var info = traverse.Field("info");

                            info.Field("isVisible").SetValue(true);
                            info.Field("isOccluded").SetValue(false);

                            returnTrue = true;



                        } catch ( Exception ex)
                        {
                            Trace.WriteLine("ESP: " + ex.Message );
                        }

                    }



                    try
                    {
                        switch (__instance.entityType)
                        {
                            case EntityType.PlayerProxy:
                                // Muda para partyBar
                                if (espHack.showPartyBar)
                                {
                                    __instance.SwitchToPartyBar();
                                    __instance.SetBarsActive(true);
                                } else
                                {

                                }

                                // Torna visivel
                                if (espHack.showPlayers)
                                    makeVisible();
                                break;
                            case EntityType.Monster:

                                string name = traverse.Field("nameText").GetValue<string>();

                                if (!name.Contains("["))
                                {
                                    __instance.SetName("[" + name + "]");
                                }



                                // Get creature name color
                                KarmaType karma = KarmaType.NEGATIVE;
                                karmaMap.TryGetValue(__instance, out karma);
                                

                                // Using b because comparing the structs is not working propely.
                                if (karma == KarmaType.POSITIVE)
                                {
                                    if (espHack.showBlueCreatures)
                                    {
                                        makeVisible();
                                    }

                                } else
                                {
                                    if (espHack.showMonsters)
                                    {
                                        makeVisible();
                                    }
                                }




                                break;
                            default:
                                break;

                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("ESP: " + ex.Message + "\n" + ex.StackTrace);
                    }


                    __result |= returnTrue;

                }
            }

        }


    }
}
