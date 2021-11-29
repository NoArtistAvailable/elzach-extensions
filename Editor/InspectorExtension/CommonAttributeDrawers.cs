using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using elZach.EditorHelper;
using UnityEditor;
using UnityEngine;

namespace elZach.Common
{
    [CustomPropertyDrawer(typeof(StringOnlyAttribute))]
    public class StringOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!(attribute as StringOnlyAttribute).showOnlyValue) position = EditorGUI.PrefixLabel(position, label);
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    GUI.Label(position, property.objectReferenceValue.name);
                    break;
                case SerializedPropertyType.String:
                    GUI.Label(position, property.stringValue);
                    break;
                case SerializedPropertyType.Integer:
                    GUI.Label(position, property.intValue.ToString());
                    break;
                case SerializedPropertyType.Float:
                    GUI.Label(position, property.floatValue.ToString(CultureInfo.InvariantCulture));
                    break;
            }
        }
    }
    
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
    
    [CustomPropertyDrawer(typeof(InfoAttribute))]
    public class InfoAttributeDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            var info = (attribute as InfoAttribute);
            if (info == null || string.IsNullOrEmpty(info.message)) return 0;
            var content = new GUIContent(info.message);
            return EditorStyles.helpBox.CalcHeight(content, Screen.width) + 2;
        }

        public override void OnGUI(Rect position)
        {
            if(position.height > 0 && attribute is InfoAttribute info && !string.IsNullOrEmpty(info.message)) 
                DrawHelpBox(ref position, info);
            // switch (info.drawType)
            // {
            //     case InfoAttribute.DrawType.Top:
            //         DrawProperty(ref position, property, label);
            //         DrawHelpBox(ref position, info);
            //         break;
            //     case InfoAttribute.DrawType.Bottom:
            //         DrawHelpBox(ref position, info);
            //         DrawProperty(ref position, property, label);
            //         break;
            //     case InfoAttribute.DrawType.OnlyInfo:
            //         DrawHelpBox(ref position, info);
            //         break;
            // }
        
        }

        void DrawHelpBox(ref Rect position, InfoAttribute info)
        {
            EditorGUI.HelpBox(position, info.message, (MessageType)(int)info.type);
        }

        // void DrawProperty(ref Rect position, SerializedProperty property, GUIContent label)
        // {
        //     position.height = base.GetPropertyHeight(property, label);
        //     EditorGUI.PropertyField(position, property, label);
        //     position.position = new Vector2(position.position.x, position.position.y + base.GetPropertyHeight(property, label));
        // }
    }
    
    [CustomPropertyDrawer(typeof(ShowSpriteAttribute))]
    public class ShowSpriteAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + (attribute as ShowSpriteAttribute).height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            EditorGUI.PropertyField(position, property, label, true);
            position.y += EditorGUIUtility.singleLineHeight;
            position.height -= EditorGUIUtility.singleLineHeight;
            var tex = property.objectReferenceValue as Sprite;
            if (tex != null)
            {
                //EditorGUI.DrawTextureTransparent(position, tex.texture, ScaleMode.ScaleToFit);
                DrawTexturePreview(position, tex);
            }
        }
        
        //taken from https://forum.unity.com/threads/drawing-a-sprite-in-editor-window.419199/
        public static void DrawTexturePreview(Rect position, Sprite sprite)
        {
            Vector2 fullSize = new Vector2(sprite.texture.width, sprite.texture.height);
            Vector2 size = new Vector2(sprite.textureRect.width, sprite.textureRect.height);
 
            Rect coords = sprite.textureRect;
            coords.x /= fullSize.x;
            coords.width /= fullSize.x;
            coords.y /= fullSize.y;
            coords.height /= fullSize.y;
 
            Vector2 ratio;
            ratio.x = position.width / size.x;
            ratio.y = position.height / size.y;
            float minRatio = Mathf.Min(ratio.x, ratio.y);
 
            Vector2 center = position.center;
            position.width = size.x * minRatio;
            position.height = size.y * minRatio;
            position.center = center;
 
            GUI.DrawTextureWithTexCoords(position, sprite.texture, coords);
        }
    }
}
