using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LockOnPluginKK
{
    [BepInPlugin(GUID, "LockOnPlugin", Version)]
    internal class LockOnPlugin : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.lockonpluginkk";
        public const string Version = "1.0.0";
        internal static new ManualLogSource Logger;

        private const string SECTION_HOTKEYS = "Keyboard Shortcuts";
        private const string SECTION_GENERAL = "General";

        private const string DESCRIPTION_TRACKSPEED = "The speed at which the target is followed.";
        private const string DESCRIPTION_SCROLLMALES = "Choose whether to include males in the rotation when switching between characters using the hotkeys from the plugin.";
        private const string DESCRIPTION_LEASHLENGTH = "The amount of slack allowed when tracking.";
        private const string DESCRIPTION_AUTOLOCK = "Lock on automatically after switching characters.";

        internal static ConfigWrapper<float> TrackingSpeedNormal { get; set; }
        internal static ConfigWrapper<bool> ScrollThroughMalesToo { get; set; }
        internal static ConfigWrapper<bool> ShowInfoMsg { get; set; }
        internal static ConfigWrapper<float> LockLeashLength { get; set; }
        internal static ConfigWrapper<bool> AutoSwitchLock { get; set; }
        internal static ConfigWrapper<bool> ShowDebugTargets { get; set; }
        internal static ConfigWrapper<KeyboardShortcut> LockOnKey { get; set; }
        internal static ConfigWrapper<KeyboardShortcut> LockOnGuiKey { get; set; }
        internal static ConfigWrapper<KeyboardShortcut> PrevCharaKey { get; set; }
        internal static ConfigWrapper<KeyboardShortcut> NextCharaKey { get; set; }

        private LockOnPlugin()
        {
            Logger = base.Logger;

            TrackingSpeedNormal = Config.GetSetting(SECTION_GENERAL, "TrackingSpeed", 0.1f, new ConfigDescription(DESCRIPTION_TRACKSPEED, new AcceptableValueRange<float>(0.01f, 0.3f)));
            ScrollThroughMalesToo = Config.GetSetting(SECTION_GENERAL, "ScrollThroughMalesToo", true, new ConfigDescription(DESCRIPTION_SCROLLMALES));
            ShowInfoMsg = Config.GetSetting(SECTION_GENERAL, "ShowInfoMsg", true);
            LockLeashLength = Config.GetSetting(SECTION_GENERAL, "LeashLength", 0f, new ConfigDescription(DESCRIPTION_LEASHLENGTH, new AcceptableValueRange<float>(0f, 0.5f)));
            AutoSwitchLock = Config.GetSetting(SECTION_GENERAL, "AutoSwitchLock", false, new ConfigDescription(DESCRIPTION_AUTOLOCK));
            ShowDebugTargets = Config.GetSetting(SECTION_GENERAL, "ShowDebugTargets", false, new ConfigDescription("", null, "Advanced"));

            LockOnKey = Config.GetSetting(SECTION_HOTKEYS, "LockOn", new KeyboardShortcut(KeyCode.Mouse4));
            LockOnGuiKey = Config.GetSetting(SECTION_HOTKEYS, "ShowTargetGUI", new KeyboardShortcut(KeyCode.None));
            PrevCharaKey = Config.GetSetting(SECTION_HOTKEYS, "SelectPrevChara", new KeyboardShortcut(KeyCode.None));
            NextCharaKey = Config.GetSetting(SECTION_HOTKEYS, "SelectNextChara", new KeyboardShortcut(KeyCode.None));
        }

        private void Awake()
        {
            TargetData.LoadData();
            SceneLoaded();
            SceneManager.sceneLoaded += SceneLoaded;
        }

#if DEBUG
        private void OnDestroy()
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }
#endif

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneLoaded();
        }

        private void SceneLoaded()
        {
            var comp = gameObject.GetComponent<LockOnBase>();

            if(FindObjectOfType<StudioScene>())
            {
                if(!comp) gameObject.AddComponent<StudioMono>();
            }
            else if(FindObjectOfType<CustomScene>())
            {
                if(!comp) gameObject.AddComponent<MakerMono>();
            }
            else if(FindObjectOfType<HSceneProc>())
            {
                if(!comp) gameObject.AddComponent<HSceneMono>();
            }
            else if(comp)
            {
                Destroy(comp);
            }
        }
    }
}
