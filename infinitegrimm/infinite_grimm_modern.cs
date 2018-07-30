using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

// ReSharper disable UnusedMember.Global

namespace infinitegrimm
{
    public class infinite_grimm_modern : MonoBehaviour
    {
        
        public GameObject grimmAnimObj;
        private PlayMakerFSM grimmFSM;
        private PlayMakerFSM stunFSM;
        private readonly GameObject[] nightmareSpikes = new GameObject[15];
        private readonly PlayMakerFSM[] nightmareSpikeFSMs = new PlayMakerFSM[15];
        private readonly tk2dSpriteAnimator[] nightmareSpikeAnims = new tk2dSpriteAnimator[15];
        private readonly GameObject[] deathWalls = new GameObject[3];
        private redwing_flamegen_returns deathWallGen;

        private int difficultyState;
        private int attacksToStun;
        private int startingAttacksToStun;
        private int stunIncAfterDmg;
        private bool runningIG;
        private readonly Random rng = new Random();


        private bool inStun;

        // Timescale memes
        private float actualTimeScale;
        private const double TS_MULT_VAL = 0.001;
        private const double TS_EXP_VAL = 0.15;
        private float lastTimeScale = 1.0f;
        private const int MAX_HITS_TO_STUN = 30;
        
        // Dance speeds
        private const float HARD_DANCE_FACTOR_TWO = 1.25f;
        private const float HARD_DANCE_FACTOR_THREE = 1.75f;
        private const float NORMAL_DANCE_FACTOR_TWO = 1f + (HARD_DANCE_FACTOR_TWO - 1f)/(2f);
        private const float NORMAL_DANCE_FACTOR_THREE = 1f + (HARD_DANCE_FACTOR_THREE - 1f)/(2f);

        private bool addedTimeAttack;
        private int damageDone;
        private int lastHitDamage;

        private int lastBalloonDamage;
        // stunning implemented in my code and not games
        private int stunCounter;

        private bool balloon1;
        private bool balloon2;

        private bool didTakeDamage;

        private InfiniteEnemy meme;
        
        private double getTimeScaleMod()
        {
            return (damageDone - infinite_globals.difficultyIncreaseValues[3]) <= 0 ? 1.0 :
                Math.Pow((TS_MULT_VAL *
                          (double) (damageDone - infinite_globals.difficultyIncreaseValues[3])) + 1.0, TS_EXP_VAL);
        }
        
        private void hookSetTimeScale1(On.GameManager.orig_SetTimeScale_1 orig, GameManager self, float newTimeScale)
        {
            if (runningIG)
            {
                lastTimeScale = newTimeScale;
                Time.timeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale) * actualTimeScale;
            }
            else
            {
                orig(self, newTimeScale);
            }
        }


        private CustomEnemySpeed.AnimationData[] allAnimationStates;
        private CustomEnemySpeed.WaitData[] allWaitStates;
        
        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= Reset;
            actualTimeScale = 1.0f;
            Time.timeScale = 1.0f;
            On.GameManager.SetTimeScale_1 -= hookSetTimeScale1;
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
                Destroy(GameManager.instance.gameObject.GetComponent<time_attack>());
                
                On.GameManager.SetTimeScale_1 -= hookSetTimeScale1;
                actualTimeScale = 1.0f;
                Time.timeScale = 1.0f;
                ModHooks.Instance.HitInstanceHook -= damage;
                ModHooks.Instance.TakeDamageHook -= oneHitKill;
                damageDone = meme.damageDone;
                if (infinite_globals.maximumDamage < damageDone)
                {
                    infinite_globals.maximumDamage = damageDone;
                    infinite_globals.log("New Damage Record!!!");
                }

                infinite_tent.damageDone = damageDone;
                StartCoroutine(infinite_globals.playerDies(damageDone));
                infinite_globals.log("Cleaned up Grimm fight.");
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
                if (infinite_globals.VALID_STUN_STATES.Any(t => currentState == t))
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
                if (infinite_globals.VALID_BALLOON_TRANSITIONS.All(t => currentState != t)) return;
                    
                if (infinite_globals.noLagMode)
                    balloonAttackNoLag();
                else
                    balloonAttack();
                    
