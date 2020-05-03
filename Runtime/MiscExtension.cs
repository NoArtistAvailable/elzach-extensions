using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace elZach.Common
{
    public static class MiscExtension
    {
        public static Keyframe GetLastKey(this AnimationCurve curve)
        {
            return curve.keys[curve.keys.Length - 1];
        }

        public static Texture2D GetCopy(this Texture2D source)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(source, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D copy = new Texture2D(source.width, source.height);
            copy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            copy.Apply();
            RenderTexture.active = previous;

            return copy;
        }
    }
}