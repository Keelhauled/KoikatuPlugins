using BepInEx;
using Harmony;
using KKAPI.Studio.SaveLoad;
using SharedPluginCode;
using UnityEngine;

namespace LightManager
{
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin(GUID, "Light Manager", Version)]
    public class LightManagerPlugin : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.lightmanager";
        public const string Version = "1.0.0";

        HarmonyInstance harmony;
        static GameObject bepinex;

        void Awake()
        {
            bepinex = gameObject;
            harmony = HarmonyInstance.Create($"{GUID}.harmony");
            harmony.PatchAll(GetType());
            StudioSaveLoadApi.RegisterExtraBehaviour<SceneDataController>(GUID);
        }

#if DEBUG
        void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
        }
#endif

        [HarmonyPrefix, HarmonyPatch(typeof(StudioScene), "Start")]
        public static void Entrypoint()
        {
            bepinex.GetOrAddComponent<LightManager>();
        }
    }
}
