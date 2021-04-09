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
        public TextAsset data;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EditorNoteBehaviour))]
    public class EditorNoteBehaviourEditor : Editor
    {
        string message;
        bool hadDataPreviousFrame;
        const float lineHeight = 15f;

        public void OnEnable()
        {
            var t = target as EditorNoteBehaviour;

            if (!t.data)
            {
                if (EditorWindow.HasOpenInstances<EditorNoteWindow>())
                {
                    EditorNoteWindow window = (EditorNoteWindow)EditorWindow.GetWindow(typeof(EditorNoteWindow));
                    if (window.file) t.data = window.file;
                    //if (isCaller) window.calledBy = this;
                }
            }

            if(t.data)
                message = t.data.text;

            hadDataPreviousFrame = t.data;
        }

        private void OnDisable()
        {
            var t = target as EditorNoteBehaviour;
            SaveNote(t);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var t = target as EditorNoteBehaviour;
            if (t.data)
            {
                if (!hadDataPreviousFrame) this.OnEnable();
                int linesCount = message.Split('\n').Length;
                float minHeight = Mathf.Max(300, (linesCount + 2) * lineHeight);
                var rect = EditorGUILayout.GetControlRect(GUILayout.MinHeight(minHeight), GUILayout.ExpandHeight(true));
                message = EditorGUI.TextArea(rect, message);
            }
            else if(GUILayout.Button("Create New"))
            {
                EditorNoteWindow window = (EditorNoteWindow)EditorWindow.GetWindow(typeof(EditorNoteWindow));
                if (window.file) t.data = window.file;
                window.calledBy = this;
            }
        }

        public bool HasData()
        {
            var t = target as EditorNoteBehaviour;
            return t.data;
        }

        public void SetNoteFile(TextAsset file)
        {
            var t = target as EditorNoteBehaviour;
            t.data = file;
            this.OnEnable();
        }

        void SaveNote(EditorNoteBehaviour t)
        {
            SaveNote(t, message);
        }

        static void SaveNote(EditorNoteBehaviour t, string message)
        {
            if (!t.data) return;
            if (message != t.data.text)
            {
                File.WriteAllText(UnityEditor.AssetDatabase.GetAssetPath(t.data), message);
                EditorUtility.SetDirty(t.data);
                AssetDatabase.Refresh();
                Debug.Log("[Note] saved changes to note.", t.data);
            }
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
                        //DragAndDrop.SetGenericData("MonoScript", notescript);
                        //DragAndDrop.objectReferences
                    }
                }
                
                //if (e.type == EventType.DragUpdated)
                //{
                //    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                //    Event.current.Use();
                //}
                //else if (e.type == EventType.DragPerform)
                //{
                //    DragAndDrop.AcceptDrag();
                //}
            }

            if(GUILayout.Button("Save As File"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Saving Note", "New Note", "txt", "A message");
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