using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace elZach.Common
{
    public class AnimationEventRelay : MonoBehaviour
    {
        public UnityEvent[] events;
        public void CallEvent(int index)
        {
            events[index].Invoke();
        }
    }
}