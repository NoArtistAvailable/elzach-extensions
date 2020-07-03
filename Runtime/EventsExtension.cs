using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace elZach.Common{
    public static class EventsExtension
    {
        
    }

    [System.Serializable]
    public class Collider2DEvent : UnityEvent<Collider2D> { } // this is not neccessary anymore and can be deleted
}