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
            return source.GetCopy(RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        }

        public static Texture2D GetCopy(this Texture2D source, RenderTextureFormat format, RenderTextureReadWrite rw)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                format,
                rw);

            Graphics.Blit(source, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D copy = new Texture2D(source.width, source.height);
            copy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            copy.Apply();
            RenderTexture.active = previous;

            return copy;
        }

        //https://answers.unity.com/questions/1652854/how-to-get-set-hdr-color-intensity.html
        private const byte k_MaxByteForOverexposedColor = 191; //internal Unity const
        public static float GetIntensity(this Color col)
        {
            var maxColorComponent = col.maxColorComponent;
            var scaleFactor = k_MaxByteForOverexposedColor / maxColorComponent;
            return Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);
        }

        public static string[] GetPropertyNames(this Shader shader)
        {
            string[] propertyNames = new string[shader.GetPropertyCount()];
            for (int i = 0; i < propertyNames.Length; i++)
            {
                propertyNames[i] = shader.GetPropertyName(i);
            }
            return propertyNames;
        }
        
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
            => source.MinBy(selector, null);
        
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
            => source.MaxBy(selector, null);

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            comparer ??= Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
                var min = sourceIterator.Current;
                var minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) >= 0) continue;
                    min = candidate;
                    minKey = candidateProjected;
                }
                return min;
            }
        }
        
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            comparer ??= Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
                var max = sourceIterator.Current;
                var maxKey = selector(max);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, maxKey) <= 0) continue;
                    max = candidate;
                    maxKey = candidateProjected;
                }
                return max;
            }
        }
    }
}