using System;
using System.Collections.Generic;
using System.Linq;
using elZach.Access;
using UnityEditor;
using UnityEngine;

public class PlaymodeSave : EditorWindow
{
   private static PlaymodeSave ActiveWindow => _activeWindow ??= GetWindow<PlaymodeSave>();
   private static PlaymodeSave _activeWindow;

   private static List<SaveProperty> savedProperties => ActiveWindow._savedProperties;
   private List<SaveProperty> _savedProperties = new List<SaveProperty>();
   

   [MenuItem("Window/Tools/Play mode Save #_s")]
   public static void Init()
   {
      var window = PlaymodeSave.GetWindow<PlaymodeSave>();
   }

   void OnEnable()
   {
      EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
      EditorApplication.playModeStateChanged += OnPlayModeChange;
      if(Application.isPlaying) Selection.selectionChanged += OnSelectionChanged;
   }

   static void OnSelectionChanged()
   {
      selectedIDs = Selection.gameObjects.Select(x => x.GetInstanceID()).ToList();
      ActiveWindow.Repaint();
   }

   private static List<int> selectedIDs = new List<int>();

   public static bool IsBlacklisted(SerializedProperty property)
   {
      var obj = property.serializedObject.targetObject;
      switch (obj)
      {
         case MeshFilter:
         // case Renderer when property.propertyPath == "m_Material":
            return true;
         default:
            return false;
      }
   }

   private static void OnPlayModeChange(PlayModeStateChange obj)
   {
      if (obj == PlayModeStateChange.EnteredPlayMode)
      {
         Selection.selectionChanged += OnSelectionChanged;
      }
      
      if (obj == PlayModeStateChange.ExitingPlayMode)
      {
         Selection.selectionChanged -= OnSelectionChanged;
         selectedIDs.Clear();
      }

      if (obj == PlayModeStateChange.EnteredEditMode)
      {
         var list = savedProperties;
         for (var i = list.Count - 1; i >= 0; i--)
         {
            var entry = list[i];
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
            list.RemoveAt(i);
         }
         //savedProperties.Clear();
      }
   }

   void OnDestroy()
   {
      EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
      EditorApplication.playModeStateChanged -= OnPlayModeChange;
      Selection.selectionChanged -= OnSelectionChanged;
      selectedIDs.Clear();
   }
   public struct SaveProperty
   {
      public int gameobjectId;
      public int id;
      public Type componentType;
      // public string propertyPath;
      public SerializedProperty property;
      public object value;
   }

   private static void Save(SaveProperty saveProperty)
   {
      var existing = savedProperties.FindIndex(x => x.id == saveProperty.id && x.property?.propertyPath == saveProperty.property?.propertyPath);
      if (existing >= 0) savedProperties[existing] = saveProperty;
      else savedProperties.Add(saveProperty);
   }
   
   private Vector2 scroll = Vector2.zero;
   
