using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace elZach.Common
{
    public class AnimatableHelpers
    {
        [Serializable]
        public class ColorReference : BasePropertyReference<Color>
        {
            [SerializeField,Dropdown(nameof(Animatable.GetValidComponents),true)] 
            private Component m_component;
            public override Component component
            {
                get => m_component;
                set
                {
                    m_component = value;
                    _propertyInfo = null;
                } 
            }
        }

        [Serializable]
        public class FloatReference : BasePropertyReference<float>
        {
            [SerializeField,Dropdown(nameof(Animatable.GetValidComponents),true)] 
            private Component m_component;
            public override Component component
            {
                get => m_component;
                set
                {
                    m_component = value;
                    _propertyInfo = null;
                } 
            }
        }
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