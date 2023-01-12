using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using elZach.Access;
using UnityEditor;
using UnityEngine;

namespace elZach.Common
{
   public class PlaymodeSave : EditorWindow
   {
      private static PlaymodeSave ActiveWindow => _activeWindow ??= GetWindow<PlaymodeSave>();
      private static PlaymodeSave _activeWindow;

      private static List<SaveProperty> savedProperties => ActiveWindow._savedProperties;
      private List<SaveProperty> _savedProperties = new List<SaveProperty>();


      [MenuItem("Window/Tools/Play mode Save #_s")]
      public static void Init()
      {
         var window = GetWindow<PlaymodeSave>();
      }

      void OnEnable()
      {
         EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
         EditorApplication.playModeStateChanged += OnPlayModeChange;
         if (Application.isPlaying) Selection.selectionChanged += OnSelectionChanged;
      }

      static void OnSelectionChanged()
      {
         selectedIDs = Selection.gameObjects.Select(x => x.GetInstanceID()).ToList();
         ActiveWindow.Repaint();
      }

      private static List<int> selectedIDs = new List<int>();

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
                  Debug.LogWarning(
                     $"[{nameof(PlaymodeSave)}] failed, because object ID cannot be found anymore. Play Mode Save only works on objects which are present in the scene file.");
#if UNITY_2022_1_OR_NEWER
                  Debug.LogWarning(
                     $"[{nameof(PlaymodeSave)}] lost data: {entry.componentType}/{entry.property.propertyPath} ({(entry.property.isArray ? "List" : $"{entry.property.boxedValue}")})");
#endif
                  continue;
               }

               var so = new SerializedObject(target);
               var prop = so.FindProperty(entry.property.propertyPath);

               CopyFromPropertyToProperty(prop, entry.property);

               so.ApplyModifiedProperties();
               list.RemoveAt(i);
            }

            //savedProperties.Clear();
            ActiveWindow.Repaint();
         }
      }

      static void CopyFromPropertyToProperty(SerializedProperty target, SerializedProperty source)
      {
         if (source.isArray)
         {
            target.arraySize = source.arraySize;
            for (int index = 0; index < source.arraySize; index++)
               CopyFromPropertyToProperty(target.GetArrayElementAtIndex(index), source.GetArrayElementAtIndex(index));
         }
         else
         {
#if UNITY_2022_1_OR_NEWER
            target.boxedValue = source.boxedValue;
#else
            target.SetBoxedValue(source.GetBoxedValue());
#endif
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
         public SerializedProperty property;
      }

      private static void Save(SaveProperty saveProperty)
      {
         var existing = savedProperties.FindIndex(x =>
            x.id == saveProperty.id && x.property?.propertyPath == saveProperty.property?.propertyPath);
         if (existing >= 0) savedProperties[existing] = saveProperty;
         else savedProperties.Add(saveProperty);
      }

      private Vector2 scroll = Vector2.zero;

      private void OnGUI()
      {
         EditorGUILayout.HelpBox("During Play mode, you may right click properties you want to save.",
            MessageType.None);
         if (!Application.isPlaying && savedProperties.Count > 0)
         {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(
               "Entries couldn't be saved during Play mode, you can still copy the values from the property fields.",
               MessageType.Warning);
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
            EditorGUILayout.LabelField($"gui height: {EditorGUI.GetPropertyHeight(entry.property, true)}");
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal(style);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(entry.property, true);//, new GUIContent(""));
            var hasChanged = EditorGUI.EndChangeCheck();
            if (hasChanged && entry.property.serializedObject != null)
            {
               savedProperties[i] = new SaveProperty()
               {
                  id = entry.id, componentType = entry.componentType, property = entry.property,
                  gameobjectId = entry.gameobjectId
               };
               entry.property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndHorizontal();
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

         var prop = property.Copy();

         menu.AddItem(new GUIContent("Save and Reapply"), false, () =>
         {
            var componentType = prop.serializedObject.targetObject.GetType();
            foreach (var component in Selection.gameObjects
               .Select(x => x.GetComponent(componentType)))
            {
               var individualProperty = new SerializedObject(component).FindProperty(prop.propertyPath);
               if (individualProperty == null)
               {
                  Debug.LogWarning($"Didn't find property at path {prop.propertyPath} on {component.name}", component);
               }

               var id = component.GetInstanceID();
               Save(new SaveProperty()
               {
                  id = id,
                  componentType = component.GetType(),
                  // propertyPath = property.propertyPath,
                  property = individualProperty,
                  gameobjectId = component.gameObject.GetInstanceID()
               });
            }

            ActiveWindow.Repaint();
         });
      }

      #region guiStyles

      private static GUIStyle EvenStyle
      {
         get
         {
            if (_evenStyle == null)
            {
               _evenStyle = new GUIStyle();
               Color col = EditorGUIUtility.isProSkin
                  ? (Color) new Color32(56, 56, 56, 255)
                  : (Color) new Color32(194, 194, 194, 255);
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
               Color col = EditorGUIUtility.isProSkin
                  ? (Color) new Color32(56, 56, 56, 255)
                  : (Color) new Color32(194, 194, 194, 255);
               _oddStyle.normal.background = new Texture2D(1, 1);
               _oddStyle.normal.background.SetPixel(0, 0, col + Color.white * 0.03f);
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
            if (_selectedStyle == null)
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
}
