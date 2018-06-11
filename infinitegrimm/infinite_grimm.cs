using System.Linq;
using UnityEngine;
using ModCommon;
using HutongGames.PlayMaker;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker.Actions;
using Modding;

namespace infinitegrimm
{
    public class infinite_grimm : MonoBehaviour
    {
        public static bool hardmode;
        public static bool noLagMode;
        
        public static float startingDanceSpeed;
        public static float maxDanceSpeed;
        public static float danceSpeedIncreaseDmg;
        
        public static int staggerIncreaseDamage;
        public static int startingStaggerHits;

        
        
        
        public bool done;
        public GameObject grimm;
        public GameObject grimmAnimObj;
        private HealthManager hm;
        private PlayMakerFSM controlFSM;
        private PlayMakerFSM controlAnimFSM;
        private PlayMakerFSM stunFSM;
        private tk2dSpriteAnimator grimmAnim;
        private double danceSpeed;
        private int attacksToStun;
        private bool runningIG;

        private int damageDone;
        private int lastHitDamage;

        private int lastBalloonDamage;
        // Health just needs to be high enough that grimm doesn't use the balloon attack (and can't be killed) naturally
        private int defaultHealth;

        // default framerates for these animations.
        private float teleinFPS;
        private float teleoutFPS;
        private float uppercutendFPS;
        private float slashrecoverFPS;
        private float evadeendFPS;

        // stunning implemented in my code and not games
        private int stunCounter;
        private int grimmchildFrameCounter;

        private bool balloon1;
        private bool balloon2;

        private int playerDieTimeout;
        private bool didDie;

        private bool didTakeDamage;

        private readonly string[] validStunStates = {"Slash Antic", "Slash 1", "Slash 2", "Slash 3", "Slash Recover", "Slash End",
            "FB Antic", "FB Cast", "FB Cast End", "Firebat 1", "Firebat 2", "Firebat 4", "Tele Out", "FB Hero Pos",
            "FB Tele R", "FB Tele In", "FB Tele L", "FB Behind", "FB Re Tele", "Slash Pos", "Slash Tele In",
            "AD Pos", "AD Retry", "AD Tele In", "AD Antic", "AD Fire", "GD Antic", "AD Edge", "G Dash",
            "G Dash Recover", "Evade", "Evade End", "After Evade", "Uppercut Antic", "Uppercut Up", "UP Explode",
            "Pillar Pos", "Pillar Tele In", "Pillar Antic", "Pillar", "Pillar End" };

        private readonly string[] validBalloonTransitions = { "Explode Pause", "Out Pause", "Spike Return" };

        private readonly string[] hardmodeAnimations2X = { "Cast Antic",
            "Cast Return", "Capespike Cast", "Explode Antic" };

        private readonly float[] hardmodeAnimationFrames2X = new float[4];

        private readonly string[] hardmodeWaitStates2X = { "Slash Antic", "FB Cast", "FB Cast End", "Spike Attack",
            "Fire Pause", "Spike Attack"};

        private readonly float[] hardmodeWaitState2XTimes = { 0.5f, 0.3f, 0.5f, 0.4f, 0.6f, 1.35f};

        private readonly string[] hardmodeWaitStates3X = { "Firebat 1", "Firebat 2", "Firebat 3", "Firebat 4", "Out Pause",
        "Pillar", "Pillar Antic"};

        private readonly float[] hardmodeWaitState3XTimes = { 0.3f, 0.3f, 0.3f, 0.6f, 0.6f, 0.75f, 0.5f};

        // ReSharper disable once InconsistentNaming
        public static readonly string[] HARDMODE_ANIMATIONS3X = { };


        public void Start()
        {
            // This should only matter after grimm quest is over
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += Reset;

            teleinFPS = -5f;
            damageDone = -1;
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
            if (!didTakeDamage) return hit;
            
            didTakeDamage = false;
            
            if (hit.DamageDealt != lastHitDamage)
            {    
                return hit;
            }
            
            AttackTypes a = hit.AttackType;
            if (a == AttackTypes.Nail || a == AttackTypes.Spell || a == AttackTypes.SharpShadow)
                stunCounter++;
            
            return hit;
        }

