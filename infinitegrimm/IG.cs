using Modding;

namespace infinitegrimm
{
    public class InfiniteGrimmMod : Mod
    {

        private static string version = "0.2.3";

        private bool startedIG;

        public override string GetVersion()
        {
            return version;
        }

        public override void Initialize()
        {
            startedIG = false;
            // just in case our mod bricks everything don't load it right away to give the
            // user time to disable it.
            ModHooks.Instance.AfterSavegameLoadHook += addToGame;
            ModHooks.Instance.NewGameHook += newGame;
        }

        public void newGame()
        {
            if (!startedIG)
            {
                GameManager.instance.gameObject.AddComponent<InfiniteDirtmouth>();
                GameManager.instance.gameObject.AddComponent<InfiniteTent>();
                GameManager.instance.gameObject.AddComponent<InfiniteGrimm>();
                Modding.Logger.Log("[Infinite Grimm] Please welcome Grimm to your world!");
            }
        }

        public void addToGame(SaveGameData data)
        {
            if (!startedIG)
            {
                GameManager.instance.gameObject.AddComponent<InfiniteDirtmouth>();
                GameManager.instance.gameObject.AddComponent<InfiniteTent>();
                GameManager.instance.gameObject.AddComponent<InfiniteGrimm>();
                Modding.Logger.Log("[Infinite Grimm] Please welcome Grimm to your world!");
            }
        }
        

    }
}
