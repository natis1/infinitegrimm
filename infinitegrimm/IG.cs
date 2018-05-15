using Modding;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace infinitegrimm
{
    public class InfiniteGrimmMod : Mod<InfiniteSettings, InfiniteGlobalSettings>
    {

        

        public bool grimmchildupgrades;

        // Version detection code originally by Seanpr, used with permission.
        public override string GetVersion()
        {
            string ver = InfiniteGlobalVars.version;
            int minAPI = 40;

            bool apiTooLow = Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minAPI;
            bool noModCommon = !(from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "ModCommon" select type).Any();
            bool gcup = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "GrimmchildUpgrades" select type).Any();
            if (gcup) ver += " + Gc U";


            if (apiTooLow) ver += " (Error: ModAPI too old)";
            if (noModCommon) ver += " (Error: Infinite Grimm requires ModCommon)";

            return ver;
        }

        public override void Initialize()
        {

            SetupSettings();

            

            grimmchildupgrades = (from assembly in AppDomain.CurrentDomain.GetAssemblies() from type in assembly.GetTypes() where type.Namespace == "GrimmchildUpgrades" select type).Any();
            if (grimmchildupgrades)
            {
                Modding.Logger.Log("[Infinite Grimm] Grimmchild, you're looking powerful as ever!");
            }
            InfiniteGrimm.hardmode = GlobalSettings.HardMode;
            InfiniteTent.hardmode = GlobalSettings.HardMode;
            
            ModHooks.Instance.AfterSavegameLoadHook += addToGame;
            ModHooks.Instance.NewGameHook += newGame;
            ModHooks.Instance.ApplicationQuitHook += SaveGlobalSettings;
            ModHooks.Instance.SavegameSaveHook += saveLocalData;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += resetModSaveData;


        }

        // quick hack to fix bug in modding api... no seriously.
        // local data doesn't reset properly.
        private void resetModSaveData(Scene arg0, Scene arg1)
        {
            if (arg1.name == "Menu_Title")
            {
                Settings.IGDamageHighScore = 0;
                Settings.IGGrimmTalkState = 0;
            }
        }

        public void newGame()
        {
            Modding.Logger.Log("[Infinite Grimm] Current damage record for this file is: " + Settings.IGDamageHighScore);
            InfiniteGlobalVars.maximumDamage = Settings.IGDamageHighScore;

            GameManager.instance.gameObject.AddComponent<InfiniteDirtmouth>();
            GameManager.instance.gameObject.AddComponent<InfiniteTent>();
            GameManager.instance.gameObject.AddComponent<InfiniteGrimm>();
            Modding.Logger.Log("[Infinite Grimm] Please welcome Grimm to your world!");
        }

        public void addToGame(SaveGameData data)
        {
            newGame();
        }

        public void saveLocalData(int saveID)
        {
            Settings.IGDamageHighScore = InfiniteGlobalVars.maximumDamage;
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

        public override int LoadPriority()
        {
            return InfiniteGlobalVars.loadOrder;
        }


    }
}
