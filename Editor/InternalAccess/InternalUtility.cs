using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace elZach.Access{

    public class InternalUtility
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern object GetStructValueInternal(
            string assemblyName,
            string nameSpace,
            string className);
    }
    
    public static class InternalUtilityExtensions
    {
        public static object GetInternalStructValue(this SerializedProperty property)
#if false
            => property.isArray ? property.GetTargetObjectOfProperty() : property.structValue;
#else
            => property.GetTargetObjectOfProperty();
#endif
        
        public static void SetInternalStructValue(this SerializedProperty property, object value)
#if UNITY_2022_1_OR_NEWER
            { 
                if (property.isArray) property.SetValue(value); 
                else property.structValue = value;
            }
#else
            => property.SetValue(value);
#endif
        
        
        //https://forum.unity.com/threads/get-a-general-object-value-from-serializedproperty.327098/#post-7569286
        public static object GetTargetObjectOfProperty(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            return GetTargetObjectFromPath(obj, path);
        }
        private static object GetTargetObjectFromPath(object obj, string path)
        {
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
 
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();
 
            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);
 
                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);
 
                type = type.BaseType;
            }
            return null;
        }
 
        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;
 
            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
        
        //https://forum.unity.com/threads/get-a-general-object-value-from-serializedproperty.327098/#post-4098484
        private static void SetValue( this SerializedProperty property, object val )
        {
            object obj = property.serializedObject.targetObject;
 
            List<KeyValuePair<FieldInfo, object>> list = new List<KeyValuePair<FieldInfo, object>>();
 
            FieldInfo field = null;
            PropertyInfo propertyInfo = null;
            foreach( var path in property.propertyPath.Split( '.' ) )
            {
                var type = obj.GetType();
                field = type.GetField( path );
                if (field == null)
                {
                    Debug.LogWarning($"null field at {path}");
                    propertyInfo = type.GetProperty(path);
                    Debug.LogWarning($"Property is {propertyInfo?.GetValue( obj )}");
                    // RegexUtility.TryGetPropertyNameFromBackingField(path, out var propertyName);
                    // field = type.
                }
                list.Add( new KeyValuePair<FieldInfo, object>( field, obj ) );
                obj = field.GetValue( obj );
            }
 
            // Now set values of all objects, from child to parent
            for( int i = list.Count - 1; i >= 0; --i )
            {
                list[i].Key.SetValue( list[i].Value, val );
                // New 'val' object will be parent of current 'val' object
                val = list[i].Value;
            }
        }
        
        public static List<object> GetObjectHierarchy(this SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var hierarchy = new List<object>() {obj};
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                    hierarchy.Add(obj);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                    hierarchy.Add(obj);
                }
            }
            return hierarchy;
        }

        public static object GetParentObject(this SerializedProperty prop)
        {
            if (prop.depth == 0) return prop.serializedObject.targetObject;
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            if (path.Contains(".")) path = path.Substring(0, path.LastIndexOf("."));
            else return prop.serializedObject.targetObject;
            return GetTargetObjectFromPath(prop.serializedObject.targetObject, path);
        }
        
        public static SerializedProperty FindPropertyByAutoPropertyName(this SerializedProperty obj, string propName)
        {
            return obj.FindPropertyRelative($"<{propName}>k__BackingField");
        }
    }
}
