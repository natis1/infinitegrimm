using Modding;
using System.IO;
using UnityEngine;


namespace infinitegrimm
{
    public class InfiniteGrimmMod : Mod<InfiniteSettings, InfiniteGlobalSettings>
    {

        private static string version = "0.2.8";

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

            SetupSettings();
            InfiniteGrimm.hardmode = GlobalSettings.HardMode;
            InfiniteTent.hardmode = GlobalSettings.HardMode;

            ModHooks.Instance.AfterSavegameLoadHook += addToGame;
            ModHooks.Instance.NewGameHook += newGame;
            ModHooks.Instance.ApplicationQuitHook += SaveGlobalSettings;

            
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

        /*
        public void saveDamageRecord(int damageDone)
        {
            if (damageDone > Settings.IGDamageHighScore)
            {
                Settings.IGDamageHighScore = damageDone;
            }
        }

        public int getDamageRecord()
        {
            return Settings.IGDamageHighScore;
        }
        */

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
