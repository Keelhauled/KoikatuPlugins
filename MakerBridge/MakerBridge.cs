using BepInEx;
using Harmony;
using System.ComponentModel;
using System.IO;
using UnityEngine;

namespace MakerBridge
{
    [BepInPlugin("keelhauled.makerbridge", "MakerBridge", "1.0.0")]
    public class MakerBridge : BaseUnityPlugin
    {
        public static string MakerCardPath;
        public static string OtherCardPath;
        static GameObject container;

        [DisplayName("Send character")]
        public static SavedKeyboardShortcut SendChara { get; set; }

        MakerBridge()
        {
            SendChara = new SavedKeyboardShortcut("SendChara", this, new KeyboardShortcut(KeyCode.C));
        }

        void Awake()
        {
            MakerCardPath = Path.Combine(Paths.PluginPath, "makerbridgecard.png");
            OtherCardPath = Path.Combine(Paths.PluginPath, "makerbridgecard2.png");

            container = new GameObject("MakerBridge");
            container.transform.SetParent(gameObject.transform);
            container.AddComponent<UnityMainThreadDispatcher>();

            var harmony = HarmonyInstance.Create("keelhauled.makerbridge.harmony");
            harmony.PatchAll(typeof(MakerBridge));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CustomScene), "Start")]
        public static void CustomSceneInit()
        {
            var comp = container.GetComponent<MakerHandler>();
            if(!comp) container.AddComponent<MakerHandler>();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CustomScene), "OnDestroy")]
        public static void CustomSceneStop()
        {
            var comp = container.GetComponent<MakerHandler>();
            if(comp) Destroy(comp);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StudioScene), "Start")]
        public static void StudioSceneInit()
        {
            var comp = container.GetComponent<StudioHandler>();
            if(!comp) container.AddComponent<StudioHandler>();
        }
    }
}
