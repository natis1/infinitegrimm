using System.Collections.Generic;
using UnityEngine;
using ModCommon;
using Modding;
using RandomizerMod.Extensions;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine.SceneManagement;


// This adds Grimm back to the Grimm_Main_Tent level
// After you kill the nightmare grimm of course.
//
// Also gets you grimmchild so you can enter the fight without the charm

namespace infinitegrimm
{
    class InfiniteTent : MonoBehaviour
    {
        public static bool hardmode;

        public static bool deletGrimmChild;
        public static int updatewait;
        public static int damageDone;

        public bool enterTent;

        string trueFromName;

        public static GameObject grimmchild;

        public GameObject grimm;

        // This code inspired by Randomizer Mod 2.0
        private static Dictionary<string, Dictionary<string, string>> langStrings;

        private string LanguageHooks(string key, string value)
        {
            if (langStrings.ContainsKey(key) && langStrings[key].ContainsKey(value))
            {
                return langStrings[key][value];
            }
            return Language.Language.GetInternal(key, value);
        }
        

        public void Start()
        {
            // If damage done remains at -1 after NKG fight it means you are doing the
            // normal nkg fight and not the infinite one, so don't spawn the damage done dialog.
            damageDone = -1;

            deletGrimmChild = false;
            enterTent = false;
            langStrings = new Dictionary<string, Dictionary<string, string>>();
            ModHooks.Instance.LanguageGetHook += LanguageHooks;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += loadGrimm;
            ModHooks.Instance.GetPlayerBoolHook += fakeGrimmchild;


        }

