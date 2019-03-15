using ChaCustom;
using Harmony;
using Manager;
using System;
using System.Collections;
using UnityEngine;

namespace CardOrganizer
{
    class CharaMaker : MonoBehaviour
    {
        CustomCharaFile customCharaFile;
        CustomFileListCtrl listCtrl;
        string folderName;

        void Start()
        {
            customCharaFile = FindObjectOfType<CustomCharaFile>();
            listCtrl = Traverse.Create(customCharaFile).Field("listCtrl").GetValue<CustomFileListCtrl>();
            BepInEx.Config.ReloadConfig();
            folderName = BepInEx.Config.GetEntry("FolderName", "", CardOrganizer.configName);
            StartCoroutine(SetListFolderDelayed());
        }

        IEnumerator SetListFolderDelayed()
        {
            yield return null;
            if(folderName != "") SetListFolder(folderName);
        }

        void SetListFolder(string folderName, bool clearList = true)
        {
            int modeSex = Singleton<CustomBase>.Instance.modeSex;
            FolderAssist folderAssist = new FolderAssist();
            string folder = Environment.CurrentDirectory + (modeSex != 0 ? string.Format("/UserData/CardOrganizer/chara/{0}/", folderName) : "/UserData/chara/male/");
            folderAssist.CreateFolderInfoEx(folder, new string[] { "*.png" }, true);
            if(clearList) listCtrl.ClearList();

            int num = 0;
            for(int i = 0; i < folderAssist.GetFileCount(); i++)
            {
                ChaFileControl chaFileControl = new ChaFileControl();
                if(!chaFileControl.LoadCharaFile(folderAssist.lstFile[i].FullPath, 255, false, true))
                {
                    int lastErrorCode = chaFileControl.GetLastErrorCode();
                }
                else if(chaFileControl.parameter.sex == modeSex)
                {
                    string club = "";
                    string personality = "";

                    if(modeSex != 0)
                    {
                        personality = Voice.Instance.voiceInfoDic.TryGetValue(chaFileControl.parameter.personality, out VoiceInfo.Param param) ? param.Personality : "不明";
                        club = Game.ClubInfos.TryGetValue(chaFileControl.parameter.clubActivities, out ClubInfo.Param param2) ? param2.Name : "不明";
                    }
                    else
                    {
                        listCtrl.DisableAddInfo();
                    }

                    listCtrl.AddList(num, chaFileControl.parameter.fullname, club, personality, folderAssist.lstFile[i].FullPath, folderAssist.lstFile[i].FileName, folderAssist.lstFile[i].time, false);
                    num++;
                }
            }

            listCtrl.Create(customCharaFile.OnChangeSelect);
        }

        void LoadCharaInMaker(string path)
        {
            //confirmpanel.gameObject.SetActive(false);
            //optionspanel.gameObject.SetActive(false);
            //if(autoClose) UISystem.gameObject.SetActive(false);

            bool face = true, body = true, hair = true;
            bool parameter = true, coordinate = true;

            ChaControl chaCtrl = CustomBase.Instance.chaCtrl;
            chaCtrl.chaFile.LoadFileLimited(path, chaCtrl.sex, face, body, hair, parameter, coordinate);
            chaCtrl.ChangeCoordinateType(true);
            chaCtrl.Reload(!coordinate, !face && !coordinate, !hair, !body);
            CustomBase.Instance.updateCustomUI = true;
            CustomHistory.Instance.Add5(chaCtrl, chaCtrl.Reload, !coordinate, !face && !coordinate, !hair, !body);
        }
    }
}
