using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

// This adds infinite grimm to the Grimm_Main_Tent level
// After you kill the nightmare grimm of course.
//
// that's basically it.

namespace infinitegrimm
{
    internal class infinite_dirtmouth : MonoBehaviour
    {
        public bool inDirtmouth;

        public void Start()
        {
            infinite_globals.log("Killed NKG? " + PlayerData.instance.killedNightmareGrimm + " killed grimm? " + PlayerData.instance.killedGrimm);
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
                infinite_globals.log("Unable to unload fake nodefeat grimm");
            }

            try {
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= isdirtmouth;
            }
            catch
            {
                infinite_globals.log("Unable to unload isDirtmouth");
            }

            infinite_globals.log("Unloaded Dirtmouth");
        }

        private void isdirtmouth(Scene from, Scene to)
        {
            if (to.name == "Town" && !inDirtmouth)
            {
                inDirtmouth = true;

                // Fixes a theoretical race condition, like the one in Infinite Tent
                // But it doesn't happen in practice for some reason.
                // Still worth fixing in case an update breaks it.
                // UnityEngine.SceneManagement.SceneManager.LoadScene(to.name);
            } else if (to.name != "Town")
            {
                inDirtmouth = false;
            }
        }

        private bool fakeNodefeatGrimm(string originalSet)
        {
            switch (originalSet)
            {
                case "defeatedNightmareGrimm" when PlayerData.instance.killedNightmareGrimm && inDirtmouth:
                case "killedNightmareGrimm" when PlayerData.instance.killedNightmareGrimm && inDirtmouth:
                    return false;
                case "troupeInTown" when PlayerData.instance.killedNightmareGrimm:
                    return true;
                default:
                    return PlayerData.instance.GetBoolInternal(originalSet);
            }
        }
    }
}
