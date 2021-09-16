#nullable enable

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace elZach.Common
{
	public abstract class ButtonBase
	{
		public abstract bool CanInvoke(object? declaringInstance, out string? failReason);
		public abstract void Invoke(object? declaringInstance);
		
		public float height;
		public Color? color;
	}

	[Serializable]
	public class InspectorButton : ButtonBase
	{
		internal readonly Action? callback;

		public InspectorButton(Action callback)
		{
			this.callback = callback;
		}

		public override bool CanInvoke(object? declaringInstance, out string? failReason)
		{
			failReason = null;
			return callback != null;
		}

		public override void Invoke(object? declaringInstance)
		{
			callback?.Invoke();
		}
	}

	[Serializable]
	public class Button<T> : ButtonBase
	{
		internal readonly Action<T>? callback;

		public Button(Action<T> callback)
		{
			this.callback = callback;
		}

		public override bool CanInvoke(object? declaringInstance, out string? failReason)
		{
			if (callback != null)
			{
				if (declaringInstance == null)
				{
					failReason = "Instance is null";
					return false;
				}
				if (!(declaringInstance is T))
				{
					failReason = $"Expected {nameof(InspectorButton)}<{declaringInstance.GetType()}> but is {nameof(InspectorButton)}<{typeof(T)}>";
					return false;
				}
			}
			failReason = null;
			return true;
		}

		public override void Invoke(object? declaringInstance)
		{
			if (declaringInstance != null && declaringInstance is T instance)
			{
				this.callback?.Invoke(instance);
			}
		}
	}


#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(InspectorButton))]
	[CustomPropertyDrawer(typeof(Button<>))]
	public class CallbackDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			Init(property);
			var height = base.GetPropertyHeight(property, label);
			if (button != null && button.height > height) return button.height;
			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			string? failReason = default;
			if (button?.CanInvoke(instance, out failReason) ?? false)
			{
				var prev = GUI.color;
				try
				{
					if (button.color != null)
						GUI.color = button.color.Value;
					if (GUI.Button(position, ObjectNames.NicifyVariableName(fieldInfo.Name)))
					{
						button.Invoke(instance);
					}
				}
				finally
				{
					GUI.color = prev;
				}
			}
			else if (failReason != null)
			{
				EditorGUI.HelpBox(position, property.name + ": " + failReason, MessageType.Error);
			}
		}

		private bool didInit;
		private Object? instance;
		private ButtonBase? button;

		private void Init(SerializedProperty property)
		{
			if (didInit) return;
			didInit = true;
			instance = property.serializedObject.targetObject;
			button = fieldInfo.GetValue(instance) as ButtonBase;
		}
	}


#endif
}
