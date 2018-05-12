using Modding;

namespace infinitegrimm
{
    public class VersionInfo
    {
        readonly public static int SettingsVer = 2;
    }

    public class InfiniteGlobalSettings : IModSettings
    {
        

        public void Reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            HardMode = false;
            SettingsVersion = VersionInfo.SettingsVer;
        }
        public int SettingsVersion { get => GetInt(); set => SetInt(value); }
        public bool HardMode { get => GetBool(); set => SetBool(value); }
    }


    public class InfiniteSettings : IModSettings
    {
        public int IGDamageHighScore { get => GetInt(); set => SetInt(value); }
        public int IGGrimmTalkState { get => GetInt(); set => SetInt(value); }
        
    }

    

}
