using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace elZach.Common
{
    public class LazyGet<T> where T : UnityEngine.Object
    {
        private Func<T> getFunction;
        private T _value;
        public T value
        {
            get
            {
                if (!_value) _value = getFunction.Invoke();
                return _value;
            }
        }
        
        public LazyGet(Func<T> getFunction)
        {
            this.getFunction = getFunction;
        }
    }
}