        // ReSharper disable once Unity.InvalidParameters Go fuck yourself ReSharper.
        private void Reset(Scene from, Scene to)
        {
            // Essentially makes sure they complete grimm quest before starting infinite grimm.
            // Otherwise the player would have a hard time upgrading their grimmchild.
            if (to.name == "Grimm_Nightmare")
            {
                if (!PlayerData.instance.GetBoolInternal("defeatedNightmareGrimm") ||
                    !PlayerData.instance.killedNightmareGrimm) return;
                
                damageDone = 0;
                lastBalloonDamage = 0;
                stunCounter = 0;
                lastHitDamage = 0;
                // Just needs to be high enough he won't naturally die or use balloon attack.
                defaultHealth = 3000;

                grimmchildFrameCounter = 300;

                // Assigning the FSMs.
                grimmAnimObj = GameObject.Find("Grimm Control").FindGameObjectInChildren("Nightmare Grimm Boss");
                grimmAnim = grimmAnimObj.GetComponent<tk2dSpriteAnimator>();
                controlFSM = FSMUtility.LocateFSM(grimm, "Control");
                controlAnimFSM = FSMUtility.LocateFSM(grimmAnimObj, "Control");
                stunFSM = FSMUtility.LocateFSM(grimmAnimObj, "Stun");

                    

                hm = grimmAnimObj.GetComponent<HealthManager>();
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
                FsmState normalStun = stunFSM.getState("Stun");
                normalStun.removeActionsOfType<IntAdd>();

                // Stops FSM from doing stuns because we are implementing them in code.
                // This essentially makes the stun FSM stuck in the "In Combo" state
                // Until we manually tell it to be in the "Stun" state.
                FsmState stunCombo = stunFSM.getState("In Combo");
                stunCombo.clearTransitions();

                // These variables are used in the difficulty section
                // Be sure to set attacks to stun and dance speed both here and there.
                attacksToStun = 8;

                danceSpeed = hardmode ? 1.0 : 0.8;
                
                if (teleinFPS < 0)
                {
                    teleinFPS = grimmAnim.GetClipByName("Tele In").fps;
                    teleoutFPS = grimmAnim.GetClipByName("Tele Out").fps;
                    uppercutendFPS = grimmAnim.GetClipByName("Uppercut End").fps;
                    slashrecoverFPS = grimmAnim.GetClipByName("Slash Recover").fps;
                    evadeendFPS = grimmAnim.GetClipByName("Evade End").fps;

                    for (int i = 0; i < hardmodeAnimations2X.Length; i++)
                    {
                        hardmodeAnimationFrames2X[i] = grimmAnim.GetClipByName(hardmodeAnimations2X[i]).fps;
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

                    if (infinite_global_vars.maximumDamage < damageDone)
                    {
                        infinite_global_vars.maximumDamage = damageDone;
                        Modding.Logger.Log("[Infinite Grimm] New Damage Record!!!");
                    }

                    grimmAnim.GetClipByName("Tele In").fps = teleinFPS;
                    grimmAnim.GetClipByName("Tele Out").fps = teleoutFPS;
                    grimmAnim.GetClipByName("Uppercut End").fps = uppercutendFPS;
                    grimmAnim.GetClipByName("Slash Recover").fps = slashrecoverFPS;
                    grimmAnim.GetClipByName("Evade End").fps = evadeendFPS;

                    for (int i = 0; i < hardmodeAnimations2X.Length; i++)
                    {
                        grimmAnim.GetClipByName(hardmodeAnimations2X[i]).fps = hardmodeAnimationFrames2X[i];
                    }

                    for (int i = 0; i < hardmodeWaitStates2X.Length; i++)
                    {
                        Wait[] w = controlAnimFSM.getState(hardmodeWaitStates2X[i]).getActionsOfType<Wait>();
                        w[0].time = hardmodeWaitState2XTimes[i];
                    }

                    for (int i = 0; i < hardmodeWaitStates3X.Length; i++)
                    {
                        Wait[] w = controlAnimFSM.getState(hardmodeWaitStates3X[i]).getActionsOfType<Wait>();
                        w[0].time = hardmodeWaitState3XTimes[i];
                    }

                    infinite_tent.damageDone = damageDone;

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
                            //InfiniteTent.grimmchild.PrintSceneHierarchyTree("fakegc.txt");
                            PlayMakerFSM gcControl = FSMUtility.LocateFSM(infinite_tent.grimmchild, "Control");
                            infinite_tent.grimmchild.SetActive(true);
                            FsmState starting = gcControl.getState("Pause");
                            starting.removeActionsOfType<BoolTest>();
                            starting.clearTransitions();
                            starting.addTransition("FINISHED", "Spawn");
                            starting.addTransition("AWOKEN", "Spawn");
                        }
                    }
                }

                if (stunCounter >= attacksToStun)
                {
                    string currentState = controlAnimFSM.ActiveStateName;
                    if (validStunStates.Any(t => currentState == t))
                    {
                        stunFSM.SetState("Stun");
                        stunCounter = 0;
                    }
                }


                // balloons should happen at increasingly rare intervals.
                // after: 400, 440, 484, 532, 585... etc damage. This lets the player keep getting stuns in even as stun rarity goes up
                if (damageDone - lastBalloonDamage <= 400 + (lastBalloonDamage / 10)) return;
                {
                    string currentState = controlAnimFSM.ActiveStateName;
                    if (validBalloonTransitions.All(t => currentState != t)) return;
                    
                    if (noLagMode)
                        balloonAttackNoLag();
                    else
                        balloonAttack();
                    
                    lastBalloonDamage = damageDone;
                }

            }
            else if (playerDieTimeout > 0 && didDie)
            {

                playerDieTimeout--;
                if (playerDieTimeout != 0) return;
                playerDies();
                didDie = false;
            }
        }

        

