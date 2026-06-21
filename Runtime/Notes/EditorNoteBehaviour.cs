using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace elZach.Common
{
    public class EditorNoteBehaviour : MonoBehaviour
    {
        public TextAsset file;
        [HideInInspector]public string text;
        public bool showInScene;

#if UNITY_EDITOR
        private static GUIStyle labelStyle = new GUIStyle();

        private void OnDrawGizmos()
        {
            if (!showInScene || (string.IsNullOrEmpty(text) && !file)) return;
            var visibility =
                (SceneView.currentDrawingSceneView.camera.transform.position - transform.position).magnitude /
                EditorNoteBehaviourEditor.viewDistance;
            if (visibility > 1) return;
            labelStyle.normal.textColor = new Color(1, 1, 1, Mathf.Clamp01(visibility.Remap(1f, 0.9f, 0f, 1f)));
            Handles.Label(transform.position, file ? file.text : text, labelStyle);
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EditorNoteBehaviour))]
    public class EditorNoteBehaviourEditor : Editor
    {
        string message;
        bool hadDataPreviousFrame;
        const float lineHeight = 15f;

        public static float viewDistance
        {
            get
            {
                if (_viewDistance == null)
                {
                    if (EditorPrefs.HasKey("EditorNote/Distance"))
                    {
                        _viewDistance = EditorPrefs.GetFloat("EditorNote/Distance");
                    }
                    else
                    {
                        _viewDistance = 1000f;
                        EditorPrefs.SetFloat("EditorNote/Distance", _viewDistance.Value);
                    }
                }
                return _viewDistance.Value;
            }
            set
            {
                if (_viewDistance == value) return;
                _viewDistance = value;
                EditorPrefs.SetFloat("EditorNote/Distance", _viewDistance.Value);
            }
        }
        private static float? _viewDistance = null;

        public void OnEnable()
        {
            var t = target as EditorNoteBehaviour;
            if (!t.file)
            {
                if (EditorWindow.HasOpenInstances<EditorNoteWindow>())
                {
                    EditorNoteWindow window = (EditorNoteWindow)EditorWindow.GetWindow(typeof(EditorNoteWindow));
                    if (window.file) t.file = window.file;
                }
            }

            if(t.file)
                message = t.file.text;
            else message = t.text;

            hadDataPreviousFrame = t.file;
        }

        private void OnDisable()
        {
            var t = target as EditorNoteBehaviour;
            SaveNote(t);
        }

        void OnFocus(bool value)
        {
            //Debug.Log("Focus: " + value);
            var t = target as EditorNoteBehaviour;
            if (!value) SaveNote(t);
            if (value) this.OnEnable();
        }

        static bool focusLastFrame;
        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            viewDistance = ExpSlider("View Distance", viewDistance, 1f, 1000f);
            DrawDefaultInspector();
            bool focus = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
            if (focus != focusLastFrame) OnFocus(focus);
            focusLastFrame = focus;
            var t = target as EditorNoteBehaviour;
            if (t.file)
            {
                if (!hadDataPreviousFrame) this.OnEnable();
            }

            int linesCount = message?.Split('\n').Length ?? 0;
            float minHeight = Mathf.Max(50, (linesCount + 2) * lineHeight);
            var rect = EditorGUILayout.GetControlRect(GUILayout.MinHeight(minHeight), GUILayout.ExpandHeight(true));
            message = EditorGUI.TextArea(rect, message);
            
            if(!t.file)
            {
                t.text = message;

                if (GUILayout.Button("Create New"))
                {
                    EditorNoteWindow window = (EditorNoteWindow)EditorWindow.GetWindow(typeof(EditorNoteWindow));
                    if (window.file) t.file = window.file;
                    window.calledBy = this;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        public bool HasData()
        {
            var t = target as EditorNoteBehaviour;
            return t.file;
        }

        public void SetNoteFile(TextAsset file)
        {
            var t = target as EditorNoteBehaviour;
            t.file = file;
            this.OnEnable();
        }

        void SaveNote(EditorNoteBehaviour t)
        {
            SaveNote(t, message);
        }

        static void SaveNote(EditorNoteBehaviour t, string message)
        {
            if (!t.file) return;
            if (message != t.file.text)
            {
                File.WriteAllText(UnityEditor.AssetDatabase.GetAssetPath(t.file), message);
                EditorUtility.SetDirty(t.file);
                AssetDatabase.Refresh();
                Debug.Log("[EditorNotes] " + t.name + " saved changes to " + t.file.name + " (Note).", t.file);
            }
        }
        
        public static float ExpSlider(string label, float value, float min, float max)
        {
            value = Mathf.Clamp(value, min, max);
            float logMin = Mathf.Log(min);
            float logMax = Mathf.Log(max);

            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, new GUIContent(label));

            Rect sliderRect = new Rect(rect.x, rect.y, rect.width - 54f, rect.height);
            Rect fieldRect = new Rect(rect.xMax - 50f, rect.y, 50f, rect.height);

            float logValue = GUI.HorizontalSlider(sliderRect, Mathf.Log(value), logMin, logMax);
            value = Mathf.Exp(logValue);

            value = Mathf.Clamp(EditorGUI.FloatField(fieldRect, value), min, max);
            return value;
        }
    }

    public class EditorNoteWindow : EditorWindow
    {
        string message;
        public TextAsset file;
        public EditorNoteBehaviourEditor calledBy;

        [MenuItem("Window/Tools/Note")]
        public static void Init()
        {
            EditorNoteWindow window = (EditorNoteWindow)EditorWindow.GetWindow(typeof(EditorNoteWindow));
            window.titleContent = new GUIContent("Editor Note");
            window.minSize = new Vector2(100, 100);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (file)
            {
                var e = Event.current;

                Rect buttonRect = EditorGUILayout.GetControlRect();
                GUIStyle b = "Button";
                EditorGUI.LabelField(buttonRect, " ", b);

                if ( buttonRect.Contains(e.mousePosition) )
                {
                    if (e.type == EventType.MouseDown)
                    {
                        DragAndDrop.StartDrag("Dragging EditorNoteBehaviour");
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        //var noteBehaviour = new EditorNoteBehaviour();
                        //noteBehaviour.data = file;
                        var go = new GameObject("tmp", typeof(EditorNoteBehaviour));
                        go.hideFlags = HideFlags.HideAndDontSave;
                        var notescript = MonoScript.FromMonoBehaviour(go.GetComponent<EditorNoteBehaviour>());
                        DragAndDrop.objectReferences = new Object[] { notescript };
                    }
                }
            }

            if(GUILayout.Button("Save As File"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Saving Note", "New Note", "md", "A message");
                File.WriteAllText(path, message);
                AssetDatabase.Refresh();
                file = (TextAsset) AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
                if (calledBy && !calledBy.HasData()) calledBy.SetNoteFile(file);
            }
            EditorGUILayout.EndHorizontal();
            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            message = EditorGUI.TextArea(rect, message);
        }
    }

#endif
}