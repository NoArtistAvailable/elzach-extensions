using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace elZach.Common
{
    public class AnimatableChildren : MonoBehaviour
    {
#pragma warning disable CS4014
        public bool animateAtOnEnable = true;
        public int animateAtOnEnableTo = 1;

        // public float indexDelay = 0.1f;
        // public Vector2 indexTimeMultiplier = Vector2.one;

        [Serializable]
        public class DriverData
        {
            public enum Key{None, Index, InverseIndex}
            public Key key;
            public Vector2 delay;
            public Vector2 durationMultiplier;

            private float? Map(int index, int count, Vector2 range)
            {
                return key switch
                {
                    Key.None => null,
                    Key.Index => (index / (float) count).Remap(0f, 1f, range.x, range.y),
                    Key.InverseIndex => (1f - index / (float) count).Remap(0f, 1f, range.x, range.y),
                    _ => null
                };
            }

            public float GetDelay(int index, int count) => Map(index, count, delay) * (key == Key.Index ? index : 1f) * (key == Key.InverseIndex ? (count - index) : 1f) ?? 0f;
            public float GetDurationMultiplier(int index, int count) => Map(index, count, durationMultiplier) ?? 1f;
        }
        
        [Serializable]
        public class DrivenClip : Animatable.Clip
        {
            public DriverData driver;
        }

        public List<DrivenClip> clips = new List<DrivenClip>()
        {
            new DrivenClip()
            {
                curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)),
                time = 0.25f,
                data = new Animatable.TransformData() { localScale = Vector3.zero },
                animate = Animatable.TransformOptions.scale
            },
            new DrivenClip()
            {
                curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.75f, 1.2f), new Keyframe(1f, 1f)),
                time = 0.35f,
                data = new Animatable.TransformData() { localScale = Vector3.one },
                animate = Animatable.TransformOptions.scale
            }
        };
        
        
        private Coroutine currentTransition;
        private event Action chainAtEndOfCurrent;

        void OnEnable()
        {
            if (animateAtOnEnable)
            {
                SetTo(0);
                Play(animateAtOnEnableTo);
            }
        }
        void OnDisable()
        {
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
            }
        }

        public void SetTo(int index) => SetTo(clips[index]);
        
        public void PlayAt(int index)
        {
            if (currentTransition != null)
                chainAtEndOfCurrent = () => Play(index);
            else
                Play(index);
        }

        public async Task Play(int index) => await Play(clips[index]);

        public async Task Play(int index, float delay)
        {
            await WebTask.Delay(delay);
            await Play(index);
        }
        
        public void SetTo(Animatable.Clip state)
        {
            //var state = clips[index];
            // Debug.Log($"Setting {gameObject.name} to state {clips.IndexOf(state)}", this);
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
            }

            for (var index = 0; index < transform.childCount; index++)
            {
                var child = transform.GetChild(index);
                if (state.animate.HasFlag(Animatable.TransformOptions.position))
                    child.localPosition = state.data.localPos;
                if (state.animate.HasFlag(Animatable.TransformOptions.rotation))
                    child.localEulerAngles = state.data.localRotation;
                if (state.animate.HasFlag(Animatable.TransformOptions.scale))
                    child.localScale = state.data.localScale;
                if (state.animate.HasFlag(Animatable.TransformOptions.color))
                    foreach (var color in state.colorData)
                        color.ApplyTo(child.gameObject, color.Value);
                if (state.animate.HasFlag(Animatable.TransformOptions.single))
                    foreach (var single in state.floatData)
                        single.ApplyTo(child.gameObject, single.Value);
            }
        }

        public async Task Play(DrivenClip clip)
        {
            // if(currentTransition!=null) Debug.Log($"Playing Clip even though one is already Playing");
            
            if (currentTransition != null)
            {
                if (clip.curve.GetLastKey().value == 0f) //if we're going into a bouncing animation we need to make sure that we don't have skewed values to start with
                    while (currentTransition != null) await Task.Yield();
                else StopCoroutine(currentTransition);
            }
            if (!gameObject.activeInHierarchy) return;
            currentTransition = StartCoroutine(TransitionTo(clip));
            while (currentTransition != null) await Task.Yield();
        }
        
        IEnumerator TransitionTo(DrivenClip clip)
        {
            //currentClip = clip;
            var children = transform.GetChildren().ToArray();
            
            Vector3[] startPos = children.Select(x=>x.localPosition).ToArray();
            Vector3 targetPos = clip.data.localPos;
            if (clip.useInitialMatrix) targetPos = Quaternion.Inverse(transform.rotation) * clip.data.localPos;

            Quaternion[] startRot = children.Select(x => x.localRotation).ToArray();
            Quaternion targetRot = Quaternion.Euler(clip.data.localRotation);

            Vector3[] startScale = children.Select(x => x.localScale).ToArray();
            Vector3 targetScale = clip.data.localScale;
            
            var customs = clip.colorData
                .Select<AnimatableHelpers.ColorReference, (object start, object target)>(x => (x.TargetSourceValue, x.Value))
                .Concat(clip.floatData.Select<AnimatableHelpers.FloatReference, (object startPos, object target)>(x=> (x.TargetSourceValue, x.Value)))
                .ToArray();

            // float progress = 0f;
            float startTime = Time.time;
            float timePassed = 0f;
            float endTime = Time.time + clip.time * clip.driver.durationMultiplier.y + (children.Length - 1) * clip.driver.delay.x;
            clip.events.OnStarted.Invoke();
            while (Time.time < endTime)
            {
                timePassed = Time.time - startTime;
                for (int i = 0; i < children.Length; i++)
                {
                    float progress = Mathf.Clamp01((timePassed - clip.driver.GetDelay(i,children.Length)) / (clip.time * clip.driver.GetDurationMultiplier(i, children.Length)));
                    clip.EvaluateOn(progress, children[i].gameObject, startPos[i], targetPos, startRot[i], targetRot, startScale[i], targetScale, customs);
                }
                yield return null;
            }
            for(int i=0; i < children.Length; i++)
                clip.EvaluateOn(1f, children[i].gameObject, startPos[i], targetPos, startRot[i], targetRot, startScale[i], targetScale, customs);

            clip.events.OnEnded.Invoke();
            currentTransition = null;
            if (clip.loop) chainAtEndOfCurrent += () => Play(clip);
            chainAtEndOfCurrent?.Invoke();
            chainAtEndOfCurrent = null;
        }
        
        public Component[] GetValidComponents() => transform.GetChild(0).GetComponents<Component>();
        
        #if UNITY_EDITOR
        [CustomEditor(typeof(AnimatableChildren))]
        public class Inspector : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                var t = target as AnimatableChildren;
                EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < t.clips.Count; i++)
                {
                    if (GUILayout.Button(i.ToString())) t.Play(i);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
            }
        }        
        #endif
        
#pragma warning restore CS4014
    }
}