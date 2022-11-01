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
        public bool showValues;

        public Vector2RangeAttribute(float min, float max, bool showValues = true)
        {
            this.minMax = new Vector2(min, max);
            this.showValues = showValues;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public readonly string Condition;

        public ShowIfAttribute(string condition)
        {
            this.Condition = condition;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class DropdownAttribute : PropertyAttribute
    {
        public readonly string FunctionName;
        public readonly bool FunctionInRootObject;

        public DropdownAttribute(string functionName, bool functionInRootObject = false)
        {
            this.FunctionName = functionName;
            this.FunctionInRootObject = functionInRootObject;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class LabelAsAttribute : PropertyAttribute
    {
        public string Label;
        public bool IsProperty;

        public LabelAsAttribute(string label, bool isProperty = false)
        {
            this.Label = label;
            this.IsProperty = isProperty;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class QuickSearchAttribute : PropertyAttribute
    {
        public string searchText;

        public QuickSearchAttribute(string searchText = "")
        {
            this.searchText = searchText;
        }
    }
}