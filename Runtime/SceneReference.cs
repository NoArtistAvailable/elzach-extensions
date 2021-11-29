using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace elZach.Common
{
    [System.Serializable]
    public class SceneReference
    {
        public string value;
#if UNITY_EDITOR
        [SerializeField] SceneAsset sceneAsset;
#endif
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneAsset = property.FindPropertyRelative("sceneAsset").objectReferenceValue as SceneAsset;
            
            position.height = EditorGUIUtility.singleLineHeight;
            var value = property.FindPropertyRelative("value").stringValue;
            sceneAsset = EditorGUI.ObjectField(position, label, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
            if (sceneAsset?.name != value)
            {
                value = sceneAsset?.name;
                // EditorUtility.SetDirty(property.serializedObject.targetObject);
                // property.serializedObject.Update();
            }
            position.y += EditorGUIUtility.singleLineHeight;
            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth;
            if (sceneAsset)
            {
                var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                var inBuild = EditorBuildSettings.scenes.FirstOrDefault(x => x.path == scenePath);
                if (inBuild != null)
                    EditorGUI.HelpBox(position, "Scene contained in build", MessageType.Info);
                else
                {
                    if (GUI.Button(position, "Scene not in build : Add"))
                    {
                        var ls = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                        ls.Add(new EditorBuildSettingsScene(scenePath, true));
                        EditorBuildSettings.scenes = ls.ToArray();
                    }
                }
            }
            else
            {
                EditorGUI.HelpBox(position, "No Scene selected yet.", MessageType.Info);
            }

            property.FindPropertyRelative("sceneAsset").objectReferenceValue = sceneAsset;
            property.FindPropertyRelative("value").stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}