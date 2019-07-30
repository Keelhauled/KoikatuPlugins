using Harmony;
using Studio;

namespace CharaStateX
{
    static class HandInfoPatch
    {
        public static void Patch(HarmonyInstance harmony)
        {
            var type = typeof(MPCharCtrl).GetNestedType("HandInfo", AccessTools.all);

            {
                var target = AccessTools.Method(type, "ChangeLeftHandAnime");
                var patch = AccessTools.Method(typeof(HandInfoPatch), nameof(Patch_ChangeLeftHandAnime));
                harmony.Patch(target, null, new HarmonyMethod(patch));
            }

            {
                var target = AccessTools.Method(type, "ChangeRightHandAnime");
                var patch = AccessTools.Method(typeof(HandInfoPatch), nameof(Patch_ChangeRightHandAnime));
                harmony.Patch(target, null, new HarmonyMethod(patch));
            }
        }

        static void Patch_ChangeLeftHandAnime(object __instance, ref int _no)
        {
            if(Utils.GetIsUpdateInfo(__instance)) return;

            foreach(var chara in Utils.GetAllSelectedButMain(__instance))
                chara.ChangeHandAnime(0, _no);
        }

        static void Patch_ChangeRightHandAnime(object __instance, ref int _no)
        {
            if(Utils.GetIsUpdateInfo(__instance)) return;

            foreach(var chara in Utils.GetAllSelectedButMain(__instance))
                chara.ChangeHandAnime(1, _no);
        }
    }
}
