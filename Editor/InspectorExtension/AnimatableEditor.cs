using System.Collections;
using System.Collections.Generic;
using elzach.Common;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Animatable))]
public class AnimatableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var t = target as Animatable;
        if (t.clips == null) return;
        EditorGUILayout.BeginHorizontal();
        for(int i=0; i < t.clips.Count;i++)
            if(GUILayout.Button(i.ToString())) t.Play(i);
        EditorGUILayout.EndHorizontal();
    }
}
