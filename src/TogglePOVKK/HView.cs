using HarmonyLib;
using Manager;
using System.Linq;

namespace TogglePOVKK
{
    internal class HView : CommonView
    {
        protected override bool CameraEnabled
        {
            get { return Singleton<CameraControl_Ver2>.Instance.enabled; }
            set { Singleton<CameraControl_Ver2>.Instance.enabled = value; }
        }

        protected override bool DepthOfField
        {
            get { return Manager.Config.EtcData.DepthOfField; }
            set { Manager.Config.EtcData.DepthOfField = value; }
        }

        protected override bool Shield
        {
            get { return Manager.Config.EtcData.Shield; }
            set { Manager.Config.EtcData.Shield = value; }
        }

        protected override ChaControl GetChara()
        {
            return Character.Instance.dictEntryChara.Values.ToList()[1];
        }

        // Restore camera when changing spot in H
        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "GotoPointMoveScene")]
        public static void MovePointRestore()
        {
            if(instance && instance.povActive)
                instance.Restore();
        }
    }
}
