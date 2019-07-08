using BepInEx;
using BepInEx.Harmony;
using BepInEx.Logging;
using HarmonyLib;
using Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using H;

namespace FreeHControl
{
    [BepInPlugin(GUID, "FreeH Control", Version)]
    public class FreeHControl : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.freehcontrol";
        public const string Version = "1.0.0";

        Harmony harmony;
        new static ManualLogSource Logger;

        void Awake()
        {
            Logger = base.Logger;
            harmony = new Harmony($"keelhauled.freehcontrol.harmony.{Guid.NewGuid()}");
            Logger.LogInfo(harmony.Id);
            harmony.PatchAll(GetType());
        }

        #if DEBUG
        void OnDestroy()
        {
            harmony.UnpatchAll();
        }
        #endif

        //[HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "CreateListAnimationFileName")]
        //public static bool UnlockAnim(HSceneProc __instance, ref bool _isAnimListCreate, ref int _list)
        //{
        //    var traverse = Traverse.Create(__instance);
        //    var lstAnimInfo = traverse.Field("lstAnimInfo").GetValue<List<HSceneProc.AnimationListInfo>[]>();
        //    var lstUseAnimInfo = traverse.Field("lstUseAnimInfo").GetValue<List<HSceneProc.AnimationListInfo>[]>();

        //    if(_isAnimListCreate)
        //        traverse.Method("CreateAllAnimationList").GetValue();

        //    for(int i = 0; i < lstAnimInfo.Length; i++)
        //    {
        //        var anims = lstAnimInfo[i].Where(x => x.lstCategory.Select(y => y.category).Any(__instance.categorys.Contains));
        //        lstUseAnimInfo[i] = new List<HSceneProc.AnimationListInfo>(anims);
        //    }

        //    return false;
        //}

        // Patch something in HSceneProc.Start
        // Stops lightCamera movement
        [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), "SetShortcutKey")]
        public static void SetLightParent(HSceneProc __instance)
        {
            __instance.lightCamera.transform.SetParent(__instance.transform);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), "Update")]
        public static void HSceneProcUpdate()
        {
            if(Input.GetMouseButtonDown(1))
                CheckHit();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameCursor), "SetCursorTexture")]
        public static void CenterCursor(GameCursor __instance, ref int _kind)
        {
            if(_kind == -2)
            {
                var sizeWindow = Traverse.Create(__instance).Field("sizeWindow").GetValue<int>();
                var tex = __instance.iconDefalutTextures[sizeWindow];
                var center = new Vector2(tex.width / 2, tex.height / 2);
                Cursor.SetCursor(tex, center, CursorMode.ForceSoftware);
            }
        }

        static void CheckHit()
        {
            if(EventSystem.current.IsPointerOverGameObject())
                return;

            var hit = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition))
                .Where(x => x.collider.tag.Contains("H/Aibu/Hit/")).OrderBy(x => x.distance).FirstOrDefault();
            if(!hit.collider)
                return;

            var chara = hit.collider.GetComponentInParent<ChaControl>();
            if(!chara)
                return;

            switch(hit.collider.name)
            {
                case "cf_hit_spine01":
                case "cf_hit_spine03":
                case "cf_hit_berry":
                {
                    chara.SetClothesStateNext(ClothingType.Top);
                    return;
                }

                case "cf_hit_bust02_L":
                case "cf_hit_bust02_R":
                {
                    if(chara.GetClothingState(ClothingType.Top) == ClothingState.On)
                        chara.SetClothesStateNext(ClothingType.Top);
                    else
                        chara.SetClothesStateNext(ClothingType.Bra);

                    return;
                }

                case "cf_hit_waist_R":
                case "cf_hit_waist_L":
                case "aibu_hit_siri_L":
                case "aibu_hit_siri_R":
                {
                    chara.SetClothesStateNext(ClothingType.Bottom);
                    return;
                }

                case "aibu_hit_ana":
                case "aibu_hit_kokan":
                {
                    chara.SetClothesStateNext(ClothingType.Pantsu);
                    return;
                }

                case "aibu_reaction_legL":
                case "aibu_reaction_legR":
                {
                    chara.SetClothesStateNext(ClothingType.Socks);
                    return;
                }

                case "aibu_reaction_thighR":
                case "aibu_reaction_thighL":
                {
                    chara.SetClothesStateNext(ClothingType.Pantyhose);
                    return;
                }

                case "cf_hit_arm_L":
                case "cf_hit_arm_R":
                case "cf_hit_wrist_L":
                case "cf_hit_wrist_R":
                {
                    chara.SetClothesStateNext(ClothingType.Gloves);
                    return;
                }

                default:
                {
                    //Logger.LogInfo(hit.collider.name);
                    break;
                }
            }
        }
    }

    public static class Extensions
    {
        public static void SetClothesStateNext(this ChaControl chara, ClothingType clothingType)
        {
            chara.SetClothesStateNext((int)clothingType);
        }

        public static ClothingState GetClothingState(this ChaControl chara, ClothingType clothingType)
        {
            return (ClothingState)chara.fileStatus.clothesState[(int)clothingType];
        }
    }

    public enum ClothingType
    {
        Top,
        Bottom,
        Bra,
        Pantsu,
        Gloves,
        Pantyhose,
        Socks,
        Shoes
    }

    public enum ClothingState
    {
        On,
        Half,
        Off,
        Special
    }
}
