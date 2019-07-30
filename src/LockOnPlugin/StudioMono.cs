﻿using Harmony;
using Studio;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LockOnPluginKK
{
    partial class StudioMono : LockOnBase
    {
        Studio.Studio studio = Studio.Studio.Instance;
        Studio.CameraControl camera = Studio.Studio.Instance.cameraCtrl;
        TreeNodeCtrl treeNodeCtrl = Studio.Studio.Instance.treeNodeCtrl;
        GuideObjectManager guideObjectManager = GuideObjectManager.Instance;

        Studio.CameraControl.CameraData cameraData;
        OCIChar currentCharaOCI;

        protected override void Start()
        {
            base.Start();

            cameraData = Traverse.Create(camera).Field("cameraData").GetValue<Studio.CameraControl.CameraData>();
            treeNodeCtrl.onSelect += new Action<TreeNodeObject>(OnSelectWork);
            studio.onDelete += new Action<ObjectCtrlInfo>(OnDeleteWork);
            var systemMenuContent = studio.transform.Find("Canvas Main Menu/04_System/Viewport/Content");
            systemMenuContent.Find("Load").GetComponent<Button>().onClick.AddListener(ResetModState);
            systemMenuContent.Find("End").GetComponent<Button>().onClick.AddListener(HideLockOnTargets);
            Guitime.pos = new Vector2(1f, 1f);
        }

        void OnSelectWork(TreeNodeObject node)
        {
            if(studio.dicInfo.TryGetValue(node, out ObjectCtrlInfo objectCtrlInfo))
            {
                if(objectCtrlInfo.kind == 0)
                {
                    var ocichar = objectCtrlInfo as OCIChar;

                    if(ocichar != currentCharaOCI)
                    {
                        currentCharaOCI = ocichar;
                        currentCharaInfo = ocichar.charInfo;
                        shouldResetLock = true;

                        if(LockOnPlugin.AutoSwitchLock.Value && lockOnTarget)
                        {
                            if(LockOn(lockOnTarget.name, true, false))
                            {
                                shouldResetLock = false;
                            }
                            else
                            {
                                LockOnRelease();
                            }
                        }
                    }
                    else
                    {
                        currentCharaOCI = ocichar;
                        currentCharaInfo = ocichar.charInfo;
                    }

                    return;
                }
            }

            currentCharaOCI = null;
            currentCharaInfo = null;
        }

        void OnDeleteWork(ObjectCtrlInfo info)
        {
            if(info.kind == 0)
            {
                currentCharaOCI = null;
                currentCharaInfo = null;
            }
        }

        protected override bool LockOn()
        {
            if(guideObjectManager.selectObject)
            {
                // hacky way to find out if the target is an FK/IK node
                if(!studio.dicObjectCtrl.TryGetValue(guideObjectManager.selectObject.dicKey, out _))
                {
                    LockOn(guideObjectManager.selectObject.transform.gameObject);
                    return true;
                }
            }

            if(base.LockOn()) return true;

            var charaNodes = LockOnPlugin.ScrollThroughMalesToo.Value ? GetCharaNodes<OCIChar>() : GetCharaNodes<OCICharFemale>();
            if(charaNodes.Count > 0)
            {
                studio.treeNodeCtrl.SelectSingle(charaNodes[0]);
                if(base.LockOn()) return true;
            }

            return false;
        }

        protected override void CharaSwitch(bool scrollDown = true)
        {
            var charaNodes = LockOnPlugin.ScrollThroughMalesToo.Value ? GetCharaNodes<OCIChar>() : GetCharaNodes<OCICharFemale>();

            for(int i = 0; i < charaNodes.Count; i++)
            {
                if(charaNodes[i] == treeNodeCtrl.selectNode)
                {
                    int next = i + 1 > charaNodes.Count - 1 ? 0 : i + 1;
                    if(!scrollDown) next = i - 1 < 0 ? charaNodes.Count - 1 : i - 1;
                    treeNodeCtrl.SelectSingle(charaNodes[next]);
                    return;
                }
            }

            if(charaNodes.Count > 0)
            {
                treeNodeCtrl.SelectSingle(charaNodes[0]);
            }
        }

        protected override void ResetModState()
        {
            base.ResetModState();
            currentCharaOCI = null;
            treeNodeCtrl.SelectSingle(null);
        }

        List<TreeNodeObject> GetCharaNodes<CharaType>()
        {
            var charaNodes = new List<TreeNodeObject>();

            int n = 0; TreeNodeObject nthNode;
            while(nthNode = treeNodeCtrl.GetNode(n))
            {
                if(nthNode.visible && studio.dicInfo.TryGetValue(nthNode, out ObjectCtrlInfo objectCtrlInfo))
                {
                    if(objectCtrlInfo is CharaType)
                    {
                        charaNodes.Add(nthNode);
                    }
                }
                n++;
            }

            return charaNodes;
        }
    }
}
