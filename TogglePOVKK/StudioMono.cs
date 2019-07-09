using Harmony;
using Studio;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TogglePOVKK
{
    class StudioMono : BaseMono
    {
        Studio.Studio studio = Studio.Studio.Instance;
        Studio.CameraControl camera = Studio.Studio.Instance.cameraCtrl;
        TreeNodeCtrl treeNodeCtrl = Studio.Studio.Instance.treeNodeCtrl;

        protected override bool CameraEnabled
        {
            get { return camera.enabled; }
            set { camera.enabled = value; }
        }

        protected override Vector3 CameraTargetPos
        {
            get { return camera.targetPos; }
        }

        protected override bool DepthOfField
        {
            get => studio.sceneInfo.enableDepth;
            set
            {
                studio.sceneInfo.enableDepth = value;
                Traverse.Create(studio.systemButtonCtrl).Field("dofInfo").Method("UpdateInfo").GetValue();
            }
        }

        protected override bool Shield
        {
            get { return Manager.Config.EtcData.Shield; }
            set { Manager.Config.EtcData.Shield = value; }
        }

        protected override bool CameraStopMoving()
        {
            var noCtrlCondition = camera.noCtrlCondition;
            bool result = false;
            if(noCtrlCondition != null)
                result = noCtrlCondition();

            return result;
        }

        protected override ChaInfo GetChara(Vector3 targetPos)
        {
            var characters = GetSelectedCharacters();
            return characters.Count > 0 ? characters[0].charInfo : null;
        }

        List<OCIChar> GetSelectedCharacters()
        {
            return GuideObjectManager.Instance.selectObjectKey.Select(x => Studio.Studio.GetCtrlInfo(x) as OCIChar).Where(x => x != null).ToList();
        }
    }
}
