using UnityEngine;
using ModCommon;
using HutongGames.PlayMaker;
using RandomizerMod.Extensions;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker.Actions;
using Modding;

namespace infinitegrimm
{
    public class InfiniteGrimm : MonoBehaviour
    {
        public static bool hardmode;

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

        int playerDieTimeout;
        bool didDie;

        bool didTakeDamage;

        readonly public string[] validStunStates = {"Slash Antic", "Slash 1", "Slash 2", "Slash 3", "Slash Recover", "Slash End",
            "FB Antic", "FB Cast", "FB Cast End", "Firebat 1", "Firebat 2", "Firebat 4", "Tele Out", "FB Hero Pos",
            "FB Tele R", "FB Tele In", "FB Tele L", "FB Behind", "FB Re Tele", "Slash Pos", "Slash Tele In",
            "AD Pos", "AD Retry", "AD Tele In", "AD Antic", "AD Fire", "GD Antic", "AD Edge", "G Dash",
            "G Dash Recover", "Evade", "Evade End", "After Evade", "Uppercut Antic", "Uppercut Up", "UP Explode",
            "Pillar Pos", "Pillar Tele In", "Pillar Antic", "Pillar", "Pillar End" };

        readonly public string[] validBalloonTransitions = { "Explode Pause", "Out Pause", "Spike Return" };

        readonly public string[] hardmodeAnimations2x = { "Cast Antic",
            "Cast Return", "Capespike Cast", "Explode Antic" };
        float[] hardmodeAnimationFrames2x = new float[4];

        readonly public string[] hardmodeWaitStates2x = { "Slash Antic", "FB Cast", "FB Cast End", "Spike Attack",
            "Fire Pause", "Spike Attack"};
        readonly float[] hardmodeWaitState2xTimes = { 0.5f, 0.3f, 0.5f, 0.4f, 0.6f, 1.35f};

        readonly public string[] hardmodeWaitStates3x = { "Firebat 1", "Firebat 2", "Firebat 3", "Firebat 4", "Out Pause",
        "Pillar", "Pillar Antic"};
        readonly float[] hardmodeWaitState3xTimes = { 0.3f, 0.3f, 0.3f, 0.6f, 0.6f, 0.75f, 0.5f};

        readonly static public string[] hardmodeAnimations3x = { };


        public void Start()
        {
            // This should only matter after grimm quest is over
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += Reset;

            teleinFPS = -5f;
        }

        public void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= Reset;

            try
            {
                ModHooks.Instance.HitInstanceHook -= damage;
            }
            catch
            {
                Modding.Logger.Log("[Infinite Grimm] Unable to remove hit instance hook because it doesn't exist");
            }

            Modding.Logger.Log("[Infinite Grimm] Unloaded Grimm!");
        }

