using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using ModCommon;
using Modding;
using HutongGames.PlayMaker;
using RandomizerMod.Extensions;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker.Actions;

namespace infinitegrimm
{
    public class InfiniteGrimm : MonoBehaviour
    {
        public bool done;
        public GameObject grimm;
        public GameObject grimm_anim_obj;
        HealthManager hm;
        PlayMakerFSM controlFSM;
        PlayMakerFSM controlAnimFSM;
        PlayMakerFSM stunFSM;
        tk2dSpriteAnimator grimm_anim;
        double danceSpeed;
        int attacksToStun;
        bool runningIG;

        int damageDone;
        int lastDifficultyIncrease;
        int lastBalloonDamage;
        int defaultHealth = 2000;

        // stunning implemented in my code and not games
        int stunCounter = 0;

        // Ruin move choice to do balloon attack
        int ruinMoveChoiceFrames;
        int ruinedFrames;
        bool balloonAttackGoing;

        //todo figure out how tf this works

        // So starting out we have the possible attack states of the enemy
        // I have no idea what the official name is though.


        public void Start()
        {
            // This should only matter after grimm quest is over.

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += Reset;            
        }

        public void Reset(Scene from, Scene to)
        {
            Modding.Logger.Log("[Infinite Grimm] Scene name: " + to.name + " defeat NKG: " + PlayerData.instance.GetBoolInternal("defeatedNightmareGrimm") + " kill nkg? " + PlayerData.instance.killedNightmareGrimm);
            if (to.name == "Grimm_Nightmare")
            {
                if (PlayerData.instance.GetBoolInternal("defeatedNightmareGrimm") && PlayerData.instance.killedNightmareGrimm)
                {
                    attacksToStun = 8;
                    danceSpeed = 0.8;
                    damageDone = 0;
                    lastBalloonDamage = 0;
                    lastDifficultyIncrease = 0;
                    balloonAttackGoing = false;
                    stunCounter = 0;
                    ruinedFrames = 0;
                    ruinMoveChoiceFrames = 700;
                    grimm = GameObject.Find("Grimm Control");
                    grimm_anim_obj = grimm.FindGameObjectInChildren("Nightmare Grimm Boss");
                    grimm_anim = grimm_anim_obj.GetComponent<tk2dSpriteAnimator>();

                    controlFSM = FSMUtility.LocateFSM(grimm, "Control");
                    controlAnimFSM = FSMUtility.LocateFSM(grimm_anim_obj, "Control");
                    stunFSM = FSMUtility.LocateFSM(grimm_anim_obj, "Stun");


                    FsmState outpause = controlAnimFSM.GetState("Out Pause");
                    outpause.ClearTransitions();
                    outpause.AddTransition("FINISHED", "Move Choice");
                    runningIG = true;
                    hm = grimm_anim_obj.GetComponent<HealthManager>();
                    hm.hp = defaultHealth;

                    // Just in case NKG has a stun limit this removes it.
                    // I don't think so judging by the -1 nail video but you never know. It might just be really high.
                    FsmState normalStun = stunFSM.GetState("Stun");
                    normalStun.RemoveActionsOfType<IntAdd>();
                    
                    // Stops FSM from doing stuns because we are implementing them in code.
                    FsmState stunCombo = stunFSM.GetState("In Combo");
                    stunCombo.ClearTransitions();
                    
                    // Actually we're just setting the difficulty the first time this is run.
                    increaseDifficulty();
                    Modding.Logger.Log("[Infinite Grimm] Setup Nightmare IG battle");
                }
            } else
            {
                runningIG = false;
            }
        }

        // place code to be run every frame of the infinite grimm.
        
        public void Update()
        {
            if (runningIG)
            {
                takeDamage();

                if (balloonAttackGoing)
                {
                    ruinedFrames++;
                }

                if (ruinedFrames > ruinMoveChoiceFrames)
                {
                    endBalloonAttack();
                }
                if (PlayerData.instance.health <= 0)
                {
                    // The death trigger doesn't affect dream bosses?
                    playerDies();
                }


            }
        }
        

        public void takeDamage()
        {
            if (hm.hp != defaultHealth)
            {
                damageDone = damageDone + (defaultHealth - hm.hp);
                hm.hp = defaultHealth;
                stunCounter++;
                if (stunCounter >= attacksToStun)
                {
                    stunFSM.SetState("Stun");
                    stunCounter = 0;

                }
            }

            
            if (damageDone - lastDifficultyIncrease > 100)
            {
                lastDifficultyIncrease = damageDone;
                increaseDifficulty();
            }

            if (damageDone - lastBalloonDamage > 600)
            {
                balloonAttack();
                lastBalloonDamage = damageDone;
            }

        }

        public void increaseDifficulty()
        {
            if (danceSpeed < 2.9995)
            {
                danceSpeed = 0.8 + (damageDone / 5000);
            } else
            {
                //Since the framerates are always rounded down, set slightly below 3 for 3 in practice
                danceSpeed = 2.9997;
            }
            attacksToStun = 8 + (damageDone / 300);

            grimm_anim.GetClipByName("Tele In").fps = (int)(grimm_anim.GetClipByName("Tele In").fps / danceSpeed);
            grimm_anim.GetClipByName("Tele Out").fps = (int)(grimm_anim.GetClipByName("Tele Out").fps / danceSpeed);
            grimm_anim.GetClipByName("Uppercut End").fps = (int)(grimm_anim.GetClipByName("Uppercut End").fps / danceSpeed);
            grimm_anim.GetClipByName("Slash Recover").fps = (int)(grimm_anim.GetClipByName("Slash Recover").fps / danceSpeed);
            grimm_anim.GetClipByName("Evade End").fps = (int)(grimm_anim.GetClipByName("Evade End").fps / danceSpeed);
        }

        public void balloonAttack()
        {

            // outpause -> balloon 3 -> move choice
            balloonAttackGoing = true;
            FsmState outpause = controlAnimFSM.GetState("Out Pause");
            outpause.ClearTransitions();
            outpause.AddTransition("FINISHED", "Balloon 3");

        }

        public void endBalloonAttack()
        {
            balloonAttackGoing = false;
            ruinedFrames = 0;
            FsmState outpause = controlAnimFSM.GetState("Out Pause");
            outpause.ClearTransitions();
            outpause.AddTransition("FINISHED", "Move Choice");

        }

        // place code to give player geo on death based on damage done. Or whatever else you want to do
        public void playerDies()
        {
            int geo = (int) (damageDone / 10.0) ;
            PlayerData.instance.AddGeo(geo);
            runningIG = false;
        }
    }
}