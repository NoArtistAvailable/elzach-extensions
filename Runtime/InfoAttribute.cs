using UnityEditor;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class InfoAttribute : PropertyAttribute
{
    public enum InfoType
    {
        None,
        Info,
        Warning,
        Error,
    }
    
    public string message;
    public InfoType type = InfoType.None;
    
    public InfoAttribute(){}

    public InfoAttribute(string message, InfoType type = InfoType.None)
    {
        this.message = message;
        this.type = type;
    }
}
// originally "MessageType" but thats Editor only
