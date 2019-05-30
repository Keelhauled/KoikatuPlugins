using BepInEx;
using Harmony;
using Studio;

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

        [HarmonyPrefix, HarmonyPatch(typeof(Studio.Studio), nameof(Studio.Studio.AddLight), new[] { typeof(int) })]
        public static bool StudioLightFix(Studio.Studio __instance, ref int _no)
        {
            var ocilight = AddObjectLight.Add(_no);
            ocilight.light.cullingMask |= layerValue;

            UndoRedoManager.Instance.Clear();

            if(Studio.Studio.optionSystem.autoHide)
                __instance.rootButtonCtrl.OnClick(-1);

            if(Studio.Studio.optionSystem.autoSelect && ocilight != null)
                __instance.treeNodeCtrl.SelectSingle(ocilight.treeNodeObject, true);

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AddObjectLight), nameof(AddObjectLight.Load), new[] { typeof(OILightInfo), typeof(ObjectCtrlInfo), typeof(TreeNodeObject) })]
        public static void LoadLightFix(OCILight __result)
        {
            __result.light.cullingMask |= layerValue;
        }
    }
}
