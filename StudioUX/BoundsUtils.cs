using System.Collections.Generic;
using UnityEngine;

namespace StudioUX
{
    public static class BoundsUtils
    {
        public static Bounds CombineBounds(Stack<SkinnedMeshRenderer> skins)
        {
            var bounds = skins.Pop().bounds;

            while(skins.Count > 0)
            {
                var skin = skins.Pop();
                bounds.Encapsulate(skin.bounds);
            }

            return bounds;
        }

        public static Rect BoundsToScreenRect(Bounds bounds, Camera camera)
        {
            Vector3 cen = bounds.center;
            Vector3 ext = bounds.extents;
            Vector2[] extentPoints = new Vector2[8]
            {
                camera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
                camera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
                camera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
                camera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
                camera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
                camera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
                camera.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
                camera.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
            };

            Vector2 min = extentPoints[0];
            Vector2 max = extentPoints[0];
            foreach(Vector2 v in extentPoints)
            {
                min = Vector2.Min(min, v);
                max = Vector2.Max(max, v);
            }

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        public static Vector2 WorldToGUIPoint(Vector3 world)
        {
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
            screenPoint.y = (float)Screen.height - screenPoint.y;
            return screenPoint;
        }
    }
}
