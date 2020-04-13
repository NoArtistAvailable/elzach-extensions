using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace elZach.Common
{
    public static class InspectorExtension 
    {
        public static void DrawButtons(this Editor editor)
        {
            var methods = editor.target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetParameters().Length == 0);

            foreach (var method in methods)
            {
                var ba = (ButtonAttribute)Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));
                
                if (ba != null)
                {
                    var buttonName = ba.Name;
                    if (GUILayout.Button(buttonName))
                    {
                        foreach (var t in editor.targets)
                        {
                            method.Invoke(t, null);
                        }
                    }
                    GUILayout.Space(10);
                }
            }
        }
    }

}