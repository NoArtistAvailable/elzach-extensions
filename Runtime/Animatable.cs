using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using elZach.Common;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
            scale = 1 << 3
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

            public bool world = false;
            public bool loop = false;
            public AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
            public float time = 0.25f;
            public TransformOptions animate;
            public TransformData data;
            public Events events;

            public void Evaluate(float progress, Transform transform, Vector3 startPos, Vector3 targetPos, Quaternion startRot, Quaternion targetRot, Vector3 startScale, Vector3 targetScale)
            {
                float value = curve.Evaluate(progress);
                if (animate.HasFlag(TransformOptions.position))
                    transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, value);
                if (animate.HasFlag(TransformOptions.rotation))
                    transform.localRotation = Quaternion.LerpUnclamped(startRot, targetRot, value);
                if (animate.HasFlag(TransformOptions.scale))
                    transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, value);
            }
            
            
#if UNITY_EDITOR
            // public Button<Clip> playClipButton = new Button<Clip>(x =>
            // {
            //     SerializedObject so = new SerializedObject(x);
            //     var anim = so.targetObject as Animatable;
            //     anim.Play(x);
            // });
#endif

        }

        public bool animateAtOnEnable = false;
        public int animateAtOnEnableTo;

        public List<Clip> clips;
        //[NonSerialized] public Clip currentClip;

        private Coroutine currentTransition;

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
            if (state.animate.HasFlag(TransformOptions.position)) transform.localPosition = state.data.localPos;
            if (state.animate.HasFlag(TransformOptions.rotation)) transform.localEulerAngles = state.data.localRotation;
            if (state.animate.HasFlag(TransformOptions.scale)) transform.localScale = state.data.localScale;
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
            if (clip.world) targetPos = Quaternion.Inverse(transform.rotation) * clip.data.localPos;

            Quaternion startRot = transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(clip.data.localRotation);

            Vector3 startScale = transform.localScale;
            Vector3 targetScale = clip.data.localScale;

            float progress = 0f;
            clip.events.OnStarted.Invoke();
            while (progress < 1f)
            {
                progress += Time.deltaTime / clip.time;
                clip.Evaluate(progress, transform, startPos, targetPos, startRot, targetRot, startScale, targetScale);
                yield return null;
            }
            clip.Evaluate(1f, transform, startPos, targetPos, startRot, targetRot, startScale, targetScale);

            clip.events.OnEnded.Invoke();
            currentTransition = null;
            if (clip.loop) chainAtEndOfCurrent += () => Play(clip);
            chainAtEndOfCurrent?.Invoke();
            chainAtEndOfCurrent = null;
        }
#pragma warning restore CS4014
    }
}
