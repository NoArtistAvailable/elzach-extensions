using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace elZach.Common{
    public static class EventsExtension
    {
        public static void SafeInvoke(this Action action)
        {
            action?.Invoke();
        }

        public static void SafeInvoke<T>(this Action<T> action, T value)
        {
            action?.Invoke(value);
        }

        public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 value1, T2 value2)
        {
            action?.Invoke(value1, value2);
        }
    }
}