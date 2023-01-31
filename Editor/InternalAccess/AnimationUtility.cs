using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public static class InternalAnimationUtility
{
    public static void AddTransformTRS(GameObject root, string path) => AnimationMode.AddTransformTRS(root, path);
    
    public static void StopAnimationPlaybackMode() => AnimationMode.StopAnimationPlaybackMode();

    public static bool InAnimationPlaybackMode() => AnimationMode.InAnimationPlaybackMode();

    public static void StartAnimationPlaybackMode() => AnimationMode.StartAnimationPlaybackMode();

    public static void AttachResponder()
    {
        // This bubbles through all the way to EditorApplication.contextualPropertyMenu
        // EditorApplication.contextualPropertyMenu += new EditorApplication.SerializedPropertyCallbackFunction(this.OnPropertyContextMenu);
        // MaterialEditor.contextualPropertyMenu += new MaterialEditor.MaterialPropertyCallbackFunction(this.OnPropertyContextMenu);
        
        AnimationPropertyContextualMenu.Instance.SetResponder(new MyResponder());
    }
    
    public static void DetachResponder()
    {
        AnimationPropertyContextualMenu.Instance.SetResponder(null);
    }

    class MyResponder : IAnimationContextualResponder
    {
        public bool IsAnimatable(PropertyModification[] modifications)
        {
            return true;
        }

        public bool IsEditable(Object targetObject)
        {
            return true;
        }

        public bool KeyExists(PropertyModification[] modifications)
        {
            return true;
        }

        public bool CandidateExists(PropertyModification[] modifications)
        {
            return true;
        }

        public bool CurveExists(PropertyModification[] modifications)
        {
            return true;
        }

        public bool HasAnyCandidates()
        {
            return true;
        }

        public bool HasAnyCurves()
        {
            return true;
        }

        public void AddKey(PropertyModification[] modifications)
        {
            Debug.Log("AddKey: " + string.Join("\n", modifications.Select(x => x.target + "." + x.propertyPath)));
        }

        public void RemoveKey(PropertyModification[] modifications)
        {
            Debug.Log("RemoveKey: " + string.Join("\n", modifications.Select(x => x.target + "." + x.propertyPath)));
        }

        public void RemoveCurve(PropertyModification[] modifications)
        {

        }

        public void AddCandidateKeys()
        {

        }

        public void AddAnimatedKeys()
        {

        }

        public void GoToNextKeyframe(PropertyModification[] modifications)
        {

        }

        public void GoToPreviousKeyframe(PropertyModification[] modifications)
        {

        }
    } 
}
