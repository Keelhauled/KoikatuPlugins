using Manager;
using System.Linq;
using UnityEngine;

namespace TogglePOVKK
{
    class HSceneMono : BaseMono
    {
        private CameraControl_Ver2 camera => Singleton<CameraControl_Ver2>.Instance;

        protected override bool CameraEnabled
        {
            get { return camera.enabled; }
            set { camera.enabled = value; }
        }

        protected override Vector3 CameraTargetPos
        {
            get { return camera.TargetPos; }
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

        protected override bool CameraStopMoving()
        {
            var noCtrlCondition = camera.NoCtrlCondition;
            bool result = false;
            if(noCtrlCondition != null)
            {
                result = noCtrlCondition();
            }
            return result;
        }

        protected override ChaInfo GetChara(Vector3 targetPos)
        {
            return Character.Instance.dictEntryChara.Values.ToList()[1];
        }
    }
}
