using System;
using System.Linq;

namespace HarmonyLib
{
    static class HarmonyExtensions
    {
        public static void UnpatchAll(this Harmony harmony, Type type)
        {
            foreach(var met in type.GetMethods().Where(x => x.IsStatic && x.IsPublic))
                foreach(var attr in met.GetCustomAttributes(false))
                    if(attr is HarmonyPatch hp)
                    {
                        var original = AccessTools.Method(hp.info.declaringType, hp.info.methodName, hp.info.argumentTypes);
                        harmony.Unpatch(original, met);
                    }
        }
    }
}
