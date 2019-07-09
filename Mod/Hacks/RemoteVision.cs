using InsanePluginLibrary;
using Profane.Gameplay.GameCamera;
using Profane.Gameplay.NetworkObjects.Characters.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using Profane.Gameplay.NetworkObjects.Characters.Input.Data;
using Profane.Gameplay.NetworkObjects.Characters.Input;
using Profane.Gameplay.NetworkObjects.Characters.Interfaces;
using System.Diagnostics;
using Mod.Helpers;
using Profane.Core.Chat.Message;
using Profane.GUI.Chat;
using Profane.Core.Chat.Enums;

namespace Mod.Hacks
{
    public class RemoteVision : IHack
    {

        public static RemoteVision remoteVision;

        Vector3 height = new Vector3(0f, 1.5f, 0f);

        GameObject sphere;
        
        PlayerOwner player;

        Traverse playerTraverse;

        CameraController camera;

        Renderer sphereRenderer;

        public float moveSpeed { get; set; } = 30f;


        private void disable()
        {
            DespawnSphere();
        }

        private void Enable()
        {
            return;
        }

        public RemoteVision()
        {
            remoteVision = this;

            this.sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            sphereRenderer = sphere.GetComponent<Renderer>();

            sphereRenderer.material.color = Color.green;



            this.player = Singleton.Get<PlayerOwner>();

            playerTraverse = Traverse.Create(player);

            this.camera = Singleton.Get<CameraController>();


        }

        public override void drawGUI()
        {
            remoteVision.IsActivated = UIHelper.CheckBox(remoteVision.IsActivated, "Remote Vision");

            if (remoteVision.IsActivated)
            {

                if (UIHelper.Button("Spawn Sphere")) remoteVision.SpawnSphere();

                if (UIHelper.Button("Despawn Sphere")) remoteVision.DespawnSphere();

                if (UIHelper.Button("Toggle Camera")) remoteVision.ToggleCamera();

                UIHelper.Label("Camera Speed");

                remoteVision.moveSpeed = UIHelper.Slider(remoteVision.moveSpeed, 6f, 90f);




            }
        }


        public void onHackUpdate()
        {
            if (isRemoteVision)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    sphere.transform.position += camera.transform.forward * moveSpeed * Time.deltaTime;
                } else if (Input.GetKey(KeyCode.S))
                {
                    sphere.transform.position -= camera.transform.forward * moveSpeed * Time.deltaTime;
                }

                if (Input.GetKey(KeyCode.D))
                {
                    sphere.transform.position += camera.transform.right * moveSpeed * Time.deltaTime;

                } else if (Input.GetKey(KeyCode.A))
                {
                    sphere.transform.position -= camera.transform.right * moveSpeed * Time.deltaTime;
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    sphere.transform.position += sphere.transform.up * moveSpeed * Time.deltaTime;

                } else if (Input.GetKey(KeyCode.LeftShift)) 
                {
                    sphere.transform.position -= sphere.transform.up * moveSpeed * Time.deltaTime;

                }



            }
            
        }

        public void SpawnSphere()
        {
            sphere.transform.position = player.transform.position + height;
        }

        public void DespawnSphere()
        {
            sphere.transform.position = Vector3.zero;
        }

        public bool isRemoteVision { get; set; }

        private Transform lastestView;

        public void ToggleCamera()
        {
            if (sphere.transform.position == Vector3.zero)
            {
                SpawnSphere();
            }

            if (isRemoteVision)
            {
                sphere.transform.localScale = new Vector3(1f, 1f, 1f);
                sphereRenderer.material.color = Color.magenta;
                camera.SetTarget( lastestView );
            }
            else
            {
                Trace.WriteLine("set sphere");

                lastestView = Traverse.Create(camera).Field("target").GetValue<Transform>();

                sphere.transform.localScale = Vector3.zero;
                sphereRenderer.material.color = Color.green;
                camera.SetTarget(sphere.transform);
            }
            isRemoteVision = !isRemoteVision;
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
                        disable();
                    }
                }

                _isActivated = value;
            }
        }

        class Patches
        {
            [HarmonyPatch(typeof(PlayerInput<IConfigurableInput>))]
            [HarmonyPatch("ProcessInputs")]
            class DisablePlayerControlWhileRemoteVisioning
            {
                static void Postfix(ref ActionData __result)
                {
                    if (remoteVision == null || !remoteVision.IsActivated) return;

                    // Make it not possible to walk in the main character
                    if (remoteVision.isRemoteVision )
                    {
                        __result.jump = false;
                        __result.moveDirection = Vector2.zero;

                    }
                }
            }

        }




    }
}
