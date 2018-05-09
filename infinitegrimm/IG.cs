using Modding;

namespace infinitegrimm
{
    public class InfiniteGrimmMod : Mod
    {

        private static string version = "0.2.1";

        public override string GetVersion()
        {
            return version;
        }

        public override void Initialize()
        {
            ModHooks.Instance.AfterSavegameLoadHook += addToGame;
            ModHooks.Instance.NewGameHook += newGame;
        }

        public void newGame()
        {
            GameManager.instance.gameObject.AddComponent<InfiniteDirtmouth>();
            GameManager.instance.gameObject.AddComponent<InfiniteTent>();
            GameManager.instance.gameObject.AddComponent<InfiniteGrimm>();
            Modding.Logger.Log("[Infinite Grimm] Please welcome Grimm to your world!");
        }

        public void addToGame(SaveGameData data)
        {
            GameManager.instance.gameObject.AddComponent<InfiniteDirtmouth>();
            GameManager.instance.gameObject.AddComponent<InfiniteTent>();
            GameManager.instance.gameObject.AddComponent<InfiniteGrimm>();
            Modding.Logger.Log("[Infinite Grimm] Please welcome Grimm to your world!");
        }
        

    }
}
