using BepInEx;
using Harmony;
using System.ComponentModel;
using UnityEngine;

namespace TogglePOVKK
{
    [BepInPlugin("togglepovkk", "TogglePOV", Version)]
    public class TogglePOV : BaseUnityPlugin
    {
        public const string Version = "1.0.0";

        [DisplayName("Toggle POV")]
        public static SavedKeyboardShortcut POVKey { get; set; }

        [DisplayName("Default fov")]
        [AcceptableValueRange(20f, 120f, false)]
        public static ConfigWrapper<float> DefaultFov { get; set; }

        [DisplayName("Show hair")]
        [Description("Controls if hair is left untouched when going into first person.")]
        public static ConfigWrapper<bool> ShowHair { get; set; }

        [DisplayName("Male offset")]
        [Advanced(true)]
        public static ConfigWrapper<float> MaleOffset { get; set; }

        [DisplayName("Female offset")]
        [Advanced(true)]
        public static ConfigWrapper<float> FemaleOffset { get; set; }

        static GameObject bepinex;

        TogglePOV()
        {
            POVKey = new SavedKeyboardShortcut("POVKey", this, new KeyboardShortcut(KeyCode.Backspace));
            DefaultFov = new ConfigWrapper<float>("DefaultFov", this, 70f);
            ShowHair = new ConfigWrapper<bool>("ShowHair", this, false);
            MaleOffset = new ConfigWrapper<float>("MaleOffset", this, 0.042f);
            FemaleOffset = new ConfigWrapper<float>("FemaleOffset", this, 0.0315f);
        }

        void Awake()
        {
            bepinex = gameObject;
            var harmony = HarmonyInstance.Create("togglepovkk.harmony");
            harmony.PatchAll(GetType());
            harmony.PatchAll(typeof(BaseMono));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Studio.Studio), "Awake")]
        public static void StudioStart()
        {
            bepinex.GetOrAddComponent<StudioMono>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "SetShortcutKey")]
        public static void HSceneStart()
        {
            bepinex.GetOrAddComponent<HSceneMono>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "OnDestroy")]
        public static void HSceneEnd()
        {
            Destroy(bepinex.GetComponent<HSceneMono>());
        }
    }
}
