using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using ModCommon;
using ModCommon.Util;
using Modding;
using UnityEngine.SceneManagement;

namespace infinitegrimm
{
    public class infinite_grimm_modern : MonoBehaviour
    {
        
        public GameObject grimmAnimObj;
        private PlayMakerFSM grimmFSM;
        private PlayMakerFSM stunFSM;
        private readonly GameObject[] nightmareSpikes = new GameObject[15];

        private int difficultyState;
        private int attacksToStun;
        private int startingAttacksToStun;
        private int stunIncAfterDmg;
        private bool runningIG;
        private bool noLagMode;
        private bool noLagMode2;
        private System.Random rng = new System.Random();

        public int maxHitsToStun = 30;

        private bool inStun = false;

        private const float HARD_DANCE_FACTOR_TWO = 1.25f;
        private const float HARD_DANCE_FACTOR_THREE = 1.75f;

        private const float NORMAL_DANCE_FACTOR_TWO = 1.075f;
        private const float NORMAL_DANCE_FACTOR_THREE = 1.15f;

        // Real values should probably be a lot higher
        private static readonly int[] DIFFICULTY_INCREASE_VALUES = new[]
        {
            1500, 4000, 9000, 12000, 15000
        };

        private int damageDone;
        private int lastHitDamage;

        private int lastBalloonDamage;
        // Health just needs to be high enough that grimm doesn't use the balloon attack (and can't be killed) naturally
        private int defaultHealth;

        // stunning implemented in my code and not games
        private int stunCounter;

        private bool balloon1;
        private bool balloon2;

        private bool didTakeDamage;

        private InfiniteEnemy meme;

        private readonly string[] validStunStates = {"Slash Antic", "Slash 1", "Slash 2", "Slash 3", "Slash Recover", "Slash End",
            "FB Antic", "FB Cast", "FB Cast End", "Firebat 1", "Firebat 2", "Firebat 4", "Tele Out", "FB Hero Pos",
            "FB Tele R", "FB Tele In", "FB Tele L", "FB Behind", "FB Re Tele", "Slash Pos", "Slash Tele In",
            "AD Pos", "AD Retry", "AD Tele In", "AD Antic", "AD Fire", "GD Antic", "AD Edge", "G Dash",
            "G Dash Recover", "Evade", "Evade End", "After Evade", "Uppercut Antic", "Uppercut Up", "UP Explode",
            "Pillar Pos", "Pillar Tele In", "Pillar Antic", "Pillar", "Pillar End" };

        private readonly string[] validBalloonTransitions = { "Explode Pause", "Out Pause", "Spike Return" };

