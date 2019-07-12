using BepInEx;
using Harmony;
using System.ComponentModel;
using System.IO;
using UnityEngine;

namespace MakerBridge
{
    [BepInPlugin(GUID, "MakerBridge", Version)]
    public class MakerBridge : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.makerbridge";
        public const string Version = "1.0.0";

        public static string MakerCardPath;
        public static string OtherCardPath;
        static GameObject bepinex;

        [DisplayName("Send character")]
        public static SavedKeyboardShortcut SendChara { get; set; }

        MakerBridge()
        {
            SendChara = new SavedKeyboardShortcut("SendChara", this, new KeyboardShortcut(KeyCode.B));
        }

        void Awake()
        {
            bepinex = gameObject;

            var tempPath = Path.GetTempPath();
            MakerCardPath = Path.Combine(tempPath, "makerbridge1.png");
            OtherCardPath = Path.Combine(tempPath, "makerbridge2.png");

            var harmony = HarmonyInstance.Create("keelhauled.makerbridge.harmony");
            harmony.PatchAll(typeof(MakerBridge));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CustomScene), "Start")]
        public static void CustomSceneInit()
        {
            bepinex.GetOrAddComponent<MakerHandler>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CustomScene), "OnDestroy")]
        public static void CustomSceneStop()
        {
            Destroy(bepinex.GetComponent<MakerHandler>());
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StudioScene), "Start")]
        public static void StudioSceneInit()
        {
            bepinex.GetOrAddComponent<StudioHandler>();
        }
    }
}