            private HitInstance damage(Fsm isGrimm, HitInstance hit)
        {
            if (didTakeDamage)
            {
                takeDamage(hit.AttackType);
                didTakeDamage = false;
            }
            return hit;
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

                    if (!hardmode)
                        danceSpeed = 0.8;
                    else
                        danceSpeed = 1.0;

                    if (teleinFPS < 0)
                    {
                        teleinFPS = grimm_anim.GetClipByName("Tele In").fps;
                        teleoutFPS = grimm_anim.GetClipByName("Tele Out").fps;
                        uppercutendFPS = grimm_anim.GetClipByName("Uppercut End").fps;
                        slashrecoverFPS = grimm_anim.GetClipByName("Slash Recover").fps;
                        evadeendFPS = grimm_anim.GetClipByName("Evade End").fps;

                        for (int i = 0; i < hardmodeAnimations2x.Length; i++)
                        {
                            hardmodeAnimationFrames2x[i] = grimm_anim.GetClipByName(hardmodeAnimations2x[i]).fps;
                        }
                    }
                    Modding.Logger.Log("[Infinite Grimm] Loading default animation speeds which are:");
                    Modding.Logger.Log("[Infinite Grimm] Tele in: " + teleinFPS + " out: " + teleoutFPS +
                        " uppercut end: " + uppercutendFPS + " slash recover: " + slashrecoverFPS + " evade end: " + evadeendFPS);

                    // Actually we're just setting the difficulty the first time this is run.
                    increaseDifficulty();

                    // gives the player geo on a delay so they can see the gain.
                    playerDieTimeout = 0;
                    didDie = false;
                    didTakeDamage = false;

                    ModHooks.Instance.HitInstanceHook += damage;

                    Modding.Logger.Log("[Infinite Grimm] Setup Nightmare IG battle");

                    if (hardmode)
                    {
                        Modding.Logger.Log("[Infinite Grimm] IG is in hard mode! Good luck...");
                    }
                        
                }
            }
            else
            {
                runningIG = false;
            }
        }

        // Code to be run every frame of the infinite grimm fight, including before he appears
        public void Update()
        {
            if (runningIG)
            {

                // don't waste cpu recalculating difficulty every frame. only after some damage. This needs to be here
                // or familiars won't count
                if (hm.hp != defaultHealth)
                    increaseDifficulty();

                if (PlayerData.instance.health <= 0)
                {
                    didDie = true;
                    runningIG = false;
                    ModHooks.Instance.HitInstanceHook -= damage;
                    playerDieTimeout = 300;

                    if (InfiniteGlobalVars.maximumDamage < damageDone)
                    {
                        InfiniteGlobalVars.maximumDamage = damageDone;
                        Modding.Logger.Log("[Infinite Grimm] New Damage Record!!!");
                    }

                    grimm_anim.GetClipByName("Tele In").fps = teleinFPS;
                    grimm_anim.GetClipByName("Tele Out").fps = teleoutFPS;
                    grimm_anim.GetClipByName("Uppercut End").fps = uppercutendFPS;
                    grimm_anim.GetClipByName("Slash Recover").fps = slashrecoverFPS;
                    grimm_anim.GetClipByName("Evade End").fps = evadeendFPS;

                    for (int i = 0; i < hardmodeAnimations2x.Length; i++)
                    {
                        grimm_anim.GetClipByName(hardmodeAnimations2x[i]).fps = hardmodeAnimationFrames2x[i];
                    }

                    for (int i = 0; i < hardmodeWaitStates2x.Length; i++)
                    {
                        Wait[] w = controlAnimFSM.GetState(hardmodeWaitStates2x[i]).GetActionsOfType<Wait>();
                        w[0].time = hardmodeWaitState2xTimes[i];
                    }

                    for (int i = 0; i < hardmodeWaitStates3x.Length; i++)
                    {
                        Wait[] w = controlAnimFSM.GetState(hardmodeWaitStates3x[i]).GetActionsOfType<Wait>();
                        w[0].time = hardmodeWaitState3xTimes[i];
                    }

                    InfiniteTent.damageDone = damageDone;

                    Modding.Logger.Log("[Infinite Grimm] Cleaned up Grimm fight.");
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

                if (stunCounter >= attacksToStun)
                {
                    string currentState = controlAnimFSM.ActiveStateName;
                    for (int i = 0; i < validStunStates.Length; i++)
                    {
                        if (currentState == validStunStates[i])
                        {
                            stunFSM.SetState("Stun");
                            stunCounter = 0;
                            break;
                        }
                    }
                }


                // balloons should happen at increasingly rare intervals.
                // after: 400, 440, 484, 532, 585... etc damage. This lets the player keep getting stuns in even as stun rarity goes up
                if (damageDone - lastBalloonDamage > 400 + (lastBalloonDamage / 10))
                {
                    string currentState = controlAnimFSM.ActiveStateName;
                    for (int i = 0; i < validBalloonTransitions.Length; i++)
                    {
                        if (currentState == validBalloonTransitions[i])
                        {
                            balloonAttack();
                            lastBalloonDamage = damageDone;
                            break;
                        }
                    }
                }

            }
            else if (playerDieTimeout > 0 && didDie)
            {

                playerDieTimeout--;
                if (playerDieTimeout == 0)
                {
                    playerDies();
                    didDie = false;
                }
            }
        }

        // This is the bulk of the running code. Tracks grimm damage taken to make him stronger.
        public void takeDamage(AttackTypes attack)
        {

            if (attack == AttackTypes.Nail || attack == AttackTypes.Spell)
                stunCounter++;
        }

        public void increaseDifficulty()
        {
            // Tracks the damage done without actually hurting grimm.
            // Also applies stun if needed
            damageDone = damageDone + (defaultHealth - hm.hp);
            hm.hp = defaultHealth;
            didTakeDamage = true;

            // becomes the normal 1.0 at 1000 damage and gets harder from there.
            if (danceSpeed < 2.9995)
            {
                if (!hardmode)
                    danceSpeed = 0.8 + (float) ( (double) damageDone / 5000.0);
                else
                    danceSpeed = 1.2 + (float) ((double)damageDone / 5000.0);
            }
            else
            {
                danceSpeed = 3.00000;
            }

            // becomes the normal 12 at 1200 damage and gets harder from there.
            attacksToStun = 8 + (damageDone / 300);

            grimm_anim.GetClipByName("Tele In").fps = (float)(teleinFPS * danceSpeed);
            grimm_anim.GetClipByName("Tele Out").fps = (float)(teleoutFPS * danceSpeed);
            grimm_anim.GetClipByName("Uppercut End").fps = (float)(uppercutendFPS * danceSpeed);
            grimm_anim.GetClipByName("Slash Recover").fps = (float)(slashrecoverFPS * danceSpeed);
            grimm_anim.GetClipByName("Evade End").fps = (float)(evadeendFPS * danceSpeed);

            double danceTwo;
            double danceThree;

            if (hardmode)
            {
                // Dance two starts at 1 and goes to 1.5 when danceSpeed is 3. | y + 1.2/x = 1, y + 3/x = 1.5
                // Dance three starts at 1 and goes to 2.5 when danceSpeed is 3. | y + 1.2/x = 1, y + 3/x = 2.5
                if (danceSpeed > 2.999)
                {
                    danceTwo = 1.5;
                    danceThree = 2.5;
                }
                else
                {
                    danceTwo = 2.0 / 3.0 + (danceSpeed / (18.0 / 5.0));
                    danceThree = (danceSpeed / (6.0 / 5.0));
                }
            } else
            {
                // Dance two starts at 1 and goes to 1.15 when danceSpeed is 3. | y + 0.8/x = 1, y + 3/x = 1.15
                // Dance three starts at 1 and goes to 1.3 when danceSpeed is 3. | y + 0.8/x = 1, y + 3/x = 1.3
                if (danceSpeed > 2.999)
                {
                    danceTwo = 1.15;
                    danceThree = 1.3;
                }
                else
                {
                    danceTwo = 52.0 / 55.0 + (danceSpeed / (44.0 / 3.0));
                    danceThree = 49.0 / 55.0 + (danceSpeed / (22.0 / 3.0));
                }
            }


            for (int i = 0; i < hardmodeAnimations2x.Length; i++)
            {
                grimm_anim.GetClipByName(hardmodeAnimations2x[i]).fps = (float)(hardmodeAnimationFrames2x[i] * danceTwo);
            }

            for (int i = 0; i < hardmodeWaitStates2x.Length; i++)
            {
                Wait[] w = controlAnimFSM.GetState(hardmodeWaitStates2x[i]).GetActionsOfType<Wait>();
                w[0].time = (float)(hardmodeWaitState2xTimes[i] / danceTwo);
            }

            for (int i = 0; i < hardmodeWaitStates3x.Length; i++)
            {
                Wait[] w = controlAnimFSM.GetState(hardmodeWaitStates3x[i]).GetActionsOfType<Wait>();
                w[0].time = (float)(hardmodeWaitState3xTimes[i] / danceThree);
            }




        }

        public void balloonAttack()
        {

            // Switch to Balloon attack. Hope this doesn't look too janky

            if (!balloon1)
            {
                controlAnimFSM.SetState("Set Balloon 1");
                balloon1 = true;
            }
            else if (!balloon2)
            {
                controlAnimFSM.SetState("Set Balloon 2");
                balloon2 = true;
            }
            else
            {
                controlAnimFSM.SetState("Set Balloon 3");
            }
        }

        // Gives geo on returning to main area after dying.
        public void playerDies()
        {
            Modding.Logger.Log("[Infinite Grimm] Good job, you did: " + damageDone + " damage!");
            int geo = (int)(damageDone / 10.0);
            if (hardmode)
                geo *= 2;

            // If you add 0 geo the menu bar gets buggy.
            if (geo > 0)
                HeroController.instance.AddGeo(geo);

        }
    }
}