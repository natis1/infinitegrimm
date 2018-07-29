using System;
using System.Collections;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace infinitegrimm
{
    // ReSharper disable once InconsistentNaming
    public class infinite_NGG : MonoBehaviour
    {
        public static bool hardmode;
        
        private float teleinFPS;
        private float teleoutFPS;
        private float uppercutendFPS;
        private float slashrecoverFPS;
        private float evadeendFPS;
        private float spikesFPS;


        private int damageDone;
        private int lastBalloonDamage;
        private int defaultHealth;
        private int grimmchildFrameCounter;

        private bool didDie;
        private int playerDieTimeout;


        private GameObject grimmContainer;
        public GameObject grimm;
        private HealthManager hm;
        private PlayMakerFSM grimmFSM;
        private PlayMakerFSM stunFSM;
        private tk2dSpriteAnimator grimmAnim;
        private bool runningIG;
        
        private GameObject memeBullshitGrimmContainer;
        public GameObject memeBullshitGrimm;
        private PlayMakerFSM memeBullshitGrimmFSM;
        private tk2dSpriteAnimator memeBullshitAnim;
        private GameObject[] memeBullshitSpikes;
        private PlayMakerFSM[] memeBullshitSpikeFSMs;
        private HealthManager memeBullshitHealthManager;
        private bool phase1, phase2, phase3, balloon1, balloon2;

        private System.Random rng;
        
        
        private const int PHASE_1_THRESHOLD = 800;
        private const int PHASE_2_THRESHOLD = 1600;
        private const int PHASE_3_THRESHOLD = 2400;
        // Phase 4 would be 3200 but this is infinite.
        
        private readonly string[] validBalloonTransitions = { "Explode Pause", "Out Pause", "Spike Return" };

        
        public void Start()
        {
            Modding.Logger.Log("Started NGG... somehow.");
            
            damageDone = -1;
         
            Modding.Logger.Log("NGG still running.");
            // This should only matter after grimm quest is over
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += Reset;
            
        }

         

        public void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= Reset;

            Modding.Logger.Log("[Infinite Grimm] Unloaded Grimm!");
        }

        private void Update()
        {
            
            if (playerDieTimeout > 0 && didDie)
            {

                playerDieTimeout--;
                if (playerDieTimeout != 0) return;
                playerDies();
                didDie = false;
            }
            
            if (!runningIG) return;
            
            
            
            // Infinite code.
            if (hm.hp != defaultHealth)
            {
                damageDone += (defaultHealth - hm.hp);
                hm.hp = defaultHealth;
                
                HeroController.instance.geoCounter.geoTextMesh.text = "" + damageDone;
                HeroController.instance.geoCounter.UpdateGeo(); // idek if this does something
            }

            if (memeBullshitHealthManager.hp != defaultHealth)
            {
                damageDone += (defaultHealth - memeBullshitHealthManager.hp);
                memeBullshitHealthManager.hp = defaultHealth;
                
                HeroController.instance.geoCounter.geoTextMesh.text = "" + damageDone;
                HeroController.instance.geoCounter.UpdateGeo(); // idek if this does something
            }

            memeBullshitGrimm.PrintSceneHierarchyTree("file.txt");
            
            if (PlayerData.instance.health <= 0)
                {
                    didDie = true;
                    runningIG = false;
                    playerDieTimeout = 300;

                    grimmAnim.GetClipByName("Tele In").fps = teleinFPS;
                    grimmAnim.GetClipByName("Tele Out").fps = teleoutFPS;
                    grimmAnim.GetClipByName("Uppercut End").fps = uppercutendFPS;
                    grimmAnim.GetClipByName("Slash Recover").fps = slashrecoverFPS;
                    grimmAnim.GetClipByName("Evade End").fps = evadeendFPS;
                    grimmAnim.GetClipByName("Spike Up").fps = spikesFPS;
                    
                    infinite_tent.damageDone = damageDone;

                    Modding.Logger.Log("[Infinite Grimm] Cleaned up NGG fight.");
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
            
            
            
            // Meme spike code. Probably works lul.
            for( int i = 0; i < 15; i++ ){
                if (memeBullshitSpikeFSMs[i].ActiveStateName != "Dormant") continue;
                memeBullshitSpikes[i].transform.position = 
                    new Vector3((float)(66 + (2.5 * i) + (rng.NextDouble() * 2.8)), 4.5f,
                        (grimmFSM.ActiveStateName == "AD Antic" || grimmFSM.ActiveStateName == "AD Fire" || 
                         grimmFSM.ActiveStateName == "AD Edge" || grimmFSM.ActiveStateName == "GD Antic" ||
                         grimmFSM.ActiveStateName == "G Dash Recover" ||
                         grimmFSM.ActiveStateName == "G Dash") ? -1.0f : 1.0f);
                
            }

            if (hardmode) return;
            doNextPhase();
        }

        private void doNextPhase()
        {
            
            
            if (!phase1 && damageDone >= PHASE_1_THRESHOLD)
            {
                // have fun, you can't cheese this badboy
                damageDone = PHASE_1_THRESHOLD;
                
                StartCoroutine(tryBalloon(1));

                // Because KDT is a meme this is actually slower than normal.
                grimmAnim.GetClipByName("Tele In").fps = 24;
                
                grimmAnim.GetClipByName("Tele Out").fps = 24;
                grimmAnim.GetClipByName("Uppercut End").fps = 24;
                grimmAnim.GetClipByName("Slash Recover").fps = 24;
                grimmAnim.GetClipByName("Spike Up").fps = 6;
                grimmAnim.GetClipByName("Evade End").fps = 24;

                phase1 = true;
            }

            if (!phase2 && damageDone >= PHASE_2_THRESHOLD)
            {
                damageDone = PHASE_2_THRESHOLD;
                StartCoroutine(tryBalloon(2));

                try
                {
                    for (int i = 0; i < 15; i++)
                    {

                        DamageHero spikeDmg = memeBullshitSpikes[i].GetComponent<DamageHero>();
                        spikeDmg.damageDealt = 4;
                    }
                }
                catch (Exception e)
                {
                    Modding.Logger.Log("[Infinite Grimm] Error setting spike damage to 2x... " +
                                       "KDT please... " + e);
                }

                    grimmAnim.GetClipByName("Tele In").fps = 36;
                    grimmAnim.GetClipByName("Tele Out").fps = 36;
                    grimmAnim.GetClipByName("Uppercut End").fps = 36;
                    grimmAnim.GetClipByName("Slash Recover").fps = 36;
                    grimmAnim.GetClipByName("Spike Up").fps = 4;
                    grimmAnim.GetClipByName("Evade End").fps = 36;
                

                phase2 = true;
            }

            if ( (!phase3 && damageDone >= PHASE_3_THRESHOLD) || hardmode)
            {
                if (!hardmode)
                {
                    damageDone = PHASE_3_THRESHOLD;
                    StartCoroutine(tryBalloon(3));
                }
                else
                {
                    memePhasethree();
                }

                phase3 = true;
            }
        }

        private void memePhasethree()
        {
            if (!hardmode)
            {
                // HAHAHAHAHA YOU THOUGHT YOU COULD GET DAMAGE IN ON THE BALLOON
                // YOU FOOLISH MORTAL.
                damageDone = PHASE_3_THRESHOLD;
            }

            try
            {
                for (int i = 0; i < 15; i++)
                {
                    DamageHero spikeDmg = memeBullshitSpikes[i].GetComponent<DamageHero>();
                    spikeDmg.damageDealt = 2;
                }
            }
            catch (Exception e)
            {
                Modding.Logger.Log("[Infinite Grimm] Error setting spike damage to 1x... " +
                                   "KDT please... " + e);
            }

            grimmAnim.GetClipByName("Tele In").fps = 48;
            grimmAnim.GetClipByName("Tele Out").fps = 48;
            grimmAnim.GetClipByName("Uppercut End").fps = 48;
            grimmAnim.GetClipByName("Slash Recover").fps = 48;
            grimmAnim.GetClipByName("Spike Up").fps = 2;
            grimmAnim.GetClipByName("Evade End").fps = 38;

            FsmUtil.changeTransition(grimmFSM, "Firebat 3", "FINISHED", "Firebat 4");
            FsmUtil.changeTransition(grimmFSM, "G Dash Recover", "FINISHED", "Tele Out");

            FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "FIREBATS", "FB Hero Pos");
            FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "SLASH", "Slash Pos");
            FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "AIR DASH", "AD Pos");
            FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "SPIKES", "Spike Attack");
            FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "PILLARS", "Pillar Pos");
        }

        private IEnumerator tryBalloon(int phase)
        {
            while ((validBalloonTransitions.All(t => grimmFSM.ActiveStateName != t)))
            {
                if (didDie)
                {
                    //rip you.
                    yield break;
                }
                
                yield return null;
            }

            if (phase == 3)
                StartCoroutine(phase3BalloonWait());
            
            balloonAttack();
        }

        private IEnumerator phase3BalloonWait()
        {
            float balloonTime = 10f;
            while (balloonTime > 0.0f)
            {
                if (didDie)
                {
                    yield break;
                }
                
                balloonTime -= Time.deltaTime;
                yield return null;

            }
            memePhasethree();
        }
        
        private void balloonAttack()
        {

            // Switch to Balloon attack. Hope this doesn't look too janky

            if (!balloon1)
            {
                grimmFSM.SetState("Set Balloon 1");
                balloon1 = true;
            }
            else if (!balloon2)
            {
                grimmFSM.SetState("Set Balloon 2");
                balloon2 = true;
            }
            else
            {
                grimmFSM.SetState("Set Balloon 3");
            }
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
                
                try
                {
                    memeBullshitSpikes = new GameObject[15];
                    memeBullshitSpikeFSMs = new PlayMakerFSM[15];
                }
                catch (Exception e)
                {
                    Modding.Logger.Log("exception " + e);
                }
                
                //lul

                try
                {
                    rng = new System.Random();

                    PlayerData.instance.AddMPCharge(99 * 2);
                    PlayerData.instance.MPCharge = 99;
                    PlayerData.instance.MPReserve = 99;
                }
                catch (Exception e)
                {
                    Modding.Logger.Log("[Infinite Grimm] Error either making the RNG or adding MP to player " + e);
                }

                phase1 = false;
                phase2 = false;
                phase3 = false;
                balloon1 = false;
                balloon2 = false;

                
                
                
                damageDone = 0;
                lastBalloonDamage = 0;
                // Just needs to be high enough he won't naturally die or use balloon attack.
                defaultHealth = 3000;

                grimmchildFrameCounter = 300;
                
                // Assigning the FSMs.
                try
                {
                    grimmContainer = GameObject.Find("Grimm Control");
                    grimm = grimmContainer.FindGameObjectInChildren("Nightmare Grimm Boss");
                    grimmAnim = grimm.GetComponent<tk2dSpriteAnimator>();
                    grimmFSM = FSMUtility.LocateFSM(grimm, "Control");
                }
                catch (Exception e)
                {
                    Modding.Logger.Log("[Infinite Grimm] Exception in finding grimm " + e);
                }
                
                teleinFPS = grimmAnim.GetClipByName("Tele In").fps;
                teleoutFPS = grimmAnim.GetClipByName("Tele Out").fps;
                uppercutendFPS = grimmAnim.GetClipByName("Uppercut End").fps;
                slashrecoverFPS = grimmAnim.GetClipByName("Slash Recover").fps;
                evadeendFPS = grimmAnim.GetClipByName("Evade End").fps;
                spikesFPS = grimmAnim.GetClipByName("Spike Up").fps;

                if (memeBullshitGrimmContainer == null)
                    memeBullshitGrimmContainer = Instantiate(grimmContainer);
                
                memeBullshitGrimm = memeBullshitGrimmContainer.FindGameObjectInChildren("Nightmare Grimm Boss");
                memeBullshitAnim = memeBullshitGrimm.GetComponent<tk2dSpriteAnimator>();
                memeBullshitGrimmFSM = FSMUtility.LocateFSM(memeBullshitGrimm, "Control");
                
                hm = grimm.GetComponent<HealthManager>();
                hm.hp = defaultHealth;

                memeBullshitHealthManager = memeBullshitGrimm.GetComponent<HealthManager>();
                memeBullshitHealthManager.hp = defaultHealth;

                // Starts the update process. This just saves CPU cycles when not in the grimm room.
                // It is done after loading the FSMs in case they error out and the Reset function never finishes.
                runningIG = true;
                
                // Maybe just set to true and leave it that way.
                didDie = false;
                playerDieTimeout = 0;
                
                
                FsmUtil.changeTransition(grimmFSM, "Move Choice", "PILLARS", "AD Pos");
                FsmUtil.changeTransition(grimmFSM, "Move Choice", "SPIKES", "Slash Pos");

                FsmUtil.changeTransition(grimmFSM, "Firebat 3", "FINISHED", "FB Behind");
                FsmUtil.changeTransition(grimmFSM, "G Dash Recover", "FINISHED", "Slash End");

                FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "FIREBATS", "Spike Attack");
                FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "SLASH", "Spike Attack");
                FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "AIR DASH", "Spike Attack");
                FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "SPIKES", "Spike Attack");
                FsmUtil.changeTransition(memeBullshitGrimmFSM, "Move Choice", "PILLARS", "Spike Attack");

                GameObject[] objects = UnityEngine.GameObject.FindObjectsOfType<GameObject>();
                int i = 0;
                foreach (GameObject go in objects)
                {
                    if (!go.name.Contains("Nightmare Spike")) continue;
                    
                    memeBullshitSpikes[i] = go;
                    memeBullshitSpikeFSMs[i] = FSMUtility.LocateFSM(go, "Control");
                    i++;
                }

                Modding.Logger.Log("[Infinite Grimm] Setup Nightmare IGG(s) battle... Have fun, puny mortal.");

                if (!hardmode) return;
                
                Modding.Logger.Log("[Infinite Grimm] IGG is in hard mode! You might as well give up now...");
                phase1 = true;
                phase2 = true;
                doNextPhase();

            }
            else
            {
                runningIG = false;
            }
        }
        
        // Gives geo on returning to main area after dying.
        private void playerDies()
        {
            Modding.Logger.Log("[Infinite Grimm] You did: " + damageDone + " damage!");
            int geo = (int)(damageDone / 5.0);
            

            // If you add 0 geo the menu bar gets buggy.
            if (geo > 0)
                HeroController.instance.AddGeo(geo);

        }
    }
}