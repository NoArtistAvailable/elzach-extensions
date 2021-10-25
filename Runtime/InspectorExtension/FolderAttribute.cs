using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class FolderAttribute : PropertyAttribute
{
    public bool relative;
    public FolderAttribute(bool relative = true)
    {
        this.relative = relative;
    }
}
