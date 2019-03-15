using ChaCustom;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace MakerBridge
{
    class MakerHandler : MonoBehaviour
    {
        void Start()
        {
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(MakerBridge.OtherCardPath),
                Filter = Path.GetFileName(MakerBridge.OtherCardPath),
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

            UnityMainThreadDispatcher.instance.Enqueue(() => LoadChara(MakerBridge.OtherCardPath, true, true, true, true, true));
        }

        void Update()
        {
            if(MakerBridge.SendChara.IsDown())
            {
                if(CustomBase.Instance)
                {
                    SaveCharacter(MakerBridge.MakerCardPath);
                }
            }
        }

        void LoadChara(string path, bool loadFace, bool loadBody, bool loadHair, bool parameter, bool loadCoord)
        {
            var chaCtrl = CustomBase.Instance.chaCtrl;
            chaCtrl.chaFile.LoadFileLimited(path, chaCtrl.sex, loadFace, loadBody, loadHair, parameter, loadCoord);
            chaCtrl.ChangeCoordinateType(true);
            chaCtrl.Reload(!loadCoord, !loadFace && !loadCoord, !loadHair, !loadBody);
            CustomBase.Instance.updateCustomUI = true;
            CustomHistory.Instance.Add5(chaCtrl, chaCtrl.Reload, !loadCoord, !loadFace && !loadCoord, !loadHair, !loadBody);
        }

        public void SaveCharacter(string path)
        {
            var customBase = CustomBase.Instance;

            var empty = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            empty.SetPixel(0, 0, Color.black);
            empty.Apply();

            var charFile = customBase.chaCtrl.chaFile;
            charFile.pngData = empty.EncodeToPNG();
            charFile.facePngData = empty.EncodeToPNG();

            customBase.chaCtrl.chaFile.SaveCharaFile(path, byte.MaxValue, false);
        }
    }
}
