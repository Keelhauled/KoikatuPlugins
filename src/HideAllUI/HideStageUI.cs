using Harmony;
using UnityEngine;

namespace HideAllUI
{
    class HideStageUI : HideUI
    {
        Canvas canvas;

        public HideStageUI(LiveCharaSelectSprite instance)
        {
            canvas = Traverse.Create(instance).Field("canvas").GetValue<Canvas>();
        }

        public override void ToggleUI()
        {
            canvas.enabled = !canvas.enabled;
        }
    }
}
