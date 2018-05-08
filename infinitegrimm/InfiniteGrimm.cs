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
        int grimmchildFrameCounter;

        bool balloon1;
        bool balloon2;


        public void Start()
        {
            // This should only matter after grimm quest is over.

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += Reset;            
        }

        public void Reset(Scene from, Scene to)
        {
            // Essentially makes sure they complete grimm quest before starting infinite grimm.
            // Otherwise the player would have a hard time upgrading their grimmchild.
            if (to.name == "Grimm_Nightmare")
            {
                

                if (PlayerData.instance.GetBoolInternal("defeatedNightmareGrimm") && PlayerData.instance.killedNightmareGrimm)
                {
                    damageDone = 0;
                    lastBalloonDamage = 0;
                    stunCounter = 0;
                    // Just needs to be high enough he won't naturally die or use balloon attack.
                    defaultHealth = 3000;

                    grimmchildFrameCounter = 300;

                    // Assigning the FSMs.
                    grimm_anim_obj = GameObject.Find("Grimm Control").FindGameObjectInChildren("Nightmare Grimm Boss");
                    grimm_anim = grimm_anim_obj.GetComponent<tk2dSpriteAnimator>();
                    controlFSM = FSMUtility.LocateFSM(grimm, "Control");
                    controlAnimFSM = FSMUtility.LocateFSM(grimm_anim_obj, "Control");
                    stunFSM = FSMUtility.LocateFSM(grimm_anim_obj, "Stun");

                    hm = grimm_anim_obj.GetComponent<HealthManager>();
                    hm.hp = defaultHealth;

                    // Starts the update process. This just saves CPU cycles when not in the grimm room.
                    // It is done after loading the FSMs in case they error out and the Reset function never finishes.
                    runningIG = true;

                    // We implement our own balloon control. So we need to track which ones have been used.
                    // Balloon 2 makes the scene look more interesting, and happens after 840 damage in our setup.
                    balloon1 = false;
                    balloon2 = false;

                    // Just in case NKG has a stun limit this removes it.
                    // I don't think so judging by the -1 nail video but you never know. It might just be really high.
                    FsmState normalStun = stunFSM.GetState("Stun");
                    normalStun.RemoveActionsOfType<IntAdd>();
                    
                    // Stops FSM from doing stuns because we are implementing them in code.
                    // This essentially makes the stun FSM stuck in the "In Combo" state
                    // Until we manually tell it to be in the "Stun" state.
                    FsmState stunCombo = stunFSM.GetState("In Combo");
                    stunCombo.ClearTransitions();

                    // These variables are used in the difficulty section
                    // Be sure to set attacks to stun and dance speed both here and there.
                    attacksToStun = 8;
                    danceSpeed = 0.8;
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

        // Code to be run every frame of the infinite grimm fight, including before he appears
        public void Update()
        {
            if (runningIG)
            {

                // This is the bulk of the running code. Tracks grimm damage taken to make him stronger.
                takeDamage();
                if (PlayerData.instance.health <= 0)
                {
                    // The death trigger doesn't affect dream bosses?
                    // So instead we need to trigger it manually with this check
                    playerDies();
                }
                
                // This is some incredibly sketchy code. Basically it waits a little before spawning grimmchild
                // But not just any grimmchild, a random copied grimmchild from the last level.
                // Not exactly elegant but it works.
                if (grimmchildFrameCounter > 0)
                {
                    grimmchildFrameCounter--;

                    if (grimmchildFrameCounter == 0)
                    {
                        if (PlayerData.instance.GetBoolInternal("equippedCharm_40"))
                        {
                            
                            Modding.Logger.Log("[Infinite Grimm] Spawning grimmchild in grimm arena.");
                            InfiniteTent.grimmchild.PrintSceneHierarchyTree("fakegc.txt");
                            PlayMakerFSM gcControl = FSMUtility.LocateFSM(InfiniteTent.grimmchild, "Control");
                            InfiniteTent.grimmchild.SetActive(true);
                            FsmState starting = gcControl.GetState("Pause");
                            starting.RemoveActionsOfType<BoolTest>();
                            starting.ClearTransitions();
                            starting.AddTransition("FINISHED", "Spawn");
                            starting.AddTransition("AWOKEN", "Spawn");
                        }
                    }
                }
            }
        }
        

        public void takeDamage()
        {
            if (hm.hp != defaultHealth)
            {
                // Tracks the damage done without actually hurting grimm.
                // Also applies stun if needed
                damageDone = damageDone + (defaultHealth - hm.hp);
                hm.hp = defaultHealth;
                stunCounter++;
                if (stunCounter >= attacksToStun)
                {
                    stunFSM.SetState("Stun");
                    stunCounter = 0;

                }
                // don't waste cpu recalculating difficulty every frame. only after some damage.
                increaseDifficulty();
            }            

            // balloons should happen at increasingly rare intervals.
            // after: 400, 440, 484, 532, 585... etc damage. This lets the player keep getting stuns in even as stun rarity goes up
            if (damageDone - lastBalloonDamage > 400 + (lastBalloonDamage / 10))
            {
                balloonAttack();
                lastBalloonDamage = damageDone;
            }

        }

        public void increaseDifficulty()
        {

            // becomes the normal 1.0 at 1000 damage and gets harder from there.
            if (danceSpeed < 2.9995)
            {
                danceSpeed = 0.8 + (damageDone / 5000);
            } else
            {
                danceSpeed = 3.00000;
            }
            // becomes the normal 12 at 1200 damage and gets harder from there.
            attacksToStun = 8 + (damageDone / 300);

            grimm_anim.GetClipByName("Tele In").fps = (float)(teleinFPS / danceSpeed);
            grimm_anim.GetClipByName("Tele Out").fps = (float)(teleoutFPS / danceSpeed);
            grimm_anim.GetClipByName("Uppercut End").fps = (float)(uppercutendFPS / danceSpeed);
            grimm_anim.GetClipByName("Slash Recover").fps = (float)(slashrecoverFPS / danceSpeed);
            grimm_anim.GetClipByName("Evade End").fps = (float)(evadeendFPS / danceSpeed);
        }

        public void balloonAttack()
        {

            // Switch to Balloon attack. Hope this doesn't look too janky

            if (!balloon1)
            {
                controlAnimFSM.SetState("Set Balloon 1");
                balloon1 = true;
            } else if (!balloon2)
            {
                controlAnimFSM.SetState("Set Balloon 2");
                balloon2 = true;
            } else
            {
                controlAnimFSM.SetState("Set Balloon 3");
            }
        }

        // Gives geo on death and turns off the script.
        // Eventually I want a better setup for this.
        public void playerDies()
        {
            Modding.Logger.Log("[Infinite Grimm] Good job, you did: " + damageDone + " damage!");
            int geo = (int) (damageDone / 10.0);
            PlayerData.instance.AddGeo(geo);

            // This saves cpu cycles and potentially stops the player from getting Geo multiple times.
            runningIG = false;
        }
    }
}