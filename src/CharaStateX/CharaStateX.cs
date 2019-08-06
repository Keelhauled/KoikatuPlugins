using BepInEx;
using Harmony;
using SharedPluginCode;

namespace CharaStateX
{
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin("keelhauled.charastatex", "CharaStateX", Version)]
    class CharaStateX : BaseUnityPlugin
    {
        public const string Version = "1.0.1";

        void Awake()
        {
            var harmony = HarmonyInstance.Create("keelhauled.charastatex.harmony");
            StateInfoPatch.Patch(harmony);
            NeckLookPatch.Patch(harmony);
            EtcInfoPatch.Patch(harmony);
            HandInfoPatch.Patch(harmony);
            harmony.PatchAll(typeof(AnimationPatch));
            JointInfoPatch.Patch(harmony);
            FKIKPatch.Patch(harmony);
        }
    }
}
