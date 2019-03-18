using BepInEx;
using BepInEx.Logging;
using ParadoxNotion.Serialization;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using UnityEngine;
using Logger = BepInEx.Logger;

namespace DefaultParamEditor
{
    [BepInProcess("CharaStudio")]
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
                    charaParam.Reset();
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
                    sceneParam.Reset();
                    SaveToFile();
                }
            }
        }

        private readonly string savePath;
        private ParamData data = new ParamData();
        private CharacterParam charaParam;
        private SceneParam sceneParam;

        public DefaultParamEditor()
        {
            var ass = Assembly.GetExecutingAssembly();
            savePath = Path.Combine(Path.GetDirectoryName(ass.Location), "DefaultParamEditorData.json");
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
                charaParam.Save();
                SaveToFile();
            }
        }

        private void SceneParamSettingDrawer()
        {
            if(GUILayout.Button("Save current as default", GUILayout.ExpandWidth(true)))
            {
                sceneParam.Save();
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

            charaParam = new CharacterParam(data.charaParamData);
            sceneParam = new SceneParam(data.sceneParamData);
        }
    }
}
