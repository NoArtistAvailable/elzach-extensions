using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace elZach.Common
{
    public class DirtyUtility
    {
        // [MenuItem("Window/Tools/Dirty Utility/Enable Notification")]
        // public static void Init()
        // {
        //     EditorSceneManager.sceneDirtied -= LogDirty;
        //     EditorSceneManager.sceneDirtied += LogDirty;
        // }
        //
        // private static void LogDirty(Scene scene)
        // {
        //     
        // }

        [MenuItem("Window/Tools/Dirty Utility/Log Current")]
        public static void LogDirtyObjects()
        {
            IsDirty(GetAllSceneComponents());
        }

        public static Component[] GetAllSceneComponents()
        {
            List<Component> all = new List<Component>();
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                all.AddRange(root.GetComponentsInChildren<Component>());
            }

            return all.ToArray();
        }

        [MenuItem("Window/Tools/Dirty Utility/Clear Current")]
        public static void ClearDirty()
        {
            //ActiveEditorTracker.sharedTracker.ClearDirty();
            foreach (var comp in GetAllSceneComponents())
                EditorUtility.ClearDirty(comp);
            EditorSceneManager.ClearSceneDirtiness(EditorSceneManager.GetActiveScene());
        }

        static void IsDirty(Component[] objs)
        {
            int dirtyCount = 0;
            foreach (var obj in objs)
            {
                var dirty = EditorUtility.IsDirty(obj);
                if (dirty)
                {
                    Debug.Log($"{obj.GetType().Name} on {obj.name} is dirty", obj);
                    dirtyCount++;
                }
            }

            Debug.Log($"{dirtyCount} components are dirty");
            // System.Type type = typeof(EditorUtility);
            // MethodInfo methodInfo = type.GetMethod("IsDirty", BindingFlags.Static | BindingFlags.NonPublic); // Get the method IsDirty
            // foreach (var obj in objs)
            // {
            //     int instanceID = obj.GetInstanceID();
            //     if(methodInfo==null) Debug.Log("MethodNotFound");
            //     bool isDirty = (bool) methodInfo.Invoke(obj, new System.Object[1] {instanceID});
            //     if(isDirty) Debug.Log($"{obj.name} is dirty", obj);
            // }
        }
    }
}
