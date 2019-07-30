using BepInEx;
using BepInEx.Logging;
using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static BepInEx.Logger;

namespace TitleShortcuts
{
    [BepInPlugin("keelhauled.titleshortcuts", "TitleShortcuts", Version)]
    [BepInProcess("Koikatu")]
    class TitleShortcuts : BaseUnityPlugin
    {
        public const string Version = "1.1.1";

        [DisplayName("Automatic start mode")]
        [Description("Choose which mode to start automatically when launching.\nDuring startup, hold esc or F1 to cancel automatic behaviour or hold another shortcut to use that instead.")]
        ConfigWrapper<AutoStartOption> AutoStart { get; }

        [DisplayName("!Start female maker")]
        SavedKeyboardShortcut StartFemaleMaker { get; }

        [DisplayName("!Start male maker")]
        SavedKeyboardShortcut StartMaleMaker { get; }

        [DisplayName("Start uploader")]
        SavedKeyboardShortcut StartUploader { get; }

        [DisplayName("Start downloader")]
        SavedKeyboardShortcut StartDownloader { get; }

        [DisplayName("Start free H")]
        SavedKeyboardShortcut StartFreeH { get; }

        [DisplayName("Start live show")]
        SavedKeyboardShortcut StartLiveShow { get; }

        bool check = false;
        bool cancelAuto = false;
        TitleScene titleScene;

        enum AutoStartOption
        {
            Disabled,
            [Description("Female maker")]
            FemaleMaker,
            [Description("Male maker")]
            MaleMaker,
            [Description("Free H")]
            FreeH,
            [Description("Live stage")]
            LiveStage
        }

        TitleShortcuts()
        {
            AutoStart = new ConfigWrapper<AutoStartOption>("AutoStart", this, AutoStartOption.Disabled);
            StartFemaleMaker = new SavedKeyboardShortcut("StartFemaleMaker", this, new KeyboardShortcut(KeyCode.F));
            StartMaleMaker = new SavedKeyboardShortcut("StartMaleMaker", this, new KeyboardShortcut(KeyCode.M));
            StartUploader = new SavedKeyboardShortcut("StartUploader", this, new KeyboardShortcut(KeyCode.U));
            StartDownloader = new SavedKeyboardShortcut("StartDownloader", this, new KeyboardShortcut(KeyCode.D));
            StartFreeH = new SavedKeyboardShortcut("StartFreeH", this, new KeyboardShortcut(KeyCode.H));
            StartLiveShow = new SavedKeyboardShortcut("StartLiveShow", this, new KeyboardShortcut(KeyCode.L));
        }

        void Awake()
        {
            SceneManager.sceneLoaded += StartInput;
        }

        void StartInput(Scene scene, LoadSceneMode mode)
        {
            var title = FindObjectOfType<TitleScene>();

            if(title)
            {
                if(!check)
                {
                    titleScene = title;
                    check = true;
                    StartCoroutine(InputCheck());
                }
            }
            else
            {
                check = false;
            }
        }

        IEnumerator InputCheck()
        {
            while(check)
            {
                if(!cancelAuto && AutoStart.Value != AutoStartOption.Disabled && (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.F1)))
                {
                    Log(LogLevel.Message, "Automatic start cancelled");
                    cancelAuto = true;
                }

                if(!Manager.Scene.Instance.IsNowLoadingFade)
                {
                    if(StartFemaleMaker.IsPressed())
                    {
                        StartMode(titleScene.OnCustomFemale, "Starting female maker");
                    }
                    else if(StartMaleMaker.IsPressed())
                    {
                        StartMode(titleScene.OnCustomMale, "Starting male maker");
                    }

                    else if(StartUploader.IsPressed())
                    {
                        StartMode(titleScene.OnUploader, "Starting uploader");
                    }
                    else if(StartDownloader.IsPressed())
                    {
                        StartMode(titleScene.OnDownloader, "Starting downloader");
                    }

                    else if(StartFreeH.IsPressed())
                    {
                        StartMode(titleScene.OnOtherFreeH, "Starting free H");
                    }
                    else if(StartLiveShow.IsPressed())
                    {
                        StartMode(titleScene.OnOtherIdolLive, "Starting live show");
                    }

                    else if(!cancelAuto && AutoStart.Value != AutoStartOption.Disabled)
                    {
                        switch(AutoStart.Value)
                        {
                            case AutoStartOption.FemaleMaker:
                                StartMode(titleScene.OnCustomFemale, "Automatically starting female maker");
                                break;

                            case AutoStartOption.MaleMaker:
                                StartMode(titleScene.OnCustomMale, "Automatically starting male maker");
                                break;

                            case AutoStartOption.FreeH:
                                StartMode(titleScene.OnOtherFreeH, "Automatically starting free H");
                                break;

                            case AutoStartOption.LiveStage:
                                StartMode(titleScene.OnOtherIdolLive, "Automatically starting live show");
                                break;
                        }
                    }

                    cancelAuto = true;
                }

                yield return null;
            }
        }

        void StartMode(UnityAction action, string msg)
        {
            if(FindObjectOfType<ConfigScene>()) return;
            Log(LogLevel.Message, msg);
            check = false;
            action();
        }
    }
}
