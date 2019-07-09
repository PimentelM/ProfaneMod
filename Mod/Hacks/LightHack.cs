using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Harmony;
using InsanePluginLibrary;
using Mod.Helpers;
using Profane;
using Profane.Gameplay.Environment.Time;
using Profane.Gameplay.Input;
using Profane.Gameplay.NetworkObjects.Characters.Player;
using usky;

namespace Mod.Hacks
{
    public class LightHack : IHack
    {


        public static LightHack lightHack;

        public uSkyTimeline uSkyTime;

        public InputLayer inputLayer = InputSystem.GetLayer("SkillManager");


        public LightHack()
        {
            lightHack = this;
            this.customTime = 7f;
            createUSkyTime();


        }

        public void initializer()
        {
            while (true)
            {
                lightHack.uSkyTime = UnityEngine.Object.FindObjectOfType<uSkyTimeline>();

                if (lightHack.uSkyTime != null)
                {
                    isReady = true;
                    return;
                }
                
                Thread.Sleep(1000);
            }


        }

        private void createUSkyTime()
        {
            Task.Factory.StartNew(initializer);
        }

        public override void drawGUI()
        {
            lightHack.IsActivated = UIHelper.CheckBox(lightHack.IsActivated, "Light Hack");

            if (lightHack.IsActivated)
            {
                lightHack.customTime = UIHelper.Slider(lightHack.customTime, 0f, 24f);
            }
        }

        public void enable()
        {
        }

        public void disable()
        {
        }


        public float customTime
        {
            set;            
            get;
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
                        enable();
                    }
                    else
                    {
                        disable();
                    }
                }

                _isActivated = value;
            }
        }

        public bool isReady { get; private set; }

        class Patches
        {

            [HarmonyPatch(typeof(GameTime))]
            [HarmonyPatch("ClearUSkyRef")]
            class ClearUSkyRef
            {
                [HarmonyPrefix]
                static void Prefix()
                {
                    lightHack.isReady = false;
                    lightHack.createUSkyTime();
                }

            }
                


            [HarmonyPatch(typeof(GameTime))]
            [HarmonyPatch("UpdateGameTime")]
            class UpdateGameTime
            {
                [HarmonyPostfix]
                static void Postfix()
                {
                    if (lightHack == null || !lightHack.IsActivated || !lightHack.isReady) return;


                    if (lightHack.inputLayer.GetKey(UnityEngine.KeyCode.LeftControl) && lightHack.inputLayer.GetKey(UnityEngine.KeyCode.X))
                        return;


                    lightHack.uSkyTime.Timeline = lightHack.customTime;





                }

                

            }

        }


    }
}