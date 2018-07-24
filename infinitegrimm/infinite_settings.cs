using Modding;
// ReSharper disable InconsistentNaming

namespace infinitegrimm
{
    public static class version_info
    {
        public const int SETTINGS_VER = 5;
    }

    public class InfiniteGlobalSettings : IModSettings
    {
        

        public void Reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            HardMode = false;
            ReduceLagInGrimmFight = false;
            EvenMoreLagReduction = false;
            ClassicMode = false;
            NightmareGodGrimm = false;
            
            StartingDanceSpeedMultiplier = 0.8f;
            MaximumDanceSpeed = 3.0f;
            DamageToIncreaseDanceSpeedByOne = 5000.0f;
            DamageToIncreaseStaggerHitsByOne = 300;
            StartingHitsToStagger = 8;
            
            settingsVersion = version_info.SETTINGS_VER;
        }
        public int settingsVersion { get => GetInt();
            private set => SetInt(value); }

        public bool HardMode { get => GetBool();
            private set => SetBool(value); }

        public bool ReduceLagInGrimmFight { get => GetBool();
            private set => SetBool(value); }
        
        public bool EvenMoreLagReduction { get => GetBool();
            private set => SetBool(value); }
        
        public bool ClassicMode { get => GetBool();
            private set => SetBool(value); }
        
        public bool NightmareGodGrimm { get => GetBool();
            private set => SetBool(value); }
        
        public float StartingDanceSpeedMultiplier { get => GetFloat();
            private set => SetFloat(value); }
        
        public float MaximumDanceSpeed { get => GetFloat();
            private set => SetFloat(value); }
        
        public float DamageToIncreaseDanceSpeedByOne { get => GetFloat();
            private set => SetFloat(value); }
        
        public int DamageToIncreaseStaggerHitsByOne { get => GetInt();
            private set => SetInt(value); }
        
        public int StartingHitsToStagger { get => GetInt();
            private set => SetInt(value); }
        
    }


    public class InfiniteSettings : IModSettings
    {
        public int IGDamageHighScore { get => GetInt(); set => SetInt(value); }
        public int IGGrimmTalkState { get => GetInt(); set => SetInt(value); }
        
    }

    

}
