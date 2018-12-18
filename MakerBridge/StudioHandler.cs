﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using UnityEngine;
using static BepInEx.Logger;
using BepInEx.Logging;
using Studio;
using Harmony;

namespace MakerBridge
{
    class StudioHandler : MonoBehaviour
    {
        void Start()
        {
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(MakerBridge.MakerCardPath),
                Filter = Path.GetFileName(MakerBridge.MakerCardPath),
                EnableRaisingEvents = true
            };

            watcher.Changed += FileChanged;
        }

        void FileChanged(object sender, FileSystemEventArgs e)
        {
            bool fileIsBusy = true;
            while(fileIsBusy)
            {
                try
                {
                    using(var file = File.Open(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read)) { }
                    fileIsBusy = false;
                }
                catch(IOException)
                {
                    //The file is still arriving, give it time to finish copying and check again
                    Console.WriteLine("File is still being written to, retrying.");
                    Thread.Sleep(100);
                }
            }

            UnityMainThreadDispatcher.instance.Enqueue(() => LoadCharas());
        }

        void Update()
        {
            if(MakerBridge.SendChara.IsDown())
            {
                SaveChara();
            }
        }

        void SaveChara()
        {
            var characters = GetSelectedCharacters();
            if(characters.Count > 0)
            {
                var empty = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                empty.SetPixel(0, 0, Color.black);
                empty.Apply();

                var charFile = characters[0].charInfo.chaFile;
                charFile.pngData = empty.EncodeToPNG();
                charFile.facePngData = empty.EncodeToPNG();

                using(var fileStream = new FileStream(MakerBridge.OtherCardPath, FileMode.Create, FileAccess.Write))
                {
                    charFile.SaveCharaFile(fileStream, true);
                }
            }
            else
            {
                Log(LogLevel.Message, "Select a character to send to maker");
            }
        }

        void LoadCharas()
        {
            var characters = GetSelectedCharacters();
            if(characters.Count > 0)
            {
                Log(LogLevel.Message, "Character received");
                
                foreach(var chara in characters)
                {
                    chara.ChangeChara(MakerBridge.MakerCardPath);
                }

                UpdateStateInfo();
            }
            else
            {
                Log(LogLevel.Message, "Select a character before replacing it");
            }
        }

        List<OCIChar> GetSelectedCharacters()
        {
            return GuideObjectManager.Instance.selectObjectKey.Select(x => Studio.Studio.GetCtrlInfo(x) as OCIChar).Where(x => x != null).ToList();
        }

        void UpdateStateInfo()
        {
            var mpCharCtrl = FindObjectOfType<MPCharCtrl>();
            if(mpCharCtrl)
            {
                int select = Traverse.Create(mpCharCtrl).Field("select").GetValue<int>();
                if(select == 0) mpCharCtrl.OnClickRoot(0);
            }
        }
    }
}
