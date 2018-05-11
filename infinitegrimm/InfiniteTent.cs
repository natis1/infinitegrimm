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
            

            deletGrimmChild = false;
            enterTent = false;
            langStrings = new Dictionary<string, Dictionary<string, string>>();
            langStrings["GRIMM_MEET1"] = new Dictionary<string, string>();
            langStrings["GRIMM_MEET1"].Add("CP2", "Hello again, my friend. You put on quite the show for the crowd. How elegant the dance of fire and void must appear. I'll be sleeping to the right, go there and demonstrate your power!");
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
            }
            if (enterTent)
            {
                if (!PlayerData.instance.GetBoolInternal("equippedCharm_40"))
                    deletGrimmChild = true;
                updatewait = 50;
            }

            if (to.name == "Grimm_Main_Tent" && trueFromName == "Town")
            {
                Modding.Logger.Log("[Infinite Grimm] Loading Grimm in tent");
                grimm = GameObject.Find("Grimm Holder");
                if (PlayerData.instance.killedNightmareGrimm)
                {
                    grimm.SetActive(true);

                    //first make it active

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
                        meeting.AddTransition("ENTER", "Meet 1");
                    }

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
                            skipwait[i].delay.Value = (float) 0.3;
                        }
                    }
                    FsmState greet = interactions.GetState("Meet 1");
                    greet.ClearTransitions();
                    greet.AddTransition("CONVO_FINISH", "Box Down 3");

                    FsmState boxDown = interactions.GetState("Box Down 3");

                    boxDown.ClearTransitions();
                    boxDown.AddTransition("FINISHED", "Tele Out Anim");

                    FsmState endState = interactions.GetState("End");

                    endState.AddTransition("FINISHED", "Check");

                    Modding.Logger.Log("[Infinite Grimm] Loaded Grimm without error");

                    //todo remove unneeded animations here

                }
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
