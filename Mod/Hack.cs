using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Mod.Helpers;
using InsanePluginLibrary;
using Mod.Hacks;
using Profane.Gameplay.Environment.Time;

using Harmony;
using System.Reflection;
using Profane.Gameplay.GameCamera;
using Profane.Gameplay.NetworkObjects.Characters.Player;
using Mod.ScriptingEngine;
using System.Diagnostics;

namespace Mod
{
    public class Hack : MonoBehaviour
    {

        public static Hack hack;

        public static EventHandler onPlayerLoaded;

        public LightHack lightHack;
        public EspHack espHack;
        public AimBot aimBot;
        public RemoteVision remoteVision;
        public ResourceFinder resourceFinder;
        public AutoWalk autoWalk;
        public Repl scriptingEngine;
        public HouseFinder houseFinder;
        public Trainer trainer;

        private bool isVisible = false;
        private bool showPersistent = false;



        public void Start()
        {
            Hack.hack = this;

            onPlayerLoaded += (object sender, EventArgs e) =>
            {
                try
                {
                    lightHack = new LightHack();
                    espHack = new EspHack();
                    aimBot = new AimBot();
                    remoteVision = new RemoteVision();
                    autoWalk = new AutoWalk();
                    trainer = new Trainer();


                    //resourceFinder = new ResourceFinder();
                    //houseFinder = new HouseFinder();


                    //scriptingEngine = new Repl();

                    trainer.IsActivated = true;

                    remoteVision.IsActivated = true;
                    lightHack.IsActivated = true;
                    espHack.IsActivated = true;
                    aimBot.IsActivated = true;
                    aimBot.allowMobs = true;
                    aimBot.allowBlues = true;

                    isVisible = true;
                }catch(Exception ex)
                {
                    Trace.WriteLine("There was an error while executing the onPlayerLoaded handler:");
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLine(ex.StackTrace);
                }

            };



        }

        private void checkToggleVisibility()
        {
            if (Input.GetKeyUp(KeyCode.F7) && Input.GetKey(KeyCode.LeftShift))
            {
                isVisible = !isVisible;
            };

            if (Input.GetKeyUp(KeyCode.F9) && Input.GetKey(KeyCode.LeftShift))
            {
                showPersistent = !showPersistent;
            };

        }

        public void Update()
        {
            checkToggleVisibility();

            if (aimBot!=null && aimBot.IsActivated) aimBot.onHackUpdate();
            if (remoteVision != null && remoteVision.IsActivated) remoteVision.onHackUpdate();




            //Do stuff here on every tick
        }

        public void OnGUI()
        {
            aimBot.drawTargetGUI();

            if (!isVisible) return;


            UIHelper.Begin("Profane Hacks",30,30,200,450,20,20,10);

            UIHelper.Label("");

            if (autoWalk != null)
            {
                autoWalk.drawGUI();
            }

            if (trainer != null)
            {
                trainer.drawGUI();
            }

            if(remoteVision != null)
            {
                remoteVision.drawGUI();        
            }

            if (houseFinder != null)
            {
                houseFinder.drawGUI();
            }

            if(resourceFinder != null)
            {
                resourceFinder.drawGUI();
            }

            

            if (showPersistent)
            {
                if (aimBot != null)
                {
                    aimBot.drawGUI();
                }

                if (lightHack != null)
                {
                    lightHack.drawGUI();
                }

                if (espHack != null)
                {
                    espHack.drawGUI();
                }

            }






        }

    }
}
