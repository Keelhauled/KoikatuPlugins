using BepInEx.Logging;
using Harmony;
using Studio;
using UnityEngine;
using static BepInEx.Logger;

namespace DefaultParamEditor
{
    public class SceneParam
    {
        private static ParamData.SceneData _sceneData;

        public SceneParam(ParamData.SceneData data)
        {
            _sceneData = data;
            var harmony = HarmonyInstance.Create("keelhauled.defaultparameditor.sceneparam.harmony");
            harmony.PatchAll(typeof(SceneParam));
        }

        public void Save()
        {
            var sceneInfo = Studio.Studio.Instance.sceneInfo;
            var systemButtonCtrl = Studio.Studio.Instance.systemButtonCtrl;

            if(sceneInfo != null && systemButtonCtrl != null)
            {
                _sceneData.aceNo = sceneInfo.aceNo;
                _sceneData.aceBlend = sceneInfo.aceBlend;
                _sceneData.enableAOE = Traverse.Create(systemButtonCtrl).Field("amplifyOcculusionEffectInfo").Property("aoe").Property("enabled").GetValue<bool>();
                _sceneData.aoeColor = sceneInfo.aoeColor;
                _sceneData.aoeRadius = sceneInfo.aoeRadius;
                _sceneData.enableBloom = sceneInfo.enableBloom;
                _sceneData.bloomIntensity = sceneInfo.bloomIntensity;
                _sceneData.bloomThreshold = sceneInfo.bloomThreshold;
                _sceneData.bloomBlur = sceneInfo.bloomBlur;
                _sceneData.enableDepth = sceneInfo.enableDepth;
                _sceneData.depthFocalSize = sceneInfo.depthFocalSize;
                _sceneData.depthAperture = sceneInfo.depthAperture;
                _sceneData.enableVignette = sceneInfo.enableVignette;
                _sceneData.enableFog = sceneInfo.enableFog;
                _sceneData.fogColor = sceneInfo.fogColor;
                _sceneData.fogHeight = sceneInfo.fogHeight;
                _sceneData.fogStartDistance = sceneInfo.fogStartDistance;
                _sceneData.enableSunShafts = sceneInfo.enableSunShafts;
                _sceneData.sunThresholdColor = sceneInfo.sunThresholdColor;
                _sceneData.sunColor = sceneInfo.sunColor;
                _sceneData.enableShadow = Traverse.Create(systemButtonCtrl).Field("selfShadowInfo").Field("toggleEnable").Property("isOn").GetValue<bool>();
                _sceneData.rampG = sceneInfo.rampG;
                _sceneData.ambientShadowG = sceneInfo.ambientShadowG;
                _sceneData.lineWidthG = sceneInfo.lineWidthG;
                _sceneData.lineColorG = sceneInfo.lineColorG;
                _sceneData.ambientShadow = sceneInfo.ambientShadow;
                _sceneData.cameraNearClip = Camera.main.nearClipPlane;
                _sceneData.fov = Studio.Studio.Instance.cameraCtrl.fieldOfView;

                _sceneData.saved = true;
                Log(LogLevel.Message, "Default scene settings saved");
            }
        }

        public void Reset()
        {
            _sceneData.saved = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SceneInfo), nameof(SceneInfo.Init))]
        public static void HarmonyPatch_SceneInfo_Init(SceneInfo __instance)
        {
            if(_sceneData.saved)
            {
                Log(LogLevel.Debug, "Loading defaults for a new scene");

                __instance.aceNo = _sceneData.aceNo;
                __instance.aceBlend = _sceneData.aceBlend;
                __instance.enableAOE = _sceneData.enableAOE;
                __instance.aoeColor = _sceneData.aoeColor;
                __instance.aoeRadius = _sceneData.aoeRadius;
                __instance.enableBloom = _sceneData.enableBloom;
                __instance.bloomIntensity = _sceneData.bloomIntensity;
                __instance.bloomThreshold = _sceneData.bloomThreshold;
                __instance.bloomBlur = _sceneData.bloomBlur;
                __instance.enableDepth = _sceneData.enableDepth;
                __instance.depthFocalSize = _sceneData.depthFocalSize;
                __instance.depthAperture = _sceneData.depthAperture;
                __instance.enableVignette = _sceneData.enableVignette;
                __instance.enableFog = _sceneData.enableFog;
                __instance.fogColor = _sceneData.fogColor;
                __instance.fogHeight = _sceneData.fogHeight;
                __instance.fogStartDistance = _sceneData.fogStartDistance;
                __instance.enableSunShafts = _sceneData.enableSunShafts;
                __instance.sunThresholdColor = _sceneData.sunThresholdColor;
                __instance.sunColor = _sceneData.sunColor;
                __instance.enableShadow = _sceneData.enableShadow;
                __instance.rampG = _sceneData.rampG;
                __instance.ambientShadowG = _sceneData.ambientShadowG;
                __instance.lineWidthG = _sceneData.lineWidthG;
                __instance.lineColorG = _sceneData.lineColorG;
                __instance.ambientShadow = _sceneData.ambientShadow;
                Camera.main.nearClipPlane = _sceneData.cameraNearClip;
                Studio.Studio.Instance.cameraCtrl.fieldOfView = _sceneData.fov;
            }
        }
    }
}
