using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MC_SVPlayerWarpIn
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "mc.starvalor.playerwarpin";
        public const string pluginName = "SV Player Warpin";
        public const string pluginVersion = "1.0.0";

        private static bool doWarp = false;

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Main));
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.SetupGame))]
        [HarmonyPostfix]
        private static void GameManagerSetupGame_Post()
        {
            if (GameManager.instance == null ||
                GameManager.instance.Player == null ||
                !GameManager.instance.inGame)
                return;

            if (doWarp)
            {
                WarpIn warpInComp = GameManager.instance.Player.AddComponent<WarpIn>();
                warpInComp.warpInTime = 0;
            }
            else
                doWarp = true;
        }

        [HarmonyPatch(typeof(MenuControl), nameof(MenuControl.LoadGame))]
        [HarmonyPostfix]
        private static void MenuControlLoadGame_Post()
        {
            doWarp = false;
        }
    }
}
