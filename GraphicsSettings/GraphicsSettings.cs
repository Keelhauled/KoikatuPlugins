using BepInEx;
using BepInEx.Logging;
using Config;
using Harmony;
using Manager;
using System.ComponentModel;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
//using MadGoat_SSAA;
using static BepInEx.Logger;

namespace GraphicsSettings
{
    [BepInPlugin("keelhauled.graphicssettings", "Graphics Settings", "1.0.2")]
    public class GraphicsSettings : BaseUnityPlugin
    {
        // settings to add
        // rimlighting toggle
        // max fov adjustment

        const string CATEGORY_RENDER = "Rendering settings";
        const string CATEGORY_SHADOW = "Shadow settings";
        const string CATEGORY_MISC = "Misc settings";
        //const string CATEGORY_NEW = "AAA New settings";

        [Browsable(true)]
        [Category(CATEGORY_RENDER)]
        [DisplayName("!Resolution")]
        [CustomSettingDraw(nameof(ResolutionDrawer))]
        string ApplyResolution { get; set; } = "";

        [Category(CATEGORY_RENDER)]
        [DisplayName("VSync level")]
        [Description("VSync synchronizes the output video of the graphics card to the refresh rate of the monitor. " +
                     "This prevents tearing and produces a smoother video output.\n\n" +
                     "Half vsync synchronizes the output to half the refresh rate of your monitor.")]
        ConfigWrapper<VSyncType> VSyncCount { get; }

        [Category(CATEGORY_RENDER)]
        [DisplayName("Frame rate limiter")]
        [Description("VSync has to be disabled for this to take effect.")]
        ConfigWrapper<bool> LimitFrameRate { get; }

        [Category(CATEGORY_RENDER)]
        [DisplayName("Frame rate limit")]
        [AcceptableValueRange(20, 200, false)]
        ConfigWrapper<int> TargetFrameRate { get; }

        [Category(CATEGORY_RENDER)]
        [DisplayName("Anti-aliasing")]
        [Description("Smooths out jagged edges on objects.")]
        [AcceptableValueList(new object[] { 0, 2, 4, 8 })]
        ConfigWrapper<int> AntiAliasing { get; }

        [Category(CATEGORY_RENDER)]
        [DisplayName("Anisotropic filtering")]
        [Description("Improves distant textures when they are being viewer from indirect angles.")]
        ConfigWrapper<AnisotropicFiltering> AnisotropicTextures { get; }

        [Category(CATEGORY_SHADOW)]
        [DisplayName("Shadow type")]
        ConfigWrapper<ShadowQuality2> ShadowType { get; }

        [Category(CATEGORY_SHADOW)]
        [DisplayName("Shadow resolution")]
        ConfigWrapper<ShadowResolution> ShadowRes { get; }

        [Category(CATEGORY_SHADOW)]
        [DisplayName("Shadow projection")]
        [Description("Close Fit renders higher resolution shadows but they can sometimes wobble slightly if the camera moves." +
                     "Stable Fit is lower resolution but no wobble.")]
        ConfigWrapper<ShadowProjection> ShadowProject { get; }

        [Category(CATEGORY_SHADOW)]
        [DisplayName("Shadow cascades")]
        [Description("Increasing the number of cascades lessens the effects of perspective aliasing on shadows.")]
        [AcceptableValueList(new object[] { 0, 2, 4 })]
        ConfigWrapper<int> ShadowCascades { get; }

        [Category(CATEGORY_SHADOW)]
        [DisplayName("Shadow distance")]
        [Description("Increasing the distance lowers the shadow resolution slighly.")]
        [AcceptableValueRange(0f, 100f, false)]
        ConfigWrapper<float> ShadowDistance { get; }

        [Category(CATEGORY_SHADOW)]
        [DisplayName("Shadow near plane offset")]
        [Description("A low shadow near plane offset value can create the appearance of holes in shadows.")]
        [AcceptableValueRange(0f, 4f, false)]
        ConfigWrapper<float> ShadowNearPlaneOffset { get; }

        // this value is not loaded on start yet
        [Category(CATEGORY_MISC)]
        [DisplayName("Camera near clip plane")]
        [Description("Determines how close the camera can be to objects without clipping into them. Lower equals closer.\n\n" +
                     "Note: The saved value is not loaded at the start currently.")]
        [AcceptableValueRange(0.01f, 0.06f, false)]
        ConfigWrapper<float> CameraNearClipPlane { get; }

        [Category(CATEGORY_MISC)]
        [DisplayName("Run game in background")]
        [Description("Should the game be running when it is in the background (when the window is not focused)?\n\n" +
                     "On \"no\", the game will stop completely when it is in the background.\n\n" +
                     "On \"limited\", the game will stop if it has been unfocused and not loading anything for a couple seconds.")]
        ConfigWrapper<BackgroundRun> RunInBackground { get; }

