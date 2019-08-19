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

namespace DefaultParamEditor
{
    [BepInProcess(KoikatuConstants.KoikatuStudioProcessName)]
    [BepInPlugin("keelhauled.defaultparameditor", "DefaultParamEditor", Version)]
    internal class DefaultParamEditor : BaseUnityPlugin
    {
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

        public SavedKeyboardShortcut LoadDefaultsKey { get; set; }

        private readonly string savePath;
        private ParamData data = new ParamData();

        public DefaultParamEditor()
        {
            var ass = Assembly.GetExecutingAssembly();
            savePath = Path.Combine(Path.GetDirectoryName(ass.Location), "DefaultParamEditorData.json");
            LoadDefaultsKey = new SavedKeyboardShortcut("LoadDefaultsKey", this, new KeyboardShortcut(KeyCode.O));
        }

        private void SaveToFile()
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

        void Update()
        {
            if(LoadDefaultsKey.IsDown())
                SceneParam.LoadDefaults();
        }
    }
}
