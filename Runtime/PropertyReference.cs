using System.Linq;
using System.Reflection;
using UnityEngine;
using Component = UnityEngine.Component;
#if UNITY_EDITOR
using UnityEditor;
using elZach.Access;
#endif

namespace elZach.Common
{
    public abstract class BasePropertyReference<T>
    {
        public abstract Component component { get; set; }
        [Dropdown(nameof(GetValidProperties))] public string propertyPath;
        [field: SerializeField] public T Value { get; private set; }

        private PropertyInfo _propertyInfo;
        private PropertyInfo propertyInfo => _propertyInfo ??= component?.GetType().GetRuntimeProperty(propertyPath);

        protected string[] GetValidProperties()
        {
            var targetType = component.GetType();
            return targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType == typeof(T))
                .Select(x => x.Name).ToArray();
        }

        public void ApplyToSource() => TargetSourceValue = Value;
        public void GetFromSource() => Value = TargetSourceValue;

        public T TargetSourceValue
        {
            get => propertyInfo == null ? default : (T) propertyInfo.GetValue(component);
            set => propertyInfo?.SetValue(component, value);
        }
    }

    [System.Serializable]
    public class PropertyReference<T> : BasePropertyReference<T>
    {
        [field: SerializeField] public override Component component { get; set; }
    }
    
    
// #if UNITY_EDITOR
//     // [CustomPropertyDrawer(typeof(FieldReference<float>))]
//     // public class FloatDrawer : Drawer<float>{}
//     [CustomPropertyDrawer(typeof(FieldReference<>))]
//     public class FieldReferenceDrawer : PropertyDrawer
//     {
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             return EditorGUIUtility.singleLineHeight * 5f;
//         }
//
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             // base.OnGUI(position, property, label);
//             // var instance = property.GetInternalStructValue() as FieldReference<T>;
//             position.height = EditorGUIUtility.singleLineHeight;
//             EditorGUI.LabelField(position, label);
//             position.y += position.height;
//             // position = EditorGUI.IndentedRect(position);
//             position.x += 16;
//             position.width -= 16;
//             position.height = EditorGUIUtility.singleLineHeight;
//             EditorGUI.PropertyField(position, property.FindPropertyRelative("component"));
//                     
//             // if (instance.component)
//             {
//                 position.y += position.height;
//                 EditorGUI.PropertyField(position, property.FindPropertyRelative("propertyPath"));
//                 // if (!string.IsNullOrEmpty(instance.propertyPath))
//                 {
//                     position.y += position.height;
//                     EditorGUI.PropertyField(position, property.FindPropertyRelative("value"));
//                 }
//             }
//         }
//     }
// #endif
    
    // [System.Serializable]
    // public class FieldReference<T> : ComponentReference<T>
    // {
    //     [field : SerializeField] public override Component component { get; set; }
    //     private FieldInfo _nfo;
    //     FieldInfo nfo => _nfo ??= component?.GetType().GetRuntimeField(propertyPath);
    //     
    //     public override void SetTargetValue(T value)=>
    //         nfo?.SetValue(component, value);
    //
    //     public override T GetTargetValue()
    //     {
    //         if (nfo == null) return default;
    //         return (T) nfo.GetValue(component);
    //     }
    //     
    //     protected override string[] GetValidProperties()
    //     {
    //         var targetType = component.GetType();
    //         return targetType.GetFields(BindingFlags.Instance | BindingFlags.Public)
    //             .Where(x => x.FieldType == typeof(T))
    //             .Select(x => x.Name).ToArray();
    //     }
    // }
    
    
}
