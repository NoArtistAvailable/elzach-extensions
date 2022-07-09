using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace elZach.Common
{
    public class Animatable : MonoBehaviour
    {
#pragma warning disable CS4014
        [System.Flags]
        public enum TransformOptions
        {
            position = 1 << 1,
            rotation = 1 << 2,
            scale = 1 << 3,
            color = 1 << 4,
            single = 1 << 5
        }

        [Serializable]
        public struct TransformData
        {
            public Vector3 localPos;
            public Vector3 localRotation;
            public Vector3 localScale;
        }

        [Serializable]
        public class Clip //: UnityEngine.Object
        {
            [System.Serializable]
            public class Events
            {
                public UnityEvent OnStarted, OnEnded;
            }

            public bool useInitialMatrix = false;
            public bool loop = false;
            public AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            public float time = 0.25f;
            public TransformOptions animate;
            public TransformData data;

            public List<AnimatableHelpers.ColorReference> colorData = new List<AnimatableHelpers.ColorReference>();
            public List<AnimatableHelpers.FloatReference> floatData = new List<AnimatableHelpers.FloatReference>();
            public Events events;

            public void Evaluate(float progress, Transform transform, 
                Vector3 startPos, Vector3 targetPos, 
                Quaternion startRot, Quaternion targetRot, 
                Vector3 startScale, Vector3 targetScale, 
                params (object start, object target)[] customStartTarget)
            {
                float value = curve.Evaluate(progress);
                EvaluateTransform(value, transform, startPos, targetPos, startRot, targetRot, startScale, targetScale);
                int customOffset = 0;
                if(animate.HasFlag(TransformOptions.color))
                    for (var i = 0; i < colorData.Count; i++)
                    {
                        var custom = colorData[i];
                        custom.TargetSourceValue = (Color) AnimatableHelpers.Lerp(customStartTarget[i].start, customStartTarget[i].target, value);
                    }

                customOffset += colorData.Count;
                if(animate.HasFlag(TransformOptions.single))
                    for (var i = customOffset; i < floatData.Count + customOffset; i++)
                    {
                        var custom = floatData[i - customOffset];
                        custom.TargetSourceValue = (float) AnimatableHelpers.Lerp(customStartTarget[i].start, customStartTarget[i].target, value);
                    }
            }

            public void EvaluateOn(float progress, GameObject target, 
                Vector3 startPos, Vector3 targetPos,
                Quaternion startRot, Quaternion targetRot, 
                Vector3 startScale, Vector3 targetScale,
                params (object start, object target)[] customStartTarget)
            {
                float value = curve.Evaluate(progress);
                EvaluateTransform(value, target.transform, startPos, targetPos, startRot, targetRot, startScale, targetScale);
                int customOffset = 0;
                if(animate.HasFlag(TransformOptions.color))
                    for (var i = 0; i < colorData.Count; i++)
                    {
                        var custom = colorData[i];
                        custom.ApplyTo(target, (Color) AnimatableHelpers.Lerp(customStartTarget[i].start, customStartTarget[i].target, value));
                    }

                customOffset += colorData.Count;
                if(animate.HasFlag(TransformOptions.single))
                    for (var i = customOffset; i < floatData.Count + customOffset; i++)
                    {
                        var custom = floatData[i - customOffset];
                        custom.ApplyTo(target, (float) AnimatableHelpers.Lerp(customStartTarget[i].start, customStartTarget[i].target, value));
                    }
            }

            private void EvaluateTransform(float value, Transform transform, Vector3 startPos, Vector3 targetPos,
                Quaternion startRot, Quaternion targetRot, Vector3 startScale, Vector3 targetScale)
            {
                if (animate.HasFlag(TransformOptions.position))
                    transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, value);
                if (animate.HasFlag(TransformOptions.rotation))
                    transform.localRotation = Quaternion.LerpUnclamped(startRot, targetRot, value);
                if (animate.HasFlag(TransformOptions.scale))
                    transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, value);
            }
        }
        
        public bool animateAtOnEnable = true;
        public int animateAtOnEnableTo = 1;
        private Matrix4x4 initialMatrix;

        public List<Clip> clips = new List<Clip>()
        {
            new Clip()
            {
                curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)),
                time = 0.25f,
                data = new TransformData() { localScale = Vector3.zero },
                animate = TransformOptions.scale
            },
            new Clip()
            {
                curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.75f,1.2f), new Keyframe(1f, 1f)),
                time = 0.35f,
                data = new TransformData() { localScale = Vector3.one },
                animate = TransformOptions.scale
            }
        };

        private Coroutine currentTransition;

        void Awake()
        {
            initialMatrix = (transform.parent ? transform.parent.worldToLocalMatrix : Matrix4x4.identity) * transform.localToWorldMatrix;
        }

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

        private event Action chainAtEndOfCurrent;

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

        public void SetTo(Clip state)
        {
            //var state = clips[index];
            // Debug.Log($"Setting {gameObject.name} to state {clips.IndexOf(state)}", this);
			if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
            }
            if (state.animate.HasFlag(TransformOptions.position)) transform.localPosition = state.useInitialMatrix ? initialMatrix.MultiplyPoint3x4(state.data.localPos) : state.data.localPos;
            if (state.animate.HasFlag(TransformOptions.rotation)) transform.localRotation = state.useInitialMatrix ? Quaternion.Euler(state.data.localRotation) * initialMatrix.rotation : Quaternion.Euler(state.data.localRotation);
            if (state.animate.HasFlag(TransformOptions.scale)) transform.localScale = state.useInitialMatrix ? initialMatrix.MultiplyVector(state.data.localScale) : state.data.localScale;
            if (state.animate.HasFlag(TransformOptions.color)) foreach(var color in state.colorData) color.ApplyToSource();
            if (state.animate.HasFlag(TransformOptions.single)) foreach(var single in state.floatData) single.ApplyToSource();
        }

        public async Task Play(Clip clip)
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

        IEnumerator TransitionTo(Clip clip)
        {
            //currentClip = clip;
            Vector3 startPos = transform.localPosition;
            Vector3 targetPos = clip.data.localPos;
            if (clip.useInitialMatrix) targetPos = initialMatrix.MultiplyPoint3x4(clip.data.localPos);

            Quaternion startRot = transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(clip.data.localRotation);
            if (clip.useInitialMatrix) targetRot = targetRot * initialMatrix.rotation;

            Vector3 startScale = transform.localScale;
            Vector3 targetScale = clip.data.localScale;
            if (clip.useInitialMatrix) targetScale = initialMatrix.MultiplyVector(targetScale);
            
            var customs = clip.colorData
                .Select<AnimatableHelpers.ColorReference, (object start, object target)>(x => (x.TargetSourceValue, x.Value))
                .Concat(clip.floatData.Select<AnimatableHelpers.FloatReference, (object startPos, object target)>(x=> (x.TargetSourceValue, x.Value)))
                .ToArray();

            float progress = 0f;
            clip.events.OnStarted.Invoke();
            while (progress < 1f)
            {
                progress += Time.deltaTime / clip.time;
                clip.Evaluate(progress, transform, startPos, targetPos, startRot, targetRot, startScale, targetScale, customs);
                yield return null;
            }
            clip.Evaluate(1f, transform, startPos, targetPos, startRot, targetRot, startScale, targetScale, customs);

            clip.events.OnEnded.Invoke();
            currentTransition = null;
            if (clip.loop) chainAtEndOfCurrent += () => Play(clip);
            chainAtEndOfCurrent?.Invoke();
            chainAtEndOfCurrent = null;
        }

        public Component[] GetValidComponents() => GetComponents<Component>();

#pragma warning restore CS4014
    }
}
