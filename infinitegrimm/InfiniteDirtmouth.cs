using UnityEngine;
using UnityEngine.SceneManagement;
using Modding;



// This adds infinite grimm to the Grimm_Main_Tent level
// After you kill the nightmare grimm of course.
//
// that's basically it.

namespace infinitegrimm
{
    class InfiniteDirtmouth : MonoBehaviour
    {

        public bool inDirtmouth;

        public void Start()
        {
            Modding.Logger.Log("[Infinite Grimm] killed NKG? " + PlayerData.instance.killedNightmareGrimm + " killed grimm? " + PlayerData.instance.killedGrimm);
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += isdirtmouth;
            ModHooks.Instance.GetPlayerBoolHook += fakeNodefeatGrimm;
            
        }

        private void isdirtmouth(Scene from, Scene to)
        {
            Modding.Logger.Log("[Infinite Grimm] from " + from.name + " to " + to.name);
            if (to.name == "Town")
            {
                inDirtmouth = true;
            } else
            {
                inDirtmouth = false;
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
