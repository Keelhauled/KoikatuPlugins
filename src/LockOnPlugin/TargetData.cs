﻿#pragma warning disable 649 // disable never assigned warning

using BepInEx.Logging;
using ParadoxNotion.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LockOnPluginKK
{
    internal class TargetData
    {
        internal static TargetData data;
        private const string dataFileName = "LockOnPluginData.json";

        public static void LoadData()
        {
            var ass = Assembly.GetExecutingAssembly();
            string dataPath = Path.Combine(Path.GetDirectoryName(ass.Location), dataFileName);

            if(File.Exists(dataPath))
            {
                try
                {
                    var json = File.ReadAllText(dataPath);
                    data = JSONSerializer.Deserialize<TargetData>(json);
                    LockOnPlugin.Logger.Log(LogLevel.Info, "Loading custom target data.");
                }
                catch(Exception)
                {
                    LockOnPlugin.Logger.Log(LogLevel.Info, "Failed to deserialize custom target data. Loading default target data.");
                    LoadResourceData();
                }
            }
            else
            {
                LockOnPlugin.Logger.Log(LogLevel.Debug, "Loading default target data.");
                LoadResourceData();
            }
        }

        private static void LoadResourceData()
        {
            string resourceName = $"{nameof(LockOnPluginKK)}.{dataFileName}";
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using(var reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    data = JSONSerializer.Deserialize<TargetData>(json);
                }
            }
        }

        public List<string> quickTargets;
        public List<CustomTarget> customTargets;
        public List<CenterWeigth> centerWeigths;
        public List<string> presenceTargets;

        public class CustomTarget
        {
            public string target;
            public string point1;
            public string point2;
            public float midpoint;
        }

        public class CenterWeigth
        {
            public string bone;
            public float weigth;
        }
    }
}
