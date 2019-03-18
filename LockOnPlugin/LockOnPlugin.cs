using BepInEx;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LockOnPluginKK
{
    [BepInPlugin("keelhauled.lockonpluginkk", "LockOnPlugin", Version)]
    class LockOnPlugin : BaseUnityPlugin
    {
        public const string Version = "1.0.0";

        [DisplayName("!Tracking speed")]
        [Description("The speed at which the target is followed.")]
        [AcceptableValueRange(0.01f, 0.3f, false)]
        public static ConfigWrapper<float> TrackingSpeedNormal { get; private set; }

        [DisplayName("Scroll through males")]
        [Description("Choose whether to include males in the rotation when switching between characters using the hotkeys from the plugin.")]
        public static ConfigWrapper<bool> ScrollThroughMalesToo { get; private set; }

        [DisplayName("Show info messages")]
        public static ConfigWrapper<bool> ShowInfoMsg { get; private set; }

        [DisplayName("!Leash length")]
        [Description("The amount of slack allowed when tracking.")]
        [AcceptableValueRange(0f, 0.5f, false)]
        public static ConfigWrapper<float> LockLeashLength { get; private set; }

        [DisplayName("Auto lock on switch")]
        [Description("Lock on automatically after switching characters.")]
        public static ConfigWrapper<bool> AutoSwitchLock { get; private set; }

        [Advanced(true)]
        [DisplayName("Show debug targets")]
        public static ConfigWrapper<bool> ShowDebugTargets { get; private set; }

        [DisplayName("!Lock on")]
        public static SavedKeyboardShortcut LockOnKey { get; private set; }

        [DisplayName("!Show target gui")]
        public static SavedKeyboardShortcut LockOnGuiKey { get; private set; }

        [DisplayName("Select previous character")]
        public static SavedKeyboardShortcut PrevCharaKey { get; private set; }

        [DisplayName("Select next character")]
        public static SavedKeyboardShortcut NextCharaKey { get; private set; }

        LockOnPlugin()
        {
            TrackingSpeedNormal = new ConfigWrapper<float>("LockedTrackingSpeed", this, 0.1f);
            ScrollThroughMalesToo = new ConfigWrapper<bool>("ScrollThroughMalesToo", this, true);
            ShowInfoMsg = new ConfigWrapper<bool>("ShowInfoMsg", this, true);
            LockLeashLength = new ConfigWrapper<float>("LockLeashLength", this, 0f);
            AutoSwitchLock = new ConfigWrapper<bool>("AutoSwitchLock", this, false);
            ShowDebugTargets = new ConfigWrapper<bool>("ShowDebugTargets", this, false);

            LockOnKey = new SavedKeyboardShortcut("LockOnKey", this, new KeyboardShortcut(KeyCode.Mouse4));
            LockOnGuiKey = new SavedKeyboardShortcut("LockOnGuiKey", this, new KeyboardShortcut(KeyCode.None));
            PrevCharaKey = new SavedKeyboardShortcut("PrevCharaKey", this, new KeyboardShortcut(KeyCode.None));
            NextCharaKey = new SavedKeyboardShortcut("NextCharaKey", this, new KeyboardShortcut(KeyCode.None));
        }

        void Awake()
        {
            TargetData.LoadData();
            SceneLoaded();
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void OnDestroy() // for ScriptEngine
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneLoaded();
        }

        void SceneLoaded()
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
