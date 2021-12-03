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
        [NonSerialized] public int currentIndex = 0;

        private Coroutine currentTransition;

        void OnEnable()
        {
            if (animateAtOnEnable)
            {
                SetTo(0);
                Play(animateAtOnEnableTo);
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
            if (state.animate.HasFlag(TransformOptions.position)) transform.localPosition = state.data.localPos;
            if (state.animate.HasFlag(TransformOptions.rotation)) transform.localEulerAngles = state.data.localRotation;
            if (state.animate.HasFlag(TransformOptions.scale)) transform.localScale = state.data.localScale;
        }

        public async Task Play(Clip clip)
        {
            // if(currentTransition!=null) Debug.Log($"Playing Clip even though one is already Playing");
            if (currentTransition != null) StopCoroutine(currentTransition);
            currentTransition = StartCoroutine(TransitionTo(clip));
            while (currentTransition != null) await Task.Yield();
        }

        IEnumerator TransitionTo(Clip clip)
        {
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
                float value = clip.curve.Evaluate(progress);
                if (clip.animate.HasFlag(TransformOptions.position))
                    transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, value);
                if (clip.animate.HasFlag(TransformOptions.rotation))
                    transform.localRotation = Quaternion.LerpUnclamped(startRot, targetRot, value);
                if (clip.animate.HasFlag(TransformOptions.scale))
                    transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, value);
                yield return null;
            }

            var endValue = clip.curve.Evaluate(1f);
            if (clip.animate.HasFlag(TransformOptions.position))
                transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, endValue);
            if (clip.animate.HasFlag(TransformOptions.rotation))
                transform.localRotation = Quaternion.LerpUnclamped(startRot, targetRot, endValue);
            if (clip.animate.HasFlag(TransformOptions.scale))
                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, endValue);

            // if (clip.animate.HasFlag(TransformOptions.rotation)) Debug.Log($"Rotation {Quaternion.LerpUnclamped(startRot, targetRot, endValue)}", this);
            clip.events.OnEnded.Invoke();
            currentTransition = null;
            if (clip.loop) chainAtEndOfCurrent += () => Play(clip);
            chainAtEndOfCurrent?.Invoke();
            chainAtEndOfCurrent = null;
        }
#pragma warning restore CS4014
    }
}
