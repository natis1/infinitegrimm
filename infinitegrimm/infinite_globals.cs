using System.Collections;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;
using Logger = Modding.Logger;

namespace infinitegrimm
{
    public static class infinite_globals
    {
        public const string VERSION = "1.0.6";
        public const int LOAD_ORDER = 26;
        public static int maximumDamage;
        
        // ReSharper disable once UnusedMember.Global Because reflection
        // ReSharper disable once InconsistentNaming Because Grimmchild Upgrades
        public static readonly int versionInt = 450;
        
        
        public static bool hardmode;
        public static bool noLagMode;
        public static bool noLagMode2;
        
        public static float startingDanceSpeed;
        public static float maxDanceSpeed;
        public static float danceSpeedIncreaseDmg;
        
        public static int staggerIncreaseDamage;
        public static int startingStaggerHits;
        
        // Stuff from global settings.
        public static int[] difficultyIncreaseValues;
        public static bool timeAttackMode;
        public static bool oneHitMode;
        public static bool godMode;
        
        public static int secondsToRun;
        
        public static readonly string[] LAG_OBJECTS_TO_KILL =
        {
            "Grimm_nightmare__0017_9 (2)", "grimm_curtain (20)", "default_particles",
            "grimm_pole_standard (5)", "water_fog", "Grimm_nightmare__0016_10", "Grimm Control.Grimm_heart",
            "Grimm Control.Crowd", "Grimm Control.Heartbeat Audio", "Grimm_Main_tent_0009_3 (14)", "grimm_rag_main_tent",
            "grimm_rag_main_tent (25)", "grimm_fader", "grimm_fader", "grimm_rag_main_tent (58)", "Grimm_nightmare__0014_12",
            "grimm_rag_main_tent (29)", "Grimm_Main_tent_0007_5 (20)", "grimm_fader (1)", "grimm_rag_main_tent (23)",
            "Grimm_Main_tent_0008_4 (8)", "grimm_wallpaper (12)", "Grimm_Main_tent_0006_6 (7)",
            "grimm_pole_standard (8)", "grimm_rag_main_tent (59)", "Grimm_Main_tent_0010_2 (16)",
            "grimm_rag_main_tent (11)", "grimm_rag_main_tent (44)", "Grimm_nightmare__0020_6", "Grimm_nightmare__0018_8",
            "grimm_curtain (2)", "Grimm_nightmare__0018_8 (5)", "Grimm_Main_tent_0006_6 (11)", "grimm_rag_main_tent (19)",
            "Grimm_nightmare__0016_10 (8)", "Grimm_nightmare__0016_10 (13)", "break_rag (6)", "grimm_fader (12)",
            "Grimm_nightmare__0017_9 (1)", "Grimm_nightmare_fabric_lantern (11)", "Grimm Control.Halfway Glow",
            "Grimm Control.Final Glow", "Grimm Control.Crowd Fader",
            "main_tent_short_pole (8)", "Grimm_nightmare__0014_12 (14)", "Grimm_nightmare__0022_4 (1)",
            "Grimm_nightmare__0018_8 (1)", "Grimm_Main_tent_0006_6 (13)", "Spotlight Appear", "grimm_rag_main_tent (54)",
            "grimm_rag_main_tent (17)", "Grimm_Main_tent_0008_4 (10)", "grimm_pole_bit", "grimm_rag_main_tent (12)",
            "Grimm_nightmare_fabric_lantern (3)", "grimm_rag_main_tent (18)", "Grimm_nightmare__0014_12 (13)",
            "Grimm_Main_tent_0009_3 (10)", "Grimm_nightmare__0014_12 (7)", "Grimm_Main_tent_0008_4 (14)",
            "grimm_rag_main_tent (22)", "Grimm_nightmare__0023_3", "break_rag (5)", "grimm_rag_main_tent (39)",
            "Grimm_nightmare__0019_7", "grimm_wallpaper (5)", "grimm_rag_main_tent (27)", "Grimm_Main_tent_0010_2 (6)",
            "grimm_fader (1)", "Grimm_nightmare__0016_10 (7)", "Grimm_nightmare_fabric_lantern (6)",
            "grimm_rag_main_tent (61)", "Grimm_nightmare__0016_10 (24)", "Grimm_nightmare__0017_9 (10)",
            "grimm_rag_main_tent (45)", "Grimm_nightmare_fabric_lantern (13)", "Grimm_nightmare__0016_10 (21)",
            "grimm_wallpaper (6)", "grimm_curtain (19)", "grimm_rag_main_tent (47)", "grimm_rag_main_tent (2)",
            "grimm_curtain_02 (15)", "Grimm_Main_tent_0006_6 (14)", "dream_particles", "grimm_pole_standard (3)",
            "Grimm_nightmare_fabric_lantern (1)", "break_rag (4)", "Incense Particle", "Grimm_nightmare_fabric_lantern (7)",
            "main_tent_short_pole (5)", "Grimm_nightmare_fabric_lantern (8)", "break_rag (1)", "Grimm_nightmare_fabric_lantern (9)",
            "Grimm_nightmare_fabric_lantern (2)", "Grimm_nightmare_fabric_lantern (5)", "grimm_pole_standard (1)",
            "Grimm_nightmare_fabric_lantern (4)", "Grimm_nightmare_fabric_lantern", "Grimm_nightmare_fabric_lantern (12)",
            "break_rag", "break_rag (3)", "break_rag (2)", "Grimm_nightmare_fabric_lantern (10)", "break_rag (7)"
        };

