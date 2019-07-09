using InsanePluginLibrary;
using Profane.Gameplay.NetworkObjects.Characters.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mod.Engines;
using Mod.Helpers;
using UnityEngine;
using System.Diagnostics;

namespace Mod.Hacks
{
    public class Trainer : IHack
    {
        public static Trainer trainer;

        private Task manaTrainer;
        private Task combatsTrainer;

        private PlayerOwner playerOwner;

        private float requiredManaToHeal = 0.35f;

        private float manaRequiredToTrainMagery = 0.9f;

        private float minimalHealthToAttack = 0.8f;

        private float maximumDistanceFromTarget = 6f;

        private float minimalDistaceFromOtherPlayers = 150f;

        private string[] playerWhitelist = new[] { "Cheshire", "Lazuli", "Whiskas" };

        private int loopDelay = 1000;

        private bool _manaTrain;
        public bool trainMagery
        {
            get
            {
                return _manaTrain;
            }
            set
            {
                if(value)
                    trainCombats = false;

                _manaTrain = value;
            }
        }

        private bool _combatsTrain;
        public bool trainCombats
        {
            get
            {
                return _combatsTrain;
            }
            set
            {
                if(value)
                    trainMagery = false;

                _combatsTrain = value;
            }
        }


        public override bool IsActivated { get; set; }

        public Trainer()
        {
            trainer = this;
            this.manaTrainer = Task.Factory.StartNew(manaTrainLoop);
            this.combatsTrainer = Task.Factory.StartNew(combatsTrainLoop);
            playerOwner = Singleton.Get<PlayerOwner>();
            this.IsActivated = true;
        }

        public void manaTrainLoop()
        {
            do
            {
                Thread.Sleep(loopDelay);

                if (!IsActivated || !trainMagery) continue;


                if (playerOwner.Mana.Ratio >= manaRequiredToTrainMagery)
                {
                    SkillCaster.HealSelf();
                    continue;
                }

            } while (true);


        }



        public void combatsTrainLoop()
        {
            try
            {
                do
                {
                    Thread.Sleep(loopDelay);

                    if (!IsActivated || !trainCombats) continue;

                    if (AimBot.aimBot == null) continue;

                    if (AimBot.aimBot.target == null)
                    {
                        FeedbackEngine.WriteMessage("Selecting nearest target.");

                        var target = (from Player in Singleton.Get<PlayerProxyList>().Values
                                      where playerWhitelist.Contains(Player.Name) &&
                                      Player.networkView.viewID != playerOwner.networkView.viewID
                                      orderby Vector3.Distance(playerOwner.transform.position, Player.transform.position) ascending
                                      select Player).FirstOrDefault();

                        AimBot.aimBot.setTarget(target);

                        continue;
                    } 


                    if (AimBot.aimBot.distanceFromTarget > maximumDistanceFromTarget)
                    {
                        FeedbackEngine.WriteMessage("Target is too far, waiting for it to be closer.");
                        continue;
                    }

                    if (ThereIsAnyPlayerAround())
                    {
                        FeedbackEngine.WriteMessage("There are other players around, the training will be paused until they move out of range.");

                        continue;
                    }


                    // Check if health is below a certain point and cast heal.
                    if (playerOwner.Mana.Ratio >= requiredManaToHeal && playerOwner.Health.Ratio < minimalHealthToAttack)
                    {
                        SkillCaster.HealSelf();
                        continue;
                    }

                    // Check if target health is above a certain point and attack.
                    if (AimBot.aimBot.target.Health.Ratio >= minimalHealthToAttack)
                    {
                        AimBot.aimBot.Aim();
                        SkillCaster.Attack();
                        continue;
                    }


                    // And lastly, if player Mana is full, then cast heal on self to train magery too.
                    if (playerOwner.Mana.Ratio >= manaRequiredToTrainMagery)
                    {
                        SkillCaster.HealSelf();
                        continue;
                    }

                } while (true);
            } catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

            }

        }

        private bool ThereIsAnyPlayerAround()
        {
            return (from Player in Singleton.Get<PlayerProxyList>().Values
                    where
                    (Player.networkView.viewID != AimBot.aimBot.target.networkView.viewID &&
                    Vector3.Distance(Player.transform.position, playerOwner.transform.position) < minimalDistaceFromOtherPlayers &&
                    !playerWhitelist.Contains(Player.Name)                    
                    ) 
                    ||
                    Player.Name.Contains("hayes")
                    select Player).Count() > 0;
        }

        public override void drawGUI()
        {

            trainMagery = UIHelper.CheckBox(trainMagery, "Train Magery");

            trainCombats = UIHelper.CheckBox(trainCombats, "Train Combats");

        }

    }
}
