using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace elZach.Common
{
    public class AnimatableHelpers
    {
        [Serializable]
        public class ColorReference : BasePropertyReference<Color>
        {
            [field: SerializeField]
            [field: Dropdown(nameof(Animatable.GetValidComponents),true)]
            public override Component component { get; set; }
        }
        [Serializable] public class FloatReference : PropertyReference<float>{}
        public static object Lerp(object a, object b, float time)
        {
            var type = a.GetType();
            if (type == typeof(float)) return Mathf.LerpUnclamped((float) a, (float) b, time);
            // if (type == typeof(Vector2)) return Vector2.LerpUnclamped((Vector2) a, (Vector2) b, time);
            // if (type == typeof(Vector3)) return Vector3.LerpUnclamped((Vector3) a, (Vector3) b, time);
            // if (type == typeof(Vector4)) return Vector4.LerpUnclamped((Vector4) a, (Vector4) b, time);
            // if (type == typeof(Quaternion)) return Quaternion.LerpUnclamped((Quaternion) a, (Quaternion) b, time);
            if (type == typeof(Color)) return Color.LerpUnclamped((Color) a, (Color) b, time);
            // if (type == typeof(int)) return Mathf.RoundToInt(Mathf.LerpUnclamped((int) a, (int) b, time));

            return null;
        }
    }
}