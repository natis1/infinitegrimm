using Modding;

namespace grimmchildtest
{
    public class GCtest : Mod
    {
        public override string GetVersion()
        {
            return "1";
        }

        public override void Initialize()
        {
            ModHooks.Instance.GetPlayerBoolHook += fakeNodefeatGrimm;
        }

        public bool fakeNodefeatGrimm(string originalSet)
        {
            if (originalSet == "defeatedNightmareGrimm")
            {
                return false;
            }
            else if (originalSet == "troupeInTown")
            {
                return true;
            }
            else if (originalSet == "equippedCharm_40")
            {
                return true;
            }
            return PlayerData.instance.GetBoolInternal(originalSet);
        }
    }
}
