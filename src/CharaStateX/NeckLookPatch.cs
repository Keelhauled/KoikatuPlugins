using Harmony;
using Studio;

namespace CharaStateX
{
    static class NeckLookPatch
    {
        public static void Patch(HarmonyInstance harmony)
        {
            {
                var type = typeof(MPCharCtrl).GetNestedType("LookAtInfo", AccessTools.all);
                var target = AccessTools.Method(type, "OnClick");
                var patch = AccessTools.Method(typeof(NeckLookPatch), nameof(Patch_LookAtInfo_OnClick));
                harmony.Patch(target, null, new HarmonyMethod(patch));
            }

            {
                var type = typeof(MPCharCtrl).GetNestedType("NeckInfo", AccessTools.all);
                var target = AccessTools.Method(type, "OnClick");
                var patch = AccessTools.Method(typeof(NeckLookPatch), nameof(Patch_NeckInfo_OnClick));
                harmony.Patch(target, null, new HarmonyMethod(patch));
            }
        }

        static void Patch_LookAtInfo_OnClick(object __instance, ref int _no)
        {
            if(Utils.GetIsUpdateInfo(__instance)) return;

            foreach(var chara in Utils.GetAllSelectedButMain(__instance))
                chara.ChangeLookEyesPtn(_no, false);
        }

        static void Patch_NeckInfo_OnClick(object __instance, ref int _idx)
        {
            if(Utils.GetIsUpdateInfo(__instance)) return;
            var patterns = Traverse.Create(__instance).Field("patterns").GetValue<int[]>();

            foreach(var chara in Utils.GetAllSelectedButMain(__instance))
                chara.ChangeLookNeckPtn(patterns[_idx]);
        }
    }
}
