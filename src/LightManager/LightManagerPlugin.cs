﻿using BepInEx;
using BepInEx.Harmony;
using HarmonyLib;
using KKAPI.Studio.SaveLoad;
using SharedPluginCode;
using UnityEngine;

namespace LightManager
{
    [BepInDependency(KKAPI.KoikatuAPI.GUID)]
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin(GUID, "Light Manager", Version)]
    public class LightManagerPlugin : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.lightmanager";
        public const string Version = "1.0.0";

        private Harmony harmony;
        private static GameObject bepinex;

        private void Awake()
        {
            bepinex = gameObject;
            harmony = new Harmony($"{GUID}.harmony");
            HarmonyWrapper.PatchAll(GetType(), harmony);
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneDataController>(GUID);
        }

#if DEBUG
        private void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
        }
#endif

        private class Hooks
        {
            [HarmonyPrefix, HarmonyPatch(typeof(StudioScene), "Start")]
            public static void Entrypoint()
            {
                bepinex.GetOrAddComponent<LightManager>();
            }
        }
    }
}