        //[Category(CATEGORY_NEW)]
        //[AcceptableValueRange(0f, 1f, false)]
        //ConfigWrapper<float> BloomIntensity { get; set; }

        //[Category(CATEGORY_NEW)]
        //[AcceptableValueRange(0f, 1f, false)]
        //ConfigWrapper<float> AOIntensity { get; set; }

        GraphicsSettings()
        {
            VSyncCount = new ConfigWrapper<VSyncType>("VSyncCount", this, VSyncType.Enabled);
            LimitFrameRate = new ConfigWrapper<bool>("EnableFramerateLimit", this, false);
            TargetFrameRate = new ConfigWrapper<int>("TargetFrameRate", this, 60);
            AntiAliasing = new ConfigWrapper<int>("AntiAliasing", this, 8);
            AnisotropicTextures = new ConfigWrapper<AnisotropicFiltering>("AnisotropicTextures", this, AnisotropicFiltering.ForceEnable);
            ShadowType = new ConfigWrapper<ShadowQuality2>("ShadowType", this, ShadowQuality2.SoftHard);
            ShadowRes = new ConfigWrapper<ShadowResolution>("ShadowRes", this, ShadowResolution.VeryHigh);
            ShadowProject = new ConfigWrapper<ShadowProjection>("ShadowProject", this, ShadowProjection.CloseFit);
            ShadowCascades = new ConfigWrapper<int>("ShadowCascades", this, 4);
            ShadowDistance = new ConfigWrapper<float>("ShadowDistance", this, 50f);
            ShadowNearPlaneOffset = new ConfigWrapper<float>("ShadowNearPlaneOffset", this, 2f);
            CameraNearClipPlane = new ConfigWrapper<float>("CameraNearClipPlane", this, 0.06f);
            RunInBackground = new ConfigWrapper<BackgroundRun>("RunInBackground", this, BackgroundRun.Limited);
            //BloomIntensity = new ConfigWrapper<float>("BloomIntensity", this, 1f);
            //AOIntensity = new ConfigWrapper<float>("AOIntensity", this, 1f);
        }

        bool fullscreen = Screen.fullScreen;
        string resolutionX = Screen.width.ToString();
        string resolutionY = Screen.height.ToString();

        void ResolutionDrawer()
        {
            fullscreen = GUILayout.Toggle(fullscreen, " Fullscreen", GUILayout.Width(90));
            string resX = GUILayout.TextField(resolutionX, GUILayout.Width(60));
            string resY = GUILayout.TextField(resolutionY, GUILayout.Width(60));

            if(resX != resolutionX && int.TryParse(resX, out _)) resolutionX = resX;
            if(resY != resolutionY && int.TryParse(resY, out _)) resolutionY = resY;

            if(GUILayout.Button("Apply", GUILayout.ExpandWidth(true)))
            {
                int x = int.Parse(resolutionX);
                int y = int.Parse(resolutionY);

                if(Screen.width != x || Screen.height != y || Screen.fullScreen != fullscreen)
                    Screen.SetResolution(x, y, fullscreen);
            }
        }