                lastBalloonDamage = damageDone;
            }
        }

        private static int oneHitKill(ref int hazardtype, int i)
        {
            return 999;
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
            
            PlayerData.instance.AddMPCharge(99 * 2);
            PlayerData.instance.MPCharge = 99;
            PlayerData.instance.MPReserve = 99;
            lastBalloonDamage = 0;
            if (infinite_globals.timeAttackMode)
            {
                stunCounter = -99999;
                lastBalloonDamage = 10000000;
            }
            else
            {
                stunCounter = 0;
            }

            lastHitDamage = 0;
            difficultyState = 0;
            damageDone = 0;
            addedTimeAttack = false;
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
            

            if (infinite_globals.hardmode)
            {
                setupWaitsHardMode();
                meme.SetStartingDanceSpeed(infinite_globals.startingDanceSpeed + 0.4f);
                actualTimeScale = 1.0f;
            }
            else
            {
                setupWaitsNormalMode();
                meme.SetStartingDanceSpeed(infinite_globals.startingDanceSpeed);
            }

            meme.SetDanceSpeedIncreaseDamage(infinite_globals.danceSpeedIncreaseDmg);
            if (infinite_globals.hardmode)
            {
                meme.SetMaxDanceSpeed(infinite_globals.maxDanceSpeed);
            }
            else
            {
                meme.SetMaxDanceSpeed(infinite_globals.maxDanceSpeed * 2);
            }
            
            startingAttacksToStun = infinite_globals.startingStaggerHits;
            stunIncAfterDmg = infinite_globals.staggerIncreaseDamage;
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
            infinite_globals.log("Setup IG modern battle");
            runningIG = true;

            StartCoroutine(infinite_globals.destroyStuff(infinite_globals.ANNOYING_OBJECTS_TO_KILL));
            if (infinite_globals.noLagMode2)
            {
                StartCoroutine(infinite_globals.destroyStuff(infinite_globals.LAG_OBJECTS_TO_KILL));
            }
            
            GameObject[] objects = FindObjectsOfType<GameObject>();
            int i = 0;
            foreach (GameObject go in objects)
            {
                if (!go.name.Contains("Nightmare Spike")) continue;

                nightmareSpikes[i] = go;
                i++;
            }
            ModHooks.Instance.HitInstanceHook += damage;
            StartCoroutine(infinite_globals.spawnGrimmchild());

            if (infinite_globals.oneHitMode)
            {
                infinite_globals.log("One hit and you're done. Have fun...");
                //PlayerData.instance.health = 2;
                //HeroController.instance.TakeDamage(HeroController.instance.gameObject, CollisionSide.other, PlayerData.instance.health - 1, 1);
                ModHooks.Instance.TakeDamageHook += oneHitKill;
            }
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public void setupNGGSpikes()
        {
            //infinite_globals.log("Setting up random spike positioning.");
            // Meme spike code. Probably works lul.
            for( int i = 0; i < 15; i++ ){
                nightmareSpikes[i].transform.position = 
                    new Vector3((float)(66 + (2.5 * i) + (rng.NextDouble() * 2.8)), 4.5f, -0.0001f);
            }
        }

        public void testFunctionWorks()
        {
            infinite_globals.log("Test function works!");
        }

        public void spikeWaitMeme()
        {
            infinite_globals.log("Starting spike coroutine. in 2.5s they will come up.");
            for (int i = 0; i < 15; i++)
            {
                StartCoroutine(setSpikeUp(i));
            }
        }

        private IEnumerator setSpikeUp(int i)
        {
            nightmareSpikeAnims[i].Play(nightmareSpikeAnims[i].GetClipByName("Spike Meme"));
            yield return new WaitForSeconds(2.5f);
            if (nightmareSpikeFSMs[i] != null)
            {
                nightmareSpikeAnims[i].Play(nightmareSpikeAnims[i].GetClipByName("Spike Up"));
                nightmareSpikeFSMs[i].SetState("Up");
            }
            yield return new WaitForSeconds(0.25f);
            if (nightmareSpikeFSMs[i] != null)
            {
                nightmareSpikeFSMs[i].SetState("Down");
                nightmareSpikeAnims[i].Play(nightmareSpikeAnims[i].GetClipByName("Spike Down"));
            }
            // ReSharper disable once InvertIf dungo
            if (nightmareSpikeFSMs[i] != null)
            {
                yield return new WaitForSeconds(0.50f);
                nightmareSpikeFSMs[i].SetState("Dormant");
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
                    infinite_globals.log(sreV3.eventMax[i].Value + ", " + sreV3.events[i].Name + ", " + sreV3.missedMax[i].Value +
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
                infinite_globals.log("Unable to remove spike event because (NRE?) " + e);
            }
            
            //FsmUtil.changeTransition(grimmFSM, "Move Choice", "SPIKES", "Slash Pos");
            
            yield return new WaitForSeconds(3f);
            
            for( int i = 0; i < 15; i++ )
            {
                
                nightmareSpikeAnims[i] = nightmareSpikes[i].GetComponent<tk2dSpriteAnimator>();
                nightmareSpikeFSMs[i] = nightmareSpikes[i].LocateMyFSM("Control");
                nightmareSpikeAnims[i].GetClipByName("Spike Ready").fps = 60f;
                nightmareSpikeAnims[i].GetClipByName("Spike Up").fps = 60f;
                //a.GetClipByName("Capespike Cast").fps = 2f;
                nightmareSpikeAnims[i].GetClipByName("Spike Down").fps = 60f;

                FsmState spikeFSMstate = nightmareSpikes[i].GetFSMState("Dormant", "Control");
                FsmState spikeFSMcancel = nightmareSpikes[i].GetFSMState("Cancel", "Control");
                FsmState spikeFSMready = nightmareSpikes[i].GetFSMState("Ready", "Control");
                nightmareSpikes[i].GetFSMState("Up", "Control").removeActionsOfType<Tk2dPlayAnimation>();
                nightmareSpikes[i].GetFSMState("Down", "Control").removeActionsOfType<Tk2dPlayAnimationWithEvents>();
                spikeFSMstate.clearTransitions();
                spikeFSMstate.addTransition("SPIKES READY", "Cancel");
                spikeFSMcancel.Actions = new FsmStateAction[0];
                spikeFSMcancel.clearTransitions();
                //spikeFSMcancel.addTransition("FINISHED", "Ready");
                SetMeshRenderer setMeshR = spikeFSMready.getActionsOfType<SetMeshRenderer>()[0];
                
                tk2dSpriteAnimationFrame[] spikeMemeFrames = nightmareSpikeAnims[i].GetClipByName(
                    spikeFSMready.getActionsOfType<Tk2dPlayAnimation>()[0].clipName
                        .Value).frames;
                
                
                tk2dSpriteAnimationClip[] clipLib = nightmareSpikeAnims[i].Library.clips;
                tk2dSpriteAnimationClip[] clipNew = new tk2dSpriteAnimationClip[clipLib.Length + 1];
                for (int j = 0; j < clipLib.Length; j++)
                {
                    clipNew[j] = clipLib[j];
                }

                clipNew[clipLib.Length] =
                    new tk2dSpriteAnimationClip
                    {
                        name = "Spike Meme",
                        fps = 0.5f,
                        frames = new []
                        {
                            spikeMemeFrames[0],
                            spikeMemeFrames[1]
                        },
                        loopStart = nightmareSpikeAnims[i].GetClipByName("Spike Ready").loopStart,
                        wrapMode = nightmareSpikeAnims[i].GetClipByName("Spike Ready").wrapMode
                    };
                nightmareSpikeAnims[i].Library.clips = clipNew;
                spikeFSMcancel.addAction(setMeshR);

                if (i == 0)
                {
                    spikeFSMcancel.addAction(new CallMethod
                    {
                        behaviour = this,
                        everyFrame = false,
                        methodName = "spikeWaitMeme",
                        parameters = new FsmVar[0]
                    });
                }
            }
            StartCoroutine(godSpikeLoop());
            
            infinite_globals.log("Setup god spikes.");
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

                yield return new WaitForSeconds((float) (9.0f));
            }
        }

        private void setupDeathWalls()
        {
            deathWallGen = new redwing_flamegen_returns(100, 500, 25);
            
            deathWalls[0] = new GameObject("IGDeathwallRight", typeof(SpriteRenderer), typeof(death_wall_behavior), typeof(NonBouncer));
            SpriteRenderer leftSprite = deathWalls[0].GetComponent<SpriteRenderer>();
            leftSprite.sprite = Sprite.Create(deathWallGen.firePillars[0], new Rect(0, 0, 100, 500), new Vector2(0.5f, 0f),
                30f);
            leftSprite.color = new Color(1f, 1f, 1f, 0f);
            leftSprite.enabled = true;
            
            deathWalls[1] = new GameObject("IGDeathwallLeft", typeof(SpriteRenderer), typeof(death_wall_behavior), typeof(NonBouncer));
            SpriteRenderer rightSprite = deathWalls[1].GetComponent<SpriteRenderer>();
            rightSprite.sprite = Sprite.Create(deathWallGen.firePillars[1], new Rect(0, 0, 100, 500), new Vector2(0.5f, 0f),
                30f);
            rightSprite.color = new Color(1f, 1f, 1f, 0f);
            rightSprite.enabled = true;
            deathWalls[0].layer = 17;
            deathWalls[1].layer = 17;
            
            deathWalls[2] = new GameObject("IGDeathwallTop", typeof(SpriteRenderer), typeof(death_wall_behavior), typeof(NonBouncer));
            SpriteRenderer topSprite = deathWalls[2].GetComponent<SpriteRenderer>();
            topSprite.sprite = Sprite.Create(deathWallGen.firePillars[2], new Rect(0, 50, 500, 50), new Vector2(0.5f, 0f),
                15f);
            topSprite.color = new Color(1f, 1f, 1f, 0f);
            topSprite.enabled = true;
            deathWalls[2].layer = 17;
            deathWalls[0].transform.position = Vector3.zero;
            deathWalls[0].transform.localPosition = new Vector3(102f, 3.4f, -1f);
            deathWalls[1].transform.position = Vector3.zero;
            deathWalls[1].transform.localPosition = new Vector3(69f, 3.4f, -1f);
            
            deathWalls[2].transform.position = Vector3.zero;
            deathWalls[2].transform.localPosition = new Vector3(86f, 18.5f, -1f);
            deathWalls[0].SetActive(true);
            deathWalls[1].SetActive(true);
            deathWalls[2].SetActive(true);
            infinite_globals.log("Setup the insane deathwalls... glhf!");
        }

        private void addNGGSpikeRNG()
        {
                
            FsmState spikeState = grimmFSM.getState("Spike Attack");
            List<FsmStateAction> actions = spikeState.Actions.ToList();
            actions.Insert(0, new CallMethod
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
                new CustomEnemySpeed.AnimationData(1.25f, "Tele In"),
                new CustomEnemySpeed.AnimationData(1.25f, "Tele Out"),
                new CustomEnemySpeed.AnimationData(1.25f, "Uppercut End"),
                new CustomEnemySpeed.AnimationData(1.25f, "Slash Recover"),
                new CustomEnemySpeed.AnimationData(1.25f, "Evade End"),
                new CustomEnemySpeed.AnimationData(NORMAL_DANCE_FACTOR_TWO, "Cast Antic"),
                new CustomEnemySpeed.AnimationData(NORMAL_DANCE_FACTOR_TWO, "Cast Return"),
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
                new CustomEnemySpeed.AnimationData(HARD_DANCE_FACTOR_TWO, "Explode Antic")
            };
            //infinite_grimm_vars.hardmode
        }

        private IEnumerator stunGrimmTracker()
        {
            inStun = true;
            yield return new WaitForSeconds(8f);
            inStun = false;
        }
        
        private HitInstance damage(Fsm isGrimm, HitInstance hit)
        {
            if (infinite_globals.oneHitMode && PlayerData.instance.GetBool("equippedCharm_6"))
            {
                hit.DamageDealt = (int) (hit.DamageDealt * 1.75);
            }
            
            if (!didTakeDamage) return hit;

            if (infinite_globals.timeAttackMode && !addedTimeAttack)
            {
                addedTimeAttack = true;
                GameManager.instance.gameObject.AddComponent<time_attack>();
            }

            if (infinite_globals.hardmode)
            {
                actualTimeScale = (float) getTimeScaleMod();
                if (actualTimeScale > 1.0001f)
                {
                    Time.timeScale = ((lastTimeScale <= 0.01f) ? 0f : 1.0f) * actualTimeScale;
                }
                
                for (int j = 0; j < infinite_globals.difficultyIncreaseValues.Length; j++)
                {
                    if (damageDone <= infinite_globals.difficultyIncreaseValues[j] ||
                        (difficultyState & (int) (Math.Round(Math.Pow(2, j)))) != 0) continue;

                    difficultyState += (int) Math.Round(Math.Pow(2, j));
                    switch (j)
                    {
                        case 0:
                            infinite_globals.log("Adding NGG spike randomness");
                            addNGGSpikeRNG();
                            break;
                        case 1:
                            StartCoroutine(addGodSpikes());
                            break;
                        case 2:
                            infinite_globals.log("Adding death walls...");
                            setupDeathWalls();
                            break;
                        case 3:
                            infinite_globals.log("Adding infinite difficulty increase through speedup...");
                            On.GameManager.SetTimeScale_1 += hookSetTimeScale1;
                            break;
                        default:
                            infinite_globals.log("Nothing setup for difficulty increase " + j);
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
            if (attacksToStun > MAX_HITS_TO_STUN)
            {
                attacksToStun = MAX_HITS_TO_STUN;
            }
            
            AttackTypes a = hit.AttackType;
            if (a == AttackTypes.Nail || a == AttackTypes.Spell || a == AttackTypes.SharpShadow)
                stunCounter++;
            
            return hit;
        }
    }
}