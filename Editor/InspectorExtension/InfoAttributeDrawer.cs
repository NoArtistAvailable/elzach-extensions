using UnityEditor;
using UnityEngine;

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