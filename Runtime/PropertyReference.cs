using System.Collections.Generic;
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
    public abstract class BasePropertyReference<T> : IGetSetSource
    {
        public abstract Component component { get; set; }
        [Dropdown(nameof(GetValidProperties))] public string propertyPath;
        [field: SerializeField] public T Value { get; private set; }

        private PropertyInfo _propertyInfo;
        private PropertyInfo propertyInfo => _propertyInfo ??= component?.GetType().GetRuntimeProperty(propertyPath);

        protected string[] GetValidProperties()
        {
            var targetType = component?.GetType();
            return targetType?.GetProperties(BindingFlags.Instance | BindingFlags.Public)
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

    interface IGetSetSource
    {
        public void ApplyToSource();
        public void GetFromSource();
    }

    [System.Serializable]
    public class PropertyReference<T> : BasePropertyReference<T>
    {
        [field: SerializeField] public override Component component { get; set; }
    }
    
    
#if UNITY_EDITOR
    // [CustomPropertyDrawer(typeof(FieldReference<float>))]
    // public class FloatDrawer : Drawer<float>{}
    [CustomPropertyDrawer(typeof(PropertyReference<>))]
    public class FieldReferenceDrawer : PropertyDrawer
    {
        private List<SerializedProperty> propertyList;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? EditorGUIUtility.singleLineHeight * 4f : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (propertyList == null)
            {
                propertyList = new List<SerializedProperty>();
                var enumerator = property.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    property = enumerator.Current as SerializedProperty;
                    propertyList.Add(property.Copy());
                }
                property.Reset();
            }
            // base.OnGUI(position, property, label);
            // var instance = property.GetInternalStructValue() as FieldReference<T>;
            
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout (position, property.isExpanded, "");
            position.width -= 60;
            EditorGUI.PropertyField(position, propertyList[1], new GUIContent(property.displayName));
            position.height += 3;
            if (GUI.Button(new Rect(position.x+position.width,position.y,30,position.height), "get"))
            {
                ((IGetSetSource) property.GetInternalStructValue()).GetFromSource();
                property.serializedObject.ApplyModifiedProperties();
            }
            if (GUI.Button(new Rect(position.x+position.width+30,position.y,30,position.height), "set"))
            {
                ((IGetSetSource) property.GetInternalStructValue()).ApplyToSource();
            }
            position.width += 60;
            if (!property.isExpanded) return;
            position.y += position.height + 2;
            EditorGUI.indentLevel++;
            position = EditorGUI.IndentedRect(position);
            // position.x += 16;
            // position.width -= 16;
            position.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.PropertyField(position, propertyList[2]);
            position.y += position.height + 2;
            EditorGUI.PropertyField(position, propertyList[0]);
            position.y += position.height + 2;
            // EditorGUI.PropertyField(position, propertyList[1]);
            EditorGUI.indentLevel--;
        }
    }
#endif
    
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
