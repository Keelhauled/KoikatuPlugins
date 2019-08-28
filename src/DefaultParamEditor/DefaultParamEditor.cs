using BepInEx;
using BepInEx.Logging;
using ParadoxNotion.Serialization;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using UnityEngine;
using Logger = BepInEx.Logger;
using SharedPluginCode;
using UnityEngine.UI;
using Harmony;
using UnityEngine.Events;

namespace DefaultParamEditor
{
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin(GUID, "DefaultParamEditor", Version)]
    internal class DefaultParamEditor : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.defaultparameditor";
        public const string Version = "1.1.0";
        private const string ResetValue = "Reset";

        [Browsable(true)]
        [DisplayName("Set default character parameters")]
        [Description("Saves parameters like Blinking, Type of shoes, Eye follow, etc.\nas the default values for newly loaded characters.\n\n" +
                     "Values are taken from the currently selected character.")]
        [CustomSettingDraw(nameof(CharaParamSettingDrawer))]
        [DefaultValue(ResetValue)]
        protected string CharaParamSetting
        {
            get => null;
            set
            {
                if(value == ResetValue)
                {
                    Logger.Log(LogLevel.Debug, "Resetting charaParam");
                    CharacterParam.Reset();
                    SaveToFile();
                }
            }
        }

        [Browsable(true)]
        [DisplayName("Set default scene parameters")]
        [Description("Saves parameters like Bloom, Vignette, Shading, etc.\nas the default values for newly created scenes.\n\n" +
                     "Values are taken from the current scene. They are used when starting Studio and when resetting the current scene.")]
        [CustomSettingDraw(nameof(SceneParamSettingDrawer))]
        [DefaultValue(ResetValue)]
        protected string SceneParamSetting
        {
            get => null;
            set
            {
                if(value == ResetValue)
                {
                    Logger.Log(LogLevel.Debug, "Resetting sceneParam");
                    SceneParam.Reset();
                    SaveToFile();
                }
            }
        }
        
        private static string savePath;
        private static ParamData data = new ParamData();

        public DefaultParamEditor()
        {
            var ass = Assembly.GetExecutingAssembly();
            savePath = Path.Combine(Path.GetDirectoryName(ass.Location), "DefaultParamEditorData.json");
        }

        private static void SaveToFile()
        {
            var json = JSONSerializer.Serialize(data.GetType(), data, true);
            File.WriteAllText(savePath, json);
        }

        private void CharaParamSettingDrawer()
        {
            if(GUILayout.Button("Save current as default", GUILayout.ExpandWidth(true)))
            {
                CharacterParam.Save();
                SaveToFile();
            }
        }

        private void SceneParamSettingDrawer()
        {
            if(GUILayout.Button("Save current as default", GUILayout.ExpandWidth(true)))
            {
                SceneParam.Save();
                SaveToFile();
            }
        }

        protected void Awake()
        {
            var harmony = HarmonyInstance.Create($"{GUID}.harmony");
            harmony.PatchAll(GetType());

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
            CreateCharaButton("Save chara param", charalist, () =>
            {
                CharacterParam.Save();
                SaveToFile();
            });
        }

        public static ScrollRect SetupList(string goPath)
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

        public static Button CreateMainButton(string name, ScrollRect scrollRect, UnityAction onClickEvent)
        {
            return CreateButton(name, scrollRect, onClickEvent, "StudioScene/Canvas Main Menu/04_System/Viewport/Content/End");
        }

        public static Button CreateCharaButton(string name, ScrollRect scrollRect, UnityAction onClickEvent)
        {
            return CreateButton(name, scrollRect, onClickEvent, "StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/00_Root/Viewport/Content/State");
        }

        public static Button CreateButton(string name, ScrollRect scrollRect, UnityAction onClickEvent, string goPath)
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
