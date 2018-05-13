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

        public void OnDestroy()
        {
            try
            {
                ModHooks.Instance.GetPlayerBoolHook -= fakeNodefeatGrimm;
            }
            catch
            {
                Modding.Logger.Log("[Infinite Grimm] Unable to unload fake nodefeat grimm");
            }

            try {
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= isdirtmouth;
            }
            catch
            {
                Modding.Logger.Log("[Infinite Grimm] Unable to unload isDirtmouth");
            }

            Modding.Logger.Log("[Infinite Grimm] Unloaded Dirtmouth");
        }

        private void isdirtmouth(Scene from, Scene to)
        {
            if (to.name == "Town" && !inDirtmouth)
            {
                inDirtmouth = true;

                // Fixes a theoretical race condition, like the one in Infinite Tent
                // But it doesn't happen in practice for some reason.
                // Still worth fixing in case an update breaks it.
                UnityEngine.SceneManagement.SceneManager.LoadScene(to.name);
            } else if (to.name != "Town")
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
    }
}
