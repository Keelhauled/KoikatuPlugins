﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UILib;
using UnityEngine;
using UnityEngine.UI;

namespace BetterSceneLoader
{
    static class Utils
    {
        public static void AddCloseSymbol(Button button)
        {
            var x1 = UIUtility.CreatePanel("x1", button.transform);
            x1.transform.SetRect(0f, 0f, 1f, 1f, 8f, 0f, -8f);
            x1.rectTransform.eulerAngles = new Vector3(0f, 0f, 45f);
            x1.color = new Color(0f, 0f, 0f, 1f);

            var x2 = UIUtility.CreatePanel("x2", button.transform);
            x2.transform.SetRect(0f, 0f, 1f, 1f, 8f, 0f, -8f);
            x2.rectTransform.eulerAngles = new Vector3(0f, 0f, -45f);
            x2.color = new Color(0f, 0f, 0f, 1f);
        }

        public static Texture2D LoadTexture(byte[] bytes)
        {
            Texture2D result;
            using(MemoryStream memStream = new MemoryStream(bytes))
            {
                long pngSize = PngFile.GetPngSize(memStream);
                if(pngSize == 0L)
                {
                    result = null;
                }
                else
                {
                    using(BinaryReader binaryReader = new BinaryReader(memStream))
                    {
                        byte[] data = binaryReader.ReadBytes((int)pngSize);
                        int num = 0;
                        int num2 = 0;
                        result = PngAssist.ChangeTextureFromPngByte(data, ref num, ref num2);
                    }
                }
            }

            return result;
        }
    }
}
