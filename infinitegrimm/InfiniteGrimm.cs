using UnityEngine;
using ModCommon;
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
        // Health just needs to be high enough that grimm doesn't use the balloon attack (and can't be killed) naturally
        int defaultHealth;

        // default framerates for these animations.
        float teleinFPS;
        float teleoutFPS;
        float uppercutendFPS;
        float slashrecoverFPS;
        float evadeendFPS;
        
        // stunning implemented in my code and not games
        int stunCounter;



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
                    stunCounter = 0;
                    defaultHealth = 3000;
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


                    teleinFPS = grimm_anim.GetClipByName("Tele In").fps;
                    teleoutFPS = grimm_anim.GetClipByName("Tele Out").fps;
                    uppercutendFPS = grimm_anim.GetClipByName("Uppercut End").fps;
                    slashrecoverFPS = grimm_anim.GetClipByName("Slash Recover").fps;
                    evadeendFPS = grimm_anim.GetClipByName("Evade End").fps;



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

            grimm_anim.GetClipByName("Tele In").fps = (float)(teleinFPS / danceSpeed);
            grimm_anim.GetClipByName("Tele Out").fps = (float)(teleoutFPS / danceSpeed);
            grimm_anim.GetClipByName("Uppercut End").fps = (float)(uppercutendFPS / danceSpeed);
            grimm_anim.GetClipByName("Slash Recover").fps = (float)(slashrecoverFPS / danceSpeed);
            grimm_anim.GetClipByName("Evade End").fps = (float)(evadeendFPS / danceSpeed);
        }

        public void balloonAttack()
        {

            // Switch to Balloon 3 attack. Hope this doesn't look too janky
            controlAnimFSM.SetState("Balloon 3");
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