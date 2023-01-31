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
    public interface IHasPropertyPath
    {
        public string propertyPath { get; }
        public Component component { get; }
    }
    
    public abstract class BasePropertyReference<T> : IHasPropertyPath, IGetSetSource
    {
        public abstract Component component { get; set; }

        [SerializeField, Dropdown(nameof(GetValidProperties))] protected string m_propertyPath;
        public string propertyPath
        {
            get => m_propertyPath;
            set
            {
                m_propertyPath = value;
                component = null;
            }
        }

        [SerializeField] private T m_value;

        public T Value
        {
            get => m_value;
            protected set => m_value = value;
        }

        protected PropertyInfo _propertyInfo;
        protected PropertyInfo propertyInfo => _propertyInfo ??= component?.GetType().GetRuntimeProperty(propertyPath);

        protected virtual string[] GetValidProperties()
        {
            var targetType = component?.GetType();
            return targetType?.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType == typeof(T))
                .Select(x => x.Name).ToArray();
        }

        public void ApplyToSource() => TargetSourceValue = Value;
        public void GetFromSource() => Value = TargetSourceValue;

        public virtual void ApplyTo(GameObject target, T targetValue)
        {
            if (target == component.gameObject)
            {
                TargetSourceValue = targetValue;
                return;
            }
            var targetComponent = target.GetComponent(component.GetType());
            propertyInfo.SetValue(targetComponent, targetValue);
        }

        public virtual T TargetSourceValue
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
        [SerializeField] private Component m_component;
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
    
    
#if UNITY_EDITOR
    // [CustomPropertyDrawer(typeof(FieldReference<float>))]
    // public class FloatDrawer : Drawer<float>{}
    [CustomPropertyDrawer(typeof(BasePropertyReference<>), true)]
    public class PropertyReferenceDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? EditorGUIUtility.singleLineHeight * 4f : EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rect = position;
            EditorGUI.BeginProperty(rect, label, property);
            rect.height = EditorGUIUtility.singleLineHeight;
            
            property.isExpanded = EditorGUI.Foldout (rect, property.isExpanded, "");
            rect.width -= 60;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("m_value"), new GUIContent(property.displayName));
            rect.height += 3;
            if (GUI.Button(new Rect(rect.x+rect.width,rect.y,30,rect.height), "get"))
            {
                ((IGetSetSource) property.GetInternalStructValue()).GetFromSource();
                property.serializedObject.ApplyModifiedProperties();
            }
            if (GUI.Button(new Rect(rect.x+rect.width+30,rect.y,30,rect.height), "set"))
            {
                ((IGetSetSource) property.GetInternalStructValue()).ApplyToSource();
            }
            rect.width += 60;
            if (property.isExpanded)
            {
                rect.y += rect.height + 2;
                EditorGUI.indentLevel++;
                rect = EditorGUI.IndentedRect(rect);
                rect.x = position.x;
                rect.width += (rect.x - position.x);
                rect.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(rect, property.FindPropertyRelative("m_component"), new GUIContent("Component"));
                rect.y += rect.height + 2;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("m_propertyPath"), new GUIContent("Property"));
                rect.y += rect.height + 2;
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }
    }
#endif
    
}
