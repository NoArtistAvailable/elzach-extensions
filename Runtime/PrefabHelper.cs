using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine.Events;

namespace elZach.Common
{
    [ExecuteInEditMode]
    public class PrefabHelper : MonoBehaviour
    {
#if UNITY_EDITOR
        [Button("Setup Editor Events")]
        public void SetEventsToEditorTime()
        {
            for (int i = 0; i < OnSavingPrefab.GetPersistentEventCount(); i++)
            {
                OnSavingPrefab.SetPersistentListenerState(i, UnityEventCallState.EditorAndRuntime);
            }
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }

        public UnityEvent OnSavingPrefab;
        public void SavingPrefab(GameObject go)
        {
            OnSavingPrefab.Invoke();
        }

        private void OnEnable()
        {
            if (Application.isPlaying) return;
            PrefabStage.prefabSaving += SavingPrefab;
        }

        private void OnDisable()
        {
            if (Application.isPlaying) return;
            PrefabStage.prefabSaving -= SavingPrefab;
        }
#endif
    }
}