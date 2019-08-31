using HarmonyLib;
using Studio;
using System.Collections.Generic;
using System.Linq;

namespace TogglePOVKK
{
    internal class StudioView : CommonView
    {
        protected override bool CameraEnabled
        {
            get { return Studio.Studio.Instance.cameraCtrl.enabled; }
            set { Studio.Studio.Instance.cameraCtrl.enabled = value; }
        }

        protected override bool DepthOfField
        {
            get => Studio.Studio.Instance.sceneInfo.enableDepth;
            set
            {
                Studio.Studio.Instance.sceneInfo.enableDepth = value;
                Traverse.Create(Studio.Studio.Instance.systemButtonCtrl).Field("dofInfo").Method("UpdateInfo").GetValue();
            }
        }

        protected override bool Shield
        {
            get { return Manager.Config.EtcData.Shield; }
            set { Manager.Config.EtcData.Shield = value; }
        }

        protected override ChaControl GetChara()
        {
            var characters = GetSelectedCharacters();
            return characters.Count > 0 ? characters[0].charInfo : null;
        }

        private List<OCIChar> GetSelectedCharacters()
        {
            return GuideObjectManager.Instance.selectObjectKey.Select(x => Studio.Studio.GetCtrlInfo(x) as OCIChar).Where(x => x != null).ToList();
        }
    }
}
