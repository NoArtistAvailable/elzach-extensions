using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Access;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

public class PlaymodeSave : EditorWindow
{
   private static PlaymodeSave ActiveWindow => _activeWindow ??= GetWindow<PlaymodeSave>();
   private static PlaymodeSave _activeWindow;

   private static List<SaveProperty> savedProperties => ActiveWindow._savedProperties;
   private List<SaveProperty> _savedProperties = new List<SaveProperty>();
   

   [MenuItem("Window/Tools/PlaymodeSave")]
   public static void Init()
   {
      var window = PlaymodeSave.GetWindow<PlaymodeSave>();
   }

   void OnEnable()
   {
      EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
      EditorApplication.playModeStateChanged += OnPlayModeChange;
   }

   private static void OnPlayModeChange(PlayModeStateChange obj)
   {
      if (obj == PlayModeStateChange.ExitingPlayMode)
      {
         
      }

      if (obj == PlayModeStateChange.EnteredEditMode)
      {
         var list = savedProperties;
         foreach (var entry in list)
         {
            var target = EditorUtility.InstanceIDToObject(entry.id);
            if (!target)
            {
               Debug.LogWarning("[PlaymodeSave] failed, because object ID cannot be found anymore. Playmode Save only works on objects which are present in the scene file.");
               Debug.LogWarning($"[PlaymodeSave] lost data: {entry.componentType}/{entry.property.propertyPath} ({entry.value})");
               continue;
            }
            var so = new SerializedObject(target);
            var prop = so.FindProperty(entry.property.propertyPath);
            var value = entry.value;
            switch (value)
            {
               case float floatValue:
                  prop.floatValue = floatValue;
                  break;
               case double doubleValue:
                  prop.doubleValue = doubleValue;
                  break;
               case string stringValue:
                  prop.stringValue = stringValue;
                  break;
               case int intValue:
                  prop.intValue = intValue;
                  break;
               case bool boolValue:
                  prop.boolValue = boolValue;
                  break;
               case AnimationCurve curveValue:
                  prop.animationCurveValue = curveValue;
                  break;
               case Vector2 vector2Value:
                  prop.vector2Value = vector2Value;
                  break;
               case Vector2Int vector2IntValue:
                  prop.vector2IntValue = vector2IntValue;
                  break;
               case Vector3 vector3Value:
                  prop.vector3Value = vector3Value;
                  break;
               case Vector3Int vector3IntValue:
                  prop.vector3IntValue = vector3IntValue;
                  break;
               case Vector4 vector4Value:
                  prop.vector4Value = vector4Value;
                  break;
               case Quaternion quaternionValue:
                  prop.quaternionValue = quaternionValue;
                  break;
               case Color colorValue:
                  prop.colorValue = colorValue;
                  break;
            }

            so.ApplyModifiedProperties();
         }
         savedProperties.Clear();
      }
   }

   void OnDestroy()
   {
      EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
      EditorApplication.playModeStateChanged -= OnPlayModeChange;
   }

   

   public struct SaveProperty
   {
      public int id;
      public Type componentType;
      // public string propertyPath;
      public SerializedProperty property;
      public object value;
   }

   private Vector2 scroll = Vector2.zero;
   private void OnGUI()
   {
      scroll = GUILayout.BeginScrollView(scroll);
      for (var i = 0; i < savedProperties.Count; i++)
      {
         var entry = savedProperties[i];
         EditorGUILayout.LabelField(
            $"[{entry.id}] - {entry.componentType} - {entry.property.propertyPath} : {entry.value}");
         var type = entry.componentType;
         var obj = EditorUtility.InstanceIDToObject(entry.id);
         EditorGUILayout.ObjectField(obj, type, true);
         EditorGUI.BeginChangeCheck();
         EditorGUILayout.PropertyField(entry.property);
         var hasChanged = EditorGUI.EndChangeCheck();
         if (hasChanged)
         {
            savedProperties[i] = new SaveProperty()
            {
               id = entry.id, componentType = entry.componentType, property = entry.property,
               value = GetValueFromProperty(entry.property)
            };
            entry.property.serializedObject.ApplyModifiedProperties();
         }
      }

      GUILayout.EndScrollView();
   }

   static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
   {
      menu.AddItem(new GUIContent("Save and Reapply"), false, () =>
      {
         var componentType = property.serializedObject.targetObject.GetType();
         foreach (var component in Selection.objects
            .Where(x => x is GameObject)
            .Select(y => ((GameObject)y).GetComponent(componentType) ))
         {
            // var component = componentObject as Component;
            var individualProperty = new SerializedObject(component).FindProperty(property.propertyPath);
            var id = component.GetInstanceID();
            savedProperties.Add(new SaveProperty()
            {
               id = id,
               componentType = component.GetType(),
               // propertyPath = property.propertyPath,
               property = individualProperty,
               value = GetValueFromProperty(individualProperty)
            });
         }

         ActiveWindow.Repaint();
      });
   }
   static object GetValueFromProperty(SerializedProperty property)
   {
      switch (property.propertyType)
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
            return property.intValue;  // <-- to check
         case SerializedPropertyType.Enum:
            return property.enumValueIndex;  //<-- to check
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
            return property.intValue;  //<-- to check
         case SerializedPropertyType.AnimationCurve:
            return property.animationCurveValue;
         case SerializedPropertyType.Bounds:
            return property.boundsValue;
         case SerializedPropertyType.Gradient:
            return null;   //<-- to check
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
            return null; //<-- to check
         default:
            throw new ArgumentOutOfRangeException();
      }
   }
}
