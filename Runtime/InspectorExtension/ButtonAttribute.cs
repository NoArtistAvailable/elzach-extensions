using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace elZach.Common
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ButtonAttribute : Attribute
    {
        string name = null;
        public string Name { get { return name; } }
        public ButtonAttribute()
        {
            this.name = "Button";
        }
        public ButtonAttribute(string name)
        {
            this.name = name;
        }
        
    }
}