using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Collections;

namespace elZach.Common
{
    public class ScriptUtilityWindow : EditorWindow
    {
        [MenuItem("Window/Tools/Script Utility")]
        static void Init()
        {
            ScriptUtilityWindow window = (ScriptUtilityWindow)EditorWindow.GetWindow(typeof(ScriptUtilityWindow));
            window.titleContent = new GUIContent("Script Utility");
            window.minSize = new Vector2(150, 50);
            window.Show();
        }
        MonoScript script;
        string iconName;

        bool foldout_icon, foldout_scriptableObject;

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Script");
            script = (MonoScript)EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
            EditorGUILayout.EndHorizontal();

            if (foldout_icon = EditorGUILayout.Foldout(foldout_icon,"Set Icon"))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Icon Name");
                iconName = EditorGUILayout.TextField(iconName);
                if(GUILayout.Button("Open List"))
                {
                    Application.OpenURL("https://unitylist.com/p/5c3/Unity-editor-icons");
                }
                EditorGUILayout.EndHorizontal();
                if (iconName != null && iconName.Length > 0)
                {
                    GUILayout.Label(EditorGUIUtility.IconContent(iconName));
                }
                if (GUILayout.Button("Assign Icon To Script"))
                {
                    AssignIcon(script, iconName);
                }
            }
            if (foldout_scriptableObject = EditorGUILayout.Foldout(foldout_scriptableObject, "Create ScriptableObject"))
            {
                if (GUILayout.Button("Create ScriptableObject"))
                {
                    CreateScriptableObject(script);
                }
            }
            
        }

        public static void CreateScriptableObject(MonoScript script)
        {
            string scriptPath = AssetDatabase.GetAssetPath(script);
            string assetPath = scriptPath.Remove(scriptPath.Length - 3) + ".asset";
            var objectClass = script.GetClass();
            if (objectClass.IsSubclassOf(typeof(ScriptableObject)))
            {
                var obj = ScriptableObject.CreateInstance(objectClass);
                AssetDatabase.CreateAsset(obj, assetPath);
                Debug.Log("Created scriptable object at: " + assetPath);
            }
            else
            {
                Debug.LogWarning("Not a valid script to generate Scriptableobject from. Make sure it inherits ScriptableObject.");
            }
        }

        public static void AssignIcon(MonoScript script, string iconName)
        {
            Texture2D tex = EditorGUIUtility.IconContent(iconName).image as Texture2D;
            Type editorGUIUtilityType = typeof(EditorGUIUtility);
            BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
            object[] args = new object[] { script, tex };
            editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
            var scriptClass = script.GetClass();
            EditorUtility.SetDirty(script);

            MethodInfo SetIconForObject = editorGUIUtilityType.GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo CopyMonoScriptIconToImporters = typeof(MonoImporter).GetMethod("CopyMonoScriptIconToImporters", BindingFlags.Static | BindingFlags.NonPublic);

            SetIconForObject.Invoke(null, new object[] { script, tex });
            CopyMonoScriptIconToImporters.Invoke(null, new object[] { script });

            DisableGizmoIcon(script);

            Debug.Log("Set icon " + iconName + " to script " + script.name);
        }

        public static void DisableGizmoIcon(MonoScript script)
        {
            //----Doesnt Work----//
            Type AnnotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var scriptClass = script.GetClass();

            MethodInfo getAnnotations = AnnotationUtility.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo setIconEnabled = AnnotationUtility.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            MethodInfo setGizmoEnabled = AnnotationUtility.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);

            var annotations = getAnnotations.Invoke(null, null);
            foreach (object annotation in (IEnumerable)annotations)
            {
                Type annotationType = annotation.GetType();
                FieldInfo classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo scriptClassField = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);
                if (classIdField != null && scriptClassField != null)
                {
                    //int classId = (int)classIdField.GetValue(annotation);
                    string _scriptClass = (string)scriptClassField.GetValue(annotation);
                    if (_scriptClass == scriptClass.Name)
                    {
                        Debug.Log(_scriptClass);
                        int classId = (int)classIdField.GetValue(annotation);
                        setIconEnabled.Invoke(null, new object[] { classId, _scriptClass, 0 });
                    }
                }
            }
        }
    }
}