using System.Collections.Generic;
using UnityEngine;
using ModCommon;
using Modding;
using RandomizerMod.Extensions;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine.SceneManagement;


// This adds infinite grimm to the Grimm_Main_Tent level
// After you kill the nightmare grimm of course.
//
// that's basically it.

namespace infinitegrimm
{
    class InfiniteTent : MonoBehaviour
    {
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
            langStrings = new Dictionary<string, Dictionary<string, string>>();
            langStrings["GRIMM_MEET1"] = new Dictionary<string, string>();
            langStrings["GRIMM_MEET1"].Add("CP2", "Hello again, my friend. You put on quite the show for the crowd. How elegant the dance of fire and void must appear. I'll be sleeping to the right, go there and demonstrate your power!");
            ModHooks.Instance.LanguageGetHook += LanguageHooks;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += loadGrimm;

            

        }

            private void loadGrimm(Scene from, Scene to)
        {
            if (to.name == "Grimm_Main_Tent" && from.name == "Town")
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

                    // skip the long cutscene
                    FsmState fastEnter = interactions.GetState("Take Control");
                    fastEnter.ClearTransitions();
                    fastEnter.AddTransition("LAND", "Grimm Appear");

                    FsmState fastEnter2 = interactions.GetState("Grimm Appear");
                    fastEnter2.RemoveActionsOfType<Wait>();
                    
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
    }
}
