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
    }

}