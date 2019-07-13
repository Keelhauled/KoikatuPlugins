using BepInEx;
using Harmony;
using System.ComponentModel;
using UnityEngine;

namespace TogglePOVKK
{
    [BepInPlugin(GUID, "TogglePOV", Version)]
    public class TogglePOV : BaseUnityPlugin
    {
        public const string GUID = "togglepovkk";
        public const string Version = "1.0.0";

        [DisplayName("Toggle POV")]
        public static SavedKeyboardShortcut POVKey { get; set; }

        [DisplayName("Default FOV")]
        [AcceptableValueRange(CommonView.MinFov, CommonView.MaxFov, false)]
        public static ConfigWrapper<float> DefaultFov { get; set; }

        [DisplayName("POV view offset")]
        public static ConfigWrapper<float> ViewOffset { get; set; }

        [DisplayName("Horizontal mouse sensitivity")]
        public static ConfigWrapper<float> VerticalSensitivity { get; set; }

        [DisplayName("Vertical mouse sensitivity")]
        public static ConfigWrapper<float> HorizontalSensitivity { get; set; }

        [DisplayName("POV camera near clip")]
        public static ConfigWrapper<float> NearClipPov { get; set; }

        [DisplayName("Clamp neck rotation")]
        public static ConfigWrapper<bool> ClampRotation { get; set; }

        static GameObject bepinex;

        TogglePOV()
        {
            POVKey = new SavedKeyboardShortcut("POVKey", this, new KeyboardShortcut(KeyCode.Backspace));
            DefaultFov = new ConfigWrapper<float>("DefaultFov", this, 70f);
            ViewOffset = new ConfigWrapper<float>("ViewOffset", this, 0.0315f);
            VerticalSensitivity = new ConfigWrapper<float>("VerticalSensitivity", this, 0.5f);
            HorizontalSensitivity = new ConfigWrapper<float>("HorizontalSensitivity", this, 0.5f);
            NearClipPov = new ConfigWrapper<float>("NearClipPov", this, 0.005f);
            ClampRotation = new ConfigWrapper<bool>("ClampRotation", this, true);
        }

        void Awake()
        {
            bepinex = gameObject;
            var harmony = HarmonyInstance.Create("togglepovkk.harmony");
            harmony.PatchAll(GetType());
            harmony.PatchAll(typeof(HView));
        }

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
