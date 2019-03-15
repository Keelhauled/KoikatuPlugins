using BepInEx;
using BepInEx.Logging;
using ChaCustom;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using static BepInEx.Logger;

namespace CharaEditTool
{
    [BepInProcess("Koikatu.exe")]
    [BepInPlugin("keelhauled.charaedittool", "CharaEditTool", "1.0.0")]
    class CharaEditTool : BaseUnityPlugin
    {
        Color? savedColor;
        bool inScene = false;
        bool checkMoreAcc = true;

        [Description("Press hotkey to save the selected color.\n" +
                     "Hold hotkey to apply the saved color to every accessory color slot that has the same color as the selected color.")]
        [DisplayName("Save/apply color")]
        SavedKeyboardShortcut ColorCopyKey { get; }
        Hotkey colorCopyHotkey;

        CharaEditTool()
        {
            ColorCopyKey = new SavedKeyboardShortcut("ColorCopyKey", this, new KeyboardShortcut(KeyCode.V));
        }

        void Awake()
        {
            SceneLoaded();
            SceneManager.sceneLoaded += SceneLoaded;
            colorCopyHotkey = new Hotkey(ColorCopyKey, 0.4f);
        }

        void OnDestroy() // for ScriptEngine
        {
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneLoaded();
        }

        void SceneLoaded()
        {
            inScene = FindObjectOfType<CustomScene>();
        }

        void Update()
        {
            if(inScene)
            {
                colorCopyHotkey.KeyHoldAction(SetColor);
                colorCopyHotkey.KeyUpAction(SaveColor);
            }
        }

        void SaveColor()
        {
            savedColor = GetSelectedColor();
            if(savedColor != null)
                Log(LogLevel.Message, $"Color saved: {savedColor.Value}");
            else
                Log(LogLevel.Message, "One of the colors in an accessory has to be selected to save.");
        }

        void SetColor()
        {
            if(savedColor != null)
            {
                var selectedColor = GetSelectedColor();

                if(selectedColor != null)
                {
                    var chara = CustomBase.Instance.chaCtrl;
                    var accMenu = FindObjectOfType<CustomAcsChangeSlot>();
                    var cvsAccessory = Traverse.Create(accMenu).Field("cvsAccessory").GetValue<CvsAccessory[]>();

                    foreach(var acc in cvsAccessory)
                    {
                        int slotNo = (int)acc.slotNo;

                        var color0 = chara.nowCoordinate.accessory.parts[slotNo].color[0];
                        if(color0 == selectedColor) acc.UpdateAcsColor01(savedColor.Value);

                        var color1 = chara.nowCoordinate.accessory.parts[slotNo].color[1];
                        if(color1 == selectedColor) acc.UpdateAcsColor02(savedColor.Value);

                        var color2 = chara.nowCoordinate.accessory.parts[slotNo].color[2];
                        if(color2 == selectedColor) acc.UpdateAcsColor03(savedColor.Value);
                    }

                    if(checkMoreAcc)
                    {
                        var moreAccObj = GameObject.Find("BepInEx_Manager").GetComponent("MoreAccessoriesKOI.MoreAccessories");
                        if(moreAccObj != null)
                        {
                            var traverse = Traverse.Create(moreAccObj);
                            var nowAccessories = traverse.Field("_charaMakerData").Field("nowAccessories").GetValue<List<ChaFileAccessory.PartsInfo>>();
                            var charaMakerSlotDataList = traverse.Field("_additionalCharaMakerSlots").GetValue<IList>();
                            foreach(var data in charaMakerSlotDataList)
                            {
                                var acc = Traverse.Create(data).Field("cvsAccessory").GetValue<CvsAccessory>();
                                int slotNo = (int)acc.slotNo - 20;

                                var color0 = nowAccessories[slotNo].color[0];
                                if(color0 == selectedColor) acc.UpdateAcsColor01(savedColor.Value);

                                var color1 = nowAccessories[slotNo].color[1];
                                if(color1 == selectedColor) acc.UpdateAcsColor02(savedColor.Value);

                                var color2 = nowAccessories[slotNo].color[2];
                                if(color2 == selectedColor) acc.UpdateAcsColor03(savedColor.Value);
                            }
                        }
                        else
                        {
                            checkMoreAcc = false;
                        }
                    }

                    Log(LogLevel.Message, "Color applied to accessories.");
                    return;
                }
            }

            Log(LogLevel.Message, "Save a color before applying it to the accessories.");
        }

        Color? GetSelectedColor()
        {
            var accMenu = FindObjectOfType<CustomAcsChangeSlot>();
            int selectIndex = accMenu.GetSelectIndex();

            if(selectIndex == -1)
            {
                if(checkMoreAcc)
                {
                    var moreAccObj = GameObject.Find("BepInEx_Manager").GetComponent("MoreAccessoriesKOI.MoreAccessories");
                    if(moreAccObj != null)
                    {
                        var traverse = Traverse.Create(moreAccObj);
                        var nowAccessories = traverse.Field("_charaMakerData").Field("nowAccessories").GetValue<List<ChaFileAccessory.PartsInfo>>();
                        int moreaccSelectedIndex = traverse.Method("GetSelectedMakerIndex").GetValue<int>();
                        var charaMakerSlotDataList = traverse.Field("_additionalCharaMakerSlots").GetValue<IList>();
                        var selected = charaMakerSlotDataList[moreaccSelectedIndex - 20];
                        var cvsAccessory = Traverse.Create(selected).Field("cvsAccessory").GetValue<CvsAccessory>();
                        var cvsColor = Traverse.Create(cvsAccessory).Field("cvsColor").GetValue<CvsColor>();

                        if(cvsColor.connectColorKind != CvsColor.ConnectColorKind.None)
                        {
                            int colorIndex = ((int)cvsColor.connectColorKind - (int)CvsColor.ConnectColorKind.AcsSlot01_01) % 4;
                            return nowAccessories[(int)cvsAccessory.slotNo - 20].color[colorIndex];
                        }
                    }
                    else
                    {
                        checkMoreAcc = false;
                    }
                }
            }
            else if(selectIndex >= 0)
            {
                var cvsAccessory = Traverse.Create(accMenu).Field("cvsAccessory").GetValue<CvsAccessory[]>();
                var selected = cvsAccessory[selectIndex];
                var cvsColor = Traverse.Create(selected).Field("cvsColor").GetValue<CvsColor>();

                if(cvsColor.connectColorKind != CvsColor.ConnectColorKind.None)
                {
                    int colorIndex = ((int)cvsColor.connectColorKind - (int)CvsColor.ConnectColorKind.AcsSlot01_01) % 4;
                    var chara = CustomBase.Instance.chaCtrl;
                    return chara.nowCoordinate.accessory.parts[(int)selected.slotNo].color[colorIndex];
                }
            }

            return null;
        }
    }
}