        private bool fakeGrimmchild(string originalSet)
        {
            if (enterTent && originalSet == "equippedCharm_40" && PlayerData.instance.killedNightmareGrimm)
            {
                return true;
            }

            return PlayerData.instance.GetBoolInternal(originalSet);

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
                Modding.Logger.Log("[Infinite Grimm] Loading Grimm in tent");
                grimm = GameObject.Find("Grimm Holder");
                if (PlayerData.instance.killedNightmareGrimm)
                {
                    grimm.SetActive(true);

                    // Setup conversation text.
                    System.Random rnd = new System.Random();
                    int convoNumber = rnd.Next(1, 6);
                    langStrings["GRIMM_MEET1"] = new Dictionary<string, string>();
                    
                    switch (convoNumber)
                    {
                        case 1:
                            langStrings["GRIMM_MEET1"].Add("CP2", "Hello again, my friend. You put on quite the show for the crowd. How elegant the dance of fire and void must appear. I'll be sleeping to the right, go there and demonstrate your power!");
                            break;
                        case 2:
                            langStrings["GRIMM_MEET1"].Add("CP2", "Welcome, my friend. Nothing but misery lies in Hallownest, so why not join me instead in an elegant dance? I'll be sleeping to the right, go there and demonstrate your power!");
                            break;
                        case 3:
                            langStrings["GRIMM_MEET1"].Add("CP2", "Greetings, my favorite vessel. How the crowd adores your incredible fighting skills. If you're interested in demonstrating them then I shall be sleeping to the right.");
                            break;
                        case 4:
                            langStrings["GRIMM_MEET1"].Add("CP2", "If it isn't the greatest nail wielder in Hallownest. Why fight for a ritual when we can fight for the spectacle, and the geo the performance brings. I shall be sleeping to the right.");
                            break;
                        case 5:
                            langStrings["GRIMM_MEET1"].Add("CP2", "Welcome back, my friend. Last time we fought you impressed the crowd and I alike. The whole world must adore your power. I'll be sleeping to the right if you wish to demonstrate it.");
                            break;
                        case 6:
                            langStrings["GRIMM_MEET1"].Add("CP2", "How wonderful seeing you again, young knight. Your mastery of soul and nail are impressive. Dance with me, my friend, and we shall show the world the power of fire and void.");
                            break;
                    }


                    FsmState f = FSMUtility.LocateFSM(grimm, "Chest Control").GetState("Init");
                    f.ClearTransitions();
                    // hack it so grimm always appears... dead and alive.
                    f.AddTransition("FINISHED", "Killed");

                    FsmState dead = FSMUtility.LocateFSM(grimm, "Chest Control").GetState("Killed");
                    dead.AddTransition("FINISHED", "Appear");

                    //then select the child to change dialog


                    // EN-CP2 for the second "dlc"
                    /* all events for grimm in order for the language files but not appearance:
                     * GRIMM_DEFEAT_2: Look here...
                     * GRIMM_RITUAL_COMPLETE: Across these lands...
                     * QUEEN_GRIMMCHILD_FULL: Your companion's eyes burn...
                     * GRIMM_DREAM: Masterful! Even a vessel...
                     * GRIMM_DEFEAT_1: Bravo, my friend. Hear...
                     * GRIMM_DEFEAT_3: Go out into the darkness..
                     * GRIMM_BATTLE_1: Wonderful. Wonderful! My kin...
                     * GRIMM_BATTLE_2: This searing fire... It carries...
                     * GRIMM_BATTLE_3: Dance with me, my friend...
                     * GRIMM_ACCEPT: As the lantern flared your...
                     * GRIMM_LEVELUP_1: I can feel it. The warmth..
                     * GRIMM_LEVELUP_2: A masterful opening act...
                     * GRIMM_LEVELUP_3: Beautiful, yes. The child has...
                     * GRIMM_MEET1: So, it was you who called us.
                     * GRIMM_MEET2: Well met, my friend. Well met...
                     * 
                     */

                    GameObject grimmScene = grimm.FindGameObjectInChildren("Grimm Scene");
                    PlayMakerFSM interactions = FSMUtility.LocateFSM(grimmScene, "Initial Scene");

                    FsmState initAppear = interactions.GetState("Init");
                    initAppear.ClearTransitions();
                    initAppear.AddTransition("FINISHED", "Meet Ready");


                    // skip the pausing part on hardmode to save time
                    if (hardmode)
                    {
                        FsmState meeting = interactions.GetState("Meet Ready");
                        meeting.ClearTransitions();
                        meeting.AddTransition("ENTER", "Grimm Appear");

                        FsmState appear = interactions.GetState("Grimm Appear");
                        appear.RemoveActionsOfType<Wait>();
                        appear.ClearTransitions();
                        appear.AddTransition("FINISHED", "Tele Out Anim");
                    }
                    else
                    {

                        // skip the long cutscene
                        FsmState fastEnter = interactions.GetState("Take Control");

                        fastEnter.ClearTransitions();

                        fastEnter.AddTransition("LAND", "Grimm Appear");

                        FsmState fastEnter2 = interactions.GetState("Grimm Appear");
                        fastEnter2.RemoveActionsOfType<Wait>();
                        SendEventByName[] skipwait = fastEnter2.GetActionsOfType<SendEventByName>();
                        for (int i = 0; i < skipwait.Length; i++)
                        {
                            if (skipwait[i].delay.Value > 0.1)
                            {
                                skipwait[i].delay.Value = (float)0.3;
                            }
                        }
                        FsmState greet = interactions.GetState("Meet 1");
                        greet.ClearTransitions();
                        greet.AddTransition("CONVO_FINISH", "Box Down 3");

                        FsmState boxDown = interactions.GetState("Box Down 3");

                        boxDown.ClearTransitions();
                        boxDown.AddTransition("FINISHED", "Tele Out Anim");

                        if (!PlayerData.instance.GetBoolInternal("equippedCharm_40"))
                            deletGrimmChild = true;
                        updatewait = 50;
                    }
                    FsmState endState = interactions.GetState("End");
                    endState.AddTransition("FINISHED", "Check");

                    Modding.Logger.Log("[Infinite Grimm] Loaded Grimm without error");

                    //todo remove unneeded animations here

                }
            } else if (to.name == "Grimm_Main_Tent" && trueFromName == "Grimm_Nightmare" && damageDone != -1)
            {
                Modding.Logger.Log("[Infinite Grimm] Loading tent from nightmare.");
                grimm = GameObject.Find("Grimm Holder");
                grimm.SetActive(true);

                FsmState f = FSMUtility.LocateFSM(grimm, "Chest Control").GetState("Init");
                f.ClearTransitions();
                // hack it so grimm always appears... dead and alive.
                f.AddTransition("FINISHED", "Killed");


                FsmState dead = FSMUtility.LocateFSM(grimm, "Chest Control").GetState("Killed");
                dead.AddTransition("FINISHED", "Appear");

                GameObject grimmScene = grimm.FindGameObjectInChildren("Grimm Scene");
                PlayMakerFSM interactions = FSMUtility.LocateFSM(grimmScene, "Initial Scene");

                FsmState initAppear = interactions.GetState("Init");
                initAppear.ClearTransitions();
                Wait addDelay = new Wait();

                // I'd love to make this 0, but it needs to be longer than however long it takes
                // for the scene to load or you get a white screen, so I have to set it to a number high enough
                // that this works on crappier computers. This is a dumb hack.
                addDelay.time = 4f;
                initAppear.AddAction(addDelay);
                initAppear.AddTransition("FINISHED", "Meet 1");
                

                FsmState greet = interactions.GetState("Meet 1");
                greet.ClearTransitions();
                greet.AddTransition("CONVO_FINISH", "Box Down 3");

                FsmState boxDown = interactions.GetState("Box Down 3");
                boxDown.ClearTransitions();
                boxDown.AddTransition("FINISHED", "End");

                langStrings["GRIMM_MEET1"] = new Dictionary<string, string>();

                if (damageDone <= 500)
                {
                    langStrings["GRIMM_MEET1"].Add("CP2", "\n\nYou only did " + damageDone + " damage. I know you are capible of better, my friend.");
                } else if (damageDone <= 1500)
                {
                    langStrings["GRIMM_MEET1"].Add("CP2", "\n\nNot bad, my friend, you did " + damageDone + " damage. But I know you're stronger than this.");
                } else if (damageDone <= 3000)
                {
                    langStrings["GRIMM_MEET1"].Add("CP2", "\n\nImpressive, little vessel, you did " + damageDone + " damage and put on quite the show.");
                } else if (damageDone <= 6000)
                {
                    langStrings["GRIMM_MEET1"].Add("CP2", "\n\nA masterful performance. You did " + damageDone + " damage... The crowd adores you!");
                } else if (damageDone <= 15000)
                {
                    langStrings["GRIMM_MEET1"].Add("CP2", "\n\nIncredible. You did an astonishing " + damageDone + " damage. Your speed and talent are extraordinary!");
                } else if (!hardmode)
                {
                    langStrings["GRIMM_MEET1"].Add("CP2", "\n\nYou did " + damageDone + " damage. Am I too slow? Why not try enabling 'HardMode' and I will show you my full speed.");
                } else
                {
                    langStrings["GRIMM_MEET1"].Add("CP2", "\n\nUnbelievable, you did " + damageDone + " damage on Hard Mode! You are no mere vessel, but a god, and nothing can stand in your way!");
                }
                interactions.SetState("Init");

                if (!PlayerData.instance.GetBoolInternal("equippedCharm_40"))
                    deletGrimmChild = true;
                updatewait = 50;

                Modding.Logger.Log("[Infinite Grimm] Finished loading tent from nightmare.");
            }

            
        }

