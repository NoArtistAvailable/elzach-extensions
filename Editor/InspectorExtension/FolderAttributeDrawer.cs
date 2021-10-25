using System.Collections;
using System.Collections.Generic;
using elZach.EditorHelper;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FolderAttribute))]
public class FolderAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float width = position.width;
        position.width = 22;
        if (GUI.Button(position, EditorGUIUtility.IconContent("Folder Icon")))
        {
            property.stringValue = EditorUtility.OpenFolderPanel("Select Folder", property.stringValue, string.Empty);
            var folderAttribute = attribute as FolderAttribute;
            if (folderAttribute.relative) property.stringValue = Datahandling.EnsureAssetDataPath(property.stringValue);
            property.serializedObject.ApplyModifiedProperties();
            GUIUtility.ExitGUI();
        }
        position.x = position.x + 22;
        position.width = width - 22;
        position = EditorGUI.PrefixLabel(position, label);
        
        GUI.Label(position, property.stringValue);
    }
}
