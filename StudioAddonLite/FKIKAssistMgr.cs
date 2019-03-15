using BepInEx.Logging;
using Studio;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static BepInEx.Logger;

namespace StudioAddonLite
{
    public class FKIKAssistMgr : MonoBehaviour
    {
        private static HashSet<string> FKIKBodyBone_Female = new HashSet<string>
        {
            "cf_J_Toes01_L",
            "cf_J_Toes01_R"
        };

        private static HashSet<string> FKIKBodyBone_Male = new HashSet<string>
        {
            "cm_J_Toes01_L",
            "cm_J_Toes01_R"
        };

        private static FieldInfo f_listBones = typeof(FKCtrl).GetField("listBones", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
        private static Type t_TargetInfo = typeof(FKCtrl).GetNestedType("TargetInfo", BindingFlags.Public | BindingFlags.NonPublic);
        private static FieldInfo f_enable = t_TargetInfo.GetField("enable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField);
        private static FieldInfo f_gameObject = t_TargetInfo.GetField("gameObject", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);

        void Start()
        {
            InstallFKMenu();
        }

        public void InstallFKMenu()
        {
            var transform = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/02_Kinematic/Viewport/Content").transform;
            var button = transform.Find("HSNA_ForceFKActive");
            if(button == null)
            {
                Transform transform2 = transform.Find("Pose");
                GameObject gameObject = Instantiate(transform2.gameObject);
                gameObject.name = "HSNA_ForceFKActive";
                gameObject.transform.SetParent(transform2.transform.parent);
                gameObject.transform.localPosition = transform2.transform.localPosition - new Vector3(0f, 30f, 0f);
                gameObject.transform.localScale = Vector3.one;

                var textMesh = gameObject.transform.Find("TextMeshPro").gameObject.GetComponent<TextMeshProUGUI>();
                textMesh.text = "FK&IK";
                textMesh.SetLayoutDirty();

                Button btn = gameObject.GetComponent<Button>();
                btn.onClick = new Button.ButtonClickedEvent();
                btn.onClick.AddListener(DoActivateFKIKForce);

                Log(LogLevel.Debug, "FK&IK button installed.");
            }
        }

        private void DoActivateFKIKForce()
        {
            foreach(ObjectCtrlInfo objectCtrlInfo in Studio.Studio.Instance.treeNodeCtrl.selectObjectCtrl)
            {
                if(objectCtrlInfo is OCIChar)
                {
                    ActivateFKIK((OCIChar)objectCtrlInfo);
                }
            }
        }

        public static bool IsFKIKActivated(OCIChar ociChar)
        {
            return ociChar.oiCharInfo.enableFK && ociChar.oiCharInfo.enableIK;
        }

        public static void ActivateFKIK(OCIChar ociChar)
        {
            HashSet<string> fkikBodyBone;
            if(ociChar.charInfo.sex == 0)
            {
                fkikBodyBone = FKIKBodyBone_Male;
            }
            else
            {
                fkikBodyBone = FKIKBodyBone_Female;
            }

            ociChar.ActiveKinematicMode((OICharInfo.KinematicMode)2, true, true);
            ociChar.fkCtrl.enabled = true;
            ociChar.oiCharInfo.enableFK = true;
            //Func<OCIChar.BoneInfo, bool> thing = null;
            for(int i = 0; i < ociChar.oiCharInfo.activeFK.Length; i++)
            {
                if(ociChar.oiCharInfo.activeFK[i])
                {
                    var boneGroup = FKCtrl.parts[i];
                    //if(boneGroup == (OIBoneInfo.BoneGroup)1)
                    //{
                    //    IEnumerable<OCIChar.BoneInfo> listBones = ociChar.listBones;
                    //    Func<OCIChar.BoneInfo, bool> predicate;
                    //    if((predicate = thing) == null)
                    //    {
                    //        predicate = (thing = ((OCIChar.BoneInfo b) => fkikBodyBone.Contains(b.guideObject.transformTarget.name)));
                    //    }
                    //    foreach(OCIChar.BoneInfo boneInfo in listBones.Where(predicate))
                    //    {
                    //        boneInfo.active = true;
                    //    }

                    //    IEnumerator enumerator2 = (f_listBones.GetValue(ociChar.fkCtrl) as IEnumerable).GetEnumerator();
                    //    while(enumerator2.MoveNext())
                    //    {
                    //        object obj = enumerator2.Current;
                    //        GameObject gameObject = FKIKAssistMgr.f_gameObject.GetValue(obj) as GameObject;
                    //        if(gameObject != null && fkikBodyBone.Contains(gameObject.name))
                    //        {
                    //            FKIKAssistMgr.f_enable.SetValue(obj, true);
                    //        }
                    //    }
                    //    goto IL_168;
                    //}
                    ociChar.fkCtrl.SetEnable(boneGroup, true);
                    ociChar.ActiveFK(boneGroup, true, true);
                }
                //IL_168:;
            }
        }
    }
}
