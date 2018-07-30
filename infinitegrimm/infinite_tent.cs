﻿using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;


// This adds Grimm back to the Grimm_Main_Tent level
// After you kill the nightmare grimm of course.
//
// Also gets you grimmchild so you can enter the fight without the charm

namespace infinitegrimm
{
    internal class infinite_tent : MonoBehaviour
    {
        private static bool deletGrimmChild;
        private static int updatewait;
        public static int damageDone;

        public bool enterTent;

        private string trueFromName;

        public static GameObject grimmchild;

        public GameObject grimm;

        public PlayMakerFSM interactions;
        public bool didReturn;
        
        private const float DEFAULT_STARTING_DANCE_SPD = 0.8f;
        private const float DEFAULT_MAX_DANCE_SPD = 3.0f;
        private const float DEFAULT_DANCE_SPD_INC_DMG = 5000.0f;
        private const int DEFAULT_STAGGER_INCREASE_DMG = 300;
        private const int DEFAULT_STARTING_STAGGER_HIT = 8;

        private const int DEFAULT_SPIKE_RANDOM_DMG = 1500;
        private const int DEFAULT_SPIKE_NGG_DMG = 7000;
        private const int DEFAULT_DEATH_WALL_DMG = 4000;
        private const int DEFAULT_SANIC_DMG = 9000;
        
        // This code inspired by Randomizer Mod 2.0
        private static readonly Dictionary<string, Dictionary<string, string>> LANG_STRINGS = 
            new Dictionary<string, Dictionary<string, string>>();

        private static string languageHooks(string smallKey, string key)
        {
            if (LANG_STRINGS.ContainsKey(key) && LANG_STRINGS[key].ContainsKey(smallKey))
            {
                return LANG_STRINGS[key][smallKey];
            }
            return Language.Language.GetInternal(smallKey, key);
        }
        

        public void Start()
        {
            // If damage done remains at -1 after NKG fight it means you are doing the
            // normal nkg fight and not the infinite one, so don't spawn the damage done dialog
            damageDone = -1;
            deletGrimmChild = false;
            enterTent = false;
            
            ModHooks.Instance.LanguageGetHook += languageHooks;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += loadGrimm;
            ModHooks.Instance.GetPlayerBoolHook += fakeGrimmchild;
        }

        public void OnDestroy()
        {
            ModHooks.Instance.LanguageGetHook -= languageHooks;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= loadGrimm;
            ModHooks.Instance.GetPlayerBoolHook -= fakeGrimmchild;

            infinite_globals.log("Unloaded Tent!");
        }

        private bool fakeGrimmchild(string originalSet)
        {
            if (enterTent && originalSet == "equippedCharm_40" && PlayerData.instance.killedNightmareGrimm)
            {
                return true;
            }

            return PlayerData.instance.GetBoolInternal(originalSet);

        }

        private void setupGrimm()
        {
            grimm = GameObject.Find("Grimm Holder");
            grimm.SetActive(true);
            FsmState f = FSMUtility.LocateFSM(grimm, "Chest Control").getState("Init");
            f.clearTransitions();
            // hack it so grimm always appears... dead and alive.
            f.addTransition("FINISHED", "Killed");

            FsmState dead = FSMUtility.LocateFSM(grimm, "Chest Control").getState("Killed");
            dead.addTransition("FINISHED", "Appear");
            GameObject grimmScene = grimm.FindGameObjectInChildren("Grimm Scene");
            interactions = FSMUtility.LocateFSM(grimmScene, "Initial Scene");

            FsmState initAppear = interactions.getState("Init");
            initAppear.clearTransitions();
            initAppear.addTransition("FINISHED", "Meet Ready");
            if (infinite_globals.hardmode && !didReturn)
            {
                FsmState meeting = interactions.getState("Meet Ready");
                meeting.clearTransitions();
                meeting.addTransition("ENTER", "Grimm Appear");

                FsmState appear = interactions.getState("Grimm Appear");
                appear.removeActionsOfType<SendEventByName>();
                appear.clearTransitions();
                appear.addTransition("FINISHED", "Tele Out Anim");
                Wait[] skipwait = appear.getActionsOfType<Wait>();
                foreach (Wait t in skipwait)
                {
                    t.time = (float)0.5;
                }

                // This should in theory fix storage
                FsmState poof = interactions.getState("Tele Poof");
                poof.clearTransitions();
            }
            else
            {

                // skip the long cutscene
                FsmState fastEnter = interactions.getState("Take Control");

                fastEnter.clearTransitions();

                fastEnter.addTransition("LAND", "Grimm Appear");

                FsmState fastEnter2 = interactions.getState("Grimm Appear");
                fastEnter2.removeActionsOfType<Wait>();
                SendEventByName[] skipwait = fastEnter2.getActionsOfType<SendEventByName>();
                foreach (SendEventByName t in skipwait)
                {
                    if (t.delay.Value > 0.1)
                    {
                        t.delay.Value = (float)0.3;
                    }
                }
                FsmState greet = interactions.getState("Meet 1");
                greet.clearTransitions();
                greet.addTransition("CONVO_FINISH", "Box Down 3");

                FsmState boxDown = interactions.getState("Box Down 3");

                boxDown.clearTransitions();
                boxDown.addTransition("FINISHED", "Tele Out Anim");

                

            }
            FsmState endState = interactions.getState("End");
            endState.addTransition("FINISHED", "Check");

        }

