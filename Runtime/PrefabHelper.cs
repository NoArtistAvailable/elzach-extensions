using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine.Events;

namespace elZach.Common
{
    public class PrefabHelper : MonoBehaviour
    {
#if UNITY_EDITOR
        public UnityEvent OnSavingPrefab;
        public void SavingPrefab(GameObject go)
        {
            OnSavingPrefab.Invoke();
        }

        private void OnEnable()
        {
            PrefabStage.prefabSaving += SavingPrefab;
        }

        private void OnDisable()
        {
            PrefabStage.prefabSaving -= SavingPrefab;
        }
#endif
    }
}