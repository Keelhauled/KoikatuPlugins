using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using HarmonyLib;
using UnityEngine;

namespace TogglePOVKK
{
    [BepInPlugin(GUID, "TogglePOV", Version)]
    public class TogglePOV : BaseUnityPlugin
    {
        public const string GUID = "togglepovkk";
        public const string Version = "1.0.0";

        internal static ConfigWrapper<KeyboardShortcut> POVKey { get; set; }
        internal static ConfigWrapper<float> DefaultFov { get; set; }
        internal static ConfigWrapper<float> ViewOffset { get; set; }
        internal static ConfigWrapper<float> VerticalSensitivity { get; set; }
        internal static ConfigWrapper<float> HorizontalSensitivity { get; set; }
        internal static ConfigWrapper<float> NearClipPov { get; set; }
        internal static ConfigWrapper<bool> ClampRotation { get; set; }

        private static GameObject bepinex;
        private static Harmony harmony;

        private TogglePOV()
        {
            POVKey = Config.GetSetting("", "TogglePOV", new KeyboardShortcut(KeyCode.Backspace));
            DefaultFov = Config.GetSetting("", "DefaultFOV", 70f, new ConfigDescription("", new AcceptableValueRange<float>(CommonView.MinFov, CommonView.MaxFov)));
            ViewOffset = Config.GetSetting("", "ViewOffset", 0.0315f);
            VerticalSensitivity = Config.GetSetting("", "VerticalSensitivity", 0.5f);
            HorizontalSensitivity = Config.GetSetting("", "HorizontalSensitivity", 0.5f);
            NearClipPov = Config.GetSetting("", "NearClipPOV", 0.005f);
            ClampRotation = Config.GetSetting("", "ClampRotation", true);
        }

        private void Awake()
        {
            bepinex = gameObject;
            harmony = new Harmony("togglepovkk.harmony");
            HarmonyWrapper.PatchAll(typeof(Hooks), harmony);
            HarmonyWrapper.PatchAll(typeof(HView), harmony);

            bepinex.GetOrAddComponent<StudioView>();
        }

#if DEBUG
        private void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
            harmony.UnpatchAll(typeof(HView));
        }
#endif

        private class Hooks
        {
            [HarmonyPrefix, HarmonyPatch(typeof(Studio.Studio), "Awake")]
            public static void StudioStart()
            {
                bepinex.GetOrAddComponent<StudioView>();
            }

            [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "SetShortcutKey")]
            public static void HSceneStart()
            {
                bepinex.GetOrAddComponent<HView>();
            }

            [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "OnDestroy")]
            public static void HSceneEnd()
            {
                Destroy(bepinex.GetComponent<HView>());
            }
        }
    }
}
