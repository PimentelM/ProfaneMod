using Harmony;
using InsanePluginLibrary;
using Mod.Helpers;
using Profane.Gameplay.GameCamera;
using Profane.Gameplay.Input;
using Profane.Gameplay.NetworkObjects.Characters;
using Profane.Gameplay.NetworkObjects.Characters.Input;
using Profane.Gameplay.NetworkObjects.Characters.Input.Data;
using Profane.Gameplay.NetworkObjects.Characters.Interfaces;
using Profane.Gameplay.NetworkObjects.Characters.Player;
using Profane.Gameplay.NetworkObjects.Mountables;
using Profane.Gameplay.NetworkObjects.Status.Karma;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mod.Hacks
{
    public class AimBot : IHack
    {

        public static AimBot aimBot;

        CameraController camera;

        Traverse cameraTraverse;

        public BaseCharacter target { get; set; } = null;

        BaseCharacter[] nearTargetList;

        PlayerOwner playerOwner;

        LineRenderer line;

        InputLayer inputLayer = InputSystem.GetLayer("SkillManager");


        private bool aimOnNextFrame = false;

        Vector3 height = new Vector3(0, 1.5f, 0);

        float minDistanceFromTarget = 4f;

        int targetIndex = 0;
        float lastTargetChange = 0;

        float cooldown = 2f;
        int quantityOfPlayers = 8;





        public AimBot()
        {
            aimBot = this;

            this.camera = Singleton.Get<CameraController>();
            this.cameraTraverse = Traverse.Create(camera);

            this.line = Hack.hack.gameObject.AddComponent<LineRenderer>();
            this.line.positionCount = 2;



            this.playerOwner = UnityEngine.Object.FindObjectOfType<PlayerOwner>();

        }

        public override void drawGUI()
        {
            aimBot.IsActivated = UIHelper.CheckBox(aimBot.IsActivated, "Aim Bot");

            if (aimBot.IsActivated)
            {
                aimBot.allowMobs = UIHelper.CheckBox(aimBot.allowMobs, "# Allow Mobs");

                aimBot.allowBlues = UIHelper.CheckBox(aimBot.allowBlues, "# Allow Blues ");

                aimBot.showDebugInfo = UIHelper.CheckBox(aimBot.showDebugInfo, "# Show Debug Info ");



                if (aimBot.showDebugInfo)
                {
                    var centeredStyle = GUI.skin.GetStyle("Label");
                    centeredStyle.alignment = TextAnchor.UpperCenter;

                    int blockSize = 40;

                    GUI.Label(new Rect(Screen.width / 2 - 50, (Screen.height / 20) + (blockSize * 1), 100, 50), aimBot.debugInfo.rotationRadians.ToString(), centeredStyle);
                    GUI.Label(new Rect(Screen.width / 2 - 50, (Screen.height / 20) + (blockSize * 2), 100, 50), "Camera Y Rotation: " + aimBot.debugInfo.cameraYRotation.ToString(), centeredStyle);
                    GUI.Label(new Rect(Screen.width / 2 - 50, (Screen.height / 20) + (blockSize * 3), 100, 50), "Camera X Rotation: " + aimBot.debugInfo.cameraXRotation.ToString(), centeredStyle);


                }

            }
        }

        private BaseCharacter[] nearTargetListCache;
        private float lastCacheUpdate = 0f;


        private BaseCharacter[] getNearTargetList
        {
            get
            {
                if ( Math.Abs(Time.time - lastCacheUpdate) > cooldown || nearTargetListCache == null )
                {
                    var res = (from Creature in UnityEngine.Object.FindObjectsOfType<BaseCharacter>()
                               where 
                               
                               // Permite Players, e criaturas caso criaturas seja permitido.
                               (Creature is PlayerProxy || (allowMobs && !(Creature is MountProxy && (Creature as MountProxy).IsMounted)))

                               // Se nao permite azul e a criatura eh azul, dropa
                               && !(!allowBlues && Creature.Karma.Type == KarmaType.POSITIVE)

                               // Se permite azul, a criatura eh azul e nao for player, dropa
                               && !( allowBlues && Creature.Karma.Type == KarmaType.POSITIVE && !(Creature is PlayerProxy))

                               // Se for o proprio jogador, dropa
                               && !(Creature is PlayerOwner)
                               orderby Vector3.Distance(playerOwner.transform.position, Creature.transform.position) ascending
                               select Creature).Take(quantityOfPlayers).ToArray();
                    lastCacheUpdate = Time.time;
                    nearTargetListCache = res;
                    return res;
                } else
                {
                    return nearTargetListCache;
                }
            }
        }

        internal void setTarget(PlayerProxy target)
        {

            this.target = target;

        }

        private void updateTargetList()
        {
            targetIndex = 0;
            nearTargetList = getNearTargetList;
        }

        internal void drawTargetGUI()
        {
            if (aimBot.target != null)
            {
                var centeredStyle = GUI.skin.GetStyle("Label");
                centeredStyle.alignment = TextAnchor.UpperCenter;
                GUI.Label(

                    new Rect(Screen.width / 2 - 50, Screen.height / 20, 100, 50),

                    aimBot.targetName
                    
                    , centeredStyle);

            }
        }

        public struct DebugInfo
        {
            public Vector3 rotationEulerAngles;
            public Vector3 rotationRadians;
            public float cameraXRotation;
            public float cameraYRotation;


            public DebugInfo(Vector3 Euler, Vector3 Radians, float X, float Y)
            {
                rotationRadians = Radians;
                rotationEulerAngles = Euler;
                cameraXRotation = X;
                cameraYRotation = Y;

            }
        }

        public DebugInfo debugInfo {get; private set;}

        private Dictionary<float, float> radiansToYRotation = new Dictionary<float, float>();

        private void makeDebugInfo()
        {
            if (!showDebugInfo) return;

            Vector3 rotationEulerAngles = camera.transform.rotation.eulerAngles;

            Vector3 rotationRadians = (rotationEulerAngles * (float)Math.PI) / 180f;

            float cameraYRotation = cameraTraverse.Field("cameraYRotation").GetValue<float>();
            float cameraXRotation = cameraTraverse.Field("cameraXRotation").GetValue<float>();

            float x = rotationRadians.x > 1.0 ? rotationRadians.x - (Mathf.PI * 2f) : rotationRadians.x;

            radiansToYRotation[x] = cameraYRotation;

            debugInfo = new DebugInfo(rotationEulerAngles, rotationRadians, cameraXRotation, cameraYRotation);

        }


        public void UpdateAim()
        {
            if (!IsActivated) return;
            
            if (cantAim)
                return;

            if (target == null)
            {
                if (getNearTargetList.Count() == 0)
                    return;

                var nearestTarget = getNearTargetList.First();

                // Caso o player esteja atacando, mirar automaticamente caso nao tenha target e um target viavel esteja em range.
                if (isPlayerAttacking && Vector3.Distance( nearestTarget.transform.position, playerOwner.transform.position) < minDistanceFromTarget)
                {
                    target = nearestTarget;
                }
                else
                {
                    return;
                }
            }



            if (inputLayer.GetKey(KeyCode.LeftShift) || aimOnNextFrame || (isPlayerAttacking && distanceFromTarget < minDistanceFromTarget ))
            {
                if (aimOnNextFrame) // If it was activated by `AimOnNetFrame`, then too
                    aimOnNextFrame = false;
                

                // Put camera position where camera center is.
                var cameraViewTarget = cameraTraverse.Field("target").GetValue<Transform>();
                camera.transform.position = cameraViewTarget.position;


                // Get Direction Vector from Player to Target
                Vector3 Direction = targetPosition - camera.transform.position;

                // Look Into Direction
                camera.transform.rotation = Quaternion.LookRotation(Direction.normalized);

                // Zoom out camera
                camera.transform.position -= camera.transform.forward * 5f;


                // Set Camera X & Y rotation

                Vector3 rotationEulerAngles = camera.transform.rotation.eulerAngles;

                Vector3 rotationRadians = (rotationEulerAngles * (float)Math.PI) / 180f;

                float cameraXRotation = rotationRadians.y - (float)Math.PI;

                float cameraYRotation;

                //Trace.WriteLine(string.Format("rotationRadians: {0}, x: {1} , y: {2}, z: {3}", rotationRadians.ToString(),rotationRadians.x,rotationRadians.y,rotationRadians.z));

                if (rotationRadians.x <= 1.0f)
                {
                    cameraYRotation = (float)Math.Pow(rotationRadians.x + 1f, 3) - 1f;
                }
                else
                {

                    float x = rotationRadians.x;

                    // Obtained through polynomial regression
                    cameraYRotation = -341.67972254179824f;
                    cameraYRotation += 161.89203257055917f * x;
                    cameraYRotation += -25.900171689403770f * (float)Math.Pow(x, 2);
                    cameraYRotation += 1.3988513317692850f * (float)Math.Pow(x, 3);

                }

                cameraTraverse.Field("cameraXRotation").SetValue(cameraXRotation);
                cameraTraverse.Field("cameraYRotation").SetValue(cameraYRotation);

            };
        }

        public bool isPlayerAttacking
        {
            get
            {
                return inputLayer.GetMouseButton(0);
            }

        }


        public void onHackUpdate()
        {

            if (inputLayer.GetKey(KeyCode.LeftControl) && inputLayer.GetKey(KeyCode.D))
            {
                clearTarget();
            }

            if (inputLayer.GetKeyUp(KeyCode.Tab))
            {
                if (inputLayer.GetKey(KeyCode.LeftShift) || inputLayer.GetKey(KeyCode.LeftControl))
                {
                    clearTarget();
                } else
                {
                    nextTarget();
                }
            }

            aimBot.drawOnTarget();


        }

        public float distanceFromTarget
        {
            get
            {
                return Vector3.Distance(playerPosition, targetPosition);
            }
        }

        public Vector3 playerPosition
        {
            get
            {
                return playerOwner.transform.position + height;
            }
        }

        public Vector3 targetPosition
        {
            get
            {
                if (target == null) return line.GetPosition(0);

                return target.transform.position + height;
            }
        }


        public void Aim()
        {
            aimOnNextFrame = true;
        }

        public void nextTarget()
        {
            if (Math.Abs(Time.time - lastTargetChange) > cooldown)
            {
                updateTargetList();
            }

            var index = targetIndex % nearTargetList.Length;

            target = nearTargetList[index];

            targetIndex += 1;

            lastTargetChange = Time.time;
        }

        public void clearTarget()
        {
            updateTargetList();
            target = null;
        }

        public void drawOnTarget()
        {
            if (target == null) return;

            Vector3 targetPosition = this.target.transform.position;
            Vector3 aboveHead = new Vector3(targetPosition.x, targetPosition.y + 2f, targetPosition.z);
            Vector3 toTheSky = new Vector3(targetPosition.x, targetPosition.y + 120f, targetPosition.z);
            this.line.SetPosition(0, aboveHead);
            this.line.SetPosition(1, toTheSky);
        }


        public string targetName {
            get
            {
                if (target != null)
                    return target.Name;
                else
                    return "";
            }
        }

        public override bool IsActivated { get; set; }

        public bool allowMobs { get; set; }
        public bool allowBlues { get; set; }
        public bool showDebugInfo { get; set; }
        public bool cantAim
        {
            get
            {
                return Hack.hack.remoteVision.isRemoteVision;
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
                    // it will actually run if this is false because of "always run" or something like that
                    __result.run = false;
                }
            }


            [HarmonyPatch(typeof(CameraController))]
            [HarmonyPatch("CharacterCameraUpdate")]
            class CharacterCameraUpdate
            {
                [HarmonyPostfix]
                static void Aim()
                {
                    if (aimBot == null) return;

                    try
                    {
                        aimBot.UpdateAim();
                        aimBot.makeDebugInfo();
                    } catch(Exception ex)
                    {
                        Trace.WriteLine("AimBot: " + ex.Message);
                    }

                }
            }
        }

    }
}
