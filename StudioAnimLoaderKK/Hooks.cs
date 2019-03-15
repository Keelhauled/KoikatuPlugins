using BepInEx;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace StudioAnimLoader
{
    public static class Hooks
    {
        static HarmonyInstance harmony;

        public static void Patch()
        {
            harmony = HarmonyInstance.Create("studioanimloaderkk.harmony");
            harmony.PatchAll(typeof(Hooks));
        }

        public static void Unpatch()
        {
            harmony.UnpatchAll(typeof(Hooks));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.LoadAnimation))]
        public static bool AnimationModifier(ChaControl __instance, ref string assetBundleName, ref string assetName, RuntimeAnimatorController __result)
        {
            Console.WriteLine($"{assetBundleName} exists: {File.Exists(assetBundleName)}");

            if(File.Exists(assetBundleName))
            {
                var runtimeAnimatorController = LoadAsset<RuntimeAnimatorController>(assetBundleName, assetName);
                __instance.animBody.runtimeAnimatorController = runtimeAnimatorController;
                __result = runtimeAnimatorController;
                return false;
            }

            return true;
        }

        public static Type LoadAsset<Type>(string assetBundlePath, string assetName) where Type : UnityEngine.Object
        {
            Type asset = null;
            AssetBundle ab = null;

            try
            {
                var bytes = File.ReadAllBytes(assetBundlePath);
                ab = AssetBundle.LoadFromMemory(bytes);
                asset = ab.LoadAsset<Type>(assetName);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error loading asset \"{assetName}\" from \"{assetBundlePath}\"");
                Console.WriteLine(ex);
            }

            if(asset == null)
                return (Type)((object)null);

            ab?.Unload(false);
            return asset;
        }
    }
}
