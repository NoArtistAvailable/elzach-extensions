#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(NonSerializedDrawer))]
// [CustomPropertyDrawer(typeof(ShowNonSerializedDrawerAttribute))]
public class NonSerializedDrawerDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		FindMembers(property);
		return membersToDraw?.Count * EditorGUIUtility.singleLineHeight ?? 0;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent _)
	{
		if(membersToDraw == null) return;
		
		if (error != null)
		{
			EditorGUI.HelpBox(position, error, MessageType.Error);
			return;
		}

		// var debugText = string.Join(", ", membersToDraw.Select(m => m.Name));

		var instance = property.serializedObject.targetObject;
		using (new EditorGUI.DisabledScope(true))
		{
			for (var index = 0; index < membersToDraw.Count; index++)
			{
				var mem = membersToDraw[index];
				var rect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * index, position.width, EditorGUIUtility.singleLineHeight);

				var prefix = mem.Name;
				prefix = ObjectNames.NicifyVariableName(prefix);
				
				GUI.Label(rect, new GUIContent(prefix));
				rect = EditorGUI.PrefixLabel(rect, new GUIContent(prefix));

				GUIContent? label = null;
				
				// first check cache if previous access did cause exception
				if (guiContentCache.Count > index)
				{
					if (guiContentCache[index].wasError)
					{
						// previous access did cause exception
						// so dont call GetValue again to avoid internal exception spam
						// this CAN happen e.g in edit time when reading a property getter
						// that expects a certain setup
						label = guiContentCache[index].label;
					}
				}

				if (label == null)
				{
					try
					{
						label = new GUIContent(GetValue(instance, mem)?.ToString() ?? "null");
						// put in cache so we can lookup by index, every member should have one label in the cache at least
						if(index >= guiContentCache.Count)
							guiContentCache.Add((label, false));
					}
					catch (Exception e)
					{
						// cache that exception the first time it happens
						if (index >= guiContentCache.Count)
						{
							var errorLabel = label = new GUIContent(e.GetType().Name, e.ToString());
							guiContentCache.Add((errorLabel, true));
						}
						else label = guiContentCache[index].label;
					}
				}
				
				GUI.Label(rect, label);
			}
		}
	}

	private List<MemberInfo>? membersToDraw = null;
	private string? error;
	private readonly List<(GUIContent label, bool wasError)> guiContentCache = new List<(GUIContent label, bool wasError)>();

	private void FindMembers(SerializedProperty property)
	{
		if (membersToDraw != null) return;
		membersToDraw = new List<MemberInfo>();
		try
		{
			var instance = property.serializedObject.targetObject;
			var type = instance.GetType();
			// declared only otherwise we easily have A LOT
			var members = type.GetMembers(
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
			var hasMembersExplicitly = false;
			
			// var field = attribute as ShowNonSerializedDrawerAttribute;
			// if (field != null && field.members != null)
			// {
			// 	foreach (var name in field.members)
			// 	{
			// 		hasMembersExplicitly = true;
			// 		var found = FindMember(name, members);
			// 		if (found != null) membersToDraw.Add(found);
			// 	}
			// }
			//
			// if (field == null)
			{
				if (fieldInfo.FieldType == typeof(NonSerializedDrawer))
				{
					var val = (NonSerializedDrawer)fieldInfo.GetValue(instance);
					if (val.members != null)
					{
						foreach (var name in val.members)
						{
							hasMembersExplicitly = true;
							var found = FindMember(name, members);
							if (found != null) membersToDraw.Add(found);
						}
					}
				}
			}

			if (!hasMembersExplicitly)
			{
				foreach (var mem in members)
				{
					if (mem.GetCustomAttribute<ShowNonSerializedAttribute>() != null && !membersToDraw.Any(m => m.Name == mem.Name))
						membersToDraw.Add(mem);

				}
			}
		}
		catch(Exception e)
		{
			error = e.Message;
		}
	}

	private MemberInfo? FindMember(string name, MemberInfo[] members)
	{
		if (membersToDraw?.Any(m => m.Name == name) ?? true) return null;
		foreach (var mem in members)
		{
			if (mem.Name == name)
			{
				return mem;
			}
		}
		return null;
	}

	private static object? GetValue(object instance, MemberInfo member)
	{
		switch (member)
		{
			case PropertyInfo prop:
				if (prop.CanRead) return prop.GetValue(instance);
				break;
			case FieldInfo field:
				return field.GetValue(instance);
		}

		return null;
	}
}
