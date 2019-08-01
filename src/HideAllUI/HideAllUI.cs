using BepInEx;
using ChaCustom;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace HideAllUI
{
    [BepInPlugin(GUID, "HideAllUI", Version)]
    public class HideAllUI : BaseUnityPlugin
    {
        public const string GUID = "keelhauled.hideallui";
        public const string Version = "1.0.0";

        // must be static for the transpiler
        public static SavedKeyboardShortcut HideHotkey { get; set; }

        HarmonyInstance harmony;
        static HideUI currentUIHandler;

        void Awake()
        {
            HideHotkey = new SavedKeyboardShortcut("HideHotkey", this, new KeyboardShortcut(KeyCode.Space));

            harmony = HarmonyInstance.Create("keelhauled.hideallui.harmony");
            harmony.PatchAll(GetType());
        }

        void OnDestroy()
        {
            harmony.UnpatchAll(GetType());
        }

        void Update()
        {
            if(currentUIHandler != null && HideHotkey.IsDown())
                currentUIHandler.ToggleUI();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(CustomControl), "Update")]
        public static IEnumerable<CodeInstruction> SetMakerHotkey(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var inputGetKeyDown = AccessTools.Method(typeof(Input), nameof(Input.GetKeyDown), new Type[] { typeof(KeyCode) });

            for(int i = 0; i < codes.Count; i++)
            {
                if(codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand is sbyte val && val == (sbyte)KeyCode.Space)
                {
                    if(codes[i + 1].opcode == OpCodes.Call && codes[i + 1].operand == inputGetKeyDown)
                    {
                        codes[i].opcode = OpCodes.Call;
                        codes[i].operand = AccessTools.Property(typeof(HideAllUI), nameof(HideHotkey)).GetGetMethod();
                        codes[i + 1] = new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(SavedKeyboardShortcut), nameof(SavedKeyboardShortcut.IsDown)));
                        break;
                    }
                }
            }

            return codes;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Studio.Studio), "Init")]
        public static void StudioInit()
        {
            currentUIHandler = new HideStudioUI();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "SetShortcutKey")]
        public static void HSceneStart()
        {
            currentUIHandler = new HideHSceneUI();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "OnDestroy")]
        public static void HSceneEnd()
        {
            currentUIHandler = null;
        }
    }
}
