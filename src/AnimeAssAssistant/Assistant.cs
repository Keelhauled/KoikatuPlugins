using BepInEx.Logging;
using ChaCustom;
using Harmony;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using Logger = BepInEx.Logger;
using UniRx;

namespace AnimeAssAssistant
{
    public class Assistant : MonoBehaviour
    {
        string currentCharacter;
        List<string> loadedCharacters = new List<string>();
        bool enableAAA = AssInit.sideBarToggle.Value;

        void Start()
        {
            MakerAPI.AddSidebarControl(AssInit.sideBarToggle).ValueChanged.Subscribe(x => enableAAA = x);
        }

        void Update()
        {
            if(enableAAA)
            {
                if(AssInit.HotkeyRandom.IsDown())
                    LoadRandomChara();
                else if(AssInit.HotkeyNext.IsDown())
                    LoadNextChara();
                else if(AssInit.HotkeyPrev.IsDown())
                    LoadPrevChara();
                else if(AssInit.HotkeyRecycle.IsDown())
                    RecycleCurrentChara();
                else if(AssInit.HotkeySecure.IsDown())
                    SaveCurrentChara();

                else if(AssInit.HotkeyOutfit1.IsDown())
                    SetOutfit(0);
                else if(AssInit.HotkeyOutfit2.IsDown())
                    SetOutfit(1);
                else if(AssInit.HotkeyOutfit3.IsDown())
                    SetOutfit(2);
                else if(AssInit.HotkeyOutfit4.IsDown())
                    SetOutfit(3);
                else if(AssInit.HotkeyOutfit5.IsDown())
                    SetOutfit(4);
                else if(AssInit.HotkeyOutfit6.IsDown())
                    SetOutfit(5);
                else if(AssInit.HotkeyOutfit7.IsDown())
                    SetOutfit(6); 
            }
        }

        void LoadRandomChara()
        {
            if(string.IsNullOrEmpty(AssInit.SearchFolder.Value))
            {
                Logger.Log(LogLevel.Message, "[AnimeAssAssistant] Search folder has not been set, please set it in ConfigManager");
                return;
            }

            var files = Directory.GetFiles(AssInit.SearchFolder.Value, "*.png");
            if(files.Length == 0)
            {
                Logger.Log(LogLevel.Message, "[AnimeAssAssistant] Search folder is empty");
                return;
            }

            var random = UnityEngine.Random.Range(0, files.Length);
            LoadChara(files[random]);
        }

        void RecycleCurrentChara()
        {
            if(!string.IsNullOrEmpty(currentCharacter))
            {
                RecycleBinUtil.MoveToRecycleBin(currentCharacter);
                loadedCharacters.Remove(currentCharacter);
                Logger.Log(LogLevel.Message, $"[AnimeAssAssistant] {currentCharacter} moved to the recycling bin.");
                LoadRandomChara(); 
            }
        }

        void SaveCurrentChara()
        {
            if(string.IsNullOrEmpty(AssInit.SaveFolder.Value))
            {
                Logger.Log(LogLevel.Info, "[AnimeAssAssistant] Save folder has not been set, please set it in ConfigManager");
                return;
            }

            var dest = Path.Combine(AssInit.SaveFolder.Value, Path.GetFileName(currentCharacter));
            File.Move(currentCharacter, dest);
            LoadRandomChara();
        }

        void LoadNextChara()
        {
            var index = loadedCharacters.IndexOf(currentCharacter);
            if(index != -1 && index != loadedCharacters.Count - 1)
                LoadChara(loadedCharacters[index + 1]);
        }

        void LoadPrevChara()
        {
            var index = loadedCharacters.IndexOf(currentCharacter);
            if(index != -1 && index != 0)
                LoadChara(loadedCharacters[index - 1]);
        }

        void SetOutfit(int index)
        {
            var dropdown = Traverse.Create(Singleton<CustomControl>.Instance).Field("ddCoordinate").GetValue<TMP_Dropdown>();
            dropdown.value = index;
        }

        void LoadChara(string path)
        {
            var cfw = GameObject.FindObjectsOfType<CustomFileWindow>().FirstOrDefault(x => x.fwType == CustomFileWindow.FileWindowType.CharaLoad);
            var loadFace = true;
            var loadBody = true;
            var loadHair = true;
            var parameter = true;
            var loadCoord = true;

            if(cfw)
            {
                loadFace = cfw.tglChaLoadFace && cfw.tglChaLoadFace.isOn;
                loadBody = cfw.tglChaLoadBody && cfw.tglChaLoadBody.isOn;
                loadHair = cfw.tglChaLoadHair && cfw.tglChaLoadHair.isOn;
                parameter = cfw.tglChaLoadParam && cfw.tglChaLoadParam.isOn;
                loadCoord = cfw.tglChaLoadCoorde && cfw.tglChaLoadCoorde.isOn;
            }

            var chaCtrl = CustomBase.Instance.chaCtrl;
            var originalSex = chaCtrl.sex;

            chaCtrl.chaFile.LoadFileLimited(path, chaCtrl.sex, loadFace, loadBody, loadHair, parameter, loadCoord);
            if(chaCtrl.chaFile.GetLastErrorCode() != 0)
                throw new Exception("LoadFileLimited failed");

            if(chaCtrl.chaFile.parameter.sex != originalSex)
            {
                chaCtrl.chaFile.parameter.sex = originalSex;
                Logger.Log(LogLevel.Message, "[AnimeAssAssistant] Warning: The character's sex has been changed to match the editor mode.");
            }

            chaCtrl.ChangeCoordinateType(true);
            chaCtrl.Reload(!loadCoord, !loadFace && !loadCoord, !loadHair, !loadBody);
            CustomBase.Instance.updateCustomUI = true;

            loadedCharacters.Add(currentCharacter = path);
        }
    }
}
