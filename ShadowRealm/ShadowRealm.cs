﻿using BepInEx;
using Harmony;
using IllusionUtility.GetUtility;
using Studio;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace ShadowRealm
{
    [BepInProcess("CharaStudio")]
    [BepInPlugin("keelhauled.shadowrealm", "ShadowRealm", "1.0.0")]
    class ShadowRealm : BaseUnityPlugin
    {
        public const string Version = "1.0.0";
        HarmonyInstance harmony;
        static int layerValue = 1 << 10;

        void Start()
        {
            harmony = HarmonyInstance.Create("keelhauled.shadowrealm.harmony");
            harmony.PatchAll(GetType());
        }

        #if DEBUG
        void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
        }
        #endif

        [HarmonyPostfix, HarmonyPatch(typeof(AddObjectLight), nameof(AddObjectLight.Load), new[] { typeof(OILightInfo), typeof(ObjectCtrlInfo), typeof(TreeNodeObject) })]
        public static void LoadLightCullingMask(OCILight __result)
        {
            __result.light.cullingMask |= layerValue;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AddObjectLight), nameof(AddObjectLight.Add), new[] { typeof(int) })]
        public static void AddLightCullingMask(OCILight __result)
        {
            __result.light.cullingMask |= layerValue;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(Studio.Studio), nameof(Studio.Studio.AddLight), new[] { typeof(int) })]
        public static IEnumerable<CodeInstruction> UnlimitedLights(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();

            codes[0].opcode = OpCodes.Nop;
            codes[1].opcode = OpCodes.Nop;
            codes[2].opcode = OpCodes.Nop;
            codes[3].opcode = OpCodes.Br;

            return codes;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.CreateReferenceInfo))]
        public static void DisableShadowcaster(ChaControl __instance, GameObject objRef)
        {
            if(__instance.objBody != null && __instance.objBody == objRef)
                objRef.transform.FindLoop("o_shadowcaster")?.SetActive(false);
        }
    }
}
