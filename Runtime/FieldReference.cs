using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
// #if UNITY_EDITOR
// using UnityEditor;
// using elZach.Access;
// #endif

namespace elZach.Common
{
    [System.Serializable]
    public abstract class ComponentReference<T>
    {
        public Component component;
        [Dropdown(nameof(GetValidProperties))] public string propertyPath;
        protected abstract string[] GetValidProperties();
        public T value;
        public void SetTargetValue() => SetTargetValue(value);
        public void SetFromSource() => value = GetTargetValue();

        public abstract void SetTargetValue(T value);
        public abstract T GetTargetValue();
    }
    
    [System.Serializable]
    public class FieldReference<T> : ComponentReference<T>
    {
        private FieldInfo _nfo;
        FieldInfo nfo => _nfo ??= component?.GetType().GetRuntimeField(propertyPath);
        
        public override void SetTargetValue(T value)=>
            nfo?.SetValue(component, value);

        public override T GetTargetValue()
        {
            if (nfo == null) return default;
            return (T) nfo.GetValue(component);
        }
        
        protected override string[] GetValidProperties()
        {
            var targetType = component.GetType();
            return targetType.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.FieldType == typeof(T))
                .Select(x => x.Name).ToArray();
        }
    }
    
    [System.Serializable]
    public class PropertyReference<T> : ComponentReference<T>
    {
        private PropertyInfo _nfo;
        PropertyInfo nfo => _nfo ??= component?.GetType().GetRuntimeProperty(propertyPath);
        
        public override void SetTargetValue(T value)=> nfo?.SetValue(component, value);
        
        public override T GetTargetValue()
        {
            if (nfo == null) return default;
            return (T) nfo.GetValue(component);
        }
        protected override string[] GetValidProperties()
        {
            var targetType = component.GetType();
            return targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType == typeof(T))
                .Select(x => x.Name).ToArray();
        }
    }
    
// #if UNITY_EDITOR
//     [CustomPropertyDrawer(typeof(FieldReference<float>))]
//     public class FloatDrawer : Drawer<float>{}
//         
//     public class Drawer<T> : PropertyDrawer
//     {
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             return EditorGUIUtility.singleLineHeight * 4f;
//         }
//
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             var instance = property.GetInternalStructValue() as FieldReference<T>;
//             position.height = EditorGUIUtility.singleLineHeight;
//             EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(FieldReference<T>.component)));
//                 
//             if (instance.component)
//             {
//                 position.y += position.height;
//                 EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(FieldReference<T>.propertyPath)));
//                 if (!string.IsNullOrEmpty(instance.propertyPath))
//                 {
//                     position.y += position.height;
//                     EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(FieldReference<T>.value)));
//                 }
//             }
//         }
//     }
// #endif
}
