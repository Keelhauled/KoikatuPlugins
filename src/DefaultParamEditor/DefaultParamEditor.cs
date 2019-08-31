﻿using BepInEx;
using BepInEx.Harmony;
using BepInEx.Logging;
using HarmonyLib;
using ParadoxNotion.Serialization;
using SharedPluginCode;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DefaultParamEditor
{
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin(GUID, "DefaultParamEditor", Version)]
    public class DefaultParamEditor : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.defaultparameditor";
        public const string Version = "1.1.0";

        internal static new ManualLogSource Logger;

        private static string savePath;
        private static ParamData data = new ParamData();

        public DefaultParamEditor()
        {
            var ass = Assembly.GetExecutingAssembly();
            savePath = Path.Combine(Path.GetDirectoryName(ass.Location), "DefaultParamEditorData.json");
        }

        private void Awake()
        {
            Logger = base.Logger;
            var harmony = new Harmony($"{GUID}.harmony");
            HarmonyWrapper.PatchAll(typeof(Hooks), harmony);

            if(File.Exists(savePath))
            {
                try
                {
                    var json = File.ReadAllText(savePath);
                    data = JSONSerializer.Deserialize<ParamData>(json);
                }
                catch(Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"[DefaultParamEditor] Failed to load settings from {savePath} with error: " + ex);
                    data = new ParamData();
                }
            }

            CharacterParam.Init(data.charaParamData);
            SceneParam.Init(data.sceneParamData);
        }

        private static void SaveToFile()
        {
            var json = JSONSerializer.Serialize(data.GetType(), data, true);
            File.WriteAllText(savePath, json);
        }

        private class Hooks
        {
            [HarmonyPostfix, HarmonyPatch(typeof(Studio.Studio), nameof(Studio.Studio.Init))]
            public static void CreateUI()
            {
                var mainlist = SetupList("StudioScene/Canvas Main Menu/04_System");
                CreateMainButton("Load scene param", mainlist, SceneParam.LoadDefaults);
                CreateMainButton("Save scene param", mainlist, () =>
                {
                    SceneParam.Save();
                    SaveToFile();
                });

                var charalist = SetupList("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/00_Root");
                //CreateCharaButton("Load chara param", charalist, CharacterParam.LoadDefaults);
                CreateCharaButton("Save chara param", charalist, () =>
                {
                    CharacterParam.Save();
                    SaveToFile();
                });
            }

            private static ScrollRect SetupList(string goPath)
            {
                var listObject = GameObject.Find(goPath);
                var scrollRect = listObject.GetComponent<ScrollRect>();
                scrollRect.content.gameObject.GetOrAddComponent<VerticalLayoutGroup>();
                scrollRect.content.gameObject.GetOrAddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                scrollRect.scrollSensitivity = 25;

                foreach(Transform item in scrollRect.content.transform)
                {
                    var layoutElement = item.gameObject.GetOrAddComponent<LayoutElement>();
                    layoutElement.preferredHeight = 40;
                }

                return scrollRect;
            }

            private static Button CreateMainButton(string name, ScrollRect scrollRect, UnityAction onClickEvent)
            {
                return CreateButton(name, scrollRect, onClickEvent, "StudioScene/Canvas Main Menu/04_System/Viewport/Content/End");
            }

            private static Button CreateCharaButton(string name, ScrollRect scrollRect, UnityAction onClickEvent)
            {
                return CreateButton(name, scrollRect, onClickEvent, "StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/00_Root/Viewport/Content/State");
            }

            private static Button CreateButton(string name, ScrollRect scrollRect, UnityAction onClickEvent, string goPath)
            {
                var template = GameObject.Find(goPath);
                var newObject = GameObject.Instantiate(template, scrollRect.content.transform);
                newObject.name = "NewObject";
                var textComponent = newObject.GetComponentInChildren<Text>();
                textComponent.text = name;
                var buttonComponent = newObject.GetComponent<Button>();
                buttonComponent.onClick.ActuallyRemoveAllListeners();
                buttonComponent.onClick.AddListener(onClickEvent);
                return buttonComponent;
            }
        }
    }
}
