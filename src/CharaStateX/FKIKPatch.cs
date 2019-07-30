using Harmony;
using Studio;

namespace CharaStateX
{
    static class FKIKPatch
    {
        public static void Patch(HarmonyInstance harmony)
        {
            {
                var type = typeof(MPCharCtrl).GetNestedType("FKInfo", AccessTools.all);
                var target = AccessTools.Method(type, "OnChangeValueFunction");
                var patch = AccessTools.Method(typeof(FKIKPatch), nameof(Patch_FKInfo_OnChangeValueFunction));
                harmony.Patch(target, null, new HarmonyMethod(patch));
            }

            {
                var type = typeof(MPCharCtrl).GetNestedType("IKInfo", AccessTools.all);
                var target = AccessTools.Method(type, "OnChangeValueFunction");
                var patch = AccessTools.Method(typeof(FKIKPatch), nameof(Patch_IKInfo_OnChangeValueFunction));
                harmony.Patch(target, null, new HarmonyMethod(patch));
            }
        }

        static void Patch_FKInfo_OnChangeValueFunction(object __instance, ref bool _value)
        {
            if(Utils.GetIsUpdateInfo(__instance)) return;

            foreach(var chara in Utils.GetAllSelectedButMain(__instance))
                chara.ActiveKinematicMode(OICharInfo.KinematicMode.FK, _value, false);
        }

        static void Patch_IKInfo_OnChangeValueFunction(object __instance, ref bool _value)
        {
            if(Utils.GetIsUpdateInfo(__instance)) return;

            foreach(var chara in Utils.GetAllSelectedButMain(__instance))
                chara.ActiveKinematicMode(OICharInfo.KinematicMode.IK, _value, false);
        }
    }
}
