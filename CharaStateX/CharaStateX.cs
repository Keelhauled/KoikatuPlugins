﻿using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Harmony;
using Studio;

namespace CharaStateX
{
    [BepInPlugin("keelhauled.charastatex", "CharaStateX", "1.0.0")]
    class CharaStateX : BaseUnityPlugin
    {
        void Awake()
        {
            var harmony = HarmonyInstance.Create("keelhauled.charastatex.harmony");
            StateInfoPatch.Patch(harmony);
            NeckLookPatch.Patch(harmony);
            EtcInfoPatch.Patch(harmony);
            HandInfoPatch.Patch(harmony);
            harmony.PatchAll(typeof(AnimationPatch));
            JointInfoPatch.Patch(harmony);
            FKIKPatch.Patch(harmony);
        }

        public static List<OCIChar> GetSelectedCharacters()
        {
            return GuideObjectManager.Instance.selectObjectKey.Select(x => Studio.Studio.GetCtrlInfo(x) as OCIChar).Where(x => x != null).ToList();
        }
    }
}