   private void OnGUI()
   {
      EditorGUILayout.HelpBox("During Play mode, you may right click properties you want to save.", MessageType.None);
      if (!Application.isPlaying && savedProperties.Count > 0)
      {
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.HelpBox("Entries couldn't be saved during Play mode, you can still copy the values from the property fields.", MessageType.Warning);
         if (GUILayout.Button("Clear", GUILayout.Width(45)))
         {
            savedProperties.Clear();
            Repaint();
         }
         EditorGUILayout.EndHorizontal();
      }
      scroll = GUILayout.BeginScrollView(scroll);
      for (var i = 0; i < savedProperties.Count; i++)
      {
         var entry = savedProperties[i];
         
         var type = entry.componentType;
         var obj = EditorUtility.InstanceIDToObject(entry.id);
         var style = (i % 2 == 0) ? EvenStyle : OddStyle;
         if (selectedIDs.Contains(entry.gameobjectId)) style = SelectedStyle;
         EditorGUILayout.BeginHorizontal(style);
         if (GUILayout.Button("X", GUILayout.Width(22)))
         {
            savedProperties.RemoveAt(i);
            Repaint();
         }
         EditorGUI.BeginDisabledGroup(true);
         EditorGUILayout.ObjectField(obj, type, true, GUILayout.Width(120));
         EditorGUI.EndDisabledGroup();
         EditorGUILayout.LabelField($"Saved Value : {entry.value}");
         EditorGUILayout.EndHorizontal();
         
         if (entry.property != null)
         {
            EditorGUILayout.BeginHorizontal(style);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(entry.property);
            var hasChanged = EditorGUI.EndChangeCheck();
            if (hasChanged && entry.property.serializedObject != null)
            {
               savedProperties[i] = new SaveProperty()
               {
                  id = entry.id, componentType = entry.componentType, property = entry.property,
                  gameobjectId = entry.gameobjectId,
                  value = GetValueFromProperty(entry.property)
               };
               entry.property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();
         }
         else EditorGUILayout.LabelField($"[{entry.id}] NO PROPERTY FOUND - {entry.componentType} : {entry.value}");
         
         
         EditorGUILayout.Space();
      }

      GUILayout.EndScrollView();
   }

   static void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
   {
      if (!Application.isPlaying)
      {
         menu.AddDisabledItem(new GUIContent("Save and Reapply"), false);
         return;
      }
      if (property == null || string.IsNullOrEmpty(property.propertyPath))
      {
         menu.AddDisabledItem(new GUIContent("Property is null or has no path"), false);
         return;
      }

      if (IsBlacklisted(property))
      {
         menu.AddDisabledItem(new GUIContent("Property is blacklisted"), false);
         return;
      }
      menu.AddItem(new GUIContent("Save and Reapply"), false, () =>
      {
         var componentType = property.serializedObject.targetObject.GetType();
         foreach (var component in Selection.gameObjects
            .Select(x => x.GetComponent(componentType) ))
         {
            var individualProperty = new SerializedObject(component).FindProperty(property.propertyPath);
            if (individualProperty == null)
            {
               Debug.LogWarning($"Didn't find property at path {property.propertyPath} on {component.name}", component);
            } 
            var id = component.GetInstanceID();
            Save(new SaveProperty()
            {
               id = id,
               componentType = component.GetType(),
               // propertyPath = property.propertyPath,
               property = individualProperty,
               value = GetValueFromProperty(individualProperty),
               gameobjectId = component.gameObject.GetInstanceID()
            });
         }

         ActiveWindow.Repaint();
      });
   }
   static object GetValueFromProperty(SerializedProperty property)
   {
      if (property == null)
      {
         Debug.LogWarning("No Property found.");
         return null;
      }
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
            Debug.LogWarning("Gradient not supported currently, sorry!");
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
            Debug.LogWarning("Managed Reference has no getter...");
            return null; //<-- to check
         default:
            throw new ArgumentOutOfRangeException();
      }
   }
   #region guiStyles
   private static GUIStyle EvenStyle
   {
      get
      {
         if (_evenStyle == null)
         {
            _evenStyle = new GUIStyle();
            Color col = EditorGUIUtility.isProSkin ? (Color) new Color32 (56, 56, 56, 255) : (Color) new Color32 (194, 194, 194, 255);
            _evenStyle.normal.background = new Texture2D(1, 1);
            _evenStyle.normal.background.SetPixel(0, 0, col);
            _evenStyle.normal.background.Apply();
         }
         return _evenStyle;
      }
   }
   private static GUIStyle _evenStyle;

   private static GUIStyle OddStyle
   {
      get
      {
         if (_oddStyle == null)
         {
            _oddStyle = new GUIStyle();
            Color col = EditorGUIUtility.isProSkin ? (Color) new Color32 (56, 56, 56, 255) : (Color) new Color32 (194, 194, 194, 255);
            _oddStyle.normal.background = new Texture2D(1, 1);
            _oddStyle.normal.background.SetPixel(0, 0, col + Color.white*0.03f);
            _oddStyle.normal.background.Apply();
         }
         return _oddStyle;
      }
   }
   private static GUIStyle _oddStyle;
   
   private static GUIStyle SelectedStyle
   {
      get
      {
         if (_oddStyle == null)
         {
            _selectedStyle = new GUIStyle();
            ColorUtility.TryParseHtmlString("#204363", out var col);
            _selectedStyle.normal.background = new Texture2D(1, 1);
            _selectedStyle.normal.background.SetPixel(0, 0, col);
            _selectedStyle.normal.background.Apply();
         }
         return _selectedStyle;
      }
   }
   private static GUIStyle _selectedStyle;
   #endregion
}
