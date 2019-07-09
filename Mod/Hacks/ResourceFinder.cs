using Harmony;
using Mod.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Profane.Gameplay.NetworkObjects.CollectableResources;
using UnityEngine;
using Profane.Gameplay.NetworkObjects.CollectableResources.Bush;
using Profane.Gameplay.NetworkObjects.CollectableResources.Tree;
using Profane.Gameplay.NetworkObjects.CollectableResources.Rock;
using Profane.Gameplay.NetworkObjects.CollectableResources.Plant;
using Profane.Gameplay.NetworkObjects.Loot;
using Profane.Gameplay.Items;
using System.Threading;

namespace Mod.Hacks
{
    public class ResourceFinder : IHack
    {
        private Task m_lineDrawer;
        public static ResourceFinder resourceFinder;

        public Queue<BaseResourceProxy> lineRendererQueue = new Queue<BaseResourceProxy>();

        public bool showTree;
        public bool showBush;
        public bool showRock;
        public bool showPlant;
        public bool showFlint;

        public ResourceFinder()
        {
            resourceFinder = this;


        }

        private void lineDrawer()
        {
            while (true)
            {
                if (lineRendererQueue.Count > 0)
                {
                    var resourceProxy = lineRendererQueue.Dequeue();

                    var lineToTheSky = resourceProxy.gameObject.AddComponent<LineToTheSky>();

                    lineToTheSky.Activate(resourceFinder, resourceProxy);
                }

                Thread.Sleep(200);

            }
        }


        public override void drawGUI()
        {
            resourceFinder.IsActivated = UIHelper.CheckBox(resourceFinder.IsActivated, "Resource Finder");

            if (resourceFinder.IsActivated)
            {
                resourceFinder.showRock = UIHelper.CheckBox(resourceFinder.showRock, "# Show Rocks");
                resourceFinder.showBush = UIHelper.CheckBox(resourceFinder.showBush, "# Show Bush");
                //resourceFinder.showFlint = UIHelper.CheckBox(resourceFinder.showFlint, "# Show Flint");
                resourceFinder.showPlant = UIHelper.CheckBox(resourceFinder.showPlant, "# Show Plant");
                resourceFinder.showTree = UIHelper.CheckBox(resourceFinder.showTree, "# Show Tree");

            }
        }






        private bool _isActivated;

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

        }

        private void Enable()
        {

            if (m_lineDrawer == null)
            {
                m_lineDrawer = new TaskFactory().StartNew(lineDrawer);
            }

        }
        class LineToTheSky : MonoBehaviour
        {
            ResourceFinder resourceFinder;
            MonoBehaviour resource;
            Traverse resourceTraverse;
            LineRenderer line;

            bool isActivated;

            public void Activate(ResourceFinder resourceFinder, MonoBehaviour resource)
            {
                this.resourceFinder = resourceFinder;
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
                if (this.resource is BushProxy)
                {
                    this.line.material.color = new Color(16, 71, 7, 1);
                }
                else if (this.resource is TreeProxy)
                {
                    this.line.material.color = new Color(91, 42, 7, 1);

                }
                else if (this.resource is RockProxy)
                {
                    this.line.material.color = new Color(84, 82, 79, 1);

                }
                else if (this.resource is PlantHarvestableProxy)
                {
                    this.line.material.color = new Color(84, 165, 206, 1);
                }
                //else if (this.resource is BaseLoot<BaseItem> && Traverse.Create(this.resource).Field("item").GetValue<BaseItem>().Type == ItemType.Flint)
                //{
                //    this.line.material.color = Color.white;
                //}
            }

            private void Update()
            {
                if (!isActivated) return;

                setVisibility();

            }

            private void setVisibility()
            {
                if (this.resource is BushProxy)
                {
                    this.line.enabled = resourceFinder.IsActivated && resourceFinder.showBush;
                }
                else if (this.resource is TreeProxy)
                {
                    this.line.enabled = resourceFinder.IsActivated && resourceFinder.showTree;
                }
                else if (this.resource is RockProxy)
                {
                    this.line.enabled = resourceFinder.IsActivated && resourceFinder.showRock;
                }
                else if (this.resource is PlantHarvestableProxy)
                {
                    this.line.enabled = resourceFinder.IsActivated && resourceFinder.showPlant;
                }
                //else if (this.resource is BaseLoot<BaseItem> && Traverse.Create(this.resource).Field("item").GetValue<BaseItem>().Type == ItemType.Flint)
                //{
                //    this.line.enabled = resourceFinder.IsActivated && resourceFinder.showFlint;
                //}
            }
        }

        class Patches
        {


            //class BaseRes
            //{
            //    [HarmonyPatch(typeof(BaseResourceProxy))]
            //    [HarmonyPatch("uLink_OnNetworkInstantiate")]
            //    class onObjectCreating
            //    {
            //        [HarmonyPostfix]
            //        static void Postfix(BaseResourceProxy __instance)
            //        {
            //            resourceFinder.lineRendererQueue.Enqueue(__instance);

            //        }
            //    }

            //}

            //class BaseLoot
            //{

            //    [HarmonyPatch(typeof(BaseLoot<BaseItem>))]
            //    [HarmonyPatch("uLink_OnNetworkInstantiate")]
            //    class onObjectCreating
            //    {
            //        [HarmonyPostfix]
            //        static void Postfix(BaseLoot<BaseItem> __instance)
            //        {
            //            new TaskFactory().StartNew(() =>
            //            {
            //                if (resourceFinder != null)
            //                {
            //                    var lineToTheSky = __instance.gameObject.AddComponent<LineToTheSky>();

            //                    lineToTheSky.Activate(resourceFinder, __instance);

            //                }

            //            });
            //        }
            //    }
            //}


        }

    }


}