        private static readonly string[] LAG_OBJECTS_TO_KILL =
        {
            "Grimm_nightmare__0017_9 (2)", "grimm_curtain (20)", "default_particles",
            "grimm_pole_standard (5)", "water_fog", "Grimm_nightmare__0016_10", "Grimm Control.Grimm_heart",
            "Grimm Control.Crowd", "Grimm Control.Heartbeat Audio", "Grimm_Main_tent_0009_3 (14)", "grimm_rag_main_tent",
            "grimm_rag_main_tent (25)", "grimm_fader", "grimm_fader", "grimm_rag_main_tent (58)", "Grimm_nightmare__0014_12",
            "grimm_rag_main_tent (29)", "Grimm_Main_tent_0007_5 (20)", "grimm_fader (1)", "grimm_rag_main_tent (23)",
            "Grimm_Main_tent_0008_4 (8)", "grimm_wallpaper (12)", "Grimm_Main_tent_0006_6 (7)",
            "grimm_pole_standard (8)", "grimm_rag_main_tent (59)", "Grimm_Main_tent_0010_2 (16)",
            "grimm_rag_main_tent (11)", "grimm_rag_main_tent (44)", "Grimm_nightmare__0020_6", "Grimm_nightmare__0018_8",
            "grimm_curtain (2)", "Grimm_nightmare__0018_8 (5)", "Grimm_Main_tent_0006_6 (11)", "grimm_rag_main_tent (19)",
            "Grimm_nightmare__0016_10 (8)", "Grimm_nightmare__0016_10 (13)", "break_rag (6)", "grimm_fader (12)",
            "Grimm_nightmare__0017_9 (1)", "Grimm_nightmare_fabric_lantern (11)", "Grimm Control.Halfway Glow",
            "Grimm Control.Final Glow", "Grimm Control.Crowd Fader",
            "main_tent_short_pole (8)", "Grimm_nightmare__0014_12 (14)", "Grimm_nightmare__0022_4 (1)",
            "Grimm_nightmare__0018_8 (1)", "Grimm_Main_tent_0006_6 (13)", "Spotlight Appear", "grimm_rag_main_tent (54)",
            "grimm_rag_main_tent (17)", "Grimm_Main_tent_0008_4 (10)", "grimm_pole_bit", "grimm_rag_main_tent (12)",
            "Grimm_nightmare_fabric_lantern (3)", "grimm_rag_main_tent (18)", "Grimm_nightmare__0014_12 (13)",
            "Grimm_Main_tent_0009_3 (10)", "Grimm_nightmare__0014_12 (7)", "Grimm_Main_tent_0008_4 (14)",
            "grimm_rag_main_tent (22)", "Grimm_nightmare__0023_3", "break_rag (5)", "grimm_rag_main_tent (39)",
            "Grimm_nightmare__0019_7", "grimm_wallpaper (5)", "grimm_rag_main_tent (27)", "Grimm_Main_tent_0010_2 (6)",
            "grimm_fader (1)", "Grimm_nightmare__0016_10 (7)", "Grimm_nightmare_fabric_lantern (6)",
            "grimm_rag_main_tent (61)", "Grimm_nightmare__0016_10 (24)", "Grimm_nightmare__0017_9 (10)",
            "grimm_rag_main_tent (45)", "Grimm_nightmare_fabric_lantern (13)", "Grimm_nightmare__0016_10 (21)",
            "grimm_wallpaper (6)", "grimm_curtain (19)", "grimm_rag_main_tent (47)", "grimm_rag_main_tent (2)",
            "grimm_curtain_02 (15)", "Grimm_Main_tent_0006_6 (14)", "dream_particles", "grimm_pole_standard (3)",
            "Grimm_nightmare_fabric_lantern (1)", "break_rag (4)", "Incense Particle", "Grimm_nightmare_fabric_lantern (7)",
            "main_tent_short_pole (5)", "Grimm_nightmare_fabric_lantern (8)", "break_rag (1)", "Grimm_nightmare_fabric_lantern (9)",
            "Grimm_nightmare_fabric_lantern (2)", "Grimm_nightmare_fabric_lantern (5)", "grimm_pole_standard (1)",
            "Grimm_nightmare_fabric_lantern (4)", "Grimm_nightmare_fabric_lantern", "Grimm_nightmare_fabric_lantern (12)",
            "break_rag", "break_rag (3)", "break_rag (2)", "Grimm_nightmare_fabric_lantern (10)", "break_rag (7)"
        };

        private static readonly string[] ANNOYING_OBJECTS_TO_KILL =
        {
            "Grimm Control.Loop Fire", "Grimm Control.Heartbeat Audio"
        };


