using System.Collections;
using System.Collections.Generic;
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
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sceneAsset = property.FindPropertyRelative("sceneAsset").objectReferenceValue as SceneAsset;
            var value = property.FindPropertyRelative("value").stringValue;
            sceneAsset = EditorGUI.ObjectField(position, label, sceneAsset, typeof(SceneAsset), false) as SceneAsset;
            if (sceneAsset?.name != value)
            {
                value = sceneAsset?.name;
                // EditorUtility.SetDirty(property.serializedObject.targetObject);
                // property.serializedObject.Update();
            }

            property.FindPropertyRelative("sceneAsset").objectReferenceValue = sceneAsset;
            property.FindPropertyRelative("value").stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}