using BepInEx.Logging;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace Spectacular
{
    static class ClothingController
    {
        static ChaClothesComponent currentClothesComponent;
        static int _SpecularColor = Shader.PropertyToID("_SpecularColor");
        static int _SpecularPower = Shader.PropertyToID("_SpecularPower");

        public static void ChangeSpecularity(float val)
        {
            if(currentClothesComponent)
            {
                var rendererArrs = GetRendererArrays(currentClothesComponent);
                var applicableRenderers = GetApplicableRenderers(rendererArrs).ToList();

                foreach(var renderer in applicableRenderers)
                {
                    if(renderer.material.HasProperty(_SpecularPower))
                        renderer.material.SetFloat(_SpecularPower, val);
                }
            }
            else
            {
                Logger.Log(LogLevel.Info, "currentClothesComponent == null");
            }
        }

        public static void ChangeSpecColor(Color val)
        {
            if(currentClothesComponent)
            {
                var rendererArrs = GetRendererArrays(currentClothesComponent);
                var applicableRenderers = GetApplicableRenderers(rendererArrs).ToList();

                foreach(var renderer in applicableRenderers)
                {
                    if(renderer.material.HasProperty(_SpecularColor))
                        renderer.material.SetColor(_SpecularColor, val);
                }
            }
            else
            {
                Logger.Log(LogLevel.Info, "currentClothesComponent == null");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeCustomClothes))]
        public static void ChangeCustomClothesPostHook(ChaControl __instance, ref bool main, ref int kind)
        {
            var clothesCtrl = GetCustomClothesComponent(__instance, main, kind);
            if(!clothesCtrl)
            {
                Logger.Log(LogLevel.Info, "No clothes component found");
                return;
            }

            currentClothesComponent = clothesCtrl;
        }

        static ChaClothesComponent GetCustomClothesComponent(ChaControl chaControl, bool main, int kind)
        {
            // for top clothes it fires once at start with first bool true (main load), then for each subpart with bool false
            // if true, objClothes are used, if false objParts                
            return main ? chaControl.GetCustomClothesComponent(kind) : chaControl.GetCustomClothesSubComponent(kind);
        }

        static Renderer[][] GetRendererArrays(ChaClothesComponent clothesCtrl)
        {
            return new[] { clothesCtrl.rendNormal01, clothesCtrl.rendNormal02, clothesCtrl.rendAlpha01, clothesCtrl.rendAlpha02 };
        }

        static IEnumerable<Renderer> GetApplicableRenderers(Renderer[][] rendererArrs)
        {
            for(var i = 0; i < rendererArrs.Length; i += 2)
            {
                var renderers = rendererArrs[i];
                var renderer1 = renderers?.ElementAtOrDefault(0);
                if(renderer1 != null)
                {
                    yield return renderer1;

                    renderers = rendererArrs.ElementAtOrDefault(i + 1);
                    var renderer2 = renderers?.ElementAtOrDefault(0);
                    if(renderer2 != null)
                        yield return renderer2;

                    yield break;
                }
            }
        }
    }
}
