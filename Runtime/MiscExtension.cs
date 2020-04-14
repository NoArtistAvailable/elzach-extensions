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
    }
}