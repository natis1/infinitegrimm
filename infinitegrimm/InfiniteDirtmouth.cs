using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModCommon;
using FsmUtil;
using Modding;
using RandomizerMod.Extensions;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using HutongGames.PlayMaker.Ecosystem;
using HutongGames.Utility;
using HutongGames.Extensions;
using MonoMod;
using System;



// This adds infinite grimm to the Grimm_Main_Tent level
// After you kill the nightmare grimm of course.
//
// that's basically it.

namespace infinitegrimm
{
    class InfiniteDirtmouth : MonoBehaviour
    {

        public bool inDirtmouth;
        public bool enterTent;

        public void Start()
        {
            Modding.Logger.Log("[Infinite Grimm] killed NKG? " + PlayerData.instance.killedNightmareGrimm + " killed grimm? " + PlayerData.instance.killedGrimm);
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += isdirtmouth;
            ModHooks.Instance.GetPlayerBoolHook += fakeNodefeatGrimm;
            //ModHooks.Instance.BeforeSceneLoadHook += Reset;
            Modding.Logger.Log("added hook to playerbool which now contains: ");
        }

        private void isdirtmouth(Scene from, Scene to)
        {
            if (to.name == "Town")
            {
                inDirtmouth = true;
                enterTent = false;
            } else if (to.name == "Grimm_Main_Tent")
            {
                inDirtmouth = false;
                enterTent = true;
            } else
            {
                inDirtmouth = false;
                enterTent = false;
            }
        }

        public bool fakeNodefeatGrimm(string originalSet)
        {
            if (originalSet == "defeatedNightmareGrimm" && PlayerData.instance.killedNightmareGrimm && inDirtmouth)
            {
                return false;
            } else if (originalSet == "troupeInTown" && PlayerData.instance.killedNightmareGrimm)
            {
                return true;
            } /*else if (originalSet == "killedGrimm" && PlayerData.instance.killedNightmareGrimm && enterTent)
            {
                return false;
            }*/

            return PlayerData.instance.GetBoolInternal(originalSet);
        }
        //Scene from, Scene to
        private string Reset(String scene)
        {
            if (PlayerData.instance.killedNightmareGrimm && PlayerData.instance.killedGrimm)
            {
                GameObject tents = GameObject.Find("grimm_tents");

                tents.SetActive(true);
                //first make it active

                // No really, the FSM is called FSM
                FsmState f = FSMUtility.LocateFSM(tents, "FSM").GetState("Check");
                f.RemoveActionsOfType<GetOwner>();
                f.RemoveActionsOfType<PlayerDataBoolTest>();
                f.ClearTransitions();
                // hack it so tents always appear after clearing it out.
                f.AddTransition("FINISHED", "True");

                FsmState f2 = FSMUtility.LocateFSM(tents, "FSM").GetState("True");
                f2.ClearTransitions();
                f2.AddTransition("FINISHED", "Active");

                Modding.Logger.Log("set tent properly");

                GameObject mainTent = tents.FindGameObjectInChildren("main_tent");
                mainTent.SetActive(true);
                FsmState nodestroy = FSMUtility.LocateFSM(mainTent, "FSM").GetState("Check");
                nodestroy.RemoveActionsOfType<PlayerDataBoolTest>();
                nodestroy.ClearTransitions();

                Modding.Logger.Log("Activated main tent");

                GameObject[] alldoors = GameObject.FindGameObjectsWithTag("TransitionGate");

                GameObject room = GameObject.Find("room_grimm");
                for (int i = 0; i < alldoors.Length; i++)
                {
                    Modding.Logger.Log("found door with name: " + alldoors[i].name);

                }
                
                Modding.Logger.Log("r1");
                room.SetActive(true);
                Modding.Logger.Log("r2");
                FsmState roomDoor = FSMUtility.LocateFSM(room, "Door Control").GetState("Can Enter?");
                Modding.Logger.Log("r3");
                roomDoor.ClearTransitions();
                Modding.Logger.Log("r4");
                roomDoor.AddTransition("FINISHED", "WP Door?");
                
                Modding.Logger.Log("Enabled door");
            }

            return scene;
        }

        public void OnDestroy()
        {
            ModHooks.Instance.GetPlayerBoolHook -= fakeNodefeatGrimm;
        }
    }
}
