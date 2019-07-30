using Harmony;
using Studio;
using System;
using System.Linq;
using UnityEngine.UI;

namespace CharaStateX
{
    static class JointInfoPatch
    {
        public static void Patch(HarmonyInstance harmony)
        {
            var jointInfoType = typeof(MPCharCtrl).GetNestedType("JointInfo", AccessTools.all);
            var target = AccessTools.Method(jointInfoType, "OnValueChanged");
            var patch = AccessTools.Method(typeof(JointInfoPatch), nameof(JointInfoPostfix));
            harmony.Patch(target, null, new HarmonyMethod(patch));

            PatchKKPE(harmony);
        }

        static void JointInfoPostfix(object __instance, ref int _group, ref bool _value)
        {
            if(Utils.GetIsUpdateInfo(__instance)) return;

            foreach(var chara in Utils.GetAllSelectedButMain(__instance))
                chara.EnableExpressionCategory(_group, _value);
        }

        static void PatchKKPE(HarmonyInstance harmony)
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((x) => x.FullName.Contains("KKPE,"));

            if(ass != null)
            {
                var type = ass.GetType("HSPE.MainWindow");
                var target = AccessTools.Method(type, "SpawnGUI");
                var patch = AccessTools.Method(typeof(JointInfoPatch), nameof(KKPESpawnGUIPostfix));
                harmony.Patch(target, null, new HarmonyMethod(patch));
            }
        }

        static void KKPESpawnGUIPostfix(object __instance)
        {
            var traverse = Traverse.Create(__instance);
            var _crotchCorrectionToggle = traverse.Field("_crotchCorrectionToggle").GetValue<Toggle>();
            var _leftFootCorrectionToggle = traverse.Field("_leftFootCorrectionToggle").GetValue<Toggle>();
            var _rightFootCorrectionToggle = traverse.Field("_rightFootCorrectionToggle").GetValue<Toggle>();

            _crotchCorrectionToggle.onValueChanged.AddListener((x) =>
            {
                foreach(var chara in Utils.GetSelectedCharacters())
                {
                    var poseTarget = chara.charInfo.gameObject.GetComponent("PoseController");
                    Traverse.Create(poseTarget).Property("crotchJointCorrection").SetValue(x);
                }
            });

            _leftFootCorrectionToggle.onValueChanged.AddListener((x) =>
            {
                foreach(var chara in Utils.GetSelectedCharacters())
                {
                    var poseTarget = chara.charInfo.gameObject.GetComponent("PoseController");
                    Traverse.Create(poseTarget).Property("leftFootJointCorrection").SetValue(x);
                }
            });

            _rightFootCorrectionToggle.onValueChanged.AddListener((x) =>
            {
                foreach(var chara in Utils.GetSelectedCharacters())
                {
                    var poseTarget = chara.charInfo.gameObject.GetComponent("PoseController");
                    Traverse.Create(poseTarget).Property("rightFootJointCorrection").SetValue(x);
                }
            });
        }
    }
}
