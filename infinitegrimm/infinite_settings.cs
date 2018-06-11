using Modding;
// ReSharper disable InconsistentNaming

namespace infinitegrimm
{
    public static class version_info
    {
        public const int SETTINGS_VER = 2;
    }

    public class InfiniteGlobalSettings : IModSettings
    {
        

        public void Reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            HardMode = false;
            settingsVersion = version_info.SETTINGS_VER;
        }
        public int settingsVersion { get => GetInt();
            private set => SetInt(value); }

        public bool HardMode { get => GetBool();
            private set => SetBool(value); }
    }


    public class InfiniteSettings : IModSettings
    {
        public int IGDamageHighScore { get => GetInt(); set => SetInt(value); }
        public int IGGrimmTalkState { get => GetInt(); set => SetInt(value); }
        
    }

    

}
