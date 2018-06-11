using System.Collections.Generic;
using UnityEngine;
using ModCommon;
using Modding;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine.SceneManagement;


// This adds Grimm back to the Grimm_Main_Tent level
// After you kill the nightmare grimm of course.
//
// Also gets you grimmchild so you can enter the fight without the charm

namespace infinitegrimm
{
    internal class infinite_tent : MonoBehaviour
    {
        public static bool hardmode;

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

        // This code inspired by Randomizer Mod 2.0
        private static Dictionary<string, Dictionary<string, string>> langStrings;

        private static string languageHooks(string smallKey, string key)
        {
            if (langStrings.ContainsKey(key) && langStrings[key].ContainsKey(smallKey))
            {
                return langStrings[key][smallKey];
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
            langStrings = new Dictionary<string, Dictionary<string, string>>();

            ModHooks.Instance.LanguageGetHook += languageHooks;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += loadGrimm;
            ModHooks.Instance.GetPlayerBoolHook += fakeGrimmchild;
        }

        public void OnDestroy()
        {
            ModHooks.Instance.LanguageGetHook -= languageHooks;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= loadGrimm;
            ModHooks.Instance.GetPlayerBoolHook -= fakeGrimmchild;

            Modding.Logger.Log("[Infinite Grimm] Unloaded Tent!");
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
            if (hardmode && !didReturn)
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
                System.Random rnd = new System.Random();
                int convoNumber = rnd.Next(1, 6);
                langStrings["CP2"] = new Dictionary<string, string>();
                    
                switch (convoNumber)
                {
                    case 1:
                        langStrings["CP2"]["GRIMM_MEET1"] = "Hello again, my friend. You put on quite the show for" +
                                                            "the crowd. How elegant the dance of fire and void must" +
                                                            "appear. I'll be sleeping to the right, go there and" +
                                                            "demonstrate your power!";
                        break;
                    case 2:
                        langStrings["CP2"]["GRIMM_MEET1"] = "Welcome, my friend. Nothing but misery lies in " +
                                                            "Hallownest, so why not join me instead in an elegant " +
                                                            "dance? I'll be sleeping to the right, go there and " +
                                                            "demonstrate your power!";
                        break;
                    case 3:
                        langStrings["CP2"]["GRIMM_MEET1"] = "Greetings, my favorite vessel. How the crowd adores your" +
                                                            " incredible fighting skills. If you're interested in " +
                                                            "demonstrating them then I shall be sleeping to the right.";
                        break;
                    case 4:
                        langStrings["CP2"]["GRIMM_MEET1"] = "If it isn't the greatest nail wielder in Hallownest. " +
                                                            "Why fight for a ritual when we can fight for the " +
                                                            "spectacle, and the geo the performance brings. I shall " +
                                                            "be sleeping to the right.";
                        break;
                    case 5:
                        langStrings["CP2"]["GRIMM_MEET1"] = "Welcome back, my friend. Last time we fought you " +
                                                            "impressed the crowd and I alike. The whole world must " +
                                                            "adore your power. I'll be sleeping to the right if you " +
                                                            "wish to demonstrate it.";
                        break;
                    case 6:
                        langStrings["CP2"]["GRIMM_MEET1"] = "How wonderful seeing you again, young knight. Your " +
                                                            "mastery of soul and nail are impressive. Dance with me, " +
                                                            "my friend, and we shall show the world the power " +
                                                            "of fire and void.";
                        break;
                    default:
                        langStrings["CP2"]["GRIMM_MEET1"] = "You have my permission to stop cheating now," +
                                                            "little knight";
                        break;
                }

                if (!PlayerData.instance.GetBoolInternal("equippedCharm_40"))
                    deletGrimmChild = true;

                updatewait = 40;

                setupGrimm();
                Modding.Logger.Log("[Infinite Grimm] Loaded Grimm without error");
            } else if (to.name == "Grimm_Main_Tent" && trueFromName == "Grimm_Nightmare" && damageDone != -1)
            {
                didReturn = true;

                langStrings["CP2"] = new Dictionary<string, string>();
                
                char moddedGrimm = basicAntiCheat();
                string append = "";
                if (moddedGrimm == '=' && hardmode)
                {
                    append = " (hard)";
                } else if (hardmode)
                {
                    append = " (hard " + moddedGrimm + ")";
                }
                else
                {
                    append = " (" + moddedGrimm + ")";
                }
                
                if (damageDone == 0)
                {
                    langStrings["CP2"]["GRIMM_MEET1"] = "\n\nI cannot dance with you without your help. " +
                                                        "You did not do any damage." + append;
                }
                else if (damageDone <= 500)
                {
                    langStrings["CP2"]["GRIMM_MEET1"] = "\n\nYou only did " + damageDone + " damage. I know you are " +
                                                        "capible of better, my friend." + append;
                } else if (damageDone <= 1500)
                {
                    langStrings["CP2"]["GRIMM_MEET1"] = "\n\nNot bad, my friend, you did " + damageDone + " damage. " +
                                                        "But I know you're stronger than this." + append;
                } else if (damageDone <= 3000)
                {
                    langStrings["CP2"]["GRIMM_MEET1"] = "\n\nImpressive, little vessel, you did " + damageDone + " " +
                                                        "damage and put on quite the show." + append;
                } else if (damageDone <= 6000)
                {
                    langStrings["CP2"]["GRIMM_MEET1"] = "\n\nA masterful performance. You did " + damageDone + " " +
                                                        "damage... The crowd adores you!" + append;
                } else if (damageDone <= 15000)
                {
                    langStrings["CP2"]["GRIMM_MEET1"] = "\n\nIncredible. You did an astonishing " + damageDone + " " +
                                                        "damage. Your speed and talent are extraordinary!" + append;
                } else if (!hardmode)
                {
                    langStrings["CP2"]["GRIMM_MEET1"] = "\n\nYou did " + damageDone + " damage. Am I too slow? Why " +
                                                        "not try enabling 'HardMode' and I will show you my full " +
                                                        "speed." + append;
                } else
                {
                    langStrings["CP2"]["GRIMM_MEET1"] = "\n\nUnbelievable, you did " + damageDone + " damage on Hard " +
                                                        "Mode! You are no mere vessel, but a god, and " +
                                                        "nothing can stand in your way!" + append;
                }

                
                

                if (!PlayerData.instance.GetBoolInternal("equippedCharm_40"))
                    deletGrimmChild = true;
                updatewait = 100;

                setupGrimm();

                Modding.Logger.Log("[Infinite Grimm] Finished loading tent from nightmare.");
            }
        }    
        
