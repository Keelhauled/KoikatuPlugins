using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using UnityEngine;
using Vectrosity;
using Studio;
using BepInEx.Logging;

namespace StudioUX
{
    [BepInPlugin("keelhauled.studiouxexperiment", "StudioUX", "1.0.0")]
    public class StudioUX : BaseUnityPlugin
    {
        List<VectorLine> lines = new List<VectorLine>();

        void Start()
        {
            var characters = FindObjectsOfType<ChaControl>();

            foreach(var chara in characters)
            {
                foreach(var renderer in chara.GetComponentsInChildren<Renderer>())
                {
                    var points = new List<Vector3>(24);
                    var line = new VectorLine("Cube", points, 2f);
                    line.MakeCube(renderer.bounds.center, renderer.bounds.extents.x, renderer.bounds.extents.z, renderer.bounds.extents.y);
                    lines.Add(line); 
                }
            }
        }

        void Update()
        {
            lines.ForEach(x => x.Draw());
        }

        void OnDestroy()
        {
            VectorLine.Destroy(lines);
        }
    }
}
