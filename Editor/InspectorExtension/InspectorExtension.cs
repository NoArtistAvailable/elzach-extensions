using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using elZach.Access;
using Object = UnityEngine.Object;

namespace elZach.Common
{
    public static class InspectorExtension 
    {
        public static void DrawButtons(this Editor editor)
        {
            // var methods = editor.target.GetType()
            //     .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            //     .Where(m => m.GetParameters().Length == 0);
            //
            // bool buttons = false;
            // foreach (var method in methods)
            // {
            //     var ba = (ButtonAttribute)Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));
            //     if (ba != null)
            //     {
            //         buttons = true;
            //         var buttonName = ba.Name;
            //         if (GUILayout.Button(buttonName))
            //         {
            //             foreach (var t in editor.targets)
            //             {
            //                 method.Invoke(t, null);
            //             }
            //         }
            //         GUILayout.Space(3);
            //     }
            // }
            // if(buttons) GUILayout.Space(10);
        }

        // public static void DrawBasicProperty(Rect position, SerializedProperty property, GUIContent label)
        // {
        //     position.height = EditorGUIUtility.singleLineHeight;
        //     EditorGUI.PropertyField(position, property, label);
        //     if (!property.isExpanded) return;
        //     position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        //     var propIterator = property.GetEnumerator();
        //     var endProperty = property.GetEndProperty();
        //     EditorGUI.indentLevel++;
        //     while (propIterator.MoveNext() && propIterator.Current != endProperty)
        //     {
        //         EditorGUI.PropertyField(position, property);
        //         position.y += EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.standardVerticalSpacing;
        //     }
        //     EditorGUI.indentLevel--;
        // }

        public static object GetBoxedValue(this SerializedProperty property)
        {
            switch(property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return property.GetInternalStructValue();
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return property.intValue;
                case SerializedPropertyType.Enum:
                    return property.intValue;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.ArraySize:
                    return property.arraySize;
                case SerializedPropertyType.Character:
                    Debug.LogWarning("Character currently not supported");
                    return property.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue;
                case SerializedPropertyType.Gradient:
                    Debug.LogWarning("Gradient currently not supported");
                    return property.intValue;
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize;
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue;
                case SerializedPropertyType.ManagedReference:
                    Debug.LogWarning("Getting ManagedReference currently not supported");
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static void SetBoxedValue(this SerializedProperty property, object value)
        {
            switch(property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    property.SetInternalStructValue(value);
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = (int) value;
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = (bool) value;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = (float) value;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = (string) value;
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = (Color) value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = (UnityEngine.Object) value;
                    break;
                case SerializedPropertyType.LayerMask:
                    property.enumValueIndex = (int) value;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = (int) value;
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2) value;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3) value;
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4) value;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect) value;
                    break;
                case SerializedPropertyType.ArraySize:
                    property.arraySize = (int) value;
                    break;
                case SerializedPropertyType.Character:
                    Debug.LogWarning("Characters not supported yet!");
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = (AnimationCurve) value;
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = (Bounds) value;
                    break;
                case SerializedPropertyType.Gradient:
                    Debug.LogWarning("Gradients not supported yet!");
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (Quaternion) value;
                    break;
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = (UnityEngine.Object) value;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = (Vector2Int) value;
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = (Vector3Int) value;
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = (RectInt) value;
                    break;
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = (BoundsInt) value;
                    break;
                case SerializedPropertyType.ManagedReference:
                    property.managedReferenceValue = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}