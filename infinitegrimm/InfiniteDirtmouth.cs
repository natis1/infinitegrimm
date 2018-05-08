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
            }

            return PlayerData.instance.GetBoolInternal(originalSet);
        }

        public void OnDestroy()
        {
            ModHooks.Instance.GetPlayerBoolHook -= fakeNodefeatGrimm;
        }
    }
}