        // Basically grimmchild doesn't spawn in right away
        // it spawns in after a little bit so you need to remove it after waiting.
        // So to avoid a race condition you need to wait before despawning it.
        public void Update()
        {
            // This is some equally sketchy code to copy grimmchild into an object and then hide the kid
            // if the player doesn't actually have them equipped (and really who would for this fight?)

            // The copied object is used if grimmchild is used in the actual infinite grimm fight.
            if (updatewait > 0)
            {
                updatewait--;
                if (updatewait <= 0)
                {
                    
                    GameObject grimmChild = GameObject.Find("Grimmchild(Clone)");
                    
                    if (grimmChild != null && deletGrimmChild)
                    {

                        // Make grimmchild completely invisible. This actually works...
                        tk2dSprite grimmSprite = grimmChild.GetComponent<tk2dSprite>();
                        Color grimmColor = grimmSprite.color;
                        grimmColor.a = 0;
                        grimmSprite.color = grimmColor;

                        FsmState grimmchildfollow = FSMUtility.LocateFSM(grimmChild, "Control").GetState("Tele Start");
                        grimmchildfollow.RemoveActionsOfType<AudioPlayerOneShotSingle>();
                        grimmchildfollow.ClearTransitions();
                        FSMUtility.LocateFSM(grimmChild, "Control").SetState("Tele Start");

                    } else if (!deletGrimmChild)
                    {
                        grimmchild = grimmChild;
                    }
                    deletGrimmChild = false;
                }
            }

        }
    }
}
