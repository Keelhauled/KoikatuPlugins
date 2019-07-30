using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using Harmony;
using ConfigurationManager;
using UnityEngine;

namespace HideAllUI
{
    [BepInPlugin(GUID, "Hide All UI", Version)]
    public class HideAllUI : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.hideallui";
        public const string Version = "1.0.0";

        SavedKeyboardShortcut HideHotkey;

        void Awake()
        {
            HideHotkey = new SavedKeyboardShortcut("HideHotkey", this, new KeyboardShortcut(KeyCode.Space));
        }

        void Update()
        {
            if(HideHotkey.IsDown())
                ToggleUI();
        }

        void ToggleUI()
        {

        }
    }
}
