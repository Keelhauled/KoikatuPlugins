using BepInEx;
using KKAPI.Maker;
using KKAPI.Maker.UI.Sidebar;
using SharedPluginCode;
using UniRx;
using UnityEngine;

namespace AnimeAssAssistant
{
    [BepInDependency(KKAPI.KoikatuAPI.GUID)]
    [BepInDependency(ConfigurationManager.ConfigurationManager.GUID)]
    [BepInProcess(KoikatuConstants.KoikatuMainProcessName)]
    [BepInProcess(KoikatuConstants.KoikatuVRProcessName)]
    [BepInProcess(KoikatuConstants.KoikatuSteamProcessName)]
    [BepInProcess(KoikatuConstants.KoikatuSteamVRProcessName)]
    [BepInPlugin(GUID, "Anime Ass Assistant", Version)]
    public class AssInit : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.animeassassistant";
        public const string Version = "1.0.0";

        public static SavedKeyboardShortcut HotkeyOutfit1 { get; set; }
        public static SavedKeyboardShortcut HotkeyOutfit2 { get; set; }
        public static SavedKeyboardShortcut HotkeyOutfit3 { get; set; }
        public static SavedKeyboardShortcut HotkeyOutfit4 { get; set; }
        public static SavedKeyboardShortcut HotkeyOutfit5 { get; set; }
        public static SavedKeyboardShortcut HotkeyOutfit6 { get; set; }
        public static SavedKeyboardShortcut HotkeyOutfit7 { get; set; }

        public static SavedKeyboardShortcut HotkeyKill { get; set; }
        public static SavedKeyboardShortcut HotkeyNext { get; set; }
        public static SavedKeyboardShortcut HotkeyPrev { get; set; }
        public static SavedKeyboardShortcut HotkeySave { get; set; }

        public static ConfigWrapper<string> SearchFolder { get; set; }
        public static ConfigWrapper<string> SaveFolder { get; set; }

        public static bool EnableAAA;

        AssInit()
        {
            HotkeyNext = new SavedKeyboardShortcut(nameof(HotkeyNext), this, new KeyboardShortcut(KeyCode.RightArrow));
            HotkeyPrev = new SavedKeyboardShortcut(nameof(HotkeyPrev), this, new KeyboardShortcut(KeyCode.LeftArrow));
            HotkeyKill = new SavedKeyboardShortcut(nameof(HotkeyKill), this, new KeyboardShortcut(KeyCode.DownArrow));
            HotkeySave = new SavedKeyboardShortcut(nameof(HotkeySave), this, new KeyboardShortcut(KeyCode.UpArrow));

            HotkeyOutfit1 = new SavedKeyboardShortcut(nameof(HotkeyOutfit1), this, new KeyboardShortcut(KeyCode.Alpha1));
            HotkeyOutfit2 = new SavedKeyboardShortcut(nameof(HotkeyOutfit2), this, new KeyboardShortcut(KeyCode.Alpha2));
            HotkeyOutfit3 = new SavedKeyboardShortcut(nameof(HotkeyOutfit3), this, new KeyboardShortcut(KeyCode.Alpha3));
            HotkeyOutfit4 = new SavedKeyboardShortcut(nameof(HotkeyOutfit4), this, new KeyboardShortcut(KeyCode.Alpha4));
            HotkeyOutfit5 = new SavedKeyboardShortcut(nameof(HotkeyOutfit5), this, new KeyboardShortcut(KeyCode.Alpha5));
            HotkeyOutfit6 = new SavedKeyboardShortcut(nameof(HotkeyOutfit6), this, new KeyboardShortcut(KeyCode.Alpha6));
            HotkeyOutfit7 = new SavedKeyboardShortcut(nameof(HotkeyOutfit7), this, new KeyboardShortcut(KeyCode.Alpha7));

            SearchFolder = new ConfigWrapper<string>(nameof(SearchFolder), this, "");
            SaveFolder = new ConfigWrapper<string>(nameof(SaveFolder), this, "");
        }

        void Start()
        {
            var sideBarToggle = new SidebarToggle("Anime Ass Assistant", false, this);

            MakerAPI.RegisterCustomSubCategories += (sender, e) =>
            {
                EnableAAA = false;
                e.AddSidebarControl(sideBarToggle).ValueChanged.Subscribe(x => EnableAAA = x);
                gameObject.GetOrAddComponent<Assistant>();
            };

            MakerAPI.MakerExiting += (sender, e) => Destroy(gameObject.GetComponent<Assistant>());
        }
    }
}