        void Awake()
        {
            //var harmony = HarmonyInstance.Create("keelhauled.graphicssettings.harmony");
            //harmony.PatchAll(typeof(GraphicsSettings));

            QualitySettings.vSyncCount = (int)VSyncCount.Value;
            VSyncCount.SettingChanged += (sender, args) => QualitySettings.vSyncCount = (int)VSyncCount.Value;

            if(LimitFrameRate.Value) Application.targetFrameRate = TargetFrameRate.Value;
            LimitFrameRate.SettingChanged += (sender, args) => Application.targetFrameRate = LimitFrameRate.Value ? TargetFrameRate.Value : -1;
            TargetFrameRate.SettingChanged += (sender, args) => { if(LimitFrameRate.Value) Application.targetFrameRate = TargetFrameRate.Value; };

            QualitySettings.antiAliasing = AntiAliasing.Value;
            AntiAliasing.SettingChanged += (sender, args) => QualitySettings.antiAliasing = AntiAliasing.Value;

            QualitySettings.anisotropicFiltering = AnisotropicTextures.Value;
            AnisotropicTextures.SettingChanged += (sender, args) => QualitySettings.anisotropicFiltering = AnisotropicTextures.Value;

            QualitySettings.shadows = (ShadowQuality)ShadowType.Value;
            ShadowType.SettingChanged += (sender, args) => QualitySettings.shadows = (ShadowQuality)ShadowType.Value;

            QualitySettings.shadowResolution = ShadowRes.Value;
            ShadowRes.SettingChanged += (sender, args) => QualitySettings.shadowResolution = ShadowRes.Value;

            QualitySettings.shadowProjection = ShadowProject.Value;
            ShadowProject.SettingChanged += (sender, args) => QualitySettings.shadowProjection = ShadowProject.Value;

            QualitySettings.shadowCascades = ShadowCascades.Value;
            ShadowCascades.SettingChanged += (sender, args) => QualitySettings.shadowCascades = ShadowCascades.Value;

            QualitySettings.shadowDistance = ShadowDistance.Value;
            ShadowDistance.SettingChanged += (sender, args) => QualitySettings.shadowDistance = ShadowDistance.Value;

            QualitySettings.shadowNearPlaneOffset = ShadowNearPlaneOffset.Value;
            ShadowNearPlaneOffset.SettingChanged += (sender, args) => QualitySettings.shadowNearPlaneOffset = ShadowNearPlaneOffset.Value;

            //SceneManager.sceneLoaded += (scene, mode) => { if(Camera.main) Camera.main.nearClipPlane = CameraNearClipPlane.Value; };
            CameraNearClipPlane.SettingChanged += (sender, args) => { if(Camera.main) Camera.main.nearClipPlane = CameraNearClipPlane.Value; };

            if(RunInBackground.Value == BackgroundRun.No)
                Application.runInBackground = false;

            RunInBackground.SettingChanged += (sender, args) =>
            {
                switch(RunInBackground.Value)
                {
                    case BackgroundRun.No:
                        Application.runInBackground = false;
                        break;

                    case BackgroundRun.Limited:
                    case BackgroundRun.Yes:
                        Application.runInBackground = true;
                        break;
                }
            };

            //BloomIntensity.SettingChanged += (sender, args) => { if(bloom) bloom.bloomIntensity = BloomIntensity.Value; };
            //AOIntensity.SettingChanged += (sender, args) => { if(amplifyOcclus) amplifyOcclus.Intensity = AOIntensity.Value; };
        }

        int _focusFrameCounter;

        void Update()
        {
            if(RunInBackground.Value != BackgroundRun.Limited) return;

            if(!Manager.Scene.Instance.IsNowLoadingFade)
            {
                // Run for a bunch of frames to let the game load anything it's currently loading (scenes, cards, etc)
                // When loading it sometimes advances a frame at which point it would stop without this
                if(_focusFrameCounter < 100)
                    _focusFrameCounter++;
                else if(_focusFrameCounter == 100)
                    Application.runInBackground = false;
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if(RunInBackground.Value != BackgroundRun.Limited) return;

            Application.runInBackground = true;
            _focusFrameCounter = 0;
        }

        //[HarmonyPrefix, HarmonyPatch(typeof(Studio.CameraControl), "Start")]
        //public static void SSAAPatch(Studio.CameraControl __instance)
        //{
        //    var ssaa = __instance.gameObject.AddComponent<MadGoatSSAA>();
        //    ssaa.SetAsSSAA(SSAAMode.SSAA_X2);
        //    ssaa.SetAsCustom(2f, Filter.BICUBIC, 0f, 0f);
        //    ssaa.SetAsAdaptive(1f, 2f);
        //    Log(LogLevel.Message, $"SSAA status: {ssaa}");
        //}

        //[HarmonyPrefix, HarmonyPatch(typeof(EtceteraSystem), nameof(EtceteraSystem.Init))]
        //public static void EtceteraSystemPatch(EtceteraSystem __instance)
        //{
        //    Log(LogLevel.Info, new string('=', 40));
        //    foreach(var item in __instance.FieldInfos)
        //    {
        //        Log(LogLevel.Info, item.Name);
        //    }
        //    Log(LogLevel.Info, new string('=', 40));
        //}

        //static BloomAndFlares bloom;
        //static AmplifyOcclusionEffect amplifyOcclus;

        //[HarmonyPrefix, HarmonyPatch(typeof(CameraEffector), "Awake")]
        //public static void CameraEffectorPatch(CameraEffector __instance)
        //{
        //    bloom = __instance.bloom;
        //    amplifyOcclus = __instance.amplifyOcclus;

        //    Log(LogLevel.Info, bloom != null);
        //    Log(LogLevel.Info, amplifyOcclus != null);
        //}

        enum VSyncType
        {
            Disabled,
            Enabled,
            Half
        }

        enum ShadowQuality2
        {
            Disabled = ShadowQuality.Disable,
            [Description("Hard only")]
            HardOnly = ShadowQuality.HardOnly,
            [Description("Soft and hard")]
            SoftHard = ShadowQuality.All
        }

        enum BackgroundRun
        {
            No,
            Yes,
            Limited
        }
    }
}