        public static readonly string[] NGG_OBJECTS_TO_KILL =
        {
            "Grimm Control(Clone).Loop Fire", "Grimm Control(Clone).Heartbeat Audio", "Grimm Control(Clone).Crowd",
            "Grimm Control(Clone).Heartbeat Audio", "Grimm Control(Clone).Halfway Glow",
            "Grimm Control(Clone).Final Glow", "Grimm Control(Clone).Crowd Fader", "Grimm Control(Clone).Grimm_heart"
        };

        public static readonly string[] ANNOYING_OBJECTS_TO_KILL =
        {
            "Grimm Control.Loop Fire", "Grimm Control.Heartbeat Audio"
        };
        
        
        public static readonly string[] VALID_STUN_STATES = {"Slash Antic", "Slash 1", "Slash 2", "Slash 3", "Slash Recover", "Slash End",
            "FB Antic", "FB Cast", "FB Cast End", "Firebat 1", "Firebat 2", "Firebat 4", "Tele Out", "FB Hero Pos",
            "FB Tele R", "FB Tele In", "FB Tele L", "FB Behind", "FB Re Tele", "Slash Pos", "Slash Tele In",
            "AD Pos", "AD Retry", "AD Tele In", "AD Antic", "AD Fire", "GD Antic", "AD Edge", "G Dash",
            "G Dash Recover", "Evade", "Evade End", "After Evade", "Uppercut Antic", "Uppercut Up", "UP Explode",
            "Pillar Pos", "Pillar Tele In", "Pillar Antic", "Pillar", "Pillar End" };

        public static readonly string[] VALID_BALLOON_TRANSITIONS = { "Explode Pause", "Out Pause", "Spike Return" };
        
        
        // Spawns in grimmchild after loading scene.
        public static IEnumerator spawnGrimmchild()
        {
            yield return new WaitForFinishedEnteringScene();
            yield return new WaitForSeconds(5f);

            if (!PlayerData.instance.GetBoolInternal("equippedCharm_40")) yield break;
            log("Spawning grimmchild in grimm arena.");
            PlayMakerFSM gcControl = FSMUtility.LocateFSM(infinite_tent.grimmchild, "Control");
            infinite_tent.grimmchild.SetActive(true);
            FsmState starting = gcControl.getState("Pause");
            starting.removeActionsOfType<BoolTest>();
            starting.clearTransitions();
            starting.addTransition("FINISHED", "Spawn");
            starting.addTransition("AWOKEN", "Spawn");
        }
        
        // Gives geo on returning to main area after dying.
        public static IEnumerator playerDies(int dmg)
        {
            yield return new WaitForSeconds(6f);
            yield return new WaitForFinishedEnteringScene();
            yield return new WaitForSeconds(2f);
            log("Good job, you did: " + dmg + " damage!");
            int geo = (int)(dmg / 10.0);
            if (hardmode)
                geo *= 2;

            // If you add 0 geo the menu bar gets buggy.
            if (geo > 0)
                HeroController.instance.AddGeo(geo);
        }

        public static IEnumerator destroyStuff(IEnumerable<string> stuffToKill)
        {
            log("Destroying unneeded objects.");
            yield return new WaitForSeconds(0.5f);
            foreach (string s in stuffToKill)
            {
                string[] gameObjs = s.Split('.');
                GameObject killMe = GameObject.Find(gameObjs[0]);

                if (killMe == null)
                {
                    log("Unable to find gameobject of name " + gameObjs[0]);
                    log("Please report this as a bug!");
                    continue;
                }

                for (int i = 1; i < gameObjs.Length; i++)
                {
                    killMe = killMe.FindGameObjectInChildren(gameObjs[i]);
                    if (killMe != null) continue;
                    log("Unable to find subobj " + gameObjs[i] + " from " + s);
                    log("Please report this as a bug!");
                }
                
                if (killMe != null)
                    Object.Destroy(killMe);
            }
        }
        
        public static void log(string str)
        {
            Logger.Log("[Infinite Grimm] " + str);
        }

    }
}
