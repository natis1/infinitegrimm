using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace infinitegrimm
{
    // ReSharper disable once InconsistentNaming because modding api.
    // ReSharper disable once UnusedMember.Global
    public class InfiniteGrimm : Mod,IGlobalSettings<InfiniteGlobalSettings>,ILocalSettings<InfiniteSettings>
    {
        private bool grimmchildupgrades;

        // Version detection code originally by Seanpr, used with permission.
        public override string GetVersion()
        {
            string ver = infinite_globals.VERSION;
            const int minApi = 40;

            bool apiTooLow = Convert.ToInt32(ModHooks.ModVersion.Split('-')[1]) < minApi;
            bool noSatchel = !hasAssembly("Satchel");
            if (grimmchildupgrades) ver += " + Gc U";
            if (apiTooLow) ver += " (Error: ModAPI too old)";
            if (noSatchel) ver += " (Error: Infinite Grimm requires Satchel)";
            Log("For debugging purposes, version is " + ver);

            return ver;
        }
        InfiniteGlobalSettings GlobalSettings = new();
        InfiniteSettings Settings = new();
        public void OnLoadGlobal(InfiniteGlobalSettings s) => GlobalSettings = s;
        public InfiniteGlobalSettings OnSaveGlobal() => GlobalSettings;
        public void OnLoadLocal(InfiniteSettings s)=>Settings = s;
        public InfiniteSettings OnSaveLocal() => Settings;
        public override void Initialize()
        {

            setupSettings();

            grimmchildupgrades = hasAssembly("GrimmchildUpgrades");
            if (grimmchildupgrades)
            {
                infinite_globals.log("Grimmchild, you're looking powerful as ever!");
            }
            infinite_globals.hardmode = GlobalSettings.HardMode;
            infinite_globals.noLagMode = GlobalSettings.ReduceLagInGrimmFight;
            infinite_globals.noLagMode2 = GlobalSettings.EvenMoreLagReduction;
            infinite_globals.danceSpeedIncreaseDmg = GlobalSettings.DamageToIncreaseDanceSpeedByOne;
            infinite_globals.maxDanceSpeed = GlobalSettings.MaximumDanceSpeed;
            infinite_globals.startingDanceSpeed = GlobalSettings.StartingDanceSpeedMultiplier;
            infinite_globals.staggerIncreaseDamage = GlobalSettings.DamageToIncreaseStaggerHitsByOne;
            infinite_globals.startingStaggerHits = GlobalSettings.StartingHitsToStagger;

            infinite_globals.godMode = GlobalSettings.NightmareGodGrimm;
            infinite_globals.hardmode = GlobalSettings.HardMode;

            infinite_globals.oneHitMode = GlobalSettings.OneHitMode;
            infinite_globals.timeAttackMode = GlobalSettings.TimeAttackMode;
            infinite_globals.difficultyIncreaseValues = new[]
            {
                GlobalSettings.modernHardRandomSpikesDmg, GlobalSettings.modernHardNGGSpikesDmg,
                GlobalSettings.modernHardDeathWallDmg, GlobalSettings.modernHardSanicDmg
            };
            
            infinite_globals.secondsToRun = GlobalSettings.TimeAttackTime;

            infinite_globals.nggDies = GlobalSettings.NightmareGodGrimmDies;
            
            ModHooks.AfterSavegameLoadHook += addToGame;
            ModHooks.NewGameHook += newGame;
            ModHooks.ApplicationQuitHook += SaveGlobalSettings;
            ModHooks.SavegameSaveHook += saveLocalData;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += resetModSaveData;


        }

        // quick hack to fix problem in modding api... no seriously.
        // local data doesn't reset properly.
        private void resetModSaveData(Scene arg0, Scene arg1)
        {
            if (arg1.name != "Menu_Title") return;
            Settings.IGDamageHighScore = 0;
        }

        private void newGame()
        {
            Log("Current damage record for this file is: " + Settings.IGDamageHighScore);
            infinite_globals.maximumDamage = Settings.IGDamageHighScore;
            GameManager.instance.gameObject.AddComponent<infinite_dirtmouth>();
            GameManager.instance.gameObject.AddComponent<infinite_tent>();
            if (!GlobalSettings.NightmareGodGrimm)
            {
                GameManager.instance.gameObject.AddComponent<infinite_grimm_modern>();
                Log("Please welcome Modern Grimm to your world!");
            }
            else
            {
                GameManager.instance.gameObject.AddComponent<infinite_NGG>();
                Log("Please welcome the Grimm Gods to your world..." +
                                   "Beware. They want your blood.");
            }

        }

        private static bool hasAssembly(string assemblyNamespaceName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    if (assembly.GetTypes().Any(type => type.Namespace == assemblyNamespaceName))
                    {
                        return true;
                    }
                }
                catch
                {
                    infinite_globals.log("You have a broken assembly named '" + assembly.FullName + "' You should probably remove it.");
                }
            }

            return false;
        }

        private void addToGame(SaveGameData data)
        {
            newGame();
        }

        private void saveLocalData(int saveId)
        {
            Settings.IGDamageHighScore = infinite_globals.maximumDamage;
        }

        private void setupSettings()
        {
            string settingsFilePath = Application.persistentDataPath + "/" + GetType().Name + ".GlobalSettings.json";

            bool forceReloadGlobalSettings = (GlobalSettings != null && GlobalSettings.settingsVersion != version_info.SETTINGS_VER);

            if (forceReloadGlobalSettings || !File.Exists(settingsFilePath))
            {
                if (forceReloadGlobalSettings)
                {
                    infinite_globals.log("Settings outdated! Rebuilding.");
                }
                else
                {
                    infinite_globals.log("Settings not found, rebuilding... File will be saved to: " + settingsFilePath);
                }

                GlobalSettings?.Reset();
            }
            SaveGlobalSettings();
        }

        public override int LoadPriority()
        {
            return infinite_globals.LOAD_ORDER;
        }


    }
}
