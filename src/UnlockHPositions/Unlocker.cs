﻿using BepInEx;
using Harmony;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace UnlockHPositions
{
    [BepInPlugin("keelhauled.unlockhpositions", "UnlockHPositions", Version)]
    public class Unlocker : BaseUnityPlugin
    {
        public const string Version = "1.1.0";

        [DisplayName("Unlock all positions")]
        [Description("Unlocks every possible position, including ones that are not supposed to be used in that spot.\n" +
                     "Warning: May break everything.\n" +
                     "Scene restart required for changes to take effect.")]
        static ConfigWrapper<bool> UnlockAll { get; set; }

        static bool fixUI = false;

        Unlocker()
        {
            UnlockAll = new ConfigWrapper<bool>("UnlockAll", this, false);
        }

        void Awake()
        {
            var harmony = HarmonyInstance.Create("keelhauled.unlockhpositions.harmony");
            harmony.PatchAll(typeof(Unlocker));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "CreateListAnimationFileName")]
        public static bool HarmonyPatch_HSceneProc_CreateListAnimationFileName(HSceneProc __instance, ref bool _isAnimListCreate, ref int _list)
        {
            var traverse = Traverse.Create(__instance);

            fixUI = false;
            var oneFem = __instance.flags.lstHeroine.Count == 1;
            var peeping = __instance.dataH.peepCategory?.FirstOrDefault() != 0;

            if(_isAnimListCreate)
                traverse.Method("CreateAllAnimationList").GetValue();

            var lstAnimInfo = traverse.Field("lstAnimInfo").GetValue<List<HSceneProc.AnimationListInfo>[]>();
            var lstUseAnimInfo = traverse.Field("lstUseAnimInfo").GetValue<List<HSceneProc.AnimationListInfo>[]>();

            for(int i = 0; i < lstAnimInfo.Length; i++)
            {
                lstUseAnimInfo[i] = new List<HSceneProc.AnimationListInfo>();
                if(_list == -1 || i == _list)
                {
                    for(int j = 0; j < lstAnimInfo[i].Count; j++)
                    {
                        if((UnlockAll.Value && oneFem && !peeping) || lstAnimInfo[i][j].lstCategory.Any(c => __instance.categorys.Contains(c.category)))
                        {
                            if(oneFem) fixUI = true;
                            lstUseAnimInfo[i].Add(lstAnimInfo[i][j]);
                        }
                    }
                }
            }

            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSprite), "CreateMotionList")]
        public static void HarmonyPatch_HSprite_CreateMotionList(HSprite __instance, ref int _kind)
        {
            if(fixUI && _kind == 2 && UnlockAll.Value && __instance.menuActionSub.GetActive(5))
            {
                var go = __instance.menuAction.GetObject(_kind);
                var rectTransform = go.transform as RectTransform;
                go = __instance.menuActionSub.GetObject(5);
                var rectTransform2 = go.transform as RectTransform;
                var anchoredPosition = rectTransform2.anchoredPosition;
                anchoredPosition.y = rectTransform.anchoredPosition.y + 350f; // may cause issues with different resolutions, fuck it
                rectTransform2.anchoredPosition = anchoredPosition;
            }
        }
    }
}
