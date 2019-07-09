using Harmony;
using Mod.Helpers;
using Profane.Gameplay.NetworkObjects.Constructions.Flag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mod.Hacks
{
    public class HouseFinder : IHack
    {
        public override bool IsActivated { get; set; }
        public static HouseFinder houseFinder;

        public Queue<FlagProxy> lineRendererQueue = new Queue<FlagProxy>();

        public float maxShields;

        public HouseFinder()
        {
            houseFinder = this;
            maxShields = 5f;

            //new TaskFactory().StartNew(lineDrawer);

        }


        private void lineDrawer()
        {
            while (true)
            {
                if (lineRendererQueue.Count > 0)
                {
                    var flagProxy = lineRendererQueue.Dequeue();

                    var lineToTheSky = flagProxy.gameObject.AddComponent<LineToTheSky>();

                    lineToTheSky.Activate(houseFinder, flagProxy);
                }

                Thread.Sleep(200);

            }

        }

        public override void drawGUI()
        {
            houseFinder.IsActivated = UIHelper.CheckBox(houseFinder.IsActivated, "House Finder ");

            if (houseFinder.IsActivated)
            {
                houseFinder.maxShields = UIHelper.Slider(houseFinder.maxShields, 0f, 6f);
            }

        }


        class LineToTheSky : MonoBehaviour
        {
            HouseFinder houseFinder;
            MonoBehaviour resource;
            Traverse resourceTraverse;
            LineRenderer line;

            bool isActivated;

            public void Activate(HouseFinder houseFinder, MonoBehaviour resource)
            {
                this.houseFinder = houseFinder;
                this.resource = resource;

                this.resourceTraverse = Traverse.Create(resource);

                this.line = this.gameObject.AddComponent<LineRenderer>();
                this.line.positionCount = 2;

                this.line.enabled = false;

                Vector3 targetPosition = this.resource.transform.position;
                Vector3 aboveHead = new Vector3(targetPosition.x, targetPosition.y + 3f, targetPosition.z);
                Vector3 toTheSky = new Vector3(targetPosition.x, targetPosition.y + 120f, targetPosition.z);
                this.line.SetPosition(0, aboveHead);
                this.line.SetPosition(1, toTheSky);



                setLineColor();

                isActivated = true;


            }

            private void setLineColor()
            {
                var ratio = (resource as FlagProxy).GetCooldownRatio();
                this.line.material.color =
                    ratio > 0.1 && ratio < 1.0 ?
                    new Color(0,0,0) :
                    new Color(66,0,0);


            }

            private void Update()
            {
                if (!isActivated) return;

                setVisibility();

            }

            private void setVisibility()
            {
                this.line.enabled = HouseFinder.houseFinder.IsActivated && (resource as FlagProxy).ShieldsRemaining <= HouseFinder.houseFinder.maxShields;

            }
        }



        class Patches
        {


            //class BaseRes
            //{
            //    [HarmonyPatch(typeof(FlagProxy))]
            //    [HarmonyPatch("uLink_OnNetworkInstantiate")]
            //    class onObjectCreating
            //    {
            //        [HarmonyPostfix]
            //        static void Postfix(FlagProxy __instance)
            //        {
            //            houseFinder.lineRendererQueue.Enqueue(__instance);

            //        }
            //    }

            //}



        }


    }
}