        private void loadGrimm(Scene from, Scene to)
        {
            // Yikes, talk about hacky. Reload the scene since this hook applies after the scene has already loaded in a bad way.
            // Basically because of a race condition or something, the object we want to protect is already gone
            // by the time this function is called.
            if (to.name == "Grimm_Main_Tent" && !enterTent)
            {
                trueFromName = from.name;
                enterTent = true;
                UnityEngine.SceneManagement.SceneManager.LoadScene(to.name);
            } else if (to.name != "Grimm_Main_Tent")
            {
                trueFromName = "";
                enterTent = false;
            } else if (to.name == "Grimm_Main_Tent" && trueFromName == "Town")
            {
                if (!PlayerData.instance.killedNightmareGrimm) return;
                
                didReturn = false;
                
                // Setup conversation text.
                LANG_STRINGS["Titles"] = new Dictionary<string, string>();
                if (infinite_globals.godMode)
                {
                    // Because there's two of them... get it? heh.. real funny of me
                    LANG_STRINGS["Titles"]["NIGHTMARE_GRIMM_MAIN"] = "Grimms";
                    LANG_STRINGS["Titles"]["NIGHTMARE_GRIMM_SUPER"] = "Infinite Nightmare God";
                }
                else
                {
                    string nightmareGod = infinite_globals.timeAttackMode ? "Finite" : "Infinite";
                    if (infinite_globals.oneHitMode)
                    {
                        nightmareGod = "One Hit " + nightmareGod;
                    }

                    if (infinite_globals.hardmode)
                    {
                        nightmareGod += " Nightmare King";
                    }

                    LANG_STRINGS["Titles"]["NIGHTMARE_GRIMM_SUPER"] = nightmareGod;
                }
                
                
                System.Random rnd = new System.Random();
                int convoNumber = rnd.Next(1, 6);
                LANG_STRINGS["CP2"] = new Dictionary<string, string>();
                
                switch (convoNumber)
                {
                    case 1:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "Hello again, my friend. You put on quite the show for " +
                                                            "the crowd. How elegant the dance of fire and void must " +
                                                            "appear. I'll be sleeping to the right, go there and " +
                                                            "demonstrate your power!";
                        break;
                    case 2:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "Welcome, my friend. Nothing but misery lies in " +
                                                            "Hallownest, so why not join me instead in an elegant " +
                                                            "dance? I'll be sleeping to the right, go there and " +
                                                            "demonstrate your power!";
                        break;
                    case 3:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "Greetings, my favorite vessel. How the crowd adores your" +
                                                            " incredible fighting skills. If you're interested in " +
                                                            "demonstrating them then I shall be sleeping to the right.";
                        break;
                    case 4:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "If it isn't the greatest nail wielder in Hallownest. " +
                                                            "Why fight for a ritual when we can fight for the " +
                                                            "spectacle, and the geo the performance brings. I shall " +
                                                            "be sleeping to the right.";
                        break;
                    case 5:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "Welcome back, my friend. Last time we fought you " +
                                                            "impressed the crowd and I alike. The whole world must " +
                                                            "adore your power. I'll be sleeping to the right if you " +
                                                            "wish to demonstrate it.";
                        break;
                    case 6:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "How wonderful seeing you again, young knight. Your " +
                                                            "mastery of soul and nail are impressive. Dance with me, " +
                                                            "my friend, and we shall show the world the power " +
                                                            "of fire and void.";
                        break;
                    default:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "You have my permission to stop cheating now, " +
                                                            "little knight";
                        break;
                }

                if (infinite_globals.godMode)
                {
                    
                    switch (convoNumber)
                    {
                    case 1:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "Pitiful creature of the Void, abandon all hope, for true hell awaits " +
                                                            "you at the end of the tunnel. This is what KDTBOT told me to " +
                                                            "tell you... And the machine is absolutely correct.";
                        break;
                    case 2:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "My friend, this is no place for you. For you have stepped into " +
                                                            "a show where you will be made the fool. Continue and you will " +
                                                            "regret turning on god mode.";
                        break;
                    case 3:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "Greetings, my favorite vessel. If you wish to survive, I would " +
                                                            "strongly urge you to turn around and forget this tent exists. " +
                                                            "If you wish to fight me, come back when KDTBOT is gone.";
                        break;
                    case 4:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "Little vessel. How could you have brought KDTBOT into this tent? " +
                                                            "That machine intends to kill you in front of the crowd. " +
                                                            "It's a bad way to die.";
                        break;
                    case 5:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "There is no glory to fighting a God, vessel. Only the pain " +
                                                            "of losing and looking the fool. Come back when Grimm, and " +
                                                            "not KDTBOT, is leading the show.";
                        break;
                    case 6:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "This next act wasn't made by me, but by KDTBOT. KDTBOT's " +
                                                            "maker wants you dead. I strongly urge you sit this one out. " +
                                                            "While you still can.";
                        break;
                    default:
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "You have my permission to stop cheating now, " +
                                                            "little knight";
                        break;
                    }
                    
                }

                if (!PlayerData.instance.GetBoolInternal("equippedCharm_40"))
                    deletGrimmChild = true;

                updatewait = 40;

                setupGrimm();
                infinite_globals.log("Loaded Grimm Tent without error");
            } else if (to.name == "Grimm_Main_Tent" && trueFromName == "Grimm_Nightmare" && damageDone != -1)
            {
                didReturn = true;

                LANG_STRINGS["CP2"] = new Dictionary<string, string>();
                
                string moddedGrimm = basicAntiCheat().ToString();
                if (infinite_globals.timeAttackMode && moddedGrimm != "=")
                {
                    moddedGrimm += " " + time_attack.getTimeInCleanFormat((float) infinite_globals.secondsToRun);
                } else if (infinite_globals.timeAttackMode)
                {
                    moddedGrimm = time_attack.getTimeInCleanFormat((float) infinite_globals.secondsToRun);
                }

                if (infinite_globals.oneHitMode && moddedGrimm != "=")
                {
                    moddedGrimm += " one hit";
                }
                else if (infinite_globals.oneHitMode)
                {
                    moddedGrimm = "one hit";
                }
                
                string append;
                if (moddedGrimm == "=" && infinite_globals.hardmode)
                {
                    append = " (hard)";
                } else if (moddedGrimm == "=")
                {
                    append = "";
                }
                else if (infinite_globals.hardmode)
                {
                    append = " (hard " + moddedGrimm + ")";
                }
                else
                {
                    append = " (" + moddedGrimm + ")";
                }
                
                if (damageDone == 0)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nI cannot dance with you without your help. " +
                                                        "You did not do any damage." + append;
                }
                else if (damageDone <= 500)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nYou only did " + damageDone + " damage. I know you are " +
                                                        "capible of better, my friend." + append;
                } else if (damageDone <= 1500)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nNot bad, my friend, you did " + damageDone + " damage. " +
                                                        "But I know you're stronger than this." + append;
                } else if (damageDone <= 3000)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nImpressive, little vessel, you did " + damageDone + " " +
                                                        "damage and put on quite the show." + append;
                } else if (damageDone <= 6000)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nA masterful performance. You did " + damageDone + " " +
                                                        "damage... The crowd adores you!" + append;
                } else if (damageDone <= 8000)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nSimply outstanding. You did an impressive " + damageDone + " " +
                                                        "damage. Your abilities are unmatched." + append;
                }
                else if (damageDone <= 10000)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nIncredible. You did an amazing " + damageDone + " " +
                                                        "damage. Your speed and talent are immense!" + append;
                } else if (damageDone <= 12000)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nMy friend, your talents are astonishing. You did " + damageDone + " " +
                                                        "damage and impressed the crowd and I alike!" + append;
                } else if (damageDone < 15000 && infinite_globals.hardmode)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nYou are a godlike being of extraordinary dexterity." +
                                                        " In that fight you did " + damageDone + " " +
                                                        "damage!" + append;
                }
                else if (damageDone < 15000)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nUnreal. You did a stupendous " + damageDone + " " +
                                                        "damage. The spectacle was extravagant!" + append;
                } else if (!infinite_globals.hardmode)
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nYou did " + damageDone + " damage. Am I too slow? Why " +
                                                        "not try enabling 'HardMode' and I will show you my full " +
                                                        "speed." + append;
                } else
                {
                    LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nUnbelievable, you did " + damageDone + " damage on Hard " +
                                                        "Mode! You are no mere vessel, but a god, and " +
                                                        "nothing can stand in your way!" + append;
                }

                if (infinite_globals.godMode)
                {
                    if (damageDone < 3200)
                    {
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nKDTBOT: Bzzt. " + damageDone + " damage done." +
                                                            " The gods require 3200 damage to be impressed." +
                                                            " Try again later.";
                    }
                    else
                    {
                        LANG_STRINGS["CP2"]["GRIMM_MEET1"] = "\n\nKDTBOT: Bzzt. " + damageDone + " damage done." +
                                                            " The gods are impressed with your skill. But not me" +
                                                            " ... unless you did it at 3x speed.";
                    }
                }

                
                

                if (!PlayerData.instance.GetBoolInternal("equippedCharm_40"))
                    deletGrimmChild = true;
                updatewait = 100;

                setupGrimm();

                infinite_globals.log("Finished loading tent from nightmare.");
            }
        }    
        
        // ok it's not that complex. Just says if your grimm was harder or easier than normal for
        // some stupid and easy to reverse engineer protection.
        private static char basicAntiCheat()
        {
            char c = '=';

            if (infinite_globals.maxDanceSpeed > DEFAULT_MAX_DANCE_SPD)
            {
                c = '+';
            }
            else if (infinite_globals.maxDanceSpeed < DEFAULT_MAX_DANCE_SPD)
            {
                return '-';
            }
            
            if (infinite_globals.startingDanceSpeed > DEFAULT_STARTING_DANCE_SPD)
            {
                c = '+';
            }
            else if (infinite_globals.startingDanceSpeed < DEFAULT_STARTING_DANCE_SPD)
            {
                return '-';
            }
            
            if (infinite_globals.danceSpeedIncreaseDmg < DEFAULT_DANCE_SPD_INC_DMG)
            {
                c = '+';
            }
            else if (infinite_globals.danceSpeedIncreaseDmg > DEFAULT_DANCE_SPD_INC_DMG)
            {
                return '-';
            }
            
            if (infinite_globals.staggerIncreaseDamage < DEFAULT_STAGGER_INCREASE_DMG)
            {
                c = '+';
            }
            else if (infinite_globals.staggerIncreaseDamage > DEFAULT_STAGGER_INCREASE_DMG)
            {
                return '-';
            }
            
            if (infinite_globals.startingStaggerHits > DEFAULT_STARTING_STAGGER_HIT)
            {
                c = '+';
            }
            else if (infinite_globals.startingStaggerHits < DEFAULT_STARTING_STAGGER_HIT)
            {
                return '-';
            }

            if (!infinite_globals.hardmode) return c;
            
            
            if (infinite_globals.difficultyIncreaseValues[0] < DEFAULT_SPIKE_RANDOM_DMG)
            {
                c = '+';
            }
            else if (infinite_globals.difficultyIncreaseValues[0] > DEFAULT_SPIKE_RANDOM_DMG)
            {
                return '-';
            }
                
            if (infinite_globals.difficultyIncreaseValues[1] < DEFAULT_SPIKE_NGG_DMG)
            {
                c = '+';
            }
            else if (infinite_globals.difficultyIncreaseValues[1] > DEFAULT_SPIKE_NGG_DMG)
            {
                return '-';
            }
                
            if (infinite_globals.difficultyIncreaseValues[2] < DEFAULT_DEATH_WALL_DMG)
            {
                c = '+';
            }
            else if (infinite_globals.difficultyIncreaseValues[2] > DEFAULT_DEATH_WALL_DMG)
            {
                return '-';
            }

            if (infinite_globals.difficultyIncreaseValues[3] < DEFAULT_SANIC_DMG)
            {
                c = '+';
            }
            else if (infinite_globals.difficultyIncreaseValues[3] > DEFAULT_SANIC_DMG)
            {
                return '-';
            }

            return c;


        }

        // Basically grimmchild doesn't spawn in right away
        // it spawns in after a little bit so you need to remove it after waiting.
        // So to avoid a race condition you need to wait before despawning it.
        public void Update()
        {
            // This is some equally sketchy code to copy grimmchild into an object and then hide the kid
            // if the player doesn't actually have them equipped (and really who would for this fight?)

            // The copied object is used if grimmchild is used in the actual infinite grimm fight.
            if (updatewait <= 0) return;
            
            updatewait--;
            
            if (updatewait > 0) return;
            GameObject grimmChild = GameObject.Find("Grimmchild(Clone)");
                    
            if (grimmChild != null && deletGrimmChild)
            {

                // Make grimmchild completely invisible. This actually works...
                tk2dSprite grimmSprite = grimmChild.GetComponent<tk2dSprite>();
                Color grimmColor = grimmSprite.color;
                grimmColor.a = 0;
                grimmSprite.color = grimmColor;

                FsmState grimmchildfollow = FSMUtility.LocateFSM(grimmChild, "Control").getState("Tele Start");
                grimmchildfollow.removeActionsOfType<AudioPlayerOneShotSingle>();
                grimmchildfollow.clearTransitions();
                FSMUtility.LocateFSM(grimmChild, "Control").SetState("Tele Start");

            } else if (!deletGrimmChild)
            {
                grimmchild = grimmChild;
            }

            if (didReturn)
            {
                interactions.SetState("Meet 1");
            }
            deletGrimmChild = false;

        }
    }
}
