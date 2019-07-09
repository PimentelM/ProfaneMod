using Harmony;
using Mod.Helpers;
using Profane.Gameplay.NetworkObjects.Characters.Player;
using Profane.Gameplay.NetworkObjects.SkillManagers;
using Profane.Gameplay.NetworkObjects.Skills;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Mod.Engines;
using InsanePluginLibrary;
using Profane.Gameplay.NetworkObjects.Characters.Input.Data;

namespace Mod.Engines
{
    public static class SkillCaster
    {


        public static bool Attack()
        {
            InputEngine.Enqueue(new Func<ActionData, ActionData>[]
            {
                    (x) =>
                    {
                        x.skillManagerCancel = true;
                        return x;
                    },
                    (x) =>
                    {
                        x.skillManagerConfirmUp = x.skillManagerConfirm = true;
                        return x;
                    },
                    (x) =>
                    {
                        x.skillManagerConfirm =
                        x.skillManagerConfirmDown = true;
                        return x;
                    },
                    (x) =>
                    {
                        x.skillManagerConfirm = true;
                        return x;
                    },
                    (x) =>
                    {
                        x.skillManagerConfirm =
                        x.skillManagerConfirmUp = true;
                        return x;
                    }

            });


            return true;
        }

        public static bool HealSelf()
        {
            CanUseSkillStatus canUseStatus;

            var playerOwner = Singleton.Get<PlayerOwner>();

            if (playerOwner.SkillManager == null)
                return false;

            if (playerOwner.SkillManager.IsUsingSkill())
            {
                Trace.WriteLine("Can't use Lesser Healing because player is already using some other skill." );
                return false;
            }

            if(!playerOwner.SkillManager.Skills[SkillType.LESSER_HEALING].CanUse(out canUseStatus))
            {

                if(canUseStatus == CanUseSkillStatus.NO_TARGET_AVAILABLE || canUseStatus == CanUseSkillStatus.INVALID_CAST_RANGE)
                {

                }
                else
                {
                    Trace.WriteLine("Can't use Lesser Healing because of: " + canUseStatus.ToString());
                    return false;
                }
            }

            InputEngine.Enqueue(new Func<ActionData, ActionData>[]
            {
                (x) =>
                {
                    x.skillManagerCancel = true;
                    return x;
                },
                (x)=>
                {
                    x.skill = new int[] { 1 };
                    return x;
                },
                x=>
                {
                    x.skillManagerSelfTarget = true;
                    return x;
                },
                x=>
                {
                    x.skillManagerSelfTarget = true;
                    return x;
                },
                (x) =>
                {
                    x.skillManagerSelfTarget = 
                    x.skillManagerConfirmDown = true;
                    return x;
                },
                (x) =>
                {
                    x.skillManagerConfirm =
                    x.skillManagerSelfTarget = true;
                    return x;
                },
                (x) =>
                {
                    x.skillManagerConfirm =
                    x.skillManagerSelfTarget =
                    x.skillManagerConfirmUp = true;
                    return x;
                }

            });


            return true;

        }






    }
}
