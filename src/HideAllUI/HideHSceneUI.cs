using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HideAllUI
{
    class HideHSceneUI : HideUI
    {
        IEnumerable<Canvas> canvasList;
        bool visible = true;

        public HideHSceneUI()
        {
            canvasList = GameObject.FindObjectsOfType<Canvas>().Where(x => x.name == "Canvas");
        }

        public override void ToggleUI()
        {
            visible = !visible;
            foreach(var canvas in canvasList.Where(x => x))
                canvas.enabled = visible;
        }
    }
}
