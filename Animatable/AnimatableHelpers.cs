using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
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
            [SerializeField, Dropdown(nameof(Animatable.GetValidComponents),true)] 
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
            protected override string[] GetValidProperties()
            {
                if (component is Renderer renderer)
                    return renderer.sharedMaterial.shader.GetPropertyNames(ShaderPropertyType.Color).ToArray();
                return base.GetValidProperties();
            }
            public override Color TargetSourceValue 
            {
                get => component is Renderer renderer ? renderer.GetColorFromBlock(propertyPath) : base.TargetSourceValue;
                set
                {
                    if (component is Renderer renderer) renderer.SetPropertyOnBlock(propertyPath, value);
                    else propertyInfo?.SetValue(component, value);
                }
            }

            public override void ApplyTo(GameObject target, Color targetValue)
            {
                if (target == component.gameObject)
                {
                    TargetSourceValue = targetValue;
                    return;
                }
                if (component is Renderer)
                {
                    var renderer = target.GetComponent<Renderer>();
                    renderer?.SetPropertyOnBlock(propertyPath, targetValue);
                    return;
                } 
                base.ApplyTo(target, targetValue);
            }
        }

        [Serializable]
        public class FloatReference : BasePropertyReference<float>
        {
            [SerializeField, Dropdown(nameof(Animatable.GetValidComponents),true)] 
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
            
            protected override string[] GetValidProperties()
            {
                if (component is Renderer renderer)
                    return renderer.sharedMaterial.shader.GetPropertyNames(ShaderPropertyType.Float)
                        .Concat(renderer.sharedMaterial.shader.GetPropertyNames(ShaderPropertyType.Range))
                        .ToArray();
                return base.GetValidProperties();
            }
            public override float TargetSourceValue 
            {
                get => component is Renderer renderer ? renderer.GetFloatFromBlock(propertyPath) : base.TargetSourceValue;
                set
                {
                    if (component is Renderer renderer) renderer.SetPropertyOnBlock(propertyPath, value);
                    else propertyInfo?.SetValue(component, value);
                }
            }
            public override void ApplyTo(GameObject target, float targetValue)
            {
                if (target == component.gameObject)
                {
                    TargetSourceValue = targetValue;
                    return;
                }
                if (component is Renderer)
                {
                    var renderer = target.GetComponent<Renderer>();
                    renderer.SetPropertyOnBlock(propertyPath, targetValue);
                    return;
                } 
                base.ApplyTo(target, targetValue);
            }
        }
        public static object Lerp(object a, object b, float time)
        {
            return a switch
            {
                float f => Mathf.LerpUnclamped(f, (float) b, time),
                // if (type == typeof(Vector2)) return Vector2.LerpUnclamped((Vector2) a, (Vector2) b, time);
                // if (type == typeof(Vector3)) return Vector3.LerpUnclamped((Vector3) a, (Vector3) b, time);
                // if (type == typeof(Vector4)) return Vector4.LerpUnclamped((Vector4) a, (Vector4) b, time);
                // if (type == typeof(Quaternion)) return Quaternion.LerpUnclamped((Quaternion) a, (Quaternion) b, time);
                Color color => Color.LerpUnclamped(color, (Color) b, time),
                _ => null
            };
        }
    }
}