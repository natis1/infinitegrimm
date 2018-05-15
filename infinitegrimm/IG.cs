using Modding;
using System;
using System.IO;
using System.Linq;
using UnityEngine;


namespace infinitegrimm
{
    public class InfiniteGrimmMod : Mod<InfiniteSettings, InfiniteGlobalSettings>
    {

        private static string version = "0.2.9.2";

        private bool startedIG;

        public static int maximumDamage;

        public bool grimmchildupgrades;

        // Version detection code originally by Seanpr, used with permission.
        public override string GetVersion()
        {
            string ver = version;
            int minAPI = 40;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minAPI;
            bool noModCommon = !(from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "ModCommon" select type).Any();
            bool gcup = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "grimmchildupgrades" select type).Any();
            if (gcup) ver += " + Gc U";


            if (apiTooLow) ver += " (Error: ModAPI too old)";
            if (noModCommon) ver += " (Error: Infinite Grimm requires ModCommon)";

            return ver;
        }

        public override void Initialize()
        {
            grimmchildupgrades = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "grimmchildupgrades" select type).Any();
            if (grimmchildupgrades)
            {
                Modding.Logger.Log("[Infinite Grimm] Grimmchild, you're looking powerful as ever!");
            }

            SetupSettings();
            InfiniteGrimm.hardmode = GlobalSettings.HardMode;
            InfiniteTent.hardmode = GlobalSettings.HardMode;
            maximumDamage = Settings.IGDamageHighScore;

            ModHooks.Instance.AfterSavegameLoadHook += addToGame;
            ModHooks.Instance.NewGameHook += newGame;
            ModHooks.Instance.ApplicationQuitHook += SaveGlobalSettings;
            
            
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
            newGame();
        }

        public void saveLocalData()
        {
            Settings.IGDamageHighScore = maximumDamage;
            Modding.Logger.Log("[Infinite Grimm] Logging your damage record of " + maximumDamage + "!");
        }

        void SetupSettings()
        {
            string settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + GetType().Name + ".GlobalSettings.json";

            bool forceReloadGlobalSettings = (GlobalSettings != null && GlobalSettings.SettingsVersion != VersionInfo.SettingsVer);

            if (forceReloadGlobalSettings || !File.Exists(settingsFilePath))
            {
                if (forceReloadGlobalSettings)
                {
                    Modding.Logger.Log("[Infinite Grimm] Settings outdated! Rebuilding.");
                }
                else
                {
                    Modding.Logger.Log("[Infinite Grimm] Settings not found, rebuilding... File will be saved to: " + settingsFilePath);
                }

                GlobalSettings.Reset();
            }
            SaveGlobalSettings();
        }
    }
}
