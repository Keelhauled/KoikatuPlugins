using BepInEx.Logging;
using Studio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using static BepInEx.Logger;

namespace TogglePOVKK
{
    class StudioMono : BaseMono
    {
        Studio.Studio studio = Studio.Studio.Instance;
        Studio.CameraControl camera = Studio.Studio.Instance.cameraCtrl;
        TreeNodeCtrl treeNodeCtrl = Studio.Studio.Instance.treeNodeCtrl;
        UnityAction UpdateDOF = null;

        void Start()
        {
            try
            {
                var field = studio.systemButtonCtrl.GetType().GetField("dofInfo", BindingFlags.NonPublic | BindingFlags.Instance);
                var method = field.FieldType.GetMethod("UpdateInfo", BindingFlags.Instance | BindingFlags.Public);
                UpdateDOF = () => method.Invoke(field.GetValue(studio.systemButtonCtrl), null);
            }
            catch(Exception ex)
            {
                Log(LogLevel.Error, ex);
                UpdateDOF = null;
            }
        }

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
            get { return studio.sceneInfo.enableDepth; }
            set
            {
                if(UpdateDOF != null)
                {
                    studio.sceneInfo.enableDepth = value;
                    UpdateDOF.Invoke();
                }
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
            {
                result = noCtrlCondition();
            }
            return result;
        }

        protected override ChaInfo GetChara(Vector3 targetPos)
        {
            var characters = GetSelectedCharacters();
            if(characters.Count > 0) return characters[0].charInfo;
            return null;
        }

        List<OCIChar> GetSelectedCharacters()
        {
            return GuideObjectManager.Instance.selectObjectKey.Select(x => Studio.Studio.GetCtrlInfo(x) as OCIChar).Where(x => x != null).ToList();
        }
    }
}
