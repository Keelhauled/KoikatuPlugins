﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace RealPov
{
    [BepInPlugin(GUID, "RealPov", Version)]
    public class RealPov : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.realpov";
        public const string Version = "1.0.0";

        private const string SECTION_GENERAL = "General";
        private const string SECTION_HOTKEYS = "Keyboard Shortcuts";

        private static ConfigWrapper<float> ViewOffset { get; set; }
        private static ConfigWrapper<float> DefaultFov { get; set; }
        private static ConfigWrapper<float> MouseSens { get; set; }
        private static ConfigWrapper<KeyboardShortcut> PovHotkey { get; set; }

        private Harmony harmony;
        private static List<Vector3> rotation = new List<Vector3> { new Vector3(), new Vector3() };
        private static ChaControl currentChara;
        private static bool povEnabled = false;
        private static float currentFov;
        private static float backupFov;
        private static Vector3 backupPos;

        private void Awake()
        {
            ViewOffset = Config.GetSetting(SECTION_GENERAL, "ViewOffset", 0.03f);
            DefaultFov = Config.GetSetting(SECTION_GENERAL, "DefaultFov", 70f);
            MouseSens = Config.GetSetting(SECTION_GENERAL, "MouseSens", 1f);
            PovHotkey = Config.GetSetting(SECTION_HOTKEYS, "PovHotkey", new KeyboardShortcut(KeyCode.Backspace));

            harmony = new Harmony("keelhauled.realpov.harmony");
            HarmonyWrapper.PatchAll(typeof(Hooks), harmony);

            currentChara = FindObjectOfType<ChaControl>();
            currentFov = DefaultFov.Value;

            foreach(var item in currentChara.neckLookCtrl.neckLookScript.aBones)
                item.neckBone.rotation = new Quaternion();
        }

        private void Update()
        {
            if(PovHotkey.Value.IsDown())
                TogglePov();

            if(povEnabled)
            {
                if(Input.GetMouseButton(0))
                {
                    var x = Input.GetAxis("Mouse X") * MouseSens.Value;
                    var y = -Input.GetAxis("Mouse Y") * MouseSens.Value;

                    rotation[0] += new Vector3(y, x, 0f);
                    rotation[1] += new Vector3(y, x, 0f);
                }
                else if(Input.GetMouseButton(1))
                {
                    currentFov += Input.GetAxis("Mouse X");
                }
            }
        }

#if DEBUG
        private void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
        }
#endif

        private void TogglePov()
        {
            if(povEnabled)
            {
                Camera.main.fieldOfView = backupFov;
                Camera.main.transform.position = backupPos;
                povEnabled = false;
            }
            else
            {
                backupFov = Camera.main.fieldOfView;
                backupPos = Camera.main.transform.position;
                povEnabled = true;
            }
        }

        private class Hooks
        {
            [HarmonyPrefix, HarmonyPatch(typeof(NeckLookControllerVer2), "LateUpdate")]
            public static bool ApplyRotation(NeckLookControllerVer2 __instance)
            {
                if(povEnabled)
                {
                    if(__instance.neckLookScript && currentChara.neckLookCtrl == __instance)
                    {
                        __instance.neckLookScript.aBones[0].neckBone.rotation = Quaternion.identity;
                        __instance.neckLookScript.aBones[1].neckBone.rotation = Quaternion.identity;

                        //__instance.neckLookScript.aBones[0].neckBone.Rotate(rotation[0]);
                        __instance.neckLookScript.aBones[1].neckBone.Rotate(rotation[1]);
                    }

                    var eyeObjs = currentChara.eyeLookCtrl.eyeLookScript.eyeObjs;
                    Camera.main.transform.position = Vector3.Lerp(eyeObjs[0].eyeTransform.position, eyeObjs[1].eyeTransform.position, 0.5f);
                    Camera.main.transform.rotation = currentChara.objHeadBone.transform.rotation;
                    Camera.main.transform.Translate(Vector3.forward * ViewOffset.Value);
                    Camera.main.fieldOfView = currentFov;

                    return false;
                }

                return true;
            }

            [HarmonyPrefix, HarmonyPatch(typeof(CameraControl_Ver2), "LateUpdate")]
            public static bool StopNormalCameraData(CameraControl_Ver2 __instance)
            {
                return !povEnabled;
            }
        }
    }
}
