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
        public const string pluginVersion = "1.0.2";

        private static bool doWarp = false;
        private static bool jumpGateWarp = false;

        private static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource(pluginName);

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
                if (jumpGateWarp)
                {
                    TSector sector = GameData.data.sectors[GameData.data.currentSectorIndex];
                    GameObject closestJumpgate = null;
                    float dist = 0;
                    foreach (JumpGate jumpGate in sector.jumpGates)
                    {
                        if (closestJumpgate == null)
                        {
                            closestJumpgate = jumpGate.jumpGateControl.gameObject;
                            dist = Vector3.Distance(GameManager.instance.Player.transform.position,
                                jumpGate.jumpGateControl.gameObject.transform.position);
                        }
                        else
                        {
                            closestJumpgate = Vector3.Distance(GameManager.instance.Player.transform.position,
                                jumpGate.jumpGateControl.gameObject.transform.position) < dist ? jumpGate.jumpGateControl.gameObject : closestJumpgate;
                        }
                    }

                    if (closestJumpgate != null)
                    {
                        GameManager.instance.Player.transform.SetPositionAndRotation(
                            closestJumpgate.transform.position,
                            closestJumpgate.transform.rotation);
                        GameManager.instance.Player.transform.Translate(GameManager.instance.Player.transform.forward * 100f);
                    }
                    jumpGateWarp = false;
                }

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

        [HarmonyPatch(typeof(JumpGateControl), "OnTriggerEnter")]
        [HarmonyPostfix]
        private static void JumpGateControlOnTriggerEnter_Post(bool ___gateEnabled)
        {
            if(___gateEnabled)
                jumpGateWarp = true;
        }
    }
}
