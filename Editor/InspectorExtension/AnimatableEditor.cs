using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using elZach.Access;

namespace elZach.Common
{
	[CustomEditor(typeof(Animatable)), CanEditMultipleObjects]
	public class AnimatableEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			//DrawDefaultInspector();
			var t = target as Animatable;

			var animateOnEnableProperty = serializedObject.FindProperty(nameof(Animatable.animateAtOnEnable));
			var animateToProperty = serializedObject.FindProperty(nameof(Animatable.animateAtOnEnableTo));
			var clipsProperty = serializedObject.FindProperty(nameof(Animatable.clips));
			
			var rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
			rect.width /= 2f;
			EditorGUI.PropertyField(rect, animateOnEnableProperty);
			rect.x += rect.width;
			
			EditorGUI.BeginDisabledGroup(!animateOnEnableProperty.boolValue);
			animateToProperty.intValue = EditorGUI.IntField(rect, animateToProperty.intValue);
			EditorGUI.EndDisabledGroup();
			
			EditorGUILayout.PropertyField(clipsProperty);
			serializedObject.ApplyModifiedProperties();
			
			if (t.clips == null) return;
			EditorGUI.BeginDisabledGroup(!Application.isPlaying);
			EditorGUILayout.BeginHorizontal();
			for(int i=0; i < t.clips.Count;i++)
				if(GUILayout.Button(i.ToString())) t.Play(i);
			EditorGUILayout.EndHorizontal();
			EditorGUI.EndDisabledGroup();
		}
	}

	[CustomPropertyDrawer(typeof(Animatable.Clip))]
	public class AnimatableClipDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
			Animatable.Clip clip = property.GetInternalStructValue() as Animatable.Clip;
			int lines = 5;
			lines += (clip.animate.HasFlag(Animatable.TransformOptions.position) ? 1 : 0)
			         + (clip.animate.HasFlag(Animatable.TransformOptions.rotation) ? 1 : 0)
			         + (clip.animate.HasFlag(Animatable.TransformOptions.scale) ? 1 : 0);
			return EditorGUIUtility.singleLineHeight * lines 
			       + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Animatable.Clip.events))) 
			       + (clip.animate.HasFlag(Animatable.TransformOptions.color) ? EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Animatable.Clip.colorData))) : 0)
			       + (clip.animate.HasFlag(Animatable.TransformOptions.single) ? EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Animatable.Clip.floatData))) : 0)
			       + 16;
		}
	
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var numberString = Regex.Match(label.text, @"\d+").Value;
			label.text = "Animation State " + numberString + " ( ";
			var animate = (Animatable.TransformOptions) property.FindPropertyRelative(nameof(Animatable.Clip.animate)).intValue;
			if (animate.HasFlag(Animatable.TransformOptions.position)) label.text += "Position ";
			if (animate.HasFlag(Animatable.TransformOptions.rotation)) label.text += "Rotation ";
			if (animate.HasFlag(Animatable.TransformOptions.scale)) label.text += "Scale ";
			if (animate.HasFlag(Animatable.TransformOptions.color)) label.text += "Color ";
			if (animate.HasFlag(Animatable.TransformOptions.single)) label.text += "Float ";
			label.text += ")";
			EditorGUI.BeginProperty(position, label, property);
			var rect = position;
			position.height = EditorGUIUtility.singleLineHeight;
			property.isExpanded = EditorGUI.Foldout (position, property.isExpanded, label);
			if (property.isExpanded)
			{
				EditorGUI.BeginChangeCheck();
				Animatable.Clip clip = property.GetInternalStructValue() as Animatable.Clip;
				rect = position;
				rect.y += EditorGUIUtility.singleLineHeight;
				rect.height = EditorGUIUtility.singleLineHeight * 2;
				property.FindPropertyRelative(nameof(Animatable.Clip.curve)).animationCurveValue =
					EditorGUI.CurveField(rect, clip.curve);
				rect.y += rect.height;
				rect.height = EditorGUIUtility.singleLineHeight;
	
				rect.width = position.width / 2f - 6;
				property.FindPropertyRelative(nameof(Animatable.Clip.time)).floatValue =
					EditorGUI.FloatField(rect, clip.time);
				rect.x += rect.width + 3;
				property.FindPropertyRelative(nameof(Animatable.Clip.animate)).intValue =
					Convert.ToInt32(EditorGUI.EnumFlagsField(rect, clip.animate));
	
				rect.y += rect.height;
				rect.x = position.x;
				rect.width = 22;
				property.FindPropertyRelative(nameof(Animatable.Clip.loop)).boolValue =
					EditorGUI.Toggle(rect, clip.loop);
				rect.x += rect.width + 3;
				rect.width = position.width / 4f - 6;
				EditorGUI.LabelField(rect, "Loop");
				rect.x += rect.width + 3;
				rect.width = 22;
				property.FindPropertyRelative(nameof(Animatable.Clip.useInitialMatrix)).boolValue =
					EditorGUI.Toggle(rect, clip.useInitialMatrix);
	
				rect.x += rect.width + 3;
				rect.width = position.width / 4f - 6;
				EditorGUI.LabelField(rect, "Initial Matrix");
	
				rect.y += rect.height + 8;
				rect.width = position.width;
				rect.x = position.x;
	
				if (clip.animate.HasFlag(Animatable.TransformOptions.position))
				{
					EditorGUI.PropertyField(rect,
						property.FindPropertyRelative(nameof(clip.data) + "." + nameof(clip.data.localPos)));
					rect.y += rect.height;
				}
	
				if (clip.animate.HasFlag(Animatable.TransformOptions.rotation))
				{
					EditorGUI.PropertyField(rect,
						property.FindPropertyRelative(nameof(clip.data) + "." + nameof(clip.data.localRotation)));
					rect.y += rect.height;
				}
	
				if (clip.animate.HasFlag(Animatable.TransformOptions.scale))
				{
					EditorGUI.PropertyField(rect,
						property.FindPropertyRelative(nameof(clip.data) + "." + nameof(clip.data.localScale)));
					rect.y += rect.height;
				}

				if (clip.animate.HasFlag(Animatable.TransformOptions.color))
				{
					var customListProperty = property.FindPropertyRelative(nameof(Animatable.Clip.colorData));
					if (customListProperty != null)
					{
						rect.height = EditorGUI.GetPropertyHeight(customListProperty);
						EditorGUI.PropertyField(rect, customListProperty);
						rect.y += rect.height;
						rect.height = EditorGUIUtility.singleLineHeight;
					}
				}
				
				if (clip.animate.HasFlag(Animatable.TransformOptions.single))
				{
					var customListProperty = property.FindPropertyRelative(nameof(Animatable.Clip.floatData));
					if (customListProperty != null)
					{
						rect.height = EditorGUI.GetPropertyHeight(customListProperty);
						EditorGUI.PropertyField(rect, customListProperty);
						rect.y += rect.height;
						rect.height = EditorGUIUtility.singleLineHeight;
					}
				}
	
				var eventsProperty = property.FindPropertyRelative(nameof(clip.events));
				// rect.height = EditorGUI.GetPropertyHeight(eventsProperty);
				rect.height = EditorGUIUtility.singleLineHeight + 8;
				//EditorGUI.PropertyField(rect, eventsProperty);
				string eventsLabel = "Events";
				int onStartedEventCount = clip.events.OnStarted.GetPersistentEventCount();
				int onExitedEventCount = clip.events.OnEnded.GetPersistentEventCount();
				eventsLabel += onStartedEventCount > 0 ? $" | Started ({onStartedEventCount})" : "";
				eventsLabel += onExitedEventCount > 0 ? $" | Ended ({onExitedEventCount})" : "";
				eventsProperty.isExpanded = EditorGUI.Foldout(rect, eventsProperty.isExpanded, eventsLabel);
				if (eventsProperty.isExpanded)
				{
					rect.y += rect.height;
					var enterEventProperty = property.FindPropertyRelative(nameof(clip.events) + "." + nameof(clip.events.OnStarted));
					rect.height = EditorGUI.GetPropertyHeight(enterEventProperty);
					EditorGUI.PropertyField(rect, enterEventProperty);
					rect.y += rect.height;
					var exitEventProperty = property.FindPropertyRelative(nameof(clip.events) + "." + nameof(clip.events.OnEnded));
					EditorGUI.PropertyField(rect, exitEventProperty);
					rect.y += EditorGUI.GetPropertyHeight(exitEventProperty);
				}
				
				if (EditorGUI.EndChangeCheck())
				{
					property.serializedObject.ApplyModifiedProperties();
				}
			}
	
			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(AnimatableChildren.DrivenClip))]
	public class DrivenClipDrawer : AnimatableClipDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) + EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(AnimatableChildren.DrivenClip.driver)));
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(AnimatableChildren.DrivenClip.driver)), true);
			position.y += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(AnimatableChildren.DrivenClip.driver)));//base.GetPropertyHeight(property, label);
			base.OnGUI(position, property, label);
			
			// property.serializedObject.ApplyModifiedProperties();
		}
	}
}
