using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace elZach.Common
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class ButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            this.DrawButtons();
            DrawDefaultInspector();
        }
    }
}