        // ok it's not that complex. Just says if your grimm was harder or easier than normal for
        // some stupid and easy to reverse engineer protection.
        private static char basicAntiCheat()
        {
            char c = '=';

            if (infinite_grimm.maxDanceSpeed > DEFAULT_MAX_DANCE_SPD)
            {
                c = '>';
            }
            else if (infinite_grimm.maxDanceSpeed < DEFAULT_MAX_DANCE_SPD)
            {
                return '<';
            }
            
            if (infinite_grimm.startingDanceSpeed > DEFAULT_STARTING_DANCE_SPD)
            {
                c = '>';
            }
            else if (infinite_grimm.startingDanceSpeed < DEFAULT_STARTING_DANCE_SPD)
            {
                return '<';
            }
            
            if (infinite_grimm.danceSpeedIncreaseDmg < DEFAULT_DANCE_SPD_INC_DMG)
            {
                c = '>';
            }
            else if (infinite_grimm.danceSpeedIncreaseDmg > DEFAULT_DANCE_SPD_INC_DMG)
            {
                return '<';
            }
            
            if (infinite_grimm.staggerIncreaseDamage < DEFAULT_STAGGER_INCREASE_DMG)
            {
                c = '>';
            }
            else if (infinite_grimm.staggerIncreaseDamage > DEFAULT_STAGGER_INCREASE_DMG)
            {
                return '<';
            }
            
            if (infinite_grimm.startingStaggerHits > DEFAULT_STARTING_STAGGER_HIT)
            {
                c = '>';
            }
            else if (infinite_grimm.startingStaggerHits < DEFAULT_STARTING_STAGGER_HIT)
            {
                return '<';
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
