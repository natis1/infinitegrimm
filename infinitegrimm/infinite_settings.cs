using Modding;

// ReSharper disable InconsistentNaming

namespace infinitegrimm
{
    public static class version_info
    {
        public const int SETTINGS_VER = 9;
    }

    public class InfiniteGlobalSettings
    {
        

        public void Reset()
        {
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
        public int settingsVersion= version_info.SETTINGS_VER;

        public bool HardMode = false;

        public bool ReduceLagInGrimmFight = false;

        public bool EvenMoreLagReduction = false;

        public bool NightmareGodGrimm = false;

        public bool NightmareGodGrimmDies = false;

        public bool TimeAttackMode = false;

        public bool OneHitMode = false;

        public float StartingDanceSpeedMultiplier = 0.8f;

        public float MaximumDanceSpeed = 3.0f;

        public float DamageToIncreaseDanceSpeedByOne = 5000.0f;

        public int DamageToIncreaseStaggerHitsByOne = 300;

        public int StartingHitsToStagger = 8;

        public int modernHardRandomSpikesDmg = 1500;

        public int modernHardNGGSpikesDmg = 7000;

        public int modernHardDeathWallDmg = 4000;

        public int modernHardSanicDmg = 9000;

        public int TimeAttackTime = 60 * 6;
        
    }


    public class InfiniteSettings
    {
        public int IGDamageHighScore = 0;
    }

    

}
