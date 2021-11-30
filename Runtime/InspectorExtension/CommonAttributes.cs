using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace elZach.Common
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class StringOnlyAttribute : PropertyAttribute
    {
        public bool showOnlyValue;

        public StringOnlyAttribute(bool showOnlyValue = false)
        {
            this.showOnlyValue = showOnlyValue;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class FolderAttribute : PropertyAttribute
    {
        public bool relative;
        public FolderAttribute(bool relative = true)
        {
            this.relative = relative;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ShowSpriteAttribute : PropertyAttribute
    {
        public float height;

        public ShowSpriteAttribute(float height)
        {
            this.height = height;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class Vector2RangeAttribute : PropertyAttribute
    {
        public Vector2 minMax;

        public Vector2RangeAttribute(float min, float max)
        {
            this.minMax = new Vector2(min, max);
        }
    }
}