        private void increaseDifficulty()
        {
            // Tracks the damage done without actually hurting grimm.
            // Also applies stun if needed
            lastHitDamage = (defaultHealth - hm.hp);
            damageDone = damageDone + lastHitDamage;
            hm.hp = defaultHealth;
            didTakeDamage = true;

            HeroController.instance.geoCounter.geoTextMesh.text = "" + damageDone;
            HeroController.instance.geoCounter.UpdateGeo(); // idek if this does something

            // becomes the normal 1.0 at 1000 damage and gets harder from there.
            if (danceSpeed < maxDanceSpeed - 0.005f)
            {
                if (!hardmode)
                    danceSpeed = startingDanceSpeed + (float) ( (double) damageDone / danceSpeedIncreaseDmg);
                else
                    danceSpeed = startingDanceSpeed + 0.4 + (float) ((double)damageDone / danceSpeedIncreaseDmg);
            }
            else
            {
                danceSpeed = maxDanceSpeed;
            }

            // becomes the normal 12 at 1200 damage and gets harder from there.
            attacksToStun = startingStaggerHits + (damageDone / staggerIncreaseDamage);

            grimmAnim.GetClipByName("Tele In").fps = (float)(teleinFPS * danceSpeed);
            grimmAnim.GetClipByName("Tele Out").fps = (float)(teleoutFPS * danceSpeed);
            grimmAnim.GetClipByName("Uppercut End").fps = (float)(uppercutendFPS * danceSpeed);
            grimmAnim.GetClipByName("Slash Recover").fps = (float)(slashrecoverFPS * danceSpeed);
            grimmAnim.GetClipByName("Evade End").fps = (float)(evadeendFPS * danceSpeed);

            double danceTwo;
            double danceThree;

            if (hardmode)
            {
                // Dance two starts at 1 and goes to 1.5 when danceSpeed is 3. | y + 1.2/x = 1, y + 3/x = 1.5
                // Dance three starts at 1 and goes to 2.5 when danceSpeed is 3. | y + 1.2/x = 1, y + 3/x = 2.5
                if (danceSpeed > maxDanceSpeed - 0.001)
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
                if (danceSpeed > maxDanceSpeed - 0.001)
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


            for (int i = 0; i < hardmodeAnimations2X.Length; i++)
            {
                grimmAnim.GetClipByName(hardmodeAnimations2X[i]).fps = (float)(hardmodeAnimationFrames2X[i] * danceTwo);
            }

            for (int i = 0; i < hardmodeWaitStates2X.Length; i++)
            {
                Wait[] w = controlAnimFSM.getState(hardmodeWaitStates2X[i]).getActionsOfType<Wait>();
                w[0].time = (float)(hardmodeWaitState2XTimes[i] / danceTwo);
            }

            for (int i = 0; i < hardmodeWaitStates3X.Length; i++)
            {
                Wait[] w = controlAnimFSM.getState(hardmodeWaitStates3X[i]).getActionsOfType<Wait>();
                w[0].time = (float)(hardmodeWaitState3XTimes[i] / danceThree);
            }




        }

        private void balloonAttackNoLag()
        {
            controlAnimFSM.SetState("Set Balloon 3");
        }

        private void balloonAttack()
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
        private void playerDies()
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
