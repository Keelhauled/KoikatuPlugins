using System;
using System.Linq;

namespace Harmony
{
    static class HarmonyExtensions
    {
        public static void UnpatchAll(this HarmonyInstance harmony, Type type)
        {
            foreach(var met in type.GetMethods().Where(x => x.IsStatic && x.IsPublic))
                foreach(var attr in met.GetCustomAttributes(false))
                    if(attr is HarmonyPatch hp)
                    {
                        var original = AccessTools.Method(hp.info.originalType, hp.info.methodName, hp.info.parameter);
                        harmony.RemovePatch(original, met);
                    }
        }
    }
}
