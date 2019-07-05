using BepInEx;
using BepInEx.Harmony;
using BepInEx.Logging;
using HarmonyLib;
using Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Vectrosity;

namespace StudioUX
{
    [BepInPlugin("keelhauled.studiouxexperiment", "StudioUX", "1.0.0")]
    public class StudioUX : BaseUnityPlugin
    {
        new static ManualLogSource Logger;

        void Start()
        {
            Logger = base.Logger;
            StartCoroutine(EndOfFrame());

            var characters = GuideObjectManager.Instance.selectObjectKey.Select(x => Studio.Studio.GetCtrlInfo(x) as OCIChar).Where(x => x != null);
            foreach(var chara in characters)
            {
                var skins = chara.charInfo.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach(var skin in skins) skin.updateWhenOffscreen = true;
                var updateSkins = new List<SkinnedMeshRenderer> { chara.charInfo.rendBody as SkinnedMeshRenderer, chara.charInfo.rendFace as SkinnedMeshRenderer };

                //var bounds3d = new VectorLineUpdater();
                //bounds3d.VectorLine = new VectorLine("Bounds3D", new List<Vector3>(24), 1f, LineType.Discrete);
                //bounds3d.Update = () =>
                //{
                //    var bounds = BoundsUtils.CombineBounds(new Stack<SkinnedMeshRenderer>(updateSkins));
                //    bounds3d.VectorLine.MakeCube(bounds.center, bounds.size.x, bounds.size.y, bounds.size.z);
                //    bounds3d.VectorLine.SetColor(Color.red);
                //    bounds3d.VectorLine.Draw();
                //};

                var bounds2d = new VectorLineUpdater();
                bounds2d.VectorLine = new VectorLine("Bounds2D", new List<Vector2>(8), 1f, LineType.Discrete);
                bounds2d.Update = () =>
                {
                    var bounds = BoundsUtils.CombineBounds(new Stack<SkinnedMeshRenderer>(updateSkins));
                    var rect = BoundsUtils.BoundsToScreenRect(bounds, Camera.main);
                    bounds2d.VectorLine.MakeRect(rect);
                    bounds2d.VectorLine.SetColor(Color.green);
                    bounds2d.VectorLine.Draw();
                };
            }
        }

        IEnumerator EndOfFrame()
        {
            while(true)
            {
                yield return new WaitForEndOfFrame();
                VectorLineUpdater.UpdateAllLines();
            }
        }

        void Update()
        {
            if(Input.GetMouseButtonDown(1))
            {
                var mousePos = Input.mousePosition;
                Logger.Log(LogLevel.Info, mousePos);
            }
        }

        void OnDestroy()
        {
            VectorLineUpdater.DeleteAllLines();
        }
    }

    public class VectorLineUpdater
    {
        public VectorLine VectorLine;
        public Action Update;

        public static List<VectorLineUpdater> lines = new List<VectorLineUpdater>();
        public VectorLineUpdater() => lines.Add(this);
        public static void UpdateAllLines() => lines.ForEach(x => x.Update());
        public static void DeleteAllLines() => lines.ForEach(x => VectorLine.Destroy(ref x.VectorLine));
    }
}
