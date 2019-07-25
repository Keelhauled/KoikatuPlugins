using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Harmony;
using BepInEx.Logging;
using Logger = BepInEx.Logger;
using UnityEngine;

namespace RealPov
{
    [BepInPlugin(GUID, "RealPov", Version)]
    public class RealPov : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.realpov";
        public const string Version = "1.0.0";

        public static ConfigWrapper<float> ViewOffset { get; set; }
        public static ConfigWrapper<float> DefaultFov { get; set; }
        public static ConfigWrapper<float> MouseSens { get; set; }
        public static SavedKeyboardShortcut PovHotkey { get; set; }

        HarmonyInstance harmony;
        static List<Vector3> rotation = new List<Vector3> { new Vector3(), new Vector3() };
        static ChaControl currentChara;
        static bool povEnabled = false;
        static float currentFov;

        static float backupFov;
        static Vector3 backupPos;

        void Awake()
        {
            ViewOffset = new ConfigWrapper<float>("ViewOffset", this, 0.03f);
            DefaultFov = new ConfigWrapper<float>("DefaultFov", this, 70f);
            MouseSens = new ConfigWrapper<float>("MouseSens", this, 1f);
            PovHotkey = new SavedKeyboardShortcut("PovHotkey", this, new KeyboardShortcut(KeyCode.Backspace));

            harmony = HarmonyInstance.Create("keelhauled.realpov.harmony");
            harmony.PatchAll(GetType());

            currentChara = FindObjectOfType<ChaControl>();
            currentFov = DefaultFov.Value;

            foreach(var item in currentChara.neckLookCtrl.neckLookScript.aBones)
                item.neckBone.rotation = new Quaternion();
        }

        void Update()
        {
            if(PovHotkey.IsDown())
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

        void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
        }

        void TogglePov()
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
