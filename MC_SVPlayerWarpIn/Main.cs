using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace MC_SVPlayerWarpIn
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "mc.starvalor.playerwarpin";
        public const string pluginName = "SV Player Warpin";
        public const string pluginVersion = "2.0.3";

        private static bool doWarp = false;
        private static bool jumpGateWarp = false;
        private static int jumpGateIndex = 0;

        private static ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource(pluginName);

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Main));
        }

        [HarmonyPatch(typeof(GameManager), "SpawnPlayerFleet")]
        [HarmonyPrefix]
        private static void GameManagerSpawnPlayerFleet_Pre(ref bool warpIn)
        {
            warpIn = doWarp;
        }

        [HarmonyPatch(typeof(GameManager), "SpawnMercenary")]
        [HarmonyPrefix]
        private static void GameManagerSpawnMercenary_Pre(ref Quaternion rotation, Transform guardTarget, AICharacter aiChar)
        {
            if (guardTarget.CompareTag("Player"))
            {
                if (!jumpGateWarp)
                {
                    rotation = guardTarget.rotation;
                    aiChar.rotationY = guardTarget.eulerAngles.y;
                }
                else
                {
                    TSector sector = GameData.data.sectors[GameData.data.currentSectorIndex];

                    if (sector.jumpGates[jumpGateIndex] != null)
                    {
                        rotation = sector.jumpGates[jumpGateIndex].jumpGateControl.transform.rotation;
                        aiChar.rotationY = sector.jumpGates[jumpGateIndex].jumpGateControl.transform.eulerAngles.y;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), "ShowWarpEffect")]
        [HarmonyPrefix]
        private static bool PlayerControlShowWarpEffect_Pre(Transform ___tf)
        {
            WarpOut wo = ___tf.gameObject.AddComponent<WarpOut>();
            wo.isPlayer = true;
            return false;
        }

        [HarmonyPatch(typeof(AIControl), "WarpDisappear", new Type[] {typeof(bool) })]
        [HarmonyPrefix]
        private static bool AIControlWarpDisappear_Pre(bool nearPlayer, Transform ___tf)
        {
            if (nearPlayer)
            {
                WarpOut wo = ___tf.gameObject.AddComponent<WarpOut>();
                wo.isPlayer = false;
            }            

            return false;
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
                    
                    if (sector.jumpGates[jumpGateIndex] != null)
                    {
                        GameManager.instance.Player.transform.SetPositionAndRotation(
                            sector.jumpGates[jumpGateIndex].jumpGateControl.transform.position,
                            new Quaternion(sector.jumpGates[jumpGateIndex].jumpGateControl.transform.rotation.x,
                            sector.jumpGates[jumpGateIndex].jumpGateControl.transform.rotation.y,
                            sector.jumpGates[jumpGateIndex].jumpGateControl.transform.rotation.z,
                            sector.jumpGates[jumpGateIndex].jumpGateControl.transform.rotation.w));
                        GameManager.instance.Player.transform.Rotate(new Vector3(0, 90, 0));
                        GameManager.instance.Player.transform.Translate(GameManager.instance.Player.transform.forward * 100f);
                    }                    
                }

                WarpIn warpInComp = GameManager.instance.Player.AddComponent<WarpIn>();
                warpInComp.warpInTime = 0;
            }
            else
                doWarp = true;

            jumpGateWarp = false;
        }

        [HarmonyPatch(typeof(MenuControl), nameof(MenuControl.LoadGame))]
        [HarmonyPostfix]
        private static void MenuControlLoadGame_Post()
        {
            doWarp = false;
            jumpGateWarp = false;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.WarpToSector))]
        [HarmonyPostfix]
        private static void PlayerControlWarpToSector_Post(TSector sector, bool usingJumpGate)
        {
            if(usingJumpGate)
            {
                jumpGateWarp = true;
                for (int j = 0; j < sector.jumpGates.Count; j++)
                {
                    if (sector.jumpGates[j].connectedSector == GameData.data.currentSectorIndex)
                    {
                        jumpGateIndex = j;
                        break;
                    }
                }
            }
        }
    }
}
