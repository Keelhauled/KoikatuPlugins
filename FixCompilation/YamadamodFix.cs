using Harmony;
using UnityEngine;

namespace FixCompilation
{
    public static class YamadamodFix
    {
        [HarmonyPrefix, HarmonyPatch(typeof(ChaCustomHairComponent), "Update")]
        public static bool HarmonyPatch_ChaCustomHairComponent_Update(ChaCustomHairComponent __instance)
        {
            if(__instance.trfLength != null)
            {
                for(int i = 0; i < __instance.trfLength.Length; i++)
                {
                    var transform = __instance.trfLength[i];
                    if(transform) // Check trfLength[i] before using it
                    {
                        float y = __instance.baseLength[i] + Mathf.Lerp(0f, __instance.addLength, __instance.lengthRate);
                        transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
                    }
                }
            }

            return false;
        }
    }
}
