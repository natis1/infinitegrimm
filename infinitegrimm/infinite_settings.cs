using Modding;

// ReSharper disable InconsistentNaming

namespace infinitegrimm
{
    public static class version_info
    {
        public const int SETTINGS_VER = 9;
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
            NightmareGodGrimm = false;
            NightmareGodGrimmDies = false;
            
            TimeAttackMode = false;
            OneHitMode = false;
            
            
            StartingDanceSpeedMultiplier = 0.8f;
            MaximumDanceSpeed = 3.0f;
            DamageToIncreaseDanceSpeedByOne = 5000.0f;
            DamageToIncreaseStaggerHitsByOne = 300;
            StartingHitsToStagger = 8;

            modernHardRandomSpikesDmg = 1500;
            modernHardNGGSpikesDmg = 7000;
            modernHardDeathWallDmg = 4000;
            modernHardSanicDmg = 9000;
            // Six minutes, slim shady.
            TimeAttackTime = 60 * 6;
            
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
        
        public bool NightmareGodGrimm { get => GetBool();
            private set => SetBool(value); }
        
        public bool NightmareGodGrimmDies { get => GetBool();
            private set => SetBool(value); }
        
        public bool TimeAttackMode { get => GetBool();
            private set => SetBool(value); }
        
        public bool OneHitMode { get => GetBool();
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
        
        public int modernHardRandomSpikesDmg { get => GetInt();
            private set => SetInt(value); }
        
        public int modernHardNGGSpikesDmg { get => GetInt();
            private set => SetInt(value); }
        
        public int modernHardDeathWallDmg { get => GetInt();
            private set => SetInt(value); }
        
        public int modernHardSanicDmg { get => GetInt();
            private set => SetInt(value); }
        
        public int TimeAttackTime { get => GetInt();
            private set => SetInt(value); }
        
    }


    public class InfiniteSettings : IModSettings
    {
        public int IGDamageHighScore { get => GetInt(); set => SetInt(value); }
    }

    

}