        private CustomEnemySpeed.AnimationData[] allAnimationStates;
        private CustomEnemySpeed.WaitData[] allWaitStates;
        
        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= Reset;
        }

        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += Reset;
        }

        private void Update()
        {
            if (!runningIG) return;
            
            if (PlayerData.instance.health <= 0)
            {
                runningIG = false;
                ModHooks.Instance.HitInstanceHook -= damage;
                damageDone = meme.damageDone;
                if (infinite_global_vars.maximumDamage < damageDone)
                {
                    infinite_global_vars.maximumDamage = damageDone;
                    Modding.Logger.Log("[Infinite Grimm] New Damage Record!!!");
                }

                infinite_tent.damageDone = damageDone;
                StartCoroutine(playerDies(damageDone));
                Modding.Logger.Log("[Infinite Grimm] Cleaned up Grimm fight.");
            }

            if (damageDone != meme.damageDone)
            {

                //log("Took damage. Damage taken was " + (meme.damageDone - damageDone) + " and stuncounter is " + stunCounter + " and atks to stun is " + attacksToStun );
                lastHitDamage = meme.damageDone - damageDone;
                didTakeDamage = true;
                damageDone = meme.damageDone;
                
                HeroController.instance.geoCounter.geoTextMesh.text = "" + damageDone;
                HeroController.instance.geoCounter.UpdateGeo();
            }

            if (stunCounter >= attacksToStun)
            {
                string currentState = grimmFSM.ActiveStateName;
                if (validStunStates.Any(t => currentState == t))
                {
                    stunFSM.SetState("Stun");
                    stunCounter = 0;
                    StartCoroutine(stunGrimmTracker());
                }
                else
                {
                    stunCounter--;
                }
            }
                

            // balloons should happen at increasingly rare intervals.
            // after: 400, 440, 484, 532, 585... etc damage. This lets the player keep getting stuns in even as stun rarity goes up
            if (damageDone - lastBalloonDamage <= 400 + (lastBalloonDamage / 10)) return;
            {
                string currentState = grimmFSM.ActiveStateName;
                if (validBalloonTransitions.All(t => currentState != t)) return;
                    
                if (noLagMode)
                    balloonAttackNoLag();
                else
                    balloonAttack();
                    
                lastBalloonDamage = damageDone;
            }
        }
        
        private void balloonAttackNoLag()
        {
            grimmFSM.SetState("Set Balloon 3");
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
            runningIG = false;
            
            // Essentially makes sure they complete grimm quest before starting infinite grimm.
            // Otherwise the player would have a hard time upgrading their grimmchild.
            if (to.name != "Grimm_Nightmare") return;
            if (!PlayerData.instance.defeatedNightmareGrimm ||
                !PlayerData.instance.killedNightmareGrimm) return;

            lastBalloonDamage = 0;
            stunCounter = 0;
            lastHitDamage = 0;
            difficultyState = 0;
            // Assigning the FSMs.
            grimmAnimObj = GameObject.Find("Grimm Control").FindGameObjectInChildren("Nightmare Grimm Boss");
            grimmFSM = grimmAnimObj.LocateMyFSM("Control");
            stunFSM = grimmAnimObj.LocateMyFSM("Stun");
            
            
            // Just in case NKG has a stun limit this removes it.
            // I don't think so judging by the -1 nail video but you never know. It might just be really high.
            FsmState normalStun = stunFSM.getState("Stun");
            normalStun.removeActionsOfType<IntAdd>();

            // Stops FSM from doing stuns because we are implementing them in code.
            // This essentially makes the stun FSM stuck in the "In Combo" state
            // Until we manually tell it to be in the "Stun" state.
            FsmState stunCombo = stunFSM.getState("In Combo");
            stunCombo.clearTransitions();
            
            meme = grimmAnimObj.GetOrAddComponent<InfiniteEnemy>();
            

            if (infinite_grimm.hardmode)
            {
                setupWaitsHardMode();
                meme.SetStartingDanceSpeed(infinite_grimm.startingDanceSpeed + 0.4f);
            }
            else
            {
                setupWaitsNormalMode();
                meme.SetStartingDanceSpeed(infinite_grimm.startingDanceSpeed);
            }

            meme.SetDanceSpeedIncreaseDamage(infinite_grimm.danceSpeedIncreaseDmg);
            meme.SetMaxDanceSpeed(infinite_grimm.maxDanceSpeed);
            startingAttacksToStun = infinite_grimm.startingStaggerHits;
            stunIncAfterDmg = infinite_grimm.staggerIncreaseDamage;
            noLagMode = infinite_grimm.noLagMode;
            noLagMode2 = infinite_grimm.noLagMode2;
            attacksToStun = startingAttacksToStun + (damageDone / stunIncAfterDmg);
            balloon1 = false;
            balloon2 = false;
            
            foreach (CustomEnemySpeed.WaitData w in allWaitStates)
            {
                meme.AddWaitData(w);
            }
            foreach (CustomEnemySpeed.AnimationData a in allAnimationStates)
            {
                meme.AddAnimationData(a);
            }
            meme.StartSpeedMod();
            didTakeDamage = false;
            Modding.Logger.Log("[Infinite Grimm] Setup IG modern battle");
            runningIG = true;

            StartCoroutine(destroyStuff(ANNOYING_OBJECTS_TO_KILL));
            if (noLagMode2)
            {
                StartCoroutine(destroyStuff(LAG_OBJECTS_TO_KILL));
            }
            
            GameObject[] objects = UnityEngine.GameObject.FindObjectsOfType<GameObject>();
            int i = 0;
            foreach (GameObject go in objects)
            {
                if (!go.name.Contains("Nightmare Spike")) continue;

                nightmareSpikes[i] = go;
                i++;
            }
            
            
            ModHooks.Instance.HitInstanceHook += damage;
            StartCoroutine(spawnGrimmchild());
        }

        private static IEnumerator spawnGrimmchild()
        {
            yield return new WaitForFinishedEnteringScene();
            yield return new WaitForSeconds(5f);

            if (!PlayerData.instance.GetBoolInternal("equippedCharm_40")) yield break;
            
            
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
        
        // Gives geo on returning to main area after dying.
        private static IEnumerator playerDies(int dmg)
        {
            yield return new WaitForSeconds(6f);
            yield return new WaitForFinishedEnteringScene();
            yield return new WaitForSeconds(2f);
            Modding.Logger.Log("[Infinite Grimm] Good job, you did: " + dmg + " damage!");
            int geo = (int)(dmg / 10.0);
            if (infinite_grimm.hardmode)
                geo *= 2;

            // If you add 0 geo the menu bar gets buggy.
            if (geo > 0)
                HeroController.instance.AddGeo(geo);
        }

        public void setupNGGSpikes()
        {
            //log("Setting up random spike positioning.");
            // Meme spike code. Probably works lul.
            for( int i = 0; i < 15; i++ ){
                nightmareSpikes[i].transform.position = 
                    new Vector3((float)(66 + (2.5 * i) + (rng.NextDouble() * 2.8)), 4.5f, -0.0001f);
            }
        }

        private IEnumerator addGodSpikes()
        {
            //FsmUtil.changeTransition(grimmFSM, "Move Choice", "PILLARS", "AD Pos");

            SendRandomEventV3 sreV3 = grimmFSM.getState("Move Choice").getActionsOfType<SendRandomEventV3>()[0];
            try
            {
                FsmInt[] maxEvents = new FsmInt[sreV3.events.Length - 1];
                FsmEvent[] events = new FsmEvent[sreV3.events.Length - 1];
                FsmInt[] missedMax = new FsmInt[sreV3.events.Length - 1];
                FsmInt[] trackingInts = new FsmInt[sreV3.events.Length - 1];
                FsmInt[] trackingIntsMissed = new FsmInt[sreV3.events.Length - 1];
                FsmFloat[] weights = new FsmFloat[sreV3.events.Length - 1];
                bool foundEvent = false;
                for (int i = 0; i < sreV3.events.Length; i++)
                {   
                    log(sreV3.eventMax[i].Value + ", " + sreV3.events[i].Name + ", " + sreV3.missedMax[i].Value +
                        ", " + sreV3.trackingInts[i].Value + ", " + sreV3.trackingIntsMissed[i].Value + ", " +
                        sreV3.weights[i].Value);
                    if (sreV3.events[i].Name == "SPIKES")
                    {
                        foundEvent = true;
                        continue;
                    }
                    if (!foundEvent)
                    {
                        maxEvents[i] = sreV3.eventMax[i];
                        events[i] = sreV3.events[i];
                        missedMax[i] = sreV3.missedMax[i];
                        trackingInts[i] = sreV3.trackingInts[i];
                        trackingIntsMissed[i] = sreV3.trackingIntsMissed[i];
                        weights[i] = sreV3.weights[i];
                    }
                    else
                    {
                        maxEvents[i - 1] = sreV3.eventMax[i];
                        events[i - 1] = sreV3.events[i];
                        missedMax[i - 1] = sreV3.missedMax[i];
                        trackingInts[i - 1] = sreV3.trackingInts[i];
                        trackingIntsMissed[i - 1] = sreV3.trackingIntsMissed[i];
                        weights[i - 1] = sreV3.weights[i];
                    }

                }

                sreV3.eventMax = maxEvents;
                sreV3.events = events;
                sreV3.missedMax = missedMax;
                sreV3.trackingInts = trackingInts;
                sreV3.trackingIntsMissed = trackingIntsMissed;
                sreV3.weights = weights;
            }
            catch (Exception e)
            {
                log("Unable to remove spike event because (NRE?) " + e);
            }
            
            //FsmUtil.changeTransition(grimmFSM, "Move Choice", "SPIKES", "Slash Pos");
            
            yield return new WaitForSeconds(3f);

            StartCoroutine(godSpikeLoop());


        }

        private IEnumerator godSpikeLoop()
        {
            SendEventByName spikeAtk = grimmFSM.getState("Spike Attack").getActionsOfType<SendEventByName>()[0];
            
            while (runningIG)
            {
                if (!inStun)
                {
                    setupNGGSpikes();
                    spikeAtk.Fsm.Event(spikeAtk.eventTarget, spikeAtk.sendEvent.Value);
                    spikeAtk.Finish();
                }

                yield return new WaitForSeconds((float) (9.0f / meme.danceSpeed));
            }
        }

        private void addNGGSpikeRNG()
        {
            FsmState spikeState = grimmFSM.getState("Spike Attack");
            List<FsmStateAction> actions = spikeState.Actions.ToList();
            actions.Insert(0, new CallMethod()
            {
                behaviour = this,
                everyFrame = false,
                methodName = "setupNGGSpikes",
                parameters = new FsmVar[0]
            });
            spikeState.Actions = actions.ToArray();
        }

        private void setupWaitsNormalMode()
        {
            allWaitStates = new[]
            {
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_TWO, "Control", "Slash Antic"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_TWO, "Control", "FB Cast"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_TWO, "Control", "FB Cast End"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_TWO, "Control", "Spike Attack"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_THREE, "Control", "Firebat 1"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_THREE, "Control", "Firebat 2"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_THREE, "Control", "Firebat 3"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_THREE, "Control", "Firebat 4"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_THREE, "Control", "Out Pause"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_THREE, "Control", "Pillar"),
                new CustomEnemySpeed.WaitData(NORMAL_DANCE_FACTOR_THREE, "Control", "Pillar Antic") 
            };
            
            allAnimationStates = new[]
            {
                new CustomEnemySpeed.AnimationData(1.5f, "Tele In"),
                new CustomEnemySpeed.AnimationData(1.5f, "Tele Out"),
                new CustomEnemySpeed.AnimationData(1.5f, "Uppercut End"),
                new CustomEnemySpeed.AnimationData(1.5f, "Slash Recover"),
                new CustomEnemySpeed.AnimationData(1.5f, "Evade End"),
                new CustomEnemySpeed.AnimationData(NORMAL_DANCE_FACTOR_TWO, "Cast Antic"),
                new CustomEnemySpeed.AnimationData(NORMAL_DANCE_FACTOR_TWO, "Cast Return"),
                new CustomEnemySpeed.AnimationData(NORMAL_DANCE_FACTOR_TWO, "Capespike Cast"),
                new CustomEnemySpeed.AnimationData(NORMAL_DANCE_FACTOR_TWO, "Explode Antic")
            };
        }

        private void setupWaitsHardMode()
        {
            allWaitStates = new[]
            {
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_TWO, "Control", "Slash Antic"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_TWO, "Control", "FB Cast"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_TWO, "Control", "FB Cast End"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_TWO, "Control", "Spike Attack"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_THREE, "Control", "Firebat 1"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_THREE, "Control", "Firebat 2"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_THREE, "Control", "Firebat 3"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_THREE, "Control", "Firebat 4"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_THREE, "Control", "Out Pause"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_THREE, "Control", "Pillar"),
                new CustomEnemySpeed.WaitData(HARD_DANCE_FACTOR_THREE, "Control", "Pillar Antic") 
            };
            
            allAnimationStates = new[]
            {
                new CustomEnemySpeed.AnimationData(1.5f, "Tele In"),
                new CustomEnemySpeed.AnimationData(1.5f, "Tele Out"),
                new CustomEnemySpeed.AnimationData(1.5f, "Uppercut End"),
                new CustomEnemySpeed.AnimationData(1.5f, "Slash Recover"),
                new CustomEnemySpeed.AnimationData(1.5f, "Evade End"),
                new CustomEnemySpeed.AnimationData(HARD_DANCE_FACTOR_TWO, "Cast Antic"),
                new CustomEnemySpeed.AnimationData(HARD_DANCE_FACTOR_TWO, "Cast Return"),
                new CustomEnemySpeed.AnimationData(HARD_DANCE_FACTOR_TWO, "Capespike Cast"),
                new CustomEnemySpeed.AnimationData(HARD_DANCE_FACTOR_TWO, "Explode Antic")
            };
            //infinite_grimm.hardmode
        }

        private IEnumerator stunGrimmTracker()
        {
            inStun = true;
            yield return new WaitForSeconds(8f);
            inStun = false;
        }

        private static IEnumerator destroyStuff(IEnumerable<string> stuffToKill)
        {
            yield return new WaitForSeconds(0.5f);
            foreach (string s in stuffToKill)
            {
                string[] gameObjs = s.Split('.');
                GameObject killMe = GameObject.Find(gameObjs[0]);

                if (killMe == null)
                {
                    log("Unable to find gameobject of name " + gameObjs[0]);
                    log("Please report this as a bug!");
                    continue;
                }

                for (int i = 1; i < gameObjs.Length; i++)
                {
                    killMe = killMe.FindGameObjectInChildren(gameObjs[i]);
                    if (killMe != null) continue;
                    log("Unable to find subobj " + gameObjs[i] + " from " + s);
                    log("Please report this as a bug!");
                }
                
                if (killMe != null)
                    Destroy(killMe);
            }
            
            
        }
        
        
        private HitInstance damage(Fsm isGrimm, HitInstance hit)
        {
            if (!didTakeDamage) return hit;

            if (infinite_grimm.hardmode)
            {
                for (int j = 0; j < DIFFICULTY_INCREASE_VALUES.Length; j++)
                {
                    if (damageDone <= DIFFICULTY_INCREASE_VALUES[j] ||
                        (difficultyState & (int) (Math.Round(Math.Pow(2, j)))) != 0) continue;

                    difficultyState += (int) Math.Round(Math.Pow(2, j));
                    switch (j)
                    {
                        case 0:
                            log("Adding NGG spike randomness");
                            addNGGSpikeRNG();
                            break;
                        case 1:
                            StartCoroutine(addGodSpikes());
                            break;
                        default:
                            log("Nothing setup for difficulty increase " + j);
                            break;
                    }
                }
            }

            didTakeDamage = false;
            if (hit.DamageDealt != lastHitDamage)
            {    
                return hit;
            }
            attacksToStun = startingAttacksToStun + (damageDone / stunIncAfterDmg);
            if (attacksToStun > maxHitsToStun)
            {
                attacksToStun = maxHitsToStun;
            }
            
            AttackTypes a = hit.AttackType;
            if (a == AttackTypes.Nail || a == AttackTypes.Spell || a == AttackTypes.SharpShadow)
                stunCounter++;
            
            return hit;
        }
        
        private static void log(string str)
        {
            
            Modding.Logger.Log("[Infinite Grimm] " + str);
        }
    }
}