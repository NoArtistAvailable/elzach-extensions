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
    public abstract class AnimatableMultiple : MonoBehaviour
    {
#pragma warning disable CS4014
        public bool animateAtOnEnable = true;
        public int animateAtOnEnableTo = 1;
        public bool unscaledTime = false;
        public float usedTime => unscaledTime ? Time.unscaledTime : Time.time;
        public event Action<Transform, int> childStartedTransition, childEndedTransition;

        private Dictionary<Transform, Matrix4x4> _initialMatrices = new Dictionary<Transform, Matrix4x4>();
        public bool IsPlaying => currentTransition != null;

        public abstract IEnumerable<Transform> Targets { get; }

        Matrix4x4 GetInitial(Transform child)
        {
            if (_initialMatrices.TryGetValue(child, out var value)) return value;
            var parent = child.parent;
            value = (parent ? parent.worldToLocalMatrix : Matrix4x4.identity) * child.localToWorldMatrix;
            // Debug.LogWarning($"{child.name} : {value.rotation.eulerAngles}");
            _initialMatrices.Add(child, value);
            return value;
        }

        [Serializable]
        public class DriverData
        {
            public enum Key{None, Index, InverseIndex}
            public Key key = Key.Index;
            public Vector2 delay;
            public Vector2 durationMultiplier;

            public DriverData(){}
            public DriverData(Key key, Vector2 delay, Vector2 durationMultiplier)
            {
                this.key = key;
                this.delay = delay;
                this.durationMultiplier = durationMultiplier;
            }
            
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
        
        public enum ParentActivator {None, OnBeginTransition, OnReachedState}
        
        [Serializable]
        public class DrivenClip : Animatable.Clip
        {
            public DriverData driver = new DriverData(DriverData.Key.Index, Vector2.one * .1f, Vector2.one);
            public ParentActivator parentActivator;
            public DrivenClip(){}

            public DrivenClip(DriverData driver, ParentActivator parentActivator)
            {
                this.driver = driver;
                this.parentActivator = parentActivator;
            }
        }

        public List<DrivenClip> clips = new List<DrivenClip>()
        {
            new DrivenClip()
            {
                driver = new DriverData(DriverData.Key.InverseIndex, Vector2.one * .1f, Vector2.one),
                curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)),
                time = 0.25f,
                data = new Animatable.TransformData() { localScale = Vector3.zero },
                animate = Animatable.TransformOptions.scale
            },
            new DrivenClip()
            {
                driver = new DriverData(DriverData.Key.Index, Vector2.one * .1f, Vector2.one),
                curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.75f, 1.2f), new Keyframe(1f, 1f)),
                time = 0.35f,
                data = new Animatable.TransformData() { localScale = Vector3.one },
                animate = Animatable.TransformOptions.scale
            }
        };
        
        
        private Coroutine currentTransition;
        private event Action chainAtEndOfCurrent;

        void Awake()
        {
            // foreach (var child in Targets)
            //     _initialMatrices.Add(child, (child.parent ? child.parent.worldToLocalMatrix : Matrix4x4.identity) * child.localToWorldMatrix);

            var parentAnimatableChildren = transform.parent?.GetComponentInParent<AnimatableChildren>();
            if (!parentAnimatableChildren) return;
            parentAnimatableChildren.childStartedTransition += (target, index) =>
            {
                if (target != transform || clips.Count <= index) return;
                if (clips[index].parentActivator == ParentActivator.OnBeginTransition) PlayAt(index);
            };
            parentAnimatableChildren.childEndedTransition += (target, index) =>
            {
                if (target != transform || clips.Count <= index) return;
                if (clips[index].parentActivator == ParentActivator.OnReachedState) PlayAt(index);
            };
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

            var children = Targets.ToList();
            for (var index = 0; index < children.Count; index++)
            {
                var child = children[index];
                if (state.animate.HasFlag(Animatable.TransformOptions.position))
                    child.localPosition = state.useInitialMatrix ? GetInitial(child).MultiplyPoint(state.data.localPos) : state.data.localPos;
                if (state.animate.HasFlag(Animatable.TransformOptions.rotation))
                    child.localRotation = state.useInitialMatrix ? Quaternion.Euler(state.data.localRotation) * GetInitial(child).rotation : Quaternion.Euler(state.data.localRotation);
                if (state.animate.HasFlag(Animatable.TransformOptions.scale))
                    child.localScale = state.useInitialMatrix ? GetInitial(child).lossyScale.ScaleAndReturn(state.data.localScale) : state.data.localScale;
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
            int clipIndex = clips.IndexOf(clip);
            var children = Targets.ToArray();
            
            Vector3[] startPos = children.Select(x=>x.localPosition).ToArray();
            Vector3[] targetPos = children.Select(x=> clip.useInitialMatrix ? GetInitial(x).MultiplyPoint3x4(clip.data.localPos) : clip.data.localPos).ToArray();

            Quaternion[] startRot = children.Select(x => x.localRotation).ToArray();
            Quaternion tRot = Quaternion.Euler(clip.data.localRotation);
            Quaternion[] targetRot = children.Select(x=> clip.useInitialMatrix ? GetInitial(x).rotation * tRot : tRot).ToArray();

            Vector3[] startScale = children.Select(x => x.localScale).ToArray();
            Vector3[] targetScale = children.Select(x=> clip.useInitialMatrix ? GetInitial(x).lossyScale.ScaleAndReturn(clip.data.localScale) : clip.data.localScale).ToArray();
            
            var customs = clip.colorData
                .Select<AnimatableHelpers.ColorReference, (object start, object target)>(x => (x.TargetSourceValue, x.Value))
                .Concat(clip.floatData.Select<AnimatableHelpers.FloatReference, (object startPos, object target)>(x=> (x.TargetSourceValue, x.Value)))
                .ToArray();

            // float progress = 0f;
            float startTime = usedTime;
            int haveToFinish = children.Length;
            clip.events.OnStarted.Invoke();
            Dictionary<int, bool> childTransitionFinished = new Dictionary<int, bool>();
            while (haveToFinish>0)
            {
                var timePassed = usedTime - startTime;
                for (int i = 0; i < children.Length; i++)
                {
                    float progress = Mathf.Clamp01((timePassed - clip.driver.GetDelay(i,children.Length)) / (clip.time * clip.driver.GetDurationMultiplier(i, children.Length)));
                    if (progress == 0f) continue;
                    
                    if (!childTransitionFinished.TryGetValue(i, out var finished))
                    {
                        childTransitionFinished.Add(i, false);
                        childStartedTransition?.Invoke(children[i], clipIndex);
                    } else if (finished) continue;
                    
                    // in case children get destroyed during the transition
                    if (!children[i])
                    {
                        childTransitionFinished[i] = true;
                        haveToFinish--;
                        continue;
                    }
                    
                    clip.EvaluateOn(progress, children[i].gameObject, startPos[i], targetPos[i], startRot[i], targetRot[i], startScale[i], targetScale[i], customs);
                    if (progress >= 1f)
                    {
                        childTransitionFinished[i] = true;
                        haveToFinish--;
                        childEndedTransition?.Invoke(children[i], clipIndex);
                    }
                }
                yield return null;
            }

            for (int i = 0; i < children.Length; i++)
            {
                if(!children[i]) continue;
                clip.EvaluateOn(1f, children[i].gameObject, startPos[i], targetPos[i], startRot[i], targetRot[i], startScale[i], targetScale[i], customs);
                // if(!childTransitionFinished.ContainsKey(i) || !childTransitionFinished[i]) childEndedTransition?.Invoke(children[i], clipIndex);
            }

            clip.events.OnEnded.Invoke();
            currentTransition = null;
            if (clip.loop) chainAtEndOfCurrent += () => Play(clip);
            chainAtEndOfCurrent?.Invoke();
            chainAtEndOfCurrent = null;
        }
        
        
        void OnValidate()   //this probably has to happen :(
        {
            var validComps = GetValidComponents();
            foreach (var comp in clips.SelectMany(clip => clip.colorData))
            {
                if (validComps.Contains(comp.component)) continue;
                var compType = comp.component.GetType();
                comp.component = validComps.FirstOrDefault(x => x.GetType().IsAssignableFrom(compType));
            }
            foreach (var comp in clips.SelectMany(clip => clip.floatData))
            {
                if (validComps.Contains(comp.component)) continue;
                var compType = comp.component.GetType();
                comp.component = validComps.FirstOrDefault(x => x.GetType().IsAssignableFrom(compType));
            }
        }

        public virtual Component[] GetValidComponents()
        {
            if (Targets == null || !Targets.Any()) return new Component[0];
            return Targets?.First()?.GetComponents<Component>();
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(AnimatableMultiple), true)]//, CanEditMultipleObjects]
        public class Inspector : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                var t = target as AnimatableMultiple;
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