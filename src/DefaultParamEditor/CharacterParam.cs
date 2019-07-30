using BepInEx.Logging;
using Harmony;
using Studio;
using System.Linq;
using static BepInEx.Logger;

namespace DefaultParamEditor
{
    public class CharacterParam
    {
        private static ParamData.CharaData _charaData;

        public CharacterParam(ParamData.CharaData data)
        {
            _charaData = data;
            var harmony = HarmonyInstance.Create("keelhauled.defaultparameditor.characterparam.harmony");
            harmony.PatchAll(typeof(CharacterParam));
        }

        public void Save()
        {
            var selected = GuideObjectManager.Instance.selectObjectKey
                .Select(x => Studio.Studio.GetCtrlInfo(x) as OCIChar).Where(x => x != null).ToList();

            if(selected.Count > 0)
            {
                var status = selected[0].charFileStatus;

                _charaData.clothesState = status.clothesState.ToArray();
                _charaData.shoesType = status.shoesType;
                _charaData.hohoAkaRate = status.hohoAkaRate;
                _charaData.nipStandRate = status.nipStandRate;
                _charaData.tearsLv = status.tearsLv;

                _charaData.eyesLookPtn = status.eyesLookPtn;
                _charaData.neckLookPtn = status.neckLookPtn;
                _charaData.eyebrowPtn = status.eyebrowPtn;
                _charaData.eyesPtn = status.eyesPtn;
                _charaData.eyesOpenMax = status.eyesOpenMax;
                _charaData.eyesBlink = status.eyesBlink;
                _charaData.mouthPtn = status.mouthPtn;

                _charaData.saved = true;
                Log(LogLevel.Message, "Default character settings saved");
            }
            else
            {
                Log(LogLevel.Message, "Warning: Select character to save default settings");
            }
        }

        public void Reset()
        {
            _charaData.saved = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ChaFileStatus), nameof(ChaFileStatus.MemberInit))]
        public static void HarmonyPatch_ChaFileStatus_MemberInit(ChaFileStatus __instance)
        {
            if(_charaData.saved)
            {
                Log(LogLevel.Debug, "Loading defaults for a new character");

                __instance.clothesState = _charaData.clothesState.ToArray();
                __instance.shoesType = _charaData.shoesType;
                __instance.hohoAkaRate = _charaData.hohoAkaRate;
                __instance.nipStandRate = _charaData.nipStandRate;
                __instance.tearsLv = _charaData.tearsLv;

                __instance.eyesLookPtn = _charaData.eyesLookPtn;
                __instance.neckLookPtn = _charaData.neckLookPtn;
                __instance.eyebrowPtn = _charaData.eyebrowPtn;
                __instance.eyesPtn = _charaData.eyesPtn;
                __instance.eyesOpenMax = _charaData.eyesOpenMax;
                __instance.eyesBlink = _charaData.eyesBlink;
                __instance.mouthPtn = _charaData.mouthPtn;
            }
        }
    }